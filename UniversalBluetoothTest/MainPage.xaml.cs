using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UniversalBluetoothTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // The Chat Server's custom service Uuid
        private static readonly Guid RfcommChatServiceUuid = Guid.Parse("72503AD7-9FE3-4EF9-8CCD-8B009F583C36");

        // The Id of the Service Name SDP attribute
        private const UInt16 SdpServiceNameAttributeId = 0x100;

        // The SDP Type of the Service Name SDP attribute.
        // The first byte in the SDP Attribute encodes the SDP Attribute Type as follows :
        //    -  the Attribute Type size in the least significant 3 bits,
        //    -  the SDP Attribute Type value in the most significant 5 bits.
        private const byte SdpServiceNameAttributeType = (4 << 3) | 5;

        // The value of the Service Name SDP attribute
        private const string SdpServiceName = "Bluetooth Rfcomm Chat Service";

        private StreamSocket socket;
        private DataWriter writer;
        private RfcommServiceProvider rfcommProvider;
        private RfcommDeviceService chatService;
        private StreamSocketListener socketListener;

        private DeviceInformationCollection chatServiceInfoCollection;

        public MainPage()
        {
            this.InitializeComponent();

            App.Current.Suspending += App_Suspending;
        }

        void App_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            // Make sure we cleanup resources on suspend
            Disconnect();
        }

        /// <summary>
        /// Initialize a server socket listening for incoming Bluetooth Rfcomm connections
        /// </summary>
        async void InitializeRfcommServer()
        {
            try
            {
                ServerButton.IsEnabled = false;
                ClientButton.IsEnabled = false;
                DisconnectButton.IsEnabled = true;

                //var devicesInfoCollection = await DeviceInformation.FindAllAsync(
                //    RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort));


                rfcommProvider = await RfcommServiceProvider.CreateAsync(
                    RfcommServiceId.FromUuid(RfcommChatServiceUuid));

                // Create a listener for this service and start listening
                socketListener = new StreamSocketListener();
                socketListener.ConnectionReceived += OnConnectionReceived;

                await socketListener.BindServiceNameAsync(rfcommProvider.ServiceId.AsString(),
                    SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication);

                // Set the SDP attributes and start Bluetooth advertising
                InitializeServiceSdpAttributes(rfcommProvider);
                rfcommProvider.StartAdvertising(socketListener);

                NotifyStatus("Listening for incoming connections");
            }
            catch (Exception e)
            {
                NotifyError(e);
            }
        }
        /// <summary>
        /// Initialize the Rfcomm service's SDP attributes.
        /// </summary>
        /// <param name="rfcommProvider">The Rfcomm service provider to initialize.</param>
        private void InitializeServiceSdpAttributes(RfcommServiceProvider rfcommProvider)
        {
            var sdpWriter = new DataWriter();

            // Write the Service Name Attribute.

            sdpWriter.WriteByte(SdpServiceNameAttributeType);

            // The length of the UTF-8 encoded Service Name SDP Attribute.
            sdpWriter.WriteByte((byte)SdpServiceName.Length);

            // The UTF-8 encoded Service Name value.
            sdpWriter.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
            sdpWriter.WriteString(SdpServiceName);

            // Set the SDP Attribute on the RFCOMM Service Provider.
            rfcommProvider.SdpRawAttributes.Add(SdpServiceNameAttributeId, sdpWriter.DetachBuffer());
        }

        /// <summary>
        /// Invoked when the socket listener accepted an incoming Bluetooth connection.
        /// </summary>
        /// <param name="sender">The socket listener that accecpted the connection.</param>
        /// <param name="args">The connection accept parameters, which contain the connected socket.</param>
        private async void OnConnectionReceived(
            StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            try
            {
                NotifyStatus("Client Connected");

                // Don't need the listener anymore
                socketListener.Dispose();
                socketListener = null;

                socket = args.Socket;

                writer = new DataWriter(socket.OutputStream);

                var reader = new DataReader(socket.InputStream);
                bool remoteDisconnection = false;
                while (true)
                {
                    uint readLength = await reader.LoadAsync(sizeof(uint));
                    if (readLength < sizeof(uint))
                    {
                        remoteDisconnection = true;
                        break;
                    }
                    uint currentLength = reader.ReadUInt32();

                    readLength = await reader.LoadAsync(currentLength);
                    if (readLength < currentLength)
                    {
                        remoteDisconnection = true;
                        break;
                    }
                    string message = reader.ReadString(currentLength);

                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        ConversationListBox.Items.Add("Received: " + message);
                    });
                }

                reader.DetachStream();
                if (remoteDisconnection)
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Disconnect();
                        NotifyStatus("Client disconnected.");
                    });
                }
            }
            catch (Exception e)
            {
                NotifyError(e);
            }
        }

        /// <summary>
        /// Send message over the Bluetooth socket.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (socket != null)
                {
                    string message = string.IsNullOrEmpty(MessageTextBox.Text) ? "This is a stinking test" : MessageTextBox.Text;
                    writer.WriteUInt32((uint)message.Length);
                    writer.WriteString(message);

                    await writer.StoreAsync();
                    ConversationListBox.Items.Add("Sent: " + message);

                    // Clear the messageTextBox for a new message
                    MessageTextBox.Text = "";
                }
                else
                {
                    NotifyStatus("No clients connected, please wait for a client to connect before attempting to send a message");
                }
            }
            catch (Exception ex)
            {
                NotifyError(ex);
            }
        }

        /// <summary>
        /// Start the Bluetooth server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListenButton_Click(object sender, RoutedEventArgs e)
        {
            InitializeRfcommServer();
        }
        
        private async void ClientButton_Click(object sender, RoutedEventArgs e)
        {
            //            chatServiceInfoCollection = await DeviceInformation.FindAllAsync();
            chatServiceInfoCollection = await DeviceInformation.FindAllAsync(
                RfcommDeviceService.GetDeviceSelector(RfcommServiceId.FromUuid(RfcommChatServiceUuid)));

            if (chatServiceInfoCollection.Count > 0)
            {
                List<string> items = new List<string>();
                foreach (var chatServiceInfo in chatServiceInfoCollection)
                {
                    items.Add(chatServiceInfo.Kind + ": " + chatServiceInfo.Name + " : " + chatServiceInfo.Id);
                }
                cvs.Source = items;
                ServiceSelector.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                NotifyStatus("No chat services were found. Please pair with a device that is advertising the chat service.");
            }
        }

        private async void ServiceList_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                ServerButton.IsEnabled = false;
                ClientButton.IsEnabled = false;
                DisconnectButton.IsEnabled = true;
                ServiceSelector.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                var chatServiceInfo = chatServiceInfoCollection[ServiceList.SelectedIndex];

                DeviceInformation deviceInfo = await DeviceInformation.CreateFromIdAsync(chatServiceInfo.Id);
                chatService = await RfcommDeviceService.FromIdAsync(deviceInfo.Id);

                if (chatService == null)
                {
                    NotifyStatus("Access to the device is denied because the application was not granted access");
                    return;
                }

                var attributes = await chatService.GetSdpRawAttributesAsync();
                if (!attributes.ContainsKey(SdpServiceNameAttributeId))
                {
                    NotifyStatus("The Chat service is not advertising the Service Name attribute (attribute id=0x100). " +
                        "Please verify that you are running the BluetoothRfcommChat server.");
                    return;
                }

                var attributeReader = DataReader.FromBuffer(attributes[SdpServiceNameAttributeId]);
                var attributeType = attributeReader.ReadByte();
                if (attributeType != SdpServiceNameAttributeType)
                {
                    NotifyStatus("The Chat service is using an unexpected format for the Service Name attribute. " +
                        "Please verify that you are running the BluetoothRfcommChat server.");
                    return;
                }

                var serviceNameLength = attributeReader.ReadByte();

                // The Service Name attribute requires UTF-8 encoding. 
                attributeReader.UnicodeEncoding = UnicodeEncoding.Utf8;
                ServiceName.Visibility = Visibility.Visible;
                ServiceName.Text = "Service Name: \"" + attributeReader.ReadString(serviceNameLength) + "\"";

                lock (this)
                {
                    socket = new StreamSocket();
                }

                await socket.ConnectAsync(chatService.ConnectionHostName, chatService.ConnectionServiceName);

                writer = new DataWriter(socket.OutputStream);

                DataReader chatReader = new DataReader(socket.InputStream);

                while (true)
                {
                    try
                    {
                        uint size = await chatReader.LoadAsync(sizeof(uint));
                        if (size < sizeof(uint))
                        {
                            // The underlying socket was closed before we were able to read the whole data 
                            break;
                        }

                        uint stringLength = chatReader.ReadUInt32();
                        uint actualStringLength = await chatReader.LoadAsync(stringLength);
                        if (actualStringLength != stringLength)
                        {
                            // The underlying socket was closed before we were able to read the whole data 
                            break;
                        }

                        ConversationListBox.Items.Add("Received: \"" + chatReader.ReadString(stringLength) + "\"");
                    }
                    catch (Exception ex)
                    {
                        lock (this)
                        {
                            if (socket == null)
                            {
                                // Do not print anything here -  the user closed the socket. 
                            }
                            else
                            {
                                NotifyStatus("Read stream failed with error: " + ex.Message);
                                Disconnect();
                            }
                        }
                    }                
                }
            }
            catch (Exception ex)
            {
                ServerButton.IsEnabled = true;
                ClientButton.IsEnabled = true;
                DisconnectButton.IsEnabled = false;

                NotifyStatus("Error: " + ex.HResult.ToString() + " - " + ex.Message);
            }
        }

        /// <summary>
        /// Stop Bluetooth server and cleanup any outstanding connections.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            Disconnect();
            NotifyStatus("Disconnected.");
        }

        /// <summary>
        /// Cleanup Bluetooth resources
        /// </summary>
        private void Disconnect()
        {
            if (rfcommProvider != null)
            {
                rfcommProvider.StopAdvertising();
                rfcommProvider = null;
            }

            if (socketListener != null)
            {
                socketListener.Dispose();
                socketListener = null;
            }

            if (writer != null)
            {
                writer.DetachStream();
                writer = null;
            }

            if (socket != null)
            {
                socket.Dispose();
                socket = null;
            }

            ServiceName.Visibility = Visibility.Collapsed;
            ServerButton.IsEnabled = true;
            ClientButton.IsEnabled = true;
            DisconnectButton.IsEnabled = false;
            ConversationListBox.Items.Clear();
        }

        private async void NotifyStatus(string message)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                ConversationListBox.Items.Add("Status: " + message);
            });
        }

        private async void NotifyError(Exception e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                ConversationListBox.Items.Add("ERROR: " + String.Format("0x{0:X8}", e.HResult) + " - " + e.Message);
            });
        }
    }
}
