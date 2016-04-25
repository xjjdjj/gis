/// <summary>  
/// 作者：陈锋 
/// 时间：2012/5/21 16:16:22  
/// 公司:南京安元科技有限公司  
/// 版权：2012-2020  
/// CLR版本：4.0.30319.261  
/// Prosess说明：
/// 唯一标识：5e303fbd-b805-41e9-9c96-4367eb103b8a  
/// </summary>

using System.Windows;
using System.Windows.Controls;

namespace AYKJ.GISDevelop
{
    public partial class Prosess : UserControl
    {
        public Prosess()
        {
            // 为初始化变量所必需
            InitializeComponent();
        }
        private double _value;

        public double Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                setValue();
            }
        }

        private void setValue()
        {
            // 在此处添加事件处理程序实现。
            double width = this.ActualWidth / 2;
            double d = width / 100 * _value;
            bm.Margin = new Thickness(d, 0, d, 0);
        }
    }
}
