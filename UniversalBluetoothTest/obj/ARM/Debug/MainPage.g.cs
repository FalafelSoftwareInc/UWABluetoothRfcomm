﻿#pragma checksum "C:\Users\Bary\Documents\Visual Studio 2015\Projects\UniversalBluetoothTest\UniversalBluetoothTest\MainPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "3B5BB7A5C3BAB87BCE822040FA932D84"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace UniversalBluetoothTest
{
    partial class MainPage : 
        global::Windows.UI.Xaml.Controls.Page, 
        global::Windows.UI.Xaml.Markup.IComponentConnector,
        global::Windows.UI.Xaml.Markup.IComponentConnector2
    {
        /// <summary>
        /// Connect()
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 14.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                {
                    this.cvs = (global::Windows.UI.Xaml.Data.CollectionViewSource)(target);
                }
                break;
            case 2:
                {
                    this.Input = (global::Windows.UI.Xaml.Controls.Grid)(target);
                }
                break;
            case 3:
                {
                    this.ServiceName = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 4:
                {
                    this.ServiceSelector = (global::Windows.UI.Xaml.Controls.Grid)(target);
                }
                break;
            case 5:
                {
                    this.MessageTextBox = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                }
                break;
            case 6:
                {
                    this.SendButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 57 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)this.SendButton).Click += this.SendButton_Click;
                    #line default
                }
                break;
            case 7:
                {
                    this.ConversationListBox = (global::Windows.UI.Xaml.Controls.ListBox)(target);
                }
                break;
            case 8:
                {
                    this.ServiceList = (global::Windows.UI.Xaml.Controls.ListBox)(target);
                    #line 46 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.ListBox)this.ServiceList).Tapped += this.ServiceList_Tapped;
                    #line default
                }
                break;
            case 9:
                {
                    this.ServerButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 26 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)this.ServerButton).Click += this.ListenButton_Click;
                    #line default
                }
                break;
            case 10:
                {
                    this.ClientButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 27 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)this.ClientButton).Click += this.ClientButton_Click;
                    #line default
                }
                break;
            case 11:
                {
                    this.DisconnectButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 28 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)this.DisconnectButton).Click += this.DisconnectButton_Click;
                    #line default
                }
                break;
            default:
                break;
            }
            this._contentLoaded = true;
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 14.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::Windows.UI.Xaml.Markup.IComponentConnector GetBindingConnector(int connectionId, object target)
        {
            global::Windows.UI.Xaml.Markup.IComponentConnector returnValue = null;
            return returnValue;
        }
    }
}

