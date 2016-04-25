/// <summary>  
/// 作者：陈锋 
/// 时间：2012/5/21 15:44:58  
/// 公司:南京安元科技有限公司  
/// 版权：2012-2020  
/// CLR版本：4.0.30319.261  
/// ConfigData说明：配置信息，App周期中一直保存
/// 唯一标识：b1c7fb37-ab0a-4741-8a44-03cf6df354de  
/// </summary>

using System.Collections.Generic;
using System.Xml.Linq;
using AYKJ.GISDevelop.Platform.Config.Entity;

namespace AYKJ.GISDevelop.Platform.Config
{
    public class ConfigData
    {
        //总的下载文件大小
        internal double FilesSize;
        //加载系统的Dll信息
        internal IList<SystemEntity> SystemEntities;
        //加载系统的Dll信息
        internal IList<StyleEntity> StyleEntities;
        //加载子系统即Part的信息
        internal IList<PartEntity> PartEntities;
        public IList<PartEntity> PartConfig { get { return PartEntities; } }
        //加载菜单的信息
        internal IList<MenuEntity> MenuEntityies;
        public IList<MenuEntity> MenuConfig { get { return MenuEntityies; } }
        //扩展的信息，供其他的业务进行操作
        internal XElement Extents;
        public XElement ExtentsConfig { get { return Extents; } }
        //调试信息
        internal XElement Debugs;
        public XElement DebugsConfig { get { return Debugs; } }
    }
}
