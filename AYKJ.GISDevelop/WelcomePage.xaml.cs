/// <summary>  
/// 作者：陈锋 
/// 时间：2012/5/21 16:18:18  
/// 公司:南京安元科技有限公司  
/// 版权：2012-2020  
/// CLR版本：4.0.30319.261  
/// WelcomePage说明：
/// 唯一标识：52a5eb73-7a59-4113-8a83-4866e48359d5  
/// </summary>

using System.Windows.Controls;
using AYKJ.GISDevelop.Platform;

namespace AYKJ.GISDevelop
{
    public partial class WelcomePage : UserControl, IWelcomePage
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public WelcomePage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 设置应用当前的状态
        /// </summary>
        /// <param name="statu"></param>
        public void SetStatu(string statu)
        {
            //Statu为TextBlock，实际开发将做效果优化
            Statu.Text = statu;
        }


        public void SetStatu(double res, double all)
        {
            prosess1.Value = (int)((res / all) * 100);
            //Statu.Text = (res / all).ToString();
        }
    }
}
