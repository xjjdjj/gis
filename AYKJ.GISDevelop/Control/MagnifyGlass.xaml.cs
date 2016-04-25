/// <summary>  
/// 作者：陈锋 
/// 时间：2012/5/24 12:19:58  
/// 公司:南京安元科技有限公司  
/// 版权：2012-2020  
/// CLR版本：4.0.30319.261  
/// MagnifyGlass说明：放大镜
/// 唯一标识：70fd45c6-faca-45c9-aafa-ee4ad9d07452  
/// </summary>
/// 
using System.Windows;
using System.Windows.Controls;
using AYKJ.GISDevelop.Platform;
using ESRI.ArcGIS.Client;

namespace AYKJ.GISDevelop.Control
{
    public partial class MagnifyGlass : UserControl
    {
        public MagnifyGlass()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MagnifyGlass_Loaded);
        }

        void MagnifyGlass_Loaded(object sender, RoutedEventArgs e)
        {
            MyMagnifyingGlass.Map = App.mainMap;
            SetMagnifyLayer(App.mainMap);

            //设置面板的起始位置
            this.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            this.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            //IMainPage imp = (App.Current as App).RootVisual as IMainPage;
            this.Margin = new Thickness(0, 0, 0, 1);
        }

        public void SetMagnifyLayer(Map map)
        {
            int lycount = map.Layers.Count;
            for (int i = 0; i < lycount; i++)
            {
                if (map.Layers[i] is ESRI.ArcGIS.Client.ArcGISTiledMapServiceLayer)
                {
                    ArcGISTiledMapServiceLayer tmp = new ArcGISTiledMapServiceLayer();
                    tmp.ID = map.Layers[i].ID;
                    tmp.Url = (map.Layers[i] as ArcGISTiledMapServiceLayer).Url;
                    MyMagnifyingGlass.Layer = tmp;
                    break;
                }
            }
        }

        #region 两侧面板的展示和关闭
        /// <summary>
        /// 面板展开
        /// </summary>
        public void Show()
        {
            //展开面板
            PFApp.Root.Children.Add(this);
        }
        /// <summary>
        /// 面板关闭方法
        /// </summary>
        public void Close()
        {
            PFApp.Root.Children.Remove(this);
        }

        #endregion

    }
}
