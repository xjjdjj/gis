/// <summary>  
/// 作者：陈锋 
/// 时间：2012/5/21 15:58:16  
/// 公司:南京安元科技有限公司  
/// 版权：2012-2020  
/// CLR版本：4.0.30319.261  
/// MessageWindow说明：
/// 唯一标识：9cf22caa-23d2-4b04-be30-31d90309296e  
/// </summary>

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System;

namespace AYKJ.GISDevelop.Platform.ToolKit
{
    public partial class PMTWindow : ChildWindow
    {
        string[] pmtUrl;

        public PMTWindow()
        {
            InitializeComponent();
        }

        public void MessageBtnOpera(string type)
        {
        }

        public PMTWindow(string title, string[] arr_url)
        {
            InitializeComponent();

            //获取传入的平面图的地址数组
            this.pmtUrl = arr_url;

            //设置窗口的标题
            StackPanel sptitle = new StackPanel();
            //sptitle.Height = 35;
            sptitle.Children.Add(new TextBlock()
            {
                Text = title,
                TextAlignment = TextAlignment.Left
            });
            this.Title = sptitle;

            TreeViewItem item1b = new TreeViewItem();
            item1b.Header = "平面图";
            item1b.Tag = "平面图";
            item1b.IsExpanded = true;   //展开节点

            for (int k = 0; k < arr_url.Length; k++)
            {
                item1b.Items.Add(new TreeViewItem() { Tag = arr_url[k], Header = "图" + k, IsSelected = false });
            }

            treePMT.Items.Add(item1b); 
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void treePMT_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem tv = treePMT.SelectedItem as TreeViewItem;
            string s = tv.Tag.ToString();
            if(s.Contains("http"))
                imgPmt.Source = new BitmapImage(new Uri(s, UriKind.RelativeOrAbsolute));
        }


    }

}

