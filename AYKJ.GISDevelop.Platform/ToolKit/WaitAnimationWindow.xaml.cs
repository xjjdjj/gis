using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace AYKJ.GISDevelop.Platform.ToolKit
{
    public partial class WaitAnimationWindow : ChildWindow
    {
        public WaitAnimationWindow()
        {
            InitializeComponent();
        }

        public WaitAnimationWindow(string strinfo)
        {
            InitializeComponent();
            if (strinfo.Trim() != "")
            {
                textBlcok.Text = strinfo;
            }
            bigRound.Begin();
            smallRound.Begin();
            Storyboard7.Begin();
            bigRound2.Begin();
            smallRound2.Begin();
        }

        public void Change(string strinfo)
        {
            if (strinfo.Trim() != "")
            {
                textBlcok.Text = strinfo;
            }
        }
    }
}

