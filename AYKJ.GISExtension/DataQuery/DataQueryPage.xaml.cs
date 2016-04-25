#region << 版 本 注 释 >>
/*
 * ========================================================================
 * Copyright(c)  陈锋, All Rights Reserved.
 * ========================================================================
 * CLR版本：       4.0.30319.261
 * 类 名 称：       DataQueryPage
 * 机器名称：       GIS-FLYH
 * 命名空间：       AYKJ.GISExtension.Query
 * 文 件 名：       DataQueryPage
 * 创建时间：       2012/7/20 11:00:42
 * 作    者：       陈锋
 * 功能说明：       
 * 修改时间：
 * 修 改 人：
 * ========================================================================
*/
#endregion

using System;
using System.Windows;
using System.Windows.Controls;
using AYKJ.GISDevelop.Platform;
using System.Collections.Generic;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;
using System.Xml.Linq;
using System.Linq;
using ESRI.ArcGIS.Client.Symbols;
using System.Windows.Media;

namespace AYKJ.GISExtension
{
    public partial class DataQueryPage : UserControl
    {
        //使用geometryservice和不使用时用两种（有缓冲区和没缓冲区）
        UserControl dataqueryspatial;

        DataQueryRadius dataqueryradius;
        static DataQueryClickPoint dataqueryclickpoint;
        DataQueryKey dataquerykey;
        DataQueryDivision dataquerydivision;
        DataQuery.DataQueryGrid dataquerygrid;
        DataQueryShow dataqueryshow;
        DataQueryStreetKey dataquerystreetkey;
        string strGeometryurl;

        public DataQueryPage()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(DataQueryPage_Loaded);
        }

        void DataQueryPage_Loaded(object sender, RoutedEventArgs e)
        {
            strGeometryurl = (from item in PFApp.Extent.Elements("GeometryService")
                              select item.Attribute("Url").Value).ToArray()[0];
            if ((Application.Current as IApp).Dict_Xzqz_sy == null ||
                (Application.Current as IApp).Dict_Xzqz_sygra == null ||
                (Application.Current as IApp).Dict_Xzqz_qx == null ||
                (Application.Current as IApp).Dict_Xzqz_qxgra == null)
            {
                rbtn_division.Visibility = System.Windows.Visibility.Collapsed;
            }
            if (!(Application.Current as IApp).IsGridFinished)
            {
                rbtn_grid.Visibility = System.Windows.Visibility.Collapsed;
            }
            //设置面板的起始位置
            this.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            this.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            this.Margin = new Thickness() { Top = 10, Right = 13 };

            grid.Children.Clear();

            //20121011
            if (dataqueryclickpoint == null)
                dataqueryclickpoint = new DataQueryClickPoint();
            grid.Children.Remove(dataqueryclickpoint);
            dataquerykey = new DataQueryKey();
            dataquerystreetkey = new DataQueryStreetKey();
            grid.Children.Add(dataquerystreetkey);//20150327
            if (PFApp.MapServerType == enumMapServerType.Baidu)
                dataqueryspatial = new DataQuerySpatialToolKit();
            else if (PFApp.MapServerType == enumMapServerType.Esri)
                dataqueryspatial = new DataQuerySpatialToolKit();
            else if (strGeometryurl != "")
                dataqueryspatial = new DataQuerySpatialToolKit();
            
            dataqueryradius = new DataQueryRadius();
            dataquerydivision = new DataQueryDivision();
            dataquerygrid = new DataQuery.DataQueryGrid();

            //20150304:江宁项目通过街道字段筛选可见图层
            dataqueryshow = new DataQueryShow();

            //20150325:江宁“街道”关键字查询
            dataquerystreetkey = new DataQueryStreetKey();

            //20130918:“去掉”了"点选"查询选项卡（width=0隐藏），初始化时设置为关键字搜索页面。
            //grid.Children.Add(dataquerykey);
            rbtn_streetkey.IsChecked = true;//20150327:默认打开"街道查询"选项卡
            Storyboard_Close.Completed += new EventHandler(Storyboard_Close_Completed);
        }

        #region 两侧面板的展示和关闭
        /// <summary>
        /// 面板展开
        /// </summary>
        public void Show()
        {
            Storyboard_Show.Begin();
            PFApp.Root.Children.Add(this);
        }
        /// <summary>
        /// 面板关闭方法
        /// </summary>
        public void Close()
        {
            grid.Children.Clear();
            Storyboard_Close.Begin();
            (Application.Current as IApp).strImageType = "defaultClick";

        }

        public void AddPage()
        {
            PFApp.Root.Children.Add(this);
        }

        void Storyboard_Close_Completed(object sender, EventArgs e)
        {
            if(PFApp.MapServerType==enumMapServerType.Baidu)
                (dataqueryspatial as DataQuerySpatialToolKit).Reset();
            else if(PFApp.MapServerType==enumMapServerType.Esri)
                (dataqueryspatial as DataQuerySpatialToolKit).Reset();
            else if(strGeometryurl!="")
                (dataqueryspatial as DataQuerySpatialToolKit).Reset();
            dataqueryradius.Reset();
            dataqueryclickpoint.Reset();
            dataquerykey.Reset();
            dataquerydivision.Reset();
            dataquerygrid.Reset();
            dataqueryshow.Reset();
            dataquerystreetkey.Reset();
            PFApp.Root.Children.Remove(this);
        }

        #endregion

        private void rbtn_Checked(object sender, RoutedEventArgs e)
        {
            if (grid == null)
                return;
            grid.Children.Clear();
            dataqueryclickpoint.Reset();
            dataquerykey.Reset();
            if (PFApp.MapServerType == enumMapServerType.Baidu)
                (dataqueryspatial as DataQuerySpatialToolKit).Reset();
            else if (PFApp.MapServerType == enumMapServerType.Esri)
                (dataqueryspatial as DataQuerySpatialToolKit).Reset();
            else if (strGeometryurl != "")
                (dataqueryspatial as DataQuerySpatialToolKit).Reset();
            dataqueryradius.Reset();
            dataquerydivision.Reset();
            dataquerygrid.Reset();
            RadioButton rbtn = sender as RadioButton;
            grid.Visibility = Visibility.Visible;//20130926：设置grid可见
            //20131008：在这里统一管理下面这个关系点击各个点图标事件的全局变量。
            (Application.Current as IApp).strImageType = "defaultClick";
            switch (rbtn.Name)
            {
                    //20130918：去掉点击查询选项卡。
                    //20130926:重新添加次选项卡，但是仅作删除用，选项卡页面为空白。
                case "rbtn_point":
                    grid.Visibility = Visibility.Collapsed;//设置grid为不可见
                    grid.Children.Add(dataqueryclickpoint);
                    PFApp.ClickType = "deleteData";
                    //(Application.Current as IApp).strImageType = "deleteData";
                    break;
                case "rbtn_key":
                    grid.Children.Add(dataquerykey);
                    break;
                case "rbtn_division":
                    grid.Children.Add(dataquerydivision);
                    break;
                case "rbtn_spatial":
                    grid.Children.Add(dataqueryspatial);
                    break;
                case "rbtn_radius":
                    grid.Children.Add(dataqueryradius);
                    break;
                case "rbtn_grid":
                    grid.Children.Add(dataquerygrid);
                    break;
                case "rbtn_show":
                    grid.Children.Add(dataqueryshow);
                    break;
                case "rbtn_streetkey":
                    grid.Children.Add(dataquerystreetkey);
                    break;
            }
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btn_Max_Click(object sender, RoutedEventArgs e)
        {
            Storyboard_Max.Begin();
            btn_MinClose.Visibility = System.Windows.Visibility.Collapsed;
            btn_Max.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void btn_Min_Click(object sender, RoutedEventArgs e)
        {
            Storyboard_Min.Begin();
            btn_MinClose.Visibility = System.Windows.Visibility.Visible;
            btn_Max.Visibility = System.Windows.Visibility.Visible;
        }
    }
}
