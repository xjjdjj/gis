﻿#pragma checksum "D:\20150114江宁智慧安监\源码20150303\AYKJ.GISDevelop\Control\Measure.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "0BC35DC32A5CF7BEAB14787906836281"
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


namespace AYKJ.GISDevelop.Control {
    
    
    public partial class Measure : System.Windows.Controls.UserControl {
        
        internal ESRI.ArcGIS.Client.Symbols.MarkerSymbol MarkerSymbolGKTip;
        
        internal ESRI.ArcGIS.Client.Symbols.MarkerSymbol MarkerSymbolEndPoint;
        
        internal ESRI.ArcGIS.Client.Symbols.MarkerSymbol MarkerSymbolMidPoint;
        
        internal ESRI.ArcGIS.Client.Symbols.MarkerSymbol MarkerSymbolAreaEndPoint;
        
        internal ESRI.ArcGIS.Client.Symbols.MarkerSymbol MarkerSymbolAreaPoint;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/AYKJ.GISDevelop;component/Control/Measure.xaml", System.UriKind.Relative));
            this.MarkerSymbolGKTip = ((ESRI.ArcGIS.Client.Symbols.MarkerSymbol)(this.FindName("MarkerSymbolGKTip")));
            this.MarkerSymbolEndPoint = ((ESRI.ArcGIS.Client.Symbols.MarkerSymbol)(this.FindName("MarkerSymbolEndPoint")));
            this.MarkerSymbolMidPoint = ((ESRI.ArcGIS.Client.Symbols.MarkerSymbol)(this.FindName("MarkerSymbolMidPoint")));
            this.MarkerSymbolAreaEndPoint = ((ESRI.ArcGIS.Client.Symbols.MarkerSymbol)(this.FindName("MarkerSymbolAreaEndPoint")));
            this.MarkerSymbolAreaPoint = ((ESRI.ArcGIS.Client.Symbols.MarkerSymbol)(this.FindName("MarkerSymbolAreaPoint")));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
        }
    }
}

