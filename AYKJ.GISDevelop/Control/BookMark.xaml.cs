/// <summary>  
/// 作者：陈锋 
/// 时间：2012/7/18 12:14:31  
/// 公司:南京安元科技有限公司  
/// 版权：2012-2020  
/// CLR版本：4.0.30319.261  
/// BookMark说明：书签
/// 唯一标识：a716a61c-c620-43ce-9404-8494024ed788  
/// </summary>

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AYKJ.GISDevelop.Platform;
using ESRI.ArcGIS.Client;
using System.IO.IsolatedStorage;
using System.IO;
using System.Windows.Controls.Primitives;

namespace AYKJ.GISDevelop.Control
{
    public partial class BookMark : UserControl
    {
        List<Workspace> List = new List<Workspace>();
        //List<Workspace> List_temp = new List<Workspace>();
        Map SystemMainMap = App.mainMap;
        private double xmax;
        private double xmin;
        private double ymax;
        private double ymin;
        private string name = string.Empty;
        //public static List<string> Wname = new List<string>();
        //public static List<string> time = new List<string>();
        //public static List<string> positon = new List<string>();
        private Color rightColor = new Color();
        private Color wrongColor = new Color();
        public ToggleButton currrentogbtn;
        public BookMark()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(BookMark_Loaded);
        }

        void BookMark_Loaded(object sender, RoutedEventArgs e)
        {
            IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
            //store.DeleteFile("bookmark.txt");
            rightColor = Color.FromArgb(255, 37, 238, 89);
            wrongColor = Color.FromArgb(255, 244, 141, 49);
            ReadIso();
            dgBookMarkList.SelectionChanged += new SelectionChangedEventHandler(dgBookMarkList_SelectionChanged);
        }

        /// <summary>
        /// 选择书签
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgBookMarkList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            txt_Suggest.Text = "";

            string nameStr = string.Empty;
            if (dgBookMarkList.SelectedIndex > -1)
            {
                Workspace workspace = dgBookMarkList.SelectedItem as Workspace;
                nameStr = workspace.wname.ToString();
                string[] pos = workspace.position.Split(',');
                if (pos.Length == 4)
                {
                    ESRI.ArcGIS.Client.Geometry.Envelope ex = new ESRI.ArcGIS.Client.Geometry.Envelope(double.Parse(pos[0].ToString()), double.Parse(pos[2].ToString()), double.Parse(pos[1].ToString()), double.Parse(pos[3].ToString()));
                    SystemMainMap.ZoomTo(ex);
                }
                else
                {
                    txt_Suggest.Text = "* 未找到坐标";
                    txt_Suggest.Foreground = new SolidColorBrush(wrongColor);
                    return;
                }
            }
        }

        /// <summary>
        /// 保存书签
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Add_Click(object sender, RoutedEventArgs e)
        {
            string listwname = "";
            txt_Suggest.Text = "";
            listwname = txt_Title.Text.Trim();
            name = txt_Title.Text.Trim();
            if (name == "")
            {
                txt_Suggest.Text = "* 请输入书签名";
                txt_Suggest.Foreground = new SolidColorBrush(wrongColor);
                return;
            }

            for (int i = 0; i < List.Count; i++)
            {
                if (List[i].wname == name)
                {
                    txt_Suggest.Text = "* 书签名已存在";
                    txt_Suggest.Foreground = new SolidColorBrush(wrongColor);

                    txt_Title.Text = "";
                    name = "";

                    return;
                }
            }

            //保存新书签
            if (App.mainMap.Extent == null)
            {
                txt_Suggest.Text = "* 请加载地图";
                txt_Suggest.Foreground = new SolidColorBrush(wrongColor);
                txt_Title.Text = "";
                name = "";
                return;
            }

            xmax = App.mainMap.Extent.XMax;
            xmin = App.mainMap.Extent.XMin;
            ymax = App.mainMap.Extent.YMax;
            ymin = App.mainMap.Extent.YMin;

            string position = xmax.ToString("f3") + "," + xmin.ToString("f3") + "," + ymax.ToString("f3") + "," + ymin.ToString("f3");

            DateTime dt = DateTime.Now;

            CreateIso(name + "%" + dt.ToString() + "%" + position + "|");
            ReadIso();
        }

        /// <summary>
        /// 清除书签
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cleanBtn_Click(object sender, RoutedEventArgs e)
        {
            txt_Suggest.Text = "";

            if (dgBookMarkList.SelectedIndex < 0)
            {
                txt_Suggest.Text = "* 请选择书签";
                txt_Suggest.Foreground = new SolidColorBrush(wrongColor);
                return;
            }

            string nameTmp = string.Empty;


            nameTmp = (dgBookMarkList.SelectedItem as Workspace).wname.ToString();

            for (int i = 0; i < List.Count; i++)
            {
                if (List[i].wname == nameTmp)
                {
                    List.RemoveAt(i);

                    dgBookMarkList.ItemsSource = null;
                    if (List.Count > 0)
                    {
                        dgBookMarkList.ItemsSource = List;
                    }
                    CreateNewIso();
                    txt_Suggest.Text = "* 书签已删除";
                    txt_Suggest.Foreground = new SolidColorBrush(rightColor);
                    return;
                }
            }
            txt_Suggest.Text = "* 书签名不存在";
            txt_Suggest.Foreground = new SolidColorBrush(wrongColor);

        }

        /// <summary>
        /// 限制输入框的长度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txt_TitleLengthChanged(object sender, EventArgs e)
        {
            TextBox tmpTextBox = new TextBox();
            tmpTextBox = sender as TextBox;

            if (tmpTextBox.Text.Length > 15)
            {
                tmpTextBox.Select(0, tmpTextBox.Text.Length - 1);
                tmpTextBox.Text = tmpTextBox.SelectedText;
                tmpTextBox.SelectAll();
                tmpTextBox.SelectionStart = tmpTextBox.Text.Length;
                tmpTextBox.Focus();
            }
        }

        /// <summary>
        /// 关闭窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #region 框架事件
        /// <summary>
        /// 面板关闭方法
        /// </summary>
        public void Close()
        {
            currrentogbtn.IsChecked = false;
            PFApp.Root.Children.Remove(this);
        }

        /// <summary>
        /// 面板展开
        /// </summary>
        public void Show()
        {
            //展开面板
            PFApp.Root.Children.Add(this);
        }
        #endregion

        public class Workspace
        {
            [Display(Name = "书签名", GroupName = "Workspace")]
            public string wname { get; set; }
            [Display(Name = "保存时间", GroupName = "Workspace")]
            public string time { get; set; }
            [Display(Name = "位置", GroupName = "Workspace")]
            public string position { get; set; }
        }


        #region 写入独立存储
        void CreateIso(string strcoor)
        {
            IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
            if (store.FileExists("bookmark.txt"))
            {
                IsolatedStorageFileStream fileStream = store.OpenFile("bookmark.txt", FileMode.Append);
                using (StreamWriter sw = new StreamWriter(fileStream))
                {
                    sw.Write(strcoor);
                }
                fileStream.Close();
            }
            else
            {
                IsolatedStorageFileStream fileStream = store.CreateFile("bookmark.txt");
                using (StreamWriter sw = new StreamWriter(fileStream))
                {
                    sw.WriteLine(strcoor);
                }
                fileStream.Close();

            }
        }

        void CreateNewIso()
        {
            IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
            if (store.FileExists("bookmark.txt"))
            {
                IsolatedStorageFileStream fileStream = store.OpenFile("bookmark.txt", FileMode.Create);
                string strcoor = string.Empty;
                for (int i = 0; i < List.Count; i++)
                {
                    strcoor = strcoor + List[i].wname + "%" + List[i].time + "%" + List[i].position + "|";
                }
                using (StreamWriter sw = new StreamWriter(fileStream))
                {
                    sw.Write(strcoor);
                }
                fileStream.Close();
            }
        }

        void ReadIso()
        {
            IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
            if (store.FileExists("bookmark.txt"))
            {
                StreamReader reader = new StreamReader(store.OpenFile("bookmark.txt",
                   FileMode.Open, FileAccess.Read));
                string strcoors = reader.ReadToEnd().Replace("\r\n", "");
                string[] arycoor = strcoors.Split('|');
                List = new List<Workspace>();
                for (int i = 0; i < arycoor.Length; i++)
                {
                    if (arycoor[i] == "")
                        continue;
                    List.Add(new Workspace()
                    {
                        position = arycoor[i].Split('%')[2],
                        time = arycoor[i].Split('%')[1],
                        wname = arycoor[i].Split('%')[0]
                    });
                }
                if (List.Count > 0)
                {
                    dgBookMarkList.ItemsSource = null;
                    dgBookMarkList.ItemsSource = List;
                }
            }
        }
        #endregion
    }
}
