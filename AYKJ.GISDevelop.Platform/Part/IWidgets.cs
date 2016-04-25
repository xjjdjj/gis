/// <summary>  
/// 作者：陈锋 
/// 时间：2012/5/21 15:48:58  
/// 公司:南京安元科技有限公司  
/// 版权：2012-2020  
/// CLR版本：4.0.30319.261  
/// IWidgets说明：IWidgets的接口，靠边可以停靠的面板
/// 唯一标识：b0d8a7f6-34d5-49d5-b312-509bda0f05df  
/// </summary>

namespace AYKJ.GISDevelop.Platform.Part
{
    public delegate void IWidgetEventHander(IWidgets sender);
    public interface IWidgets : IPart
    {
        void Open();
        event IWidgetEventHander OpenEnd;

    }
}
