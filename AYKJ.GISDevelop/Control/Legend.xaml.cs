/// <summary>  
/// 作者：陈锋 
/// 时间：2012/5/24 12:27:16  
/// 公司:南京安元科技有限公司  
/// 版权：2012-2020  
/// CLR版本：4.0.30319.261  
/// Legend说明：系统图例，因暂时没有图例，先拿平台说明
/// 唯一标识：dc957a17-c613-41c7-bafa-94942b35b23c  
/// </summary>
/// 
using System;
using System.Windows;
using System.Windows.Controls;
using AYKJ.GISDevelop.Platform;
using System.Windows.Controls.Primitives;

namespace AYKJ.GISDevelop.Control
{
    public partial class Legend : UserControl
    {
        public ToggleButton currrentogbtn;
        public Legend()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(Legend_Loaded);
        }

        void Legend_Loaded(object sender, RoutedEventArgs e)
        {
            //设置面板的起始位置
            this.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            this.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            this.Margin = new Thickness(0, 0, 30, 5);
            Storyboard_Close.Completed += new EventHandler(Storyboard_Close_Completed);
        }

        #region 两侧面板的展示和关闭
        /// <summary>
        /// 面板展开
        /// </summary>
        public void Show()
        {
            //展开面板
            PFApp.Root.Children.Add(this);
            Storyboard_Show.Begin();
        }
        /// <summary>
        /// 面板关闭方法
        /// </summary>
        public void Close()
        {
            currrentogbtn.IsChecked = false;
            Storyboard_Close.Begin();
        }

        void Storyboard_Close_Completed(object sender, EventArgs e)
        {
            //关闭动画结束后移除该面板
            PFApp.Root.Children.Remove(this);
        }

        #endregion

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
