/// 使用的时候请在sl宿主页面加入样式
///  .inputcss
///  {
///     border-collapse: collapse;
///     border: solid 0px Transparent; background-color: Transparent;

/// }
/// .divInputcss
/// {

///     border: solid 0px Transparent; background-color:Transparent;
///      position: absolute; display: none;  z-index:1000;

/// }
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace AYKJ.GISKeysearch
{
    public partial class Input : UserControl
    {
        HtmlElement divIndicatorName;
        HtmlElement txtIndicatorNameElements;
        public delegate void KeyDownHandel(object sender, string keyCode);
        public event KeyDownHandel KeyDownHandelEvent;
        public Input()
        {
            InitializeComponent();
            System.Windows.Interop.SilverlightHost host = Application.Current.Host;
            System.Windows.Interop.Settings setting = host.Settings;
            bool isWindowless = setting.Windowless;

            if (isWindowless == true)
            {
                CreateHtmlElement();
                this.SizeChanged += new SizeChangedEventHandler(EsmsInput_SizeChanged);
                this.txtBox.Visibility = Visibility.Collapsed;
                this.txtIndicatorName.Visibility = Visibility.Visible;

            }
        }
        /// <summary>
        /// 当这个控件大小发生了变化，需要重新调整input的大小
        /// </summary>
        void EsmsInput_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            divIndicatorName.SetStyleAttribute("width", e.NewSize.Width.ToString() + "px");
            divIndicatorName.SetStyleAttribute("heght", e.NewSize.Height.ToString() + "px");
            txtIndicatorNameElements.SetStyleAttribute("width", e.NewSize.Width.ToString() + "px");
            txtIndicatorNameElements.SetStyleAttribute("heght", e.NewSize.Height.ToString() + "px");

        }
        /// <summary>
        /// 创建一个input 元素作为输入设备
        /// </summary>
        void CreateHtmlElement()
        {
            divIndicatorName = HtmlPage.Document.CreateElement("div");
            txtIndicatorNameElements = HtmlPage.Document.CreateElement("input");
            divIndicatorName.AppendChild(txtIndicatorNameElements);
            divIndicatorName.SetStyleAttribute("display", "none");
            divIndicatorName.SetStyleAttribute("position", "absolute");
            divIndicatorName.SetStyleAttribute("z-index", "10000");
            divIndicatorName.SetStyleAttribute("left", string.Format("{0}px", 0));
            divIndicatorName.SetStyleAttribute("top", string.Format("{0}px", 0));
            //这个样式必须放在html中，动态生成的样式不起作用，原因不明
            divIndicatorName.CssClass = "divInputcss";
            txtIndicatorNameElements.SetAttribute("type","text");
            txtIndicatorNameElements.SetStyleAttribute("background-color", "Transparent");
            //这个样式必须放在html中，动态生成的样式不起作用，原因不明
            txtIndicatorNameElements.CssClass = "inputcss";
            HtmlPage.Document.Body.AppendChild(divIndicatorName);
            txtIndicatorNameElements.AttachEvent("onblur", new EventHandler(onLostFocus));

            //注册一个keydown事件用于托管代码中调用
            txtIndicatorNameElements.AttachEvent("onkeydown", new EventHandler(onExecuteQueryByonKeyDown));
            //这是一个用border画的虚假的输入框，当它被点击的时候，显示input元素，并定位到这个border上面
            this.bdInputName.MouseLeftButtonDown += new MouseButtonEventHandler(bdInputName_MouseLeftButtonDown);
        }


        private void onLostFocus(object sender, EventArgs e)
        {
            hideHtmlElementByResize(null, null);

        }

        private void onExecuteQueryByonKeyDown(object sender, EventArgs e)
        {
            if (KeyDownHandelEvent != null)
            {
                string keyCode = HtmlPage.Window.Eval("event.keyCode").ToString();
                KeyDownHandelEvent(this, keyCode);
            }
        }

        private void hideHtmlElementByResize(object sender, EventArgs e)
        {
            divIndicatorName.SetStyleAttribute("display", "none");
            divIndicatorName.SetStyleAttribute("left", string.Format("{0}px", 0));
            divIndicatorName.SetStyleAttribute("top", string.Format("{0}px", 0));
            this.txtIndicatorName.Text = txtIndicatorNameElements.GetProperty("value").ToString();

            this.txtIndicatorName.Opacity = 1;
            Application.Current.Host.Content.Resized -= new EventHandler(hideHtmlElementByResize);
        }

        void bdInputName_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point posRoot = e.GetPosition(null);
            Point posRela = e.GetPosition(this.bdInputName);
            double left = posRoot.X - posRela.X + 2;
            double top = posRoot.Y - posRela.Y + 2;

            divIndicatorName.SetStyleAttribute("display", "block");
            divIndicatorName.SetStyleAttribute("left", string.Format("{0}px", left));
            divIndicatorName.SetStyleAttribute("top", string.Format("{0}px", top));
            divIndicatorName.SetStyleAttribute("position", "absolute");
            txtIndicatorNameElements.SetAttribute("value", this.txtIndicatorName.Text);
            txtIndicatorNameElements.Focus();
            this.txtIndicatorName.Opacity = 0;
            Application.Current.Host.Content.Resized += new EventHandler(hideHtmlElementByResize);
        }
        public double EsmsWidth
        {
            set
            {
                this.bdInputName.Width = value;
                EsmsInputWidth = value;
            }
            get
            {
                return this.bdInputName.Width;
            }
        }
        public double EsmsHeight
        {
            set
            {
                this.bdInputName.Height = value;
                EsmsInputHeight = value;
            }
            get
            {
                return this.bdInputName.Height;
            }
        }
        public double EsmsInputWidth
        {
            set
            {
                if (this.txtIndicatorNameElements != null)
                {
                    this.txtIndicatorNameElements.SetStyleAttribute("width", (value - 4).ToString());
                    this.divIndicatorName.SetStyleAttribute("width", (value - 4).ToString());
                    this.txtIndicatorName.Width = value;
                }
            }
            get
            {
                return this.bdInputName.Width;
            }
        }
        public double EsmsInputHeight
        {
            set
            {

                if (this.txtIndicatorNameElements != null)
                {
                    this.txtIndicatorNameElements.SetStyleAttribute("height", (value - 4).ToString());
                    this.divIndicatorName.SetStyleAttribute("height", (value - 4).ToString());
                }
            }
            get
            {
                return this.bdInputName.Height;
            }
        }
        public string Text
        {
            get
            {
                if (txtIndicatorNameElements != null)
                {
                    this.txtIndicatorName.Text = txtIndicatorNameElements.GetProperty("value").ToString();
                }
                else
                {
                    return this.txtBox.Text;
                }
                return this.txtIndicatorName.Text;
            }
            set
            {
                this.txtIndicatorName.Text = value;
                if (txtIndicatorNameElements != null)
                {
                    txtIndicatorNameElements.SetProperty("value", value ?? "");
                }
            }
        }
    }
}
