/// <summary>  
/// 作者：陈锋 
/// 时间：2012/5/21 15:41:55  
/// 公司:南京安元科技有限公司  
/// 版权：2012-2020  
/// CLR版本：4.0.30319.261  
/// PartEntity说明：part程序集的配置信息
/// 唯一标识：1d82adb1-8cf0-4d87-beed-74d28179c65d  
/// </summary>

using System.Reflection;

namespace AYKJ.GISDevelop.Platform.Config.Entity
{
    public class PartEntity : AssemblyEntity
    {
        //程序集信息
        internal Assembly assembly { get; set; }
    }
}
