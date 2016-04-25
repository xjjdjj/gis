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
using Divelements.SilverlightTools;

namespace AYKJ.GISDevelop
{
    public partial class SwfWindow : UserControl
    {
        public SwfWindow()
        {
            InitializeComponent();
        }

        public void BindControl(Button btn,double x,double y)
        {
            this.x = x;
            this.y = y;
            //btn.MouseEnter += new MouseEventHandler(btn_MouseEnter);
            
            btn.Click += new RoutedEventHandler(btn_Click);
        }

        double x, y;
        void btn_MouseEnter(object sender, MouseEventArgs e)
        {
            
        }

        public void CancelBindControl(Button btn)
        {
            //this.tip.IsOpen = false;
            btn.Click -= btn_Click;
        }

        void btn_Click(object sender, RoutedEventArgs e)
        {
            string uri = (sender as Button).Tag as string;
            GetRichContent(uri, UriKind.RelativeOrAbsolute);
            if (this.tip.IsOpen)
            {
                this.tip.IsOpen = false;
            }
            else
            {
                this.tip.IsOpen = true;
            }
            
            Button btn = sender as Button;

            this.tip.HorizontalOffset = x;
            this.tip.VerticalOffset = y;
        }

        //获取Rich Content
        void GetRichContent(string uri, UriKind uk)
        {
            Container.Children.Clear();
            ControlHtmlHost chtml = new ControlHtmlHost();
            HtmlHost hh = chtml.FindName("htmlHost") as HtmlHost;
            hh.SourceUri = new Uri(uri, uk);
            Container.Children.Add(chtml);
        }
    }
}
