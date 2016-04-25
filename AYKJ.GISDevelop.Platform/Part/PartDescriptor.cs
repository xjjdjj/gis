/// <summary>  
/// 作者：陈锋 
/// 时间：2012/5/21 15:49:27  
/// 公司:南京安元科技有限公司  
/// 版权：2012-2020  
/// CLR版本：4.0.30319.261  
/// PartDescriptor说明：Part描述信息
/// 唯一标识：66bc366e-7f08-45b3-91fe-6b0cb0590875  
/// </summary>

using System;
using System.Windows.Media;

namespace AYKJ.GISDevelop.Platform.Part
{
    public class PartDescriptor
    {
        //唯一的名称
        public string Name { get; set; }
        //类型信息
        internal Type type { get; set; }
        //类型名称
        internal string typeName { get; set; }
        //图标
        public string ICon { get; set; }
        //图形
        public ImageSource IConImage { get; set; }
        //标题
        public string Title { get; set; }
        //组索引
        internal int GroupIndex { get; set; }
        //组名称
        internal string GroupName { get; set; }

    }
}
