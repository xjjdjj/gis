﻿#pragma checksum "E:\13-参与项目\石家庄安监局\源码20150715\AYKJ.GISDevelop\Control\Identify.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "66E8E9186CD9F3810E92442CDF39143E"
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


namespace AYKJ.GISDevelop.Control {
    
    
    public partial class Identify : System.Windows.Controls.UserControl {
        
        internal System.Windows.Media.Animation.Storyboard bigRound;
        
        internal System.Windows.Media.Animation.DoubleAnimation z;
        
        internal System.Windows.Media.Animation.Storyboard smallRound;
        
        internal System.Windows.Media.Animation.DoubleAnimation s;
        
        internal System.Windows.Media.Animation.Storyboard Storyboard7;
        
        internal System.Windows.Media.Animation.Storyboard bigRound2;
        
        internal System.Windows.Media.Animation.DoubleAnimation z2;
        
        internal System.Windows.Media.Animation.Storyboard smallRound2;
        
        internal System.Windows.Media.Animation.DoubleAnimation s2;
        
        internal System.Windows.Media.Animation.Storyboard Storyboard_Show;
        
        internal System.Windows.Media.Animation.Storyboard Storyboard_Close;
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.Button btn_close;
        
        internal System.Windows.Controls.TextBox txt_layer;
        
        internal System.Windows.Controls.Button btn_showlayer;
        
        internal System.Windows.Controls.TreeView layertree;
        
        internal System.Windows.Controls.TextBox txt_coor;
        
        internal System.Windows.Controls.DataGrid dataresult;
        
        internal System.Windows.Controls.TextBlock txt_count;
        
        internal System.Windows.Controls.Grid GridLayer;
        
        internal System.Windows.Controls.TreeView tree;
        
        internal System.Windows.Controls.Grid tip_show_canvas;
        
        internal System.Windows.Shapes.Path roundB2;
        
        internal System.Windows.Shapes.Path roundB;
        
        internal System.Windows.Shapes.Path roundS2;
        
        internal System.Windows.Shapes.Path roundS;
        
        internal System.Windows.Controls.TextBlock textBlcok;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/AYKJ.GISDevelop;component/Control/Identify.xaml", System.UriKind.Relative));
            this.bigRound = ((System.Windows.Media.Animation.Storyboard)(this.FindName("bigRound")));
            this.z = ((System.Windows.Media.Animation.DoubleAnimation)(this.FindName("z")));
            this.smallRound = ((System.Windows.Media.Animation.Storyboard)(this.FindName("smallRound")));
            this.s = ((System.Windows.Media.Animation.DoubleAnimation)(this.FindName("s")));
            this.Storyboard7 = ((System.Windows.Media.Animation.Storyboard)(this.FindName("Storyboard7")));
            this.bigRound2 = ((System.Windows.Media.Animation.Storyboard)(this.FindName("bigRound2")));
            this.z2 = ((System.Windows.Media.Animation.DoubleAnimation)(this.FindName("z2")));
            this.smallRound2 = ((System.Windows.Media.Animation.Storyboard)(this.FindName("smallRound2")));
            this.s2 = ((System.Windows.Media.Animation.DoubleAnimation)(this.FindName("s2")));
            this.Storyboard_Show = ((System.Windows.Media.Animation.Storyboard)(this.FindName("Storyboard_Show")));
            this.Storyboard_Close = ((System.Windows.Media.Animation.Storyboard)(this.FindName("Storyboard_Close")));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.btn_close = ((System.Windows.Controls.Button)(this.FindName("btn_close")));
            this.txt_layer = ((System.Windows.Controls.TextBox)(this.FindName("txt_layer")));
            this.btn_showlayer = ((System.Windows.Controls.Button)(this.FindName("btn_showlayer")));
            this.layertree = ((System.Windows.Controls.TreeView)(this.FindName("layertree")));
            this.txt_coor = ((System.Windows.Controls.TextBox)(this.FindName("txt_coor")));
            this.dataresult = ((System.Windows.Controls.DataGrid)(this.FindName("dataresult")));
            this.txt_count = ((System.Windows.Controls.TextBlock)(this.FindName("txt_count")));
            this.GridLayer = ((System.Windows.Controls.Grid)(this.FindName("GridLayer")));
            this.tree = ((System.Windows.Controls.TreeView)(this.FindName("tree")));
            this.tip_show_canvas = ((System.Windows.Controls.Grid)(this.FindName("tip_show_canvas")));
            this.roundB2 = ((System.Windows.Shapes.Path)(this.FindName("roundB2")));
            this.roundB = ((System.Windows.Shapes.Path)(this.FindName("roundB")));
            this.roundS2 = ((System.Windows.Shapes.Path)(this.FindName("roundS2")));
            this.roundS = ((System.Windows.Shapes.Path)(this.FindName("roundS")));
            this.textBlcok = ((System.Windows.Controls.TextBlock)(this.FindName("textBlcok")));
        }
    }
}

