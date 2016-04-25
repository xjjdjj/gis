using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using AYKJ.GISDevelop.Platform;
using ESRI.ArcGIS.Client;

namespace AYKJ.GISExtension
{
    public partial class DataQueryPageExpose : UserControl
    {
        DataQuerySpatial dataqueryspatial;
        DataQueryRadius dataqueryradius;
        DataQueryClickPoint dataqueryclickpoint;
        DataQueryKey dataquerykey;
        DataQueryDivision dataquerydivision;

        public bool isClosed = false;
        string strChecked = "rbtn_radius";
        string[] queryType;
        List<Graphic> lstreturngra;
        Graphic draw_graphic;

        public DataQueryPageExpose()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(DataQueryPage_Loaded);
        }

        public DataQueryPageExpose(string check, List<Graphic> lstreturngra,Graphic draw_graphic,string[] querytype)
        {
            this.lstreturngra = lstreturngra;
            this.strChecked = check;
            this.draw_graphic = draw_graphic;
            this.queryType = querytype;
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(DataQueryPage_Loaded);
        }

        void DataQueryPage_Loaded(object sender, RoutedEventArgs e)
        {
            if ((Application.Current as IApp).Dict_Xzqz_sy == null ||
                (Application.Current as IApp).Dict_Xzqz_sygra == null ||
                (Application.Current as IApp).Dict_Xzqz_qx == null ||
                (Application.Current as IApp).Dict_Xzqz_qxgra == null)
            {
               // rbtn.Visibility = System.Windows.Visibility.Collapsed;
            }
            //设置面板的起始位置
            this.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            this.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            this.Margin = new Thickness() { Top = 10, Right = 13 };

            grid.Children.Clear();

            switch (this.strChecked)
            {
                case "rbtn_point":
                    dataqueryclickpoint = new DataQueryClickPoint();
                    break;
                case "rbtn_key":
                    dataquerykey = new DataQueryKey();
                    break;
                case "rbtn_division":
                    dataquerydivision = new DataQueryDivision();
                    break;
                case "rbtn_spatial":
                    dataqueryspatial = new DataQuerySpatial();
                    break;
                case "rbtn_radius":
                    dataqueryradius = new DataQueryRadius();
                    //this.rbtn.Content = "半径搜索";
                    dataQueryRadius();
                    break;
            }

           
            //rbtn.IsChecked = true;
            grid.Children.Clear();
            grid.Children.Add(dataqueryradius);
            Storyboard_Close.Completed += new EventHandler(Storyboard_Close_Completed);
        }

        void dataQueryRadius()
        {
            dataqueryradius.clsRangeQueryExpose(lstreturngra, draw_graphic,this.queryType);
        }

        public void dataQueryRadius(List<Graphic> lstreturngra,Graphic draw_graphic,string[] queryType)
        {
            dataqueryradius.clsRangeQueryExpose(lstreturngra, draw_graphic, queryType);
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
        }

        public void AddPage()
        {
            PFApp.Root.Children.Add(this);
        }

        void Storyboard_Close_Completed(object sender, EventArgs e)
        {
            switch (this.strChecked)
            {
                case "rbtn_point":
                    dataqueryclickpoint.Reset();
                    break;
                case "rbtn_key":
                    dataquerykey.Reset();
                    break;
                case "rbtn_division":
                    dataquerydivision.Reset();
                    break;
                case "rbtn_spatial":
                    dataqueryspatial.Reset();
                    break;
                case "rbtn_radius":
                    dataqueryradius.Reset();
                    break;
            }
            isClosed = true;
            PFApp.Root.Children.Remove(this);
        }

        #endregion

        
        private void rbtn_Checked(object sender, RoutedEventArgs e)
        {
            if (grid == null)
                return;
            grid.Children.Clear();
            /*
            dataqueryclickpoint.Reset();
            dataquerykey.Reset();
            dataqueryspatial.Reset();
            dataqueryradius.Reset();
            dataquerydivision.Reset();
            */
            switch (this.strChecked)
            {
                case "rbtn_point":
                    grid.Children.Add(dataqueryclickpoint);
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
