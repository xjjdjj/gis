﻿#pragma checksum "D:\项目\百度地图\石家庄源码-百度地图20150715\AYKJ.GISDevelop\Control\MapNavControl.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "870617D3D8D9DB0BCE03D91661EE8D78"
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


namespace AYKJ.GISDevelop.Control {
    
    
    public partial class MapNavControl : System.Windows.Controls.Page {
        
        internal ESRI.ArcGIS.Client.Symbols.MarkerSymbol ThematicSymbol;
        
        internal ESRI.ArcGIS.Client.Symbols.MarkerSymbol ThematicSymbol_WXY;
        
        internal ESRI.ArcGIS.Client.Symbols.MarkerSymbol HighMarkerStyle;
        
        internal System.Windows.Style TipHelp_xx;
        
        internal System.Windows.Style TipHelp_sp;
        
        internal System.Windows.Style TipHelp_pmt;
        
        internal System.Windows.Style TipHelp_jy;
        
        internal ESRI.ArcGIS.Client.Symbols.MarkerSymbol BusinessSymbol;
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/AYKJ.GISDevelop;component/Control/MapNavControl.xaml", System.UriKind.Relative));
            this.ThematicSymbol = ((ESRI.ArcGIS.Client.Symbols.MarkerSymbol)(this.FindName("ThematicSymbol")));
            this.ThematicSymbol_WXY = ((ESRI.ArcGIS.Client.Symbols.MarkerSymbol)(this.FindName("ThematicSymbol_WXY")));
            this.HighMarkerStyle = ((ESRI.ArcGIS.Client.Symbols.MarkerSymbol)(this.FindName("HighMarkerStyle")));
            this.TipHelp_xx = ((System.Windows.Style)(this.FindName("TipHelp_xx")));
            this.TipHelp_sp = ((System.Windows.Style)(this.FindName("TipHelp_sp")));
            this.TipHelp_pmt = ((System.Windows.Style)(this.FindName("TipHelp_pmt")));
            this.TipHelp_jy = ((System.Windows.Style)(this.FindName("TipHelp_jy")));
            this.BusinessSymbol = ((ESRI.ArcGIS.Client.Symbols.MarkerSymbol)(this.FindName("BusinessSymbol")));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
        }
    }
}

