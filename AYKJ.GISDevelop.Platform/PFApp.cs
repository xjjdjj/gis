/// <summary>  
/// 作者：陈锋 
/// 时间：2012/5/21 16:05:51  
/// 公司:南京安元科技有限公司  
/// 版权：2012-2020  
/// CLR版本：4.0.30319.261  
/// PFApp说明：完成加载配置和资源
/// 唯一标识：463d80ed-4028-4ef6-afd7-c43bb24986aa  
/// </summary>

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using AYKJ.GISDevelop.Platform.Config;
using AYKJ.GISDevelop.Platform.Config.Entity;
using AYKJ.GISDevelop.Platform.Part;
using System.ServiceModel;
using System.Net;
using System.IO;
using System;
using AYKJ.GISDevelop.Platform.ToolKit;

namespace AYKJ.GISDevelop.Platform
{
    public abstract class PFApp : Application
    {
        public static enumMapServerType MapServerType;
        //根视图
        public static Grid Root;
        //次根视图
        public static StackPanel SpRoot;

        //原始的配置信息
        protected internal static ConfigData Config;

        //菜单的配置信息
        private static IList<MenuEntity> menus;
        public static IList<MenuEntity> Menus { get { return menus; } }

        private static string clickType;

        public static string ClickType
        {
            get { return PFApp.clickType; }
            set { PFApp.clickType = value; }
        }
        
        //扩展配置信息
        public static XElement Extent { get { return Config.Extents; } }

        public static XElement Debugs { get { return Config.Debugs; } }

        //所有的配置信息
        public static Dictionary<string, UIElement> UIS;

        //Part即应用的配置信息
        internal static Dictionary<string, IList<PartDescriptor>> parts;
        public static Dictionary<string, IList<PartDescriptor>> Parts { get { return parts; } }

        //Part的顺序描述
        internal static Dictionary<int, string> partsIndex;
        public static Dictionary<int, string> PartsIndex { get { return partsIndex; } }

        //程序集的加载类
        private AssemblyLoad assemblyLoad;

        //重写了Application的根视图
        private Grid rootVisual = new Grid();
        new public UIElement RootVisual
        {
            get
            {
                if ((base.RootVisual as Grid).Children.Count == 0)
                {
                    return null;
                }
                else
                {
                    return (base.RootVisual as Grid).Children[0];
                }
            }
            set
            {
                (base.RootVisual as Grid).Children.Clear();
                (base.RootVisual as Grid).Children.Add(value);
            }
        }

        /// <summary>
        /// 完成一些初始化操作
        /// </summary>
        public PFApp()
        {
            parts = new Dictionary<string, IList<PartDescriptor>>();
            partsIndex = new Dictionary<int, string>();
            UIS = new Dictionary<string, UIElement>();
            this.Startup += new StartupEventHandler(Application_Startup);
        }

        /// <summary>
        /// 应用启动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //设置根视图
            base.RootVisual = new Grid();
            //加载欢迎页
            LoadWelcomePage();
            //改变欢迎页的状态为加载配置
            //setStatu(Constant.LoadConfigTip);
            //加载配置（已经同步实现）
            LoadConfig();
        }

        /// <summary>
        /// 加载配置文件
        /// </summary>
        public void LoadConfig()
        {
            WebClient webclient = new WebClient();
            webclient.OpenReadCompleted += new OpenReadCompletedEventHandler(webclient_OpenReadCompleted);
            //Uri uriHost = App.Current.Host.Source;
            //string host = uriHost.AbsoluteUri.Replace(uriHost.AbsolutePath, "");
            //Uri pUri = new Uri(host + "/Config.xml");
            string[] ary = Application.Current.Host.Source.OriginalString.Split('/');
            string strurl = string.Empty;
            for (int i = 0; i < ary.Length - 1; i++)
            {
                strurl = strurl + ary[i] + "/";
            }
            strurl = strurl + "Config/Plat.xml";
            webclient.OpenReadAsync(new System.Uri(strurl,System.UriKind.Absolute));
        }

        void webclient_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            TextReader pReader = new StreamReader(e.Result);
            string data = pReader.ReadToEnd();
            pReader.Close();
            XDocument pDocument=null;
            string strurl="";
            bool parse = false;
            //加载配置文件，判断是从远程读取配置文件地址还是从当前读取
            try
            {
                pDocument = XDocument.Parse(data);
                parse = true;
                //解析正确，但是没有配置远程字段Service
                strurl = pDocument.Element("Config").Element("Assembly").Element("Service").Attribute("Source").Value;
            }
            catch (Exception ex)
            {
                if (!parse)
                {
                    //如果配置文件解析出错，无法继续也无法向远程地址发起请求，直接退出
                    Message.ShowErrorInfo("加载配置文件失败", ex.Message);
                    return;
                }
                else
                {
                    //配置文件解析正确，但是没有配置远程字段Service
                    //什么都不做
                }
            }
            if (parse)
            {
                if (strurl.Trim() == "")
                {
                    Config = new ConfigManage().LoadConfig(pDocument);
                    //Config = new ConfigManage().LoadConfig(Constant.ConfigUrl);
                    if (Config == null)
                        return;
                    menus = Config.MenuConfig;
                    //加载实际配置的资源
                    this.LoadResources();
                }
                else
                {
                    //设置配置信息
                    WcfXmlService.Service1Client WcfXmlServiceClient = new WcfXmlService.Service1Client(new BasicHttpBinding(), new EndpointAddress(strurl));
                    WcfXmlServiceClient.CreateXmlCompleted += new System.EventHandler<WcfXmlService.CreateXmlCompletedEventArgs>(WcfXmlServiceClient_CreateXmlCompleted);
                    WcfXmlServiceClient.CreateXmlAsync();
                }
            }            
        }

        void WcfXmlServiceClient_CreateXmlCompleted(object sender, WcfXmlService.CreateXmlCompletedEventArgs e)
        {
            if (e.Error != null || e.Result.Substring(0, 3) == "Err")
            {
                Config = new ConfigManage().LoadConfig(Constant.ConfigUrl);
                menus = Config.MenuConfig;
                //加载实际配置的资源
                this.LoadResources();
            }
            else if (e.Result.Substring(0, 2) == "Ok")
            {
                string str = e.Result.Substring(3, e.Result.Length - 3);
                Config = new ConfigManage().LoadXml(str);
                menus = Config.MenuConfig;
                //加载实际配置的资源
                this.LoadResources();
            }
        }

        /// <summary>
        /// 加载配置文件中配的资源
        /// </summary>
        public void LoadResources()
        {
            //加载程序集
            assemblyLoad = new AssemblyLoad(Config);
            assemblyLoad.Load();
        }

        internal static void setStatu(string sta)
        {
            IWelcomePage wp = (PFApp.Current as PFApp).RootVisual as IWelcomePage;
            wp.SetStatu(sta);
        }

        internal static void setStatu(double res, double all)
        {
            IWelcomePage wp = (PFApp.Current as PFApp).RootVisual as IWelcomePage;
            wp.SetStatu(res, all);
        }

        /// <summary>
        /// 加载欢迎页
        /// </summary>
        protected internal abstract void LoadWelcomePage();

        /// <summary>
        /// 加载主页
        /// </summary>
        protected internal abstract void LoadMainPage();

        /// <summary>
        /// 刷新
        /// </summary>
        public void ReFresh()
        {

        }
    }
}
