#region << 版 本 注 释 >>
/*
 * ========================================================================
 * Copyright(c)  陈锋, All Rights Reserved.
 * ========================================================================
 * CLR版本：       4.0.30319.261
 * 类 名 称：       MainPage
 * 机器名称：       GIS-FLYH
 * 命名空间：       AYKJ.GISExtension
 * 文 件 名：       MainPage
 * 创建时间：       2012/7/20 10:24:16
 * 作    者：       陈锋
 * 功能说明：       安元科技GIS扩展集合
 * 修改时间：
 * 修 改 人：
 * ========================================================================
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Linq;
using AYKJ.GISDevelop.Platform;
using AYKJ.GISDevelop.Platform.Part;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Toolkit;
using System.Windows.Controls.Primitives;
using AYKJ.GISDevelop.Platform.ToolKit;

namespace AYKJ.GISExtension
{
    public partial class MainPage : UserControl, IWidgets
    {
        public DataQueryPage dataquerypage;
        DataQueryKey dataquerykey;

        public MainPage()
        {
            InitializeComponent();
            dataquerypage = new DataQueryPage();
        }

        #region 接口平台实现
        /// <summary>
        /// 响应Gis平台主页面的调用并返回值
        /// </summary>
        /// <param name="s">传入值</param>
        /// <param name="m">区分指向的功能</param>
        /// <returns>返回值</returns>
        public string LinkReturnGisPlatform(string m, string s)
        {
            //Message.Show("This is AYKJ.GisExtension_MainPage");
            string rs = "<GisExtensionMainPage-" + s + ">";
            switch (m)
            {
                case "DataQueryKey":
                    //主动传递所有标注信息给“子页面”
                    dataquerykey = new DataQueryKey();
                    dataquerykey.getAllDataFromMainPage(s);
                    break;
            }
            return rs;
        }

        public string LinkReturnGisPlatform(string m,object obj1,object obj2 )
        {
            Dictionary<string, string> dic_encn = obj1 as Dictionary<string,string>;
            Dictionary<string, List<Dictionary<string, string>>> dic_thematic = obj2 as Dictionary<string, List<Dictionary<string, string>>>;
            //Message.Show("This is AYKJ.GisExtension_MainPage");
            string rs = "<GisExtensionMainPage-" + "" + ">";
            switch (m)
            {
                case "DataQueryKey":
                    //主动传递所有标注信息给“子页面”
                    dataquerykey = new DataQueryKey();
                    dataquerykey.getAllDataFromMainPage(dic_encn,dic_thematic);
                    break;
            }
            return rs;
        }

        public string LinkFromGiPlatform(string oAction, string oStr, object oArr, object oCls, object[] oArrStr, object[] oArrArr, object[] oArrCls)
        {            
            return "";
        }
        #endregion

        #region 界面打开关闭接口信息
        public void Open()
        {
            Show();
        }

        public event IWidgetEventHander OpenEnd;

        public event PartEventHander ReInitEnd;

        public event PartEventHander ShowEnd;

        public event PartEventHander CloseEnd;

        public event PartEventHander LinkGisPlatformEnd;

        public bool IsOpen
        {
            get { throw new NotImplementedException(); }
        }

        public PartDescriptor Descri
        {
            get { return new PartDescriptor() { Name = "AYKJ.GISExtension" }; }
        }

        public void ReInit()
        {
            throw new NotImplementedException();
        }

        public void Show()
        {
            if (dataquerypage.Parent == null)
            {
                dataquerypage.Show();
            }
            else
            {
                if (dataquerypage.Visibility == System.Windows.Visibility.Collapsed)
                {
                    dataquerypage.Visibility = System.Windows.Visibility.Visible;
                    dataquerypage.Show();
                }
                else
                {
                    dataquerypage.Close();
                }
            }
        }

        public void Close()
        {
            if (dataquerypage.Parent == null)
            {
                dataquerypage.Show();
            }
            else
            {
                if (dataquerypage.Visibility == System.Windows.Visibility.Collapsed)
                {
                    dataquerypage.Visibility = System.Windows.Visibility.Visible;
                    dataquerypage.Show();
                }
                else
                {
                    dataquerypage.Close();
                }
            }
        }

        void Storyboard_Close_Completed(object sender, EventArgs e)
        {
            (this.Parent as Panel).Children.Remove(this);
        }

        #endregion

        /// <summary>
        /// 清除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ClearReset()
        {
            Map map = (Application.Current as IApp).MainMap;
            foreach (var item in map.Layers)
            {
                if ((item is GraphicsLayer) && !(Application.Current as IApp).Dict_ThematicLayer.Keys.ToList().Contains(item.ID))
                {
                    (item as GraphicsLayer).ClearGraphics();
                }
            }
            map.Cursor = Cursors.Arrow;
        }
    }
}
