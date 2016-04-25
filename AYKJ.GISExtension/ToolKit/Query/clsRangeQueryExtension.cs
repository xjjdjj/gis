using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ESRI.ArcGIS.Client;
using System.Collections.Generic;
using AYKJ.GISDevelop.Platform;

namespace AYKJ.GISExtension
{
    /// <summary>
    /// 委托
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void QueryExtensionDelegate(object sender, EventArgs e);

    public class clsRangeQueryExtension
    {
        //定义查询事件
        public event QueryExtensionDelegate QueryExtensionEvent;
        public event QueryExtensionDelegate QueryExtensionFaildEvent;
        //空间查询类
        clsRangeQuery clsrangequery;
        //空间查询url
        string strurl;
        //查询的次数
        int int_count;
        //查询的空间范围列表
        List<Graphic> lstgra;
        //被查询的数据
        List<Graphic> lstbygra;
        //返回的数据
        public List<Dictionary<clswxy, Graphic>> lst_Return;

        void ProcessAction(object sender, EventArgs e)
        {
            if (QueryExtensionEvent == null)
                QueryExtensionEvent += new QueryExtensionDelegate(QueryExtensionErrorEvent);
            QueryExtensionEvent(sender, e);
        }

        /// <summary>
        /// 如果没有自己指定关联方法，将会调用该方法抛出错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void QueryExtensionErrorEvent(object sender, EventArgs e)
        {
            QueryExtensionFaildEvent(sender, e);
        }

        public void QueryExtension(string str, List<Graphic> lst)
        {
            lst_Return = new List<Dictionary<clswxy, Graphic>>();
            int_count = 0;
            lstgra = lst;
            strurl = str;
            lstbygra = (Application.Current as IApp).lstThematic;
            clsrangequery = new clsRangeQuery();
            clsrangequery.RangeQueryEvent += new RangeQueryDelegate(clsrangequery_RangeQueryEvent);
            clsrangequery.RangeQueryFaildEvent += new RangeQueryDelegate(clsrangequery_RangeQueryFaildEvent);
            clsrangequery.RangeQuery(strurl, lstgra[int_count].Geometry, lstbygra);
        }

        void clsrangequery_RangeQueryEvent(object sender, EventArgs e)
        {
            List<Graphic> lstreturngra = (sender as clsRangeQuery).lstReturnGraphic;
            Dictionary<clswxy, Graphic> Dict_ResultGraphic = new Dictionary<clswxy, Graphic>();
            for (int i = 0; i < lstreturngra.Count; i++)
            {
                string[] arytmp = lstreturngra[i].Attributes["StaTag"].ToString().Split('|');
                clswxy tmpclswxy = new clswxy()
                {
                    wxyid = arytmp[1],
                    wxyname = arytmp[3],
                    wxytype = (Application.Current as IApp).DictThematicEnCn[arytmp[0]],
                    wxydwdm = arytmp[2]
                };
                Dict_ResultGraphic.Add(tmpclswxy, lstreturngra[i]);
            }
            lst_Return.Add(Dict_ResultGraphic);
            int_count = int_count + 1;
            if (int_count > lstgra.Count - 1)
            {
                ProcessAction(this, EventArgs.Empty);
            }
            else
            {
                clsrangequery.RangeQuery(strurl, lstgra[int_count].Geometry, lstbygra);
            }
        }

        /// <summary>
        /// 查询出错返回错误信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clsrangequery_RangeQueryFaildEvent(object sender, EventArgs e)
        {
            QueryExtensionEvent(sender, e);
        }

    }
}
