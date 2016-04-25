/// <summary> 
/// 作者：陈锋 
/// 时间：2012/5/24 12:46:24  
/// 公司:南京安元科技有限公司  
/// 版权：2012-2020  
/// CLR版本：4.0.30319.261  
/// MouseCoordinate说明：坐标展示
/// 唯一标识：573bbe33-3b5e-4ca2-bd39-1932fe34b6b6  
/// <summary> 

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AYKJ.GISDevelop.Platform;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;

namespace AYKJ.GISDevelop.Control
{
    public partial class MouseCoordinate : UserControl
    {
        //地图
        Map map;
        public MouseCoordinate()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MouseCoordinate_Loaded);         
        }

        void MouseCoordinate_Loaded(object sender, RoutedEventArgs e)
        {
            //设置面板的起始位置
            this.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            this.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            //IMainPage imp = (App.Current as App).RootVisual as IMainPage;
            this.Margin = new Thickness(0, 0,30, 5);

            map = App.mainMap;
            map.MouseMove += new MouseEventHandler(map_MouseMove);

            Storyboard_close.Completed += new EventHandler(Storyboard_close_Completed);
        }

        void map_MouseMove(object sender, MouseEventArgs e)
        {
            if (map != null)
            {
                MapPoint pt = map.ScreenToMap(e.GetPosition(map));
                if (pt != null)
                {
                    Cord_XY.Text = string.Format("X:{0}; Y:{1}", Math.Round(pt.X, 6), Math.Round(pt.Y, 6));
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

            //执行展开动画
            Storyboard_show.Begin();
        }
        /// <summary>
        /// 面板关闭方法
        /// </summary>
        public void Close()
        {
            //面板关闭动画
            Storyboard_close.Begin();
        }

        void Measure_ClearAll()
        {
            Close();
        }

        void Storyboard_close_Completed(object sender, EventArgs e)
        {
            //关闭动画结束后移除该面板
            PFApp.Root.Children.Remove(this);
        }

        #endregion
    }
}
