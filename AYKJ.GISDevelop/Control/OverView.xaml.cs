/// <summary>  
/// 作者：陈锋 
/// 时间：2012/5/24 12:18:45  
/// 公司:南京安元科技有限公司  
/// 版权：2012-2020  
/// CLR版本：4.0.30319.261  
/// OverView说明：鹰眼
/// 唯一标识：db5a7289-2b37-4a6b-9e74-9c9bcc0c2a9f  
/// </summary>

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AYKJ.GISDevelop.Platform;
using AYKJ.GISDevelop.Platform.ToolKit;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;

namespace AYKJ.GISDevelop.Control
{
    public partial class OverView : UserControl
    {
        public bool isVisible = true;
        private Map activeMap;
        private bool isFirst = true;
        public OverView()
        {
            InitializeComponent();
            this.HorizontalAlignment = HorizontalAlignment.Right;
            this.VerticalAlignment = VerticalAlignment.Bottom;      
            overMap.Projection = new PlaneProjection();
            this.Projection = new PlaneProjection();
            btn.Projection = new PlaneProjection();

            activeMap = App.mainMap;
            activeMap.ExtentChanged -= activeMap_ExtentChanged;
            activeMap.ExtentChanged += new EventHandler<ExtentEventArgs>(activeMap_ExtentChanged);
            addLayers();
        }

        /// <summary>
        /// map的范围改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void activeMap_ExtentChanged(object sender, ExtentEventArgs e)
        {
            if (activeMap.Extent == null)
                return;
            double xlen =  activeMap.Extent.Width / 3;
            double ylen = activeMap.Extent.Height / 3;

            Envelope target = new Envelope(activeMap.Extent.XMin - xlen, activeMap.Extent.YMin - ylen, activeMap.Extent.XMax + xlen, activeMap.Extent.YMax+ylen);
            //if (target.XMin < -180)
            //{
            //    target.XMin = -180;
            //}
            //if (target.YMin < -90)
            //{
            //    target.YMin = -90;
            //}
            //if (target.XMax > 180)
            //{
            //    target.XMax = 180;
            //}
            //if (target.YMax > 90)
            //{
            //    target.YMax = 90;
            //}

            overMap.Extent = target;


            MapPoint mapPointMin = new MapPoint(activeMap.Extent.XMin, activeMap.Extent.YMax);
            MapPoint mapPointMax = new MapPoint(activeMap.Extent.XMax, activeMap.Extent.YMin);

            Point screenPointMin = overMap.MapToScreen(mapPointMin);
            Point screenPointMax = overMap.MapToScreen(mapPointMax);
            
            double width = Math.Abs(screenPointMin.X - screenPointMax.X);
            double height = Math.Abs(screenPointMin.Y - screenPointMax.Y);

            rect.Margin = new Thickness(screenPointMin.X < 0 ? 0 : screenPointMin.X, screenPointMin.Y < 0 ? 0 : screenPointMin.Y, 0, 0);
            rect.Width = width > rect.MaxWidth ? rect.MaxWidth : width;
            rect.Height = height > rect.MaxHeight ? rect.MaxHeight : height;
            rect.RenderTransform = null;
            
        }

        /// <summary>
        /// 加图层
        /// </summary>
        private void addLayers()
        {
            var mapServices = (from item in PFApp.Extent.Element("OverviewMaps").Elements("OverviewMap")
                           select new
                            {
                                Type = item.Attribute("Type").Value,
                                Url = item.Attribute("Url").Value,
                                RMin = item.Attribute("RMin").Value,
                                RMax = item.Attribute("RMax").Value
                            }).ToList();
            foreach (var item in mapServices)
            {
                Layer layer = null;
                switch (item.Type)
                {
                    case "Baidu":
                        {
                            //layer = new TiledLayerForBaidu()
                            //{
                            //    MapUrl = item.Url,
                            //    ID = "overLayer",
                            // };
                            layer = new BaiduMapLayer()
                            {
                                UriPattern = item.Url,
                                buri = true
                            };
                        }
                        break;
                    case "Tiled":
                        {
                            layer = new ArcGISTiledMapServiceLayer()
                            {
                                Url = item.Url,
                            };
                            break;
                        }
                    case "Image":
                        {
                            layer = new ArcGISImageServiceLayer()
                            {
                                Url = item.Url,
                            };
                            break;
                        }
                    case "Dynamic":
                        {
                            layer = new ArcGISDynamicMapServiceLayer()
                            {
                                Url = item.Url,
                            };
                            break;
                        }   
                }
                if (!string.IsNullOrEmpty(item.RMin))
                {
                    layer.MinimumResolution = double.Parse(item.RMin);
                }
                if (!string.IsNullOrEmpty(item.RMax))
                {
                    layer.MaximumResolution = double.Parse(item.RMax);
                }
                layer.InitializationFailed += (ss, ee) =>
                {
                    if (this.isFirst)
                    {
                        this.isFirst = false;
                        overMap.Layers.Remove(ss as Layer);
                        this.addLayers();
                    }
                    else
                    {
                        isVisible = false;
                        this.LayoutRoot.Visibility = Visibility.Collapsed;
                        //Message.Show(string.Format("由于网络原因，鹰眼服务{0}加载出错", (ss as dynamic).Url));
                        overMap.Layers.Remove(ss as Layer);
                    }
                };
                overMap.Layers.Add(layer);
            }
        }

        private void MouseDragElementBehavior_DragFinished(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.LayoutRoot.UpdateLayout();

            System.Windows.Media.MatrixTransform mt = rect.RenderTransform as MatrixTransform;
            double x = mt.Matrix.OffsetX;
            double y = mt.Matrix.OffsetY;
            
            double screenxmin = rect.Margin.Left+x;
            double screenymax = rect.Margin.Top+y;
            
            
            Envelope old = activeMap.Extent;

            Point pt = new Point(screenxmin, screenymax);
            MapPoint mp = overMap.ScreenToMap(pt);

            double xminlen = mp.X - old.XMin;
            double ymaxlen = mp.Y - old.YMax;

            Envelope news = new Envelope()
            {
                XMax = old.XMax + xminlen,
                XMin = mp.X,
                YMax = mp.Y,
                YMin = old.YMin + ymaxlen
            };

            activeMap.Extent = news;
        }

        bool isshow = false;

        /// <summary>
        /// 按钮单击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Click(object sender, RoutedEventArgs e)
        {
            if (isshow == true)
            {
                Storyboard_close.Begin();                
                isshow = false;
            }
            else
            {
                Storyboard_show.Begin();
                isshow = true;
            }

        }

        /// <summary>
        /// 清除监听事件
        /// </summary>
        public void Exit()
        {
            activeMap.ExtentChanged -= activeMap_ExtentChanged;
        }

        #region 鹰眼显示和关闭
        /// <summary>
        /// 删除鹰眼动画完毕
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void removeSb_Completed(object sender, EventArgs e)
        {
            this.LayoutRoot.Children.Remove(overMap);
            btn.Background = null;
            //(btn.Projection as PlaneProjection).RotationZ = 180;
        }

        /// <summary>
        /// 显示鹰眼动画完毕
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Storyboard_show_Completed(object sender, EventArgs e)
        {
            activeMap_ExtentChanged(null, null);
            //(btn.Projection as PlaneProjection).RotationZ = 0;
            btn.Background = new SolidColorBrush() { Color = Colors.Black };
        }
        #endregion

        #region 左右移动
        ///// <summary>
        ///// 左移
        ///// </summary>
        ///// <param name="len"></param>
        ///// <param name="dur"></param>
        //public void MoveLeft(double len, int milliSeconds)
        //{
        //    Move(0-len, milliSeconds);
        //}

        ///// <summary>
        ///// 右移
        ///// </summary>
        ///// <param name="len"></param>
        ///// <param name="dur"></param>
        //public void MoveRight(double len, int milliSeconds)
        //{
        //    Move(len, milliSeconds);
        //}

        ///// <summary>
        ///// 移动
        ///// </summary>
        ///// <param name="len"></param>
        ///// <param name="milliSeconds"></param>
        //private void Move(double len, int milliSeconds)
        //{            
        //    PlaneProjection planPro = this.Projection as PlaneProjection;
           
        //    Storyboard removeSb = new Storyboard();
        //    //X轴
        //    DoubleAnimation daz = new DoubleAnimation()
        //    {
        //        //From = planPro.GlobalOffsetX,
        //        To = planPro.GlobalOffsetX + len,
        //        Duration = new Duration(TimeSpan.FromMilliseconds(milliSeconds))
        //    };
        //    removeSb.Children.Add(daz);
        //    Storyboard.SetTarget(daz, planPro);
        //    Storyboard.SetTargetProperty(daz, new PropertyPath("GlobalOffsetX"));
        //    removeSb.Begin();
        //}
        #endregion

        
    }
}
