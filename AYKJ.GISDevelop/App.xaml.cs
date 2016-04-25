using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using AYKJ.GISDevelop.Platform;
using System.Windows.Controls;

namespace AYKJ.GISDevelop
{
    //改变显示的状态                                   
    public partial class App : PFApp, IApp
    {
        /// <summary>
        /// 江宁系统记录登录系统的角色信息
        /// </summary>
        internal static string sysrole;
        public string sysRole
        {
            get
            {
                return sysrole;
            }
            set
            {
                sysrole = value;
            }
        }

        /// <summary>
        /// 江宁系统记录已选择显示的“街道”
        /// </summary>
        internal static List<String> ilistselectstreet;
        public List<String> iListSelectStreet
        {
            get
            {
                return ilistselectstreet;
            }
            set
            {
                ilistselectstreet = value;
            }
        }

        /// <summary>
        /// 江宁系统记录已选择显示的“企业类型”
        /// </summary>
        internal static List<String> ilistselectenterprise;
        public List<String> iListSelectEnterprise
        {
            get
            {
                return ilistselectenterprise;
            }
            set
            {
                ilistselectenterprise = value;
            }
        }

        /// <summary>
        /// WCF服务的标志位
        /// </summary>
        internal static string strwcfmark;
        public string strWcfMark
        {
            get
            {
                return strwcfmark;
            }
            set
            {
                strwcfmark = value;
            }
        }

        #region//20121011：此处代码有更新
        /// <summary>
        /// 加载的专题数据的图片状态
        /// </summary>
        internal static string strimagetype;
        public string strImageType
        {
            get
            {
                return strimagetype;
            }
            set
            {
                strimagetype = value;
            }
        }

        /// <summary>
        /// 打点功能，地图点击事件标志位
        /// </summary>
        internal static string straddmark;
        public string strAddMark
        {
            get
            {
                return straddmark;
            }
            set
            {
                straddmark = value;
            }
        }

        /// <summary>
        /// 打点功能，传入参数
        /// </summary>
        internal static string stradddata;
        public string strAddData
        {
            get
            {
                return stradddata;
            }
            set
            {
                stradddata = value;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        internal static Dictionary<string, List<clsThematic>> dict_Thematic;
        public Dictionary<string, List<clsThematic>> dict_thematic
        {
            get { return dict_Thematic; }
        }

        internal static Dictionary<string, List<clsThematic>> dict_MissThematic;
        public Dictionary<string, List<clsThematic>> dict_missthematic
        {
            get { return dict_MissThematic; }
        }

        /// <summary>
        /// 
        /// </summary>
        internal static Grid maplayoutroot;
        public Grid mapLayoutRoot
        {
            get
            {
                return maplayoutroot;
            }
            set
            {
                maplayoutroot = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal static List<Image> sysListThisImg;
        public List<Image> syslistthisimg
        {
            get
            {
                return sysListThisImg;
            }
            set
            {
                sysListThisImg = value;
            }
        }

        #endregion

        /// <summary>
        /// 所有加载的专题数据
        /// </summary>
        internal static List<Graphic> lstthematic;
        public List<Graphic> lstThematic
        {
            get { return lstthematic; }
        }

        /// <summary>
        /// 加载的专题数据中英文对照
        /// </summary>
        internal static Dictionary<string, string> dictthematicencn;
        public Dictionary<string, string> DictThematicEnCn
        {
            get { return dictthematicencn; }
        }

        internal static Dictionary<string, string> dictthematicentype;
        public Dictionary<string, string> DictThematicEnType
        {
            get { return dictthematicentype; }
        }

        /// <summary>
        /// 
        /// </summary>
        internal static Dictionary<string, GraphicsLayer> dict_thematiclayer;
        public Dictionary<string, GraphicsLayer> Dict_ThematicLayer
        {
            get { return dict_thematiclayer; }
        }
        #region 行政区划
        /// <summary>
        /// 加载的市域级行政区域
        /// </summary>
        internal static Dictionary<string, string> dict_xzqz_sy;
        public Dictionary<string, string> Dict_Xzqz_sy
        {
            get { return dict_xzqz_sy; }
        }
        /// <summary>
        /// 加载的市域级行政区域范围
        /// </summary>
        internal static Dictionary<string, Graphic> dict_xzqz_sygra;
        public Dictionary<string, Graphic> Dict_Xzqz_sygra
        {
            get { return dict_xzqz_sygra; }
        }
        /// <summary>
        /// 加载的区县级行政区域
        /// </summary>
        internal static Dictionary<string, string> dict_xzqz_qx;
        public Dictionary<string, string> Dict_Xzqz_qx
        {
            get { return dict_xzqz_qx; }
        }
        /// <summary>
        /// 加载的区县级行政区域范围
        /// </summary>
        internal static Dictionary<string, Graphic> dict_xzqz_qxgra;
        public Dictionary<string, Graphic> Dict_Xzqz_qxgra
        {
            get { return dict_xzqz_qxgra; }
        }

        internal static bool isXZQYFinished;
        public bool IsXZQYFinished
        {
            get { return isXZQYFinished; }
        }
        #endregion
        #region 网格查看
        internal static Dictionary<string, Graphic> dict_grid_gra;
        public Dictionary<string, Graphic> Dict_grid_gra
        {
            get { return dict_grid_gra; }
        }

        internal static Dictionary<string, Graphic> dict_subgrid_gra;
        public Dictionary<string, Graphic> Dict_subgrid_gra
        {
            get { return dict_subgrid_gra; }
        }

        internal static Dictionary<string, string> dict_grid;
        public Dictionary<string, string> Dict_grid
        {
            get { return dict_grid; }
        }

        internal static Dictionary<string, string> dict_subgrid;
        public Dictionary<string, string> Dict_subgrid
        {
            get { return dict_subgrid; }
        }
        internal static Dictionary<string, string> dict_subcode_fid;
        public Dictionary<string, string> Dict_SubCode_FID
        {
            get { return dict_subcode_fid; }
        }
        internal static bool isGridFinished;
        public bool IsGridFinished
        {
            get { return isGridFinished; }
        }
        #endregion
        /// <summary>
        /// 清空事件的委托
        /// </summary>
        internal List<ClearAll> lstClearAll;

        #region 地图相关
        /// <summary>
        /// 系统的主地图
        /// </summary> 
        internal static Map mainMap;
        public Map MainMap
        {
            get { return mainMap; }
        }

        /// <summary>
        /// 清除的事件
        /// </summary>
        public event ClearAll ClearAll
        {
            add
            {
                lstClearAll.Add(value);
            }
            remove
            {
                lstClearAll.Remove(value);
            }
        }
        #endregion

        /// <summary>
        /// 主页，所有页面的承载页
        /// </summary>
        IMainPage IApp.MainPage
        {
            get { return this.RootVisual as IMainPage; }
        }

        public App()
        {
            this.UnhandledException += this.Application_UnhandledException;
            this.Startup += new StartupEventHandler(App_Startup);
            this.Exit += new EventHandler(App_Exit);
            InitializeComponent();
            Init();
        }

        /// <summary>
        /// 兼容旧版本中的某些功能
        /// </summary>
        public void Init()
        {
            mainMap = new Map();
            lstClearAll = new List<ClearAll>();
        }

        void App_Startup(object sender, StartupEventArgs e)
        {
            //获取分辨率相关信息
            //ScriptObject screen = (ScriptObject)HtmlPage.Window.GetProperty("screen");
            //string wh = string.Format("{0}*{1}", screen.GetProperty("width"), screen.GetProperty("height"));
            //Message.Show(wh);
            //Host.Content.Zoomed += (s, ee) =>
            //    {
            //        Message.Show(Host.Content.ZoomFactor.ToString());
            //    };
        }

        /// <summary>
        /// 系统退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void App_Exit(object sender, EventArgs e)
        {


        }


        #region 系统自动生成 异常处理
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                e.Handled = true;
                Deployment.Current.Dispatcher.BeginInvoke(delegate { ReportErrorToDOM(e); });
            }
        }

        private void ReportErrorToDOM(ApplicationUnhandledExceptionEventArgs e)
        {
            try
            {
                string errorMsg = e.ExceptionObject.Message + e.ExceptionObject.StackTrace;
                errorMsg = errorMsg.Replace('"', '\'').Replace("\r\n", @"\n");
                System.Windows.Browser.HtmlPage.Window.Eval("alert(" + errorMsg + ");");
            }
            catch (Exception)
            {
            }
        }
        #endregion

        /// <summary>
        /// 加载欢迎页
        /// </summary>
        protected override void LoadWelcomePage()
        {
            this.RootVisual = new WelcomePage();
        }

        /// <summary>
        /// 加载主页
        /// </summary>
        protected override void LoadMainPage()
        {
            WelcomePage wp = this.RootVisual as WelcomePage;
            wp.Projection = new PlaneProjection();


            Dictionary<string, UIElement> temp = new Dictionary<string, UIElement>();
            foreach (var item in UIS)
            {
                string key = item.Key;
                UIElement ui = item.Value;
                temp.Add(key, ui.GetType().Assembly.CreateInstance(ui.GetType().FullName) as UIElement);
            }
            UIS = temp;

            this.RootVisual = new MainPage();
        }

        public void ChangeUser()
        {
            Init();
            Dictionary<string, UIElement> temp = new Dictionary<string, UIElement>();
            foreach (var item in UIS)
            {
                string key = item.Key;
                UIElement ui = item.Value;
                temp.Add(key, ui.GetType().Assembly.CreateInstance(ui.GetType().FullName) as UIElement);
            }
            UIS = temp;
            this.RootVisual = new MainPage();
        }

        public void ExitSys()
        {
            System.Windows.Browser.HtmlPage.Window.Eval(" window.opener=null;window.open('','_self');window.close();");
        }

    }
}
