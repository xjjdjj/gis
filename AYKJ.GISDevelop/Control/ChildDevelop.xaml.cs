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
using System.Windows.Navigation;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using AYKJ.GISDevelop.Platform;

namespace AYKJ.GISDevelop
{
    public partial class ChildDevelop : ChildWindow
    {
        string m_wxyid;
        string m_wxytype;
        string m_dwdm;
        string m_idkey;
        string m_remark;

        //从配置文件读取对应名称
        private static List<string> listName;
        //配置文件
        XElement xele;
        //点的信息
        private static List<clsPoint> listPoint;


        public ChildDevelop()
        {
            InitializeComponent();
        }

        public void initData(string s)
        {

            //获取点选查询的配置
            listName = new List<string>();
            xele = PFApp.Extent;
            var ln = (from item in xele.Element("PointNames").Elements("name")
                      select new
                      {
                          n = item.Attribute("n").Value,
                      }).ToList();
            foreach (var item in ln)
            {
                listName.Add(item.n);
            }

            listPoint = new List<clsPoint>();
            clsPoint cp;
            for (int i = 0; i < s.Split('|').Length; i++)
            {
                cp = new clsPoint();
                cp.value = s.Split('|')[i];
                cp.name = listName[i];

                listPoint.Add(cp);
            }

            childDg.ItemsSource = null;
            childDg.ItemsSource = listPoint;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        public class clsPoint
        {
            [Display(Name = "属性", GroupName = "clsPoint")]
            public string name { set; get; }
            [Display(Name = "值", GroupName = "clsPoint")]
            public string value { set; get; }
        }

    }
}
