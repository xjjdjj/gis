﻿#pragma checksum "D:\20150114江宁智慧安监\源码20150303\AYKJ.GISDevelop.Platform\ToolKit\ToolKitMessage\副本 MessageWindow.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "516BAE5CD1BE0212417C0AD5A85861F0"
//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.1008
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace AYKJ.GISDevelop.Platform.ToolKit {
    
    
    public partial class MessageWindow : System.Windows.Controls.ChildWindow {
        
        internal System.Windows.Media.Animation.Storyboard MoreInfoExpand;
        
        internal System.Windows.Media.Animation.Storyboard MoreInfoCollapse;
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.TextBox TxtInfo;
        
        internal System.Windows.Controls.Button CancelButton;
        
        internal System.Windows.Controls.Button OKButton;
        
        internal System.Windows.Controls.Button OKButtonOnly;
        
        internal System.Windows.Controls.Button MIButton;
        
        internal System.Windows.Controls.StackPanel MoreInfoGrid;
        
        internal System.Windows.Controls.TextBox MoreInfoCode;
        
        internal System.Windows.Controls.TextBox MoreInfoInfo;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/AYKJ.GISDevelop.Platform;component/ToolKit/ToolKitMessage/%E5%89%AF%E6%9C%AC%20M" +
                        "essageWindow.xaml", System.UriKind.Relative));
            this.MoreInfoExpand = ((System.Windows.Media.Animation.Storyboard)(this.FindName("MoreInfoExpand")));
            this.MoreInfoCollapse = ((System.Windows.Media.Animation.Storyboard)(this.FindName("MoreInfoCollapse")));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.TxtInfo = ((System.Windows.Controls.TextBox)(this.FindName("TxtInfo")));
            this.CancelButton = ((System.Windows.Controls.Button)(this.FindName("CancelButton")));
            this.OKButton = ((System.Windows.Controls.Button)(this.FindName("OKButton")));
            this.OKButtonOnly = ((System.Windows.Controls.Button)(this.FindName("OKButtonOnly")));
            this.MIButton = ((System.Windows.Controls.Button)(this.FindName("MIButton")));
            this.MoreInfoGrid = ((System.Windows.Controls.StackPanel)(this.FindName("MoreInfoGrid")));
            this.MoreInfoCode = ((System.Windows.Controls.TextBox)(this.FindName("MoreInfoCode")));
            this.MoreInfoInfo = ((System.Windows.Controls.TextBox)(this.FindName("MoreInfoInfo")));
        }
    }
}

