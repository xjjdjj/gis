﻿#pragma checksum "D:\项目\百度地图\石家庄源码-百度地图20150715\AYKJ.GISInterface\Control\AdvAPP\DataTools.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "66B7B2FE01505CE9324BC762A55E20C0"
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


namespace AYKJ.GISInterface.Control.AdvAPP {
    
    
    public partial class DataTools : System.Windows.Controls.UserControl {
        
        internal ESRI.ArcGIS.Client.Symbols.MarkerSymbol ThematicSymbol;
        
        internal System.Windows.Style TipHelp_xx;
        
        internal System.Windows.Style TipHelp_sp;
        
        internal System.Windows.Style TipHelp_pmt;
        
        internal System.Windows.Style TipHelp_jy;
        
        internal ESRI.ArcGIS.Client.Symbols.MarkerSymbol BusinessSymbol;
        
        internal ESRI.ArcGIS.Client.Symbols.MarkerSymbol HighMarkerStyle;
        
        internal System.Windows.Media.Animation.Storyboard Storyboard_fadeout;
        
        internal System.Windows.Media.Animation.Storyboard Storyboard_fadein;
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.Border border;
        
        internal System.Windows.Controls.Button btn_close;
        
        internal System.Windows.Controls.StackPanel ToolBar;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/AYKJ.GISInterface;component/Control/AdvAPP/DataTools.xaml", System.UriKind.Relative));
            this.ThematicSymbol = ((ESRI.ArcGIS.Client.Symbols.MarkerSymbol)(this.FindName("ThematicSymbol")));
            this.TipHelp_xx = ((System.Windows.Style)(this.FindName("TipHelp_xx")));
            this.TipHelp_sp = ((System.Windows.Style)(this.FindName("TipHelp_sp")));
            this.TipHelp_pmt = ((System.Windows.Style)(this.FindName("TipHelp_pmt")));
            this.TipHelp_jy = ((System.Windows.Style)(this.FindName("TipHelp_jy")));
            this.BusinessSymbol = ((ESRI.ArcGIS.Client.Symbols.MarkerSymbol)(this.FindName("BusinessSymbol")));
            this.HighMarkerStyle = ((ESRI.ArcGIS.Client.Symbols.MarkerSymbol)(this.FindName("HighMarkerStyle")));
            this.Storyboard_fadeout = ((System.Windows.Media.Animation.Storyboard)(this.FindName("Storyboard_fadeout")));
            this.Storyboard_fadein = ((System.Windows.Media.Animation.Storyboard)(this.FindName("Storyboard_fadein")));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.border = ((System.Windows.Controls.Border)(this.FindName("border")));
            this.btn_close = ((System.Windows.Controls.Button)(this.FindName("btn_close")));
            this.ToolBar = ((System.Windows.Controls.StackPanel)(this.FindName("ToolBar")));
        }
    }
}

