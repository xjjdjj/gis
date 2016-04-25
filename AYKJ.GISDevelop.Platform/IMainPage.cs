/// <summary>  
/// 作者：陈锋 
/// 时间：2012/5/21 16:04:02  
/// 公司:南京安元科技有限公司  
/// 版权：2012-2020  
/// CLR版本：4.0.30319.261  
/// IMainPage说明：系统首页的接口
/// 唯一标识：03816bee-7899-485b-9c28-8b788cf8f140  
/// </summary>

using System.Windows.Controls;
using AYKJ.GISDevelop.Platform.Part;

namespace AYKJ.GISDevelop.Platform
{
    public interface IMainPage
    {
        void AddToRight(IPart part);
        void RemoveFromRight(IPart part);
        void UpdateMenuButtonTo(string name);
        void CloseMenu(string name);
    }
}
