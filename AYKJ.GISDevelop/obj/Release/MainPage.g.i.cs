﻿#pragma checksum "E:\13-参与项目\石家庄安监局\源码20150715\AYKJ.GISDevelop\MainPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "F6F75D829BD4522A46EF8F9707D629BF"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17929
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using AYKJ.GISDevelop.Control;
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


namespace AYKJ.GISDevelop {
    
    
    public partial class MainPage : System.Windows.Controls.UserControl {
        
        internal System.Windows.Style TipHelp_xx;
        
        internal System.Windows.Style TipHelp_sp;
        
        internal System.Windows.Style TipHelp_pmt;
        
        internal System.Windows.Style TipHelp_jy;
        
        internal ESRI.ArcGIS.Client.Symbols.MarkerSymbol BusinessSymbol;
        
        internal ESRI.ArcGIS.Client.Symbols.MarkerSymbol HighMarkerStyle;
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal AYKJ.GISDevelop.Control.OverView viewMap;
        
        internal System.Windows.Controls.Grid CRoot;
        
        internal System.Windows.Controls.StackPanel DRoot;
        
        internal ESRI.ArcGIS.Client.Toolkit.Navigation MapNavigation;
        
        internal AYKJ.GISDevelop.Control.BarArea menuBar;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/AYKJ.GISDevelop;component/MainPage.xaml", System.UriKind.Relative));
            this.TipHelp_xx = ((System.Windows.Style)(this.FindName("TipHelp_xx")));
            this.TipHelp_sp = ((System.Windows.Style)(this.FindName("TipHelp_sp")));
            this.TipHelp_pmt = ((System.Windows.Style)(this.FindName("TipHelp_pmt")));
            this.TipHelp_jy = ((System.Windows.Style)(this.FindName("TipHelp_jy")));
            this.BusinessSymbol = ((ESRI.ArcGIS.Client.Symbols.MarkerSymbol)(this.FindName("BusinessSymbol")));
            this.HighMarkerStyle = ((ESRI.ArcGIS.Client.Symbols.MarkerSymbol)(this.FindName("HighMarkerStyle")));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.viewMap = ((AYKJ.GISDevelop.Control.OverView)(this.FindName("viewMap")));
            this.CRoot = ((System.Windows.Controls.Grid)(this.FindName("CRoot")));
            this.DRoot = ((System.Windows.Controls.StackPanel)(this.FindName("DRoot")));
            this.MapNavigation = ((ESRI.ArcGIS.Client.Toolkit.Navigation)(this.FindName("MapNavigation")));
            this.menuBar = ((AYKJ.GISDevelop.Control.BarArea)(this.FindName("menuBar")));
        }
    }
}

