﻿#pragma checksum "D:\项目\百度地图\石家庄源码-百度地图20150715\AYKJ.GISStatistics\MainPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "16E813E47A68BA018A7FB9ABB7C2BEF5"
//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.18444
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


namespace AYKJ.GISStatistics {
    
    
    public partial class MainPage : System.Windows.Controls.UserControl {
        
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
        
        internal System.Windows.Controls.ComboBox cbx_county;
        
        internal System.Windows.Controls.ComboBox cbx_town;
        
        internal System.Windows.Controls.Button btn_Finish;
        
        internal System.Windows.Controls.ScrollViewer scrolls1;
        
        internal System.Windows.Controls.StackPanel sp_layer;
        
        internal System.Windows.Controls.CheckBox chkLayer;
        
        internal System.Windows.Controls.RadioButton rbtn_data;
        
        internal System.Windows.Controls.RadioButton rbtn_chart;
        
        internal System.Windows.Controls.RadioButton rbtn_chartpie;
        
        internal System.Windows.Controls.TextBlock txt_count;
        
        internal System.Windows.Controls.Grid grid_data;
        
        internal System.Windows.Controls.DataGrid data_result;
        
        internal System.Windows.Controls.Grid grid_chart;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/AYKJ.GISStatistics;component/MainPage.xaml", System.UriKind.Relative));
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
            this.cbx_county = ((System.Windows.Controls.ComboBox)(this.FindName("cbx_county")));
            this.cbx_town = ((System.Windows.Controls.ComboBox)(this.FindName("cbx_town")));
            this.btn_Finish = ((System.Windows.Controls.Button)(this.FindName("btn_Finish")));
            this.scrolls1 = ((System.Windows.Controls.ScrollViewer)(this.FindName("scrolls1")));
            this.sp_layer = ((System.Windows.Controls.StackPanel)(this.FindName("sp_layer")));
            this.chkLayer = ((System.Windows.Controls.CheckBox)(this.FindName("chkLayer")));
            this.rbtn_data = ((System.Windows.Controls.RadioButton)(this.FindName("rbtn_data")));
            this.rbtn_chart = ((System.Windows.Controls.RadioButton)(this.FindName("rbtn_chart")));
            this.rbtn_chartpie = ((System.Windows.Controls.RadioButton)(this.FindName("rbtn_chartpie")));
            this.txt_count = ((System.Windows.Controls.TextBlock)(this.FindName("txt_count")));
            this.grid_data = ((System.Windows.Controls.Grid)(this.FindName("grid_data")));
            this.data_result = ((System.Windows.Controls.DataGrid)(this.FindName("data_result")));
            this.grid_chart = ((System.Windows.Controls.Grid)(this.FindName("grid_chart")));
        }
    }
}

