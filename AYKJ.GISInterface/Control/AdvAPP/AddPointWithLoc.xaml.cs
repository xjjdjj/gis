using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using AYKJ.GISDevelop.Platform;
using AYKJ.GISDevelop.Platform.ToolKit;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.FeatureService.Symbols;
using System.Windows.Media;
using System.Text;
using System.Windows.Input;
using System.IO;

namespace AYKJ.GISInterface
{
    public partial class AddPointWithLoc : UserControl
    {
        //承载的地图
        Map mainmap;
        //动画
        WaitAnimationWindow waw;
        //20130922
        string t_oParm;
        string m_wxyid;
        string m_wxytype;
        string m_dwdm;
        string m_idkey;
        string m_remark;
        string m_x;
        string m_y;
        GISInterface.MainPage mp;
        TextBox tb_x;
        TextBox tb_y;
        public AddPointWithLoc()
        {
            InitializeComponent();
        }

        void InitAutoSearch()
        {
            //设置面板的起始位置
            this.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            this.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            this.Margin = new Thickness() { Top = 10, Right = 13 };

            mainmap = (Application.Current as IApp).MainMap;
            
            Storyboard_Close.Completed += new EventHandler(Storyboard_Close_Completed);
        }

        private void InitPanel(string oParm)
        {
            t_oParm = oParm;
            try
            {
                m_wxyid = oParm.Split('|')[0];
                m_wxytype = oParm.Split('|')[1];
                m_dwdm = oParm.Split('|')[2];
                m_remark = oParm.Split('|')[3];

                m_x = oParm.Split('|')[4];
                m_y = oParm.Split('|')[5];
            }
            catch(Exception e)
            {
                MessageBox.Show("参数解析错误，请检查传入参数。");
            }

            StackPanel spAddPoint = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Vertical,
                Margin = new Thickness(15, 0, 15, 15)
            };

            StackPanel sp_X = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Horizontal
            };
            Label lbl_x = new Label()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(5, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Bottom,
                Content = "X 坐标：",
                Foreground = new SolidColorBrush( Color.FromArgb(255, 255, 255, 255))
            };

            tb_x = new TextBox()
            {
                Name = "tb_x",
                Margin = new Thickness(5, 5, 5, 0),
                TextWrapping = TextWrapping.Wrap,
                Width = 130,
                Text = m_x,
                TextAlignment = System.Windows.TextAlignment.Right, 
                VerticalAlignment = VerticalAlignment.Center
            };
            tb_x.Style = this.Resources["TextBoxStyle"] as Style;
            sp_X.Children.Add(lbl_x);
            sp_X.Children.Add(tb_x);

            StackPanel sp_Y = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Horizontal
            };
            Label lbl_y = new Label()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(5, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Bottom,
                Content = "Y 坐标：",
                Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))
            };
            tb_y = new TextBox()
            {
                Name = "tb_y",
                Margin = new Thickness(5, 5, 5, 0),
                TextWrapping = TextWrapping.Wrap,
                Width = 130,
                Text = m_y,
                TextAlignment = System.Windows.TextAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };
            tb_y.Style = this.Resources["TextBoxStyle"] as Style;
            sp_Y.Children.Add(lbl_y);
            sp_Y.Children.Add(tb_y);

            spAddPoint.Children.Add(sp_X);
            spAddPoint.Children.Add(sp_Y);


            addPoint_grid.Children.Add(spAddPoint);


        }

        public void LoadAddPointWithLoc(string oParm)
        {
            if (PFApp.Root.Children.Count > 0)
            {
                for (int i = 0; i < PFApp.Root.Children.Count; i++)
                {
                    if (PFApp.Root.Children[i].GetType().Name == "AddPointWithLoc")
                    {
                        PFApp.Root.Children.RemoveAt(i);
                    }
                }
            }
            PFApp.Root.Children.Add(this);
            Storyboard_Show.Begin();

            InitPanel(oParm);
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            switch (btn.Name)
            {
                case "btn_reset":
                    tb_x.Text = m_x;
                    tb_y.Text = m_y;
                    break;
                case "btn_cancle":
                    tb_x.Text = "";
                    tb_y.Text = "";
                    Storyboard_Close.Begin();
                    break;
                case "btn_confirm":
                    tb_x.Text = "";
                    tb_y.Text = "";
                    Storyboard_Close.Begin();
                    mp = new GISInterface.MainPage();//2130922：是否可以这样new一个有待验证。
                    mp.link_addData_WithLocation(t_oParm);

                    break;
            }
        }

        /// <summary>
        /// 数据重置
        /// </summary>
        public void Reset()
        {
            try
            {

            }
            catch (Exception)
            { }
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            Storyboard_Close.Begin();
        }

        void Storyboard_Close_Completed(object sender, EventArgs e)
        {
            Reset();
            if (PFApp.Root.Children.Contains(this))
            {
                PFApp.Root.Children.Remove(this);
            }
        }

        #region 缩小放大
        private void btn_Max_Click(object sender, RoutedEventArgs e)
        {
            Storyboard_Max.Begin();
            btn_MinClose.Visibility = System.Windows.Visibility.Collapsed;
            btn_Max.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void btn_Min_Click(object sender, RoutedEventArgs e)
        {
            Storyboard_Min.Begin();
            btn_MinClose.Visibility = System.Windows.Visibility.Visible;
            btn_Max.Visibility = System.Windows.Visibility.Visible;
        }
        #endregion

        /// <summary>
        /// 文字名缩减
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        string EllipsisName(string str)
        {
            string tmpstr = string.Empty;
            int length = Encoding.Unicode.GetByteCount(str);
            if (length < 19)
            {
                tmpstr = str;
            }
            else
            {
                tmpstr = str.Substring(0, 9) + "...";
            }
            return tmpstr;
        }

        private void data_result_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            TextBox tbx = new TextBox()
            {
                FontFamily = new FontFamily("NSimSun"),
                FontSize = 12,
                Background = new SolidColorBrush(Colors.Black),
                Foreground = new SolidColorBrush(Colors.White),
                BorderBrush = null,
                Margin = new Thickness(-9, -3, -9, -3)
            };
            tbx.Text = (e.Row.DataContext as clskmwxy).wxytip;
            ToolTipService.SetToolTip(e.Row, tbx);
        }

        /// <summary>
        /// 只允许输入数字
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBok_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            TextBox txt = sender as TextBox;
            //屏蔽非法按键
            if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
            {
                e.Handled = false;
            }
            else if ((e.Key >= Key.D0 && e.Key <= Key.D9))
            {

                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }
    }

}
