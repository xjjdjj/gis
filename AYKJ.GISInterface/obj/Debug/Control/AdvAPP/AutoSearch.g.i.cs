﻿#pragma checksum "D:\20150114江宁智慧安监\源码20150303\AYKJ.GISInterface\Control\AdvAPP\AutoSearch.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "473C28FD9872F632BABC3C563926EB37"
//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.1008
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

using ESRI.ArcGIS.Client.Symbols;
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


namespace AYKJ.GISInterface {
    
    
    public partial class AutoSearch : System.Windows.Controls.UserControl {
        
        internal ESRI.ArcGIS.Client.Symbols.MarkerSymbol HighMarkerStyle;
        
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
        
        internal System.Windows.Controls.TextBox txt_every;
        
        internal System.Windows.Controls.TextBox txt_all;
        
        internal System.Windows.Controls.Button btn_point;
        
        internal System.Windows.Controls.Button btn_clear;
        
        internal System.Windows.Controls.ScrollViewer scrolls1;
        
        internal System.Windows.Controls.StackPanel sp_layer;
        
        internal System.Windows.Controls.Label label1;
        
        internal System.Windows.Controls.CheckBox chkLayer;
        
        internal System.Windows.Controls.DataGrid data_result;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/AYKJ.GISInterface;component/Control/AdvAPP/AutoSearch.xaml", System.UriKind.Relative));
            this.HighMarkerStyle = ((ESRI.ArcGIS.Client.Symbols.MarkerSymbol)(this.FindName("HighMarkerStyle")));
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
            this.txt_every = ((System.Windows.Controls.TextBox)(this.FindName("txt_every")));
            this.txt_all = ((System.Windows.Controls.TextBox)(this.FindName("txt_all")));
            this.btn_point = ((System.Windows.Controls.Button)(this.FindName("btn_point")));
            this.btn_clear = ((System.Windows.Controls.Button)(this.FindName("btn_clear")));
            this.scrolls1 = ((System.Windows.Controls.ScrollViewer)(this.FindName("scrolls1")));
            this.sp_layer = ((System.Windows.Controls.StackPanel)(this.FindName("sp_layer")));
            this.label1 = ((System.Windows.Controls.Label)(this.FindName("label1")));
            this.chkLayer = ((System.Windows.Controls.CheckBox)(this.FindName("chkLayer")));
            this.data_result = ((System.Windows.Controls.DataGrid)(this.FindName("data_result")));
        }
    }
}

