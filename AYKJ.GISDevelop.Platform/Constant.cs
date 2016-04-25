/// <summary>  
/// 作者：陈锋 
/// 时间：2012/5/21 16:02:55  
/// 公司:南京安元科技有限公司  
/// 版权：2012-2020  
/// CLR版本：4.0.30319.261  
/// Constant说明：平台中需要用到的常量
/// 唯一标识：b9d844a9-4153-4399-8f32-851a2cea895f  
/// </summary>

namespace AYKJ.GISDevelop.Platform
{
    internal class Constant
    {
        /// <summary>
        /// 程序集信息
        /// </summary>
        public static readonly string AssmblyPath = "Assembly";

        /// <summary>
        /// 程序集大小
        /// </summary>
        public static readonly string AssmblySize = "Size";

        /// <summary>
        /// 首页最低层借口
        /// </summary>
        public static readonly string IPartName = "AYKJ.GISDevelop.Platform.Part.IPart";

        /// <summary>
        /// 配置文件
        /// </summary>
        public static readonly string ConfigUrl = "ClientBin/Config/Plat.xml";

        /// <summary>
        /// 斜杠
        /// </summary>
        public static readonly char Sprit = '/';

        /// <summary>
        /// 点
        /// </summary>
        public static readonly char Dot = '.';

        /// <summary>
        /// 分号
        /// </summary>
        public static readonly char Semicolon = ';';

        /// <summary>
        /// 横杠
        /// </summary>
        public static readonly char Horizontal = '-';

        /// <summary>
        /// 系统资源目录的节点路径
        /// </summary>
        public static readonly string SystemPath = "Assembly.System";

        /// <summary>
        ///  菜单配置信息的节点路径
        /// </summary>
        public static readonly string MenuPath = "Menu.MenuItem";

        /// <summary>
        ///  应用的节点路径
        /// </summary>
        public static readonly string PartPath = "Assembly.Part";


        /// <summary>
        ///  应用的节点路径
        /// </summary>
        public static readonly string StylePath = "Assembly.Style";

        /// <summary>
        ///  扩展的节点路径
        /// </summary>
        public static readonly string ExtentPath = "Extent";

        /// <summary>
        /// Debug配置信息
        /// </summary>
        public static readonly string DebugsPath = "Debugs";

        /// <summary>
        /// 请求资源的javascript字符串
        /// </summary>
        public static readonly string JSSendRequest = "var objXmlHttp=null;try{objXmlHttp = new XMLHttpRequest();}catch(e){try{objXmlHttp = new ActiveXObject('Microsoft.XMLHTTP');}catch(e){try{objXmlHttp = new ActiveXObject('Msxml2.XMLHTTP');}catch(e){alert('error opening XMLHTTP');}}}objXmlHttp.open('GET','ClientBin/Config/Plat.xml',false);objXmlHttp.send();";

        /// <summary>
        /// 获得资源的内容
        /// </summary>
        public static readonly string JSRequestText = "objXmlHttp.responseText";

        /// <summary>
        /// 获得资源的状态
        /// </summary>
        public static readonly string JSRequestStatu = "objXmlHttp.status";

        /// <summary>
        /// 获得资源的目标状态
        /// </summary>
        public static readonly string JSRequestTargetStatu = "200";

        /// <summary>
        /// 配置的节点名称 Source
        /// </summary>
        public static readonly string AttributeSource = "Source";

        /// <summary>
        /// 配置的节点名称 Title
        /// </summary>
        public static readonly string AttributeTitle = "Title";

        /// <summary>
        /// 配置的节点名称 MenuName
        /// </summary>
        public static readonly string AttributeMenuName = "MenuName";

        /// <summary>
        /// 配置的节点名称 Describe
        /// </summary>
        public static readonly string AttributeDescribe = "Describe";

        /// <summary>
        /// 配置的节点名称 Describe
        /// </summary>
        public static readonly string AttributeType = "Type";

        /// <summary>
        /// 配置的节点是否可见 Describe
        /// </summary>
        public static readonly string AttributeVisible = "Visible";

        /// <summary>
        /// 配置加载错误
        /// </summary>
        public static readonly string ConfigLoadError = "配置文件加载错误！";

        /// <summary>
        /// 加载系统资源信息
        /// </summary>
        public static readonly string LoadSysTip = "正在加载系统资源……";

        /// <summary>
        /// 加载功能模块
        /// </summary>
        public static readonly string LoadPartTip = "正在加载功能模块……";

        /// <summary>
        /// 加载配置文件
        /// </summary>
        public static readonly string LoadConfigTip = "正在加载配置信息……";

        /// <summary>
        /// 加载样式资源
        /// </summary>
        public static readonly string LoadStyleTip = "正在加载样式资源……";

    }
}
