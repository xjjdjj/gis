﻿#pragma checksum "E:\13-参与项目\石家庄安监局\源码20150715\AYKJ.GISExtension\DataQuery\DataQueryPageExpose.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "5E21BBFAB8F09FFAFDB263EA9E28DDBD"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17929
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
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


namespace AYKJ.GISExtension {
    
    
    public partial class DataQueryPageExpose : System.Windows.Controls.UserControl {
        
        internal System.Windows.Media.Animation.Storyboard Storyboard_Show;
        
        internal System.Windows.Media.Animation.Storyboard Storyboard_Close;
        
        internal System.Windows.Media.Animation.Storyboard Storyboard_Min;
        
        internal System.Windows.Media.Animation.Storyboard Storyboard_Max;
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.Border border;
        
        internal System.Windows.Controls.Border border1;
        
        internal System.Windows.Controls.Button btn_close;
        
        internal System.Windows.Controls.Button btn_Min;
        
        internal System.Windows.Controls.Button btn_Max;
        
        internal System.Windows.Controls.Button btn_MinClose;
        
        internal System.Windows.Controls.Grid grid;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/AYKJ.GISExtension;component/DataQuery/DataQueryPageExpose.xaml", System.UriKind.Relative));
            this.Storyboard_Show = ((System.Windows.Media.Animation.Storyboard)(this.FindName("Storyboard_Show")));
            this.Storyboard_Close = ((System.Windows.Media.Animation.Storyboard)(this.FindName("Storyboard_Close")));
            this.Storyboard_Min = ((System.Windows.Media.Animation.Storyboard)(this.FindName("Storyboard_Min")));
            this.Storyboard_Max = ((System.Windows.Media.Animation.Storyboard)(this.FindName("Storyboard_Max")));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.border = ((System.Windows.Controls.Border)(this.FindName("border")));
            this.border1 = ((System.Windows.Controls.Border)(this.FindName("border1")));
            this.btn_close = ((System.Windows.Controls.Button)(this.FindName("btn_close")));
            this.btn_Min = ((System.Windows.Controls.Button)(this.FindName("btn_Min")));
            this.btn_Max = ((System.Windows.Controls.Button)(this.FindName("btn_Max")));
            this.btn_MinClose = ((System.Windows.Controls.Button)(this.FindName("btn_MinClose")));
            this.grid = ((System.Windows.Controls.Grid)(this.FindName("grid")));
        }
    }
}

