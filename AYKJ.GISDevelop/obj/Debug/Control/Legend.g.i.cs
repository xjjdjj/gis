﻿#pragma checksum "D:\20150114江宁智慧安监\源码20150303\AYKJ.GISDevelop\Control\Legend.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "B1D1E638E7B70F427FA2267B715FB527"
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


namespace AYKJ.GISDevelop.Control {
    
    
    public partial class Legend : System.Windows.Controls.UserControl {
        
        internal System.Windows.Media.Animation.Storyboard Storyboard_Show;
        
        internal System.Windows.Media.Animation.Storyboard Storyboard_Close;
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.Button btn_close;
        
        internal System.Windows.Controls.Image Img_Legend;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/AYKJ.GISDevelop;component/Control/Legend.xaml", System.UriKind.Relative));
            this.Storyboard_Show = ((System.Windows.Media.Animation.Storyboard)(this.FindName("Storyboard_Show")));
            this.Storyboard_Close = ((System.Windows.Media.Animation.Storyboard)(this.FindName("Storyboard_Close")));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.btn_close = ((System.Windows.Controls.Button)(this.FindName("btn_close")));
            this.Img_Legend = ((System.Windows.Controls.Image)(this.FindName("Img_Legend")));
        }
    }
}

