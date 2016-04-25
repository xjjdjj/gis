/// <summary>  
/// 作者：陈锋 
/// 时间：2012/5/21 15:39:50  
/// 公司:南京安元科技有限公司  
/// 版权：2012-2020  
/// CLR版本：4.0.30319.261  
/// AssemblyEntity说明：程序集实体，记录了程序集文件位置及加载操作
/// 唯一标识：ecfc4c5b-7fe9-41e5-8eaf-d339d6974c8f  
/// </summary>

namespace AYKJ.GISDevelop.Platform.Config.Entity
{
    public abstract class AssemblyEntity
    {
        //程序集文件的地址信息
        internal string Source { get; set; }
    }
}
