﻿#pragma checksum "D:\项目\百度地图\石家庄源码-百度地图20150715\AYKJ.GISDevelop.Platform\ToolKit\WaitAnimationWindow.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "31A1CBDE0A01EA19463612E2CFBBB2B4"
//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.18444
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


namespace AYKJ.GISDevelop.Platform.ToolKit {
    
    
    public partial class WaitAnimationWindow : System.Windows.Controls.ChildWindow {
        
        internal System.Windows.Media.Animation.Storyboard bigRound;
        
        internal System.Windows.Media.Animation.DoubleAnimation z;
        
        internal System.Windows.Media.Animation.Storyboard smallRound;
        
        internal System.Windows.Media.Animation.DoubleAnimation s;
        
        internal System.Windows.Media.Animation.Storyboard Storyboard7;
        
        internal System.Windows.Media.Animation.Storyboard bigRound2;
        
        internal System.Windows.Media.Animation.DoubleAnimation z2;
        
        internal System.Windows.Media.Animation.Storyboard smallRound2;
        
        internal System.Windows.Media.Animation.DoubleAnimation s2;
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/AYKJ.GISDevelop.Platform;component/ToolKit/WaitAnimationWindow.xaml", System.UriKind.Relative));
            this.bigRound = ((System.Windows.Media.Animation.Storyboard)(this.FindName("bigRound")));
            this.z = ((System.Windows.Media.Animation.DoubleAnimation)(this.FindName("z")));
            this.smallRound = ((System.Windows.Media.Animation.Storyboard)(this.FindName("smallRound")));
            this.s = ((System.Windows.Media.Animation.DoubleAnimation)(this.FindName("s")));
            this.Storyboard7 = ((System.Windows.Media.Animation.Storyboard)(this.FindName("Storyboard7")));
            this.bigRound2 = ((System.Windows.Media.Animation.Storyboard)(this.FindName("bigRound2")));
            this.z2 = ((System.Windows.Media.Animation.DoubleAnimation)(this.FindName("z2")));
            this.smallRound2 = ((System.Windows.Media.Animation.Storyboard)(this.FindName("smallRound2")));
            this.s2 = ((System.Windows.Media.Animation.DoubleAnimation)(this.FindName("s2")));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.roundB2 = ((System.Windows.Shapes.Path)(this.FindName("roundB2")));
            this.roundB = ((System.Windows.Shapes.Path)(this.FindName("roundB")));
            this.roundS2 = ((System.Windows.Shapes.Path)(this.FindName("roundS2")));
            this.roundS = ((System.Windows.Shapes.Path)(this.FindName("roundS")));
            this.textBlcok = ((System.Windows.Controls.TextBlock)(this.FindName("textBlcok")));
        }
    }
}

