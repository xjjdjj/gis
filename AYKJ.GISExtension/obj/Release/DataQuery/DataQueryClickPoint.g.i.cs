﻿#pragma checksum "E:\13-参与项目\石家庄安监局\源码20150715\AYKJ.GISExtension\DataQuery\DataQueryClickPoint.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "5D34F289D79C1561BDE025EFD98DB275"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17929
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
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


namespace AYKJ.GISExtension {
    
    
    public partial class DataQueryClickPoint : System.Windows.Controls.UserControl {
        
        internal ESRI.ArcGIS.Client.Symbols.MarkerSymbol HighMarkerStyle;
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.Button btn_clickPoint;
        
        internal System.Windows.Controls.Button btn_clicDelete;
        
        internal System.Windows.Controls.DataGrid data_result_c;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/AYKJ.GISExtension;component/DataQuery/DataQueryClickPoint.xaml", System.UriKind.Relative));
            this.HighMarkerStyle = ((ESRI.ArcGIS.Client.Symbols.MarkerSymbol)(this.FindName("HighMarkerStyle")));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.btn_clickPoint = ((System.Windows.Controls.Button)(this.FindName("btn_clickPoint")));
            this.btn_clicDelete = ((System.Windows.Controls.Button)(this.FindName("btn_clicDelete")));
            this.data_result_c = ((System.Windows.Controls.DataGrid)(this.FindName("data_result_c")));
        }
    }
}

