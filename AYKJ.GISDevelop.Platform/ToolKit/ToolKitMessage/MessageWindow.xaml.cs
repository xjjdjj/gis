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

namespace AYKJ.GISDevelop.Platform.ToolKit
{
    public partial class MessageWindow : ChildWindow
    {
        public MessageWindow()
        {
            InitializeComponent();
        }

        public void MessageBtnOpera(string type)
        {
            if(type=="only"){
                OKButton.Visibility = Visibility.Collapsed;
                CancelButton.Visibility = Visibility.Collapsed;
                OKButtonOnly.Visibility = Visibility.Visible;
            }
        }

        public MessageWindow(MsgType type, string title, string info)
        {
            InitializeComponent();
            //base.Content = this.LayoutRoot;
            //
            StackPanel sptitle = new StackPanel();
            //sptitle.Height = 35;
            sptitle.Children.Add(new TextBlock()
            {
                Text = title,
                TextAlignment = TextAlignment.Left
            });

            this.Title = sptitle;
            TxtInfo.Text = info;

        }

        public void setMoreInfo(string code, string info)
        {
            MIButton.Visibility = Visibility.Visible;
            MoreInfoCode.Text = "问题代码：" + code;
            MoreInfoInfo.Text = "详细内容：" + info;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private string MITran = "MoreInfoCollapse";
		
        private void MIButton_Click(object sender, RoutedEventArgs e)
        {
            if (MoreInfoGrid.Visibility == Visibility.Collapsed)
                MoreInfoGrid.Visibility = Visibility.Visible;
            
            if (MITran == "MoreInfoExpand")
                MITran = "MoreInfoCollapse";
            else
                MITran = "MoreInfoExpand";
            var collapseAnimation = (Storyboard)Resources[MITran];
            collapseAnimation.Completed += (m, n) =>
            {

            };
            collapseAnimation.Begin();
        }

    }

    public enum MsgType
    {
        Error,
        Info
    }
}

