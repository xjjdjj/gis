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
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client;
using AYKJ.GISDevelop.Platform.ToolKit;

namespace AYKJ.GISInterface
{
    /// <summary>
    /// 委托
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void LeakDelegate(object sender, EventArgs e);

    public partial class WindPage : ChildWindow
    {
        //定义查询事件
        public event LeakDelegate LeakEvent;
        public event LeakDelegate LeakFaildEvent;
        public List<Graphic> lst_Return;
        public List<Graphic> lst_Txt;
        public Graphic gra_leak;
        //风向度数
        public double windDirection = 0;
        //是否移动指针
        bool ifCapture = false;
        //泄漏模式
        public string strleaktype;
        //动画
        public WaitAnimationWindow waitanimationwindow;

        clsInstantaneousLeak clsinstantaneousleak;
        clsContinuousLeak clscontinuousleak;

        public Map LeakMap;
        public string strurl;
        #region  瞬时泄漏传入参数
        public double dbx;
        public double dby;
        public double[] arysize;
        public double[] aryradius;
        #endregion

        #region 连续泄漏传入参数
        public double dbCombustionStart;
        public double[] aryCombustionValue;
        public double dbCombustionStep;
        public double dbPoisoningStart;
        public double[] aryPoisoningValue;
        public double dbPoisoningStep;
        public string strContinuedType;
        //燃烧面积
        public double dbCombustionArea;
        //中毒面积
        public double dbPoisoningArea;
        #endregion

        void ProcessAction(object sender, EventArgs e)
        {
            if (LeakEvent == null)
                LeakEvent += new LeakDelegate(LeakFaildEvent);
            LeakEvent(sender, e);
        }

        /// <summary>
        /// 如果没有自己指定关联方法，将会调用该方法抛出错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ExplosionLeaErrorEvent(object sender, EventArgs e)
        {
            LeakFaildEvent(sender, e);
        }

        public WindPage()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;

            if (strleaktype == "Instantaneous")
            {
                waitanimationwindow = new WaitAnimationWindow("瞬时泄漏模拟中，请稍候...");
                clsinstantaneousleak = new clsInstantaneousLeak();
                clsinstantaneousleak.InstantaneousLeakEvent += new InstantaneousLeakDelegate(clsinstantaneousleak_InstantaneousLeakEvent);
                clsinstantaneousleak.InstantaneousLeakFaildEvent += new InstantaneousLeakDelegate(clsinstantaneousleak_InstantaneousLeakFaildEvent);
                clsinstantaneousleak.LeakShaft(LeakMap, strurl, dbx, dby, arysize, aryradius, windDirection);
            }
            else if (strleaktype == "Continuous")
            {
                waitanimationwindow = new WaitAnimationWindow("连续泄漏模拟中，请稍候...");
                clscontinuousleak = new clsContinuousLeak();
                clscontinuousleak.ContinuousLeakEvent += new ContinuousLeakDelegate(clscontinuousleak_ContinuousLeakEvent);
                clscontinuousleak.ContinuousLeakFaildEvent += new ContinuousLeakDelegate(clscontinuousleak_ContinuousLeakFaildEvent);
                clscontinuousleak.LeakContinued(LeakMap, strurl, dbx, dby, dbCombustionStep, aryCombustionValue, dbCombustionStart,
                     dbPoisoningStep, aryPoisoningValue, dbPoisoningStart, windDirection, strContinuedType);
            }
            //waitanimationwindow.Show();
        }

        void clscontinuousleak_ContinuousLeakFaildEvent(object sender, EventArgs e)
        {
            LeakFaildEvent(sender, e);
        }

        void clscontinuousleak_ContinuousLeakEvent(object sender, EventArgs e)
        {
            lst_Return = (sender as clsContinuousLeak).lst_Return;
            lst_Txt = (sender as clsContinuousLeak).lst_Txt;
            ProcessAction(this, EventArgs.Empty);
        }

        void clsinstantaneousleak_InstantaneousLeakEvent(object sender, EventArgs e)
        {
            lst_Return = (sender as clsInstantaneousLeak).lst_Return;
            gra_leak = (sender as clsInstantaneousLeak).graUnio;
            ProcessAction(this, EventArgs.Empty);
        }

        void clsinstantaneousleak_InstantaneousLeakFaildEvent(object sender, EventArgs e)
        {
            LeakFaildEvent(sender, e);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void btnArrow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.btnArrow.CaptureMouse();
            ifCapture = true;
        }

        private void btnArrow_MouseMove(object sender, MouseEventArgs e)
        {
            if (ifCapture)
            {
                //随着鼠标移动，转动角度
                double x = e.GetPosition(wdLayoutRoot).X;
                double y = e.GetPosition(wdLayoutRoot).Y;

                int tmpAngle = Convert.ToInt32((Math.Atan2((y - 97), (x - 138))) * 180 / Math.PI);

                if (tmpAngle >= -180 && tmpAngle <= -90)
                {

                    this.lblName.Content = (tmpAngle + 450).ToString() + "度";
                    windDirection = tmpAngle + 450;
                }
                else
                {
                    this.lblName.Content = (tmpAngle + 90).ToString() + "度";
                    windDirection = tmpAngle + 90;
                }
                if (tmpAngle == -90)
                {
                    this.lblName.Content = "0度";
                    windDirection = 0;
                }
                this.btnArrow.RenderTransform.SetValue(RotateTransform.AngleProperty, Convert.ToDouble(tmpAngle + 90));
            }
        }

        private void btnArrow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ifCapture = false;
            this.btnArrow.ReleaseMouseCapture();
        }


    }
}

