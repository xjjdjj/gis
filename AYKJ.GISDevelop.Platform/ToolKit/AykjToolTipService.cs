using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using Microsoft.Expression.Controls;
using Microsoft.Expression.Media;

namespace AYKJ.GISDevelop.Platform
{
    public class AykjToolTipService : DependencyObject
    {
        //局部变量声明
        private static Popup TipPopup;               //提示弹出容器
        private static Callout TipCallout;           //提示标准
        private static TextBlock TipTextBlock;       //提示文本框
        private static DispatcherTimer TipTimer;     //提示计时器
        private static DateTime OwnerTipStartTime;   //依赖对象提示开始时间
        private static int OwnerTipDuration;         //依赖对象提示停留时间
        private static bool NotifyPropertyChangeIf;  //属性更改通知标志
        //静态构造函数(初始化局部变量)
        static AykjToolTipService()
        {
            TipPopup = new Popup();
            TipCallout = new Callout();
            TipTextBlock = new TextBlock();
            TipPopup.Child = TipCallout;
            TipCallout.Content = TipTextBlock;
            TipTextBlock.TextWrapping = TextWrapping.Wrap;
            TipTextBlock.VerticalAlignment = VerticalAlignment.Center;
            TipTimer = new DispatcherTimer();
            TipTimer.Stop(); TipTimer.Tick += TipTimer_Tick;
            NotifyPropertyChangeIf = true;
        }
        //启动计时器
        private static void TipTimerStart(FrameworkElement ElOwner)
        {
            TipTimer.Stop();
            OwnerTipStartTime = DateTime.Now;
            OwnerTipDuration = GetTipDuration(ElOwner);
            var TickInterval = OwnerTipDuration / 5;
            TipTimer.Interval = new TimeSpan(0, 0, 0, 0, TickInterval);
            TipTimer.Start();
        }
        //停止计时器
        private static void TipTimerStop()
        {
            TipTimer.Stop();
        }
        //侦测依赖对象提示信息显示时间
        private static void TipTimer_Tick(object sender, EventArgs e)
        {
            var TipEndTime = DateTime.Now;
            var TipSpan = TipEndTime.Subtract(OwnerTipStartTime);
            if (Convert.ToInt32(TipSpan.TotalMilliseconds) > OwnerTipDuration)
            {
                TipPopup.IsOpen = false; TipTimer.Stop();
            }
        }
        //依赖属性定义－提示文本字体
        public static FontFamily GetTipTextFontFamily(DependencyObject DpObj)
        {
            return (FontFamily)DpObj.GetValue(TipTextFontFamilyProperty);
        }
        public static void SetTipTextFontFamily(DependencyObject DpObj, FontFamily DpValue)
        {
            DpObj.SetValue(TipTextFontFamilyProperty, DpValue);
        }
        public static readonly DependencyProperty TipTextFontFamilyProperty =
            DependencyProperty.RegisterAttached(
            "TipTextFontFamily", typeof(FontFamily), typeof(FrameworkElement),
            new PropertyMetadata(new FontFamily("Arial,NSimSun")));
        //依赖属性定义－提示文本字号
        public static double GetTipTextFontSize(DependencyObject DpObj)
        {
            return (double)DpObj.GetValue(TipTextFontSizeProperty);
        }
        public static void SetTipTextFontSize(DependencyObject DpObj, double DpValue)
        {
            DpObj.SetValue(TipTextFontSizeProperty, DpValue);
        }
        public static readonly DependencyProperty TipTextFontSizeProperty =
            DependencyProperty.RegisterAttached(
            "TipTextFontSize", typeof(double), typeof(FrameworkElement),
            new PropertyMetadata(13.0));
        //依赖属性定义－提示文本颜色
        public static Brush GetTipTextForeground(DependencyObject DpObj)
        {
            return (Brush)DpObj.GetValue(TipTextForegroundProperty);
        }
        public static void SetTipTextForeground(DependencyObject DpObj, Brush DpValue)
        {
            DpObj.SetValue(TipTextForegroundProperty, DpValue);
        }
        public static readonly DependencyProperty TipTextForegroundProperty =
            DependencyProperty.RegisterAttached(
            "TipTextForeground", typeof(Brush), typeof(FrameworkElement),
            new PropertyMetadata(new SolidColorBrush(SystemColors.InfoTextColor)));
        //依赖属性定义－提示文本边距
        public static Thickness GetTipTextMargin(DependencyObject DpObj)
        {
            return (Thickness)DpObj.GetValue(TipTextMarginProperty);
        }
        public static void SetTipTextMargin(DependencyObject DpObj, Thickness DpValue)
        {
            DpObj.SetValue(TipTextMarginProperty, DpValue);
        }
        public static readonly DependencyProperty TipTextMarginProperty =
            DependencyProperty.RegisterAttached(
            "TipTextMargin", typeof(Thickness), typeof(FrameworkElement),
            new PropertyMetadata(new Thickness(4.0)));
        //依赖属性定义－提示标注形状
        public static T4TipCalloutStyle GetTipCalloutStyle(DependencyObject DpObj)
        {
            return (T4TipCalloutStyle)DpObj.GetValue(TipCalloutStyleProperty);
        }
        public static void SetTipCalloutStyle(DependencyObject DpObj, T4TipCalloutStyle DpValue)
        {
            DpObj.SetValue(TipCalloutStyleProperty, DpValue);
        }
        public static readonly DependencyProperty TipCalloutStyleProperty =
            DependencyProperty.RegisterAttached(
            "TipCalloutStyle", typeof(T4TipCalloutStyle), typeof(FrameworkElement),
            new PropertyMetadata(T4TipCalloutStyle.RoundedRectangle));
        //依赖属性定义－提示标注线条颜色
        public static Brush GetTipCalloutStroke(DependencyObject DpObj)
        {
            return (Brush)DpObj.GetValue(TipCalloutStrokeProperty);
        }
        public static void SetTipCalloutStroke(DependencyObject DpObj, Brush DpValue)
        {
            DpObj.SetValue(TipCalloutStrokeProperty, DpValue);
        }
        public static readonly DependencyProperty TipCalloutStrokeProperty =
            DependencyProperty.RegisterAttached(
            "TipCalloutStroke", typeof(Brush), typeof(FrameworkElement),
            new PropertyMetadata(new SolidColorBrush(Colors.Gray)));
        //依赖属性定义－提示标填充颜色
        public static Brush GetTipCalloutFill(DependencyObject DpObj)
        {
            return (Brush)DpObj.GetValue(TipCalloutFillProperty);
        }
        public static void SetTipCalloutFill(DependencyObject DpObj, Brush DpValue)
        {
            DpObj.SetValue(TipCalloutFillProperty, DpValue);
        }
        public static readonly DependencyProperty TipCalloutFillProperty =
            DependencyProperty.RegisterAttached(
            "TipCalloutFill", typeof(Brush), typeof(FrameworkElement),
            new PropertyMetadata(new SolidColorBrush(SystemColors.InfoColor)));
        //依赖属性定义－提示标箭头高度
        public static double GetTipCalloutArrowHeight(DependencyObject DpObj)
        {
            return (double)DpObj.GetValue(TipCalloutArrowHeightProperty);
        }
        public static void SetTipCalloutArrowHeight(DependencyObject DpObj, double DpValue)
        {
            DpObj.SetValue(TipCalloutArrowHeightProperty, DpValue);
        }
        public static readonly DependencyProperty TipCalloutArrowHeightProperty =
            DependencyProperty.RegisterAttached(
            "TipCalloutArrowHeight", typeof(double), typeof(FrameworkElement),
            new PropertyMetadata(16.0));
        //依赖属性定义－提示体最大宽度
        public static double GetTipMaxWidth(DependencyObject DpObj)
        {
            return (double)DpObj.GetValue(TipMaxWidthProperty);
        }
        public static void SetTipMaxWidth(DependencyObject DpObj, double DpValue)
        {
            DpObj.SetValue(TipMaxWidthProperty, DpValue);
        }
        public static readonly DependencyProperty TipMaxWidthProperty =
            DependencyProperty.RegisterAttached(
            "TipMaxWidth", typeof(double), typeof(FrameworkElement),
            new PropertyMetadata(400.0));
        //依赖属性定义－提示体最大高度
        public static double GetTipMaxHeight(DependencyObject DpObj)
        {
            return (double)DpObj.GetValue(TipMaxHeightProperty);
        }
        public static void SetTipMaxHeight(DependencyObject DpObj, double DpValue)
        {
            DpObj.SetValue(TipMaxHeightProperty, DpValue);
        }
        public static readonly DependencyProperty TipMaxHeightProperty =
            DependencyProperty.RegisterAttached(
            "TipMaxHeight", typeof(double), typeof(FrameworkElement),
            new PropertyMetadata(300.0));
        //依赖属性定义－提示体最小宽度
        public static double GetTipMinWidth(DependencyObject DpObj)
        {
            return (double)DpObj.GetValue(TipMinWidthProperty);
        }

        public static void SetTipMinWidth(DependencyObject DpObj, double DpValue)
        {
            DpObj.SetValue(TipMinWidthProperty, DpValue);
        }
        public static readonly DependencyProperty TipMinWidthProperty =
            DependencyProperty.RegisterAttached(
            "TipMinWidth", typeof(double), typeof(FrameworkElement),
            new PropertyMetadata(100.00));
        //依赖属性定义－提示体最小高度
        public static double GetTipMinHeight(DependencyObject DpObj)
        {
            return (double)DpObj.GetValue(TipMinHeightProperty);
        }
        public static void SetTipMinHeight(DependencyObject DpObj, double DpValue)
        {
            DpObj.SetValue(TipMinHeightProperty, DpValue);
        }
        public static readonly DependencyProperty TipMinHeightProperty =
            DependencyProperty.RegisterAttached(
            "TipMinHeight", typeof(double), typeof(FrameworkElement),
            new PropertyMetadata(30.0));
        //依赖属性定义－提示体停留时间
        public static int GetTipDuration(DependencyObject DpObj)
        {
            return (int)DpObj.GetValue(TipDurationProperty);
        }
        public static void SetTipDuration(DependencyObject DpObj, int DpValue)
        {
            DpObj.SetValue(TipDurationProperty, DpValue);
        }
        public static readonly DependencyProperty TipDurationProperty =
            DependencyProperty.RegisterAttached(
            "TipDuration", typeof(int), typeof(FrameworkElement),
            new PropertyMetadata(2000));
        //依赖属性定义－提示体效果
        public static Effect GetTipEffct(DependencyObject DpObj)
        {
            return (Effect)DpObj.GetValue(TipEffctProperty);
        }
        public static void SetTipEffct(DependencyObject DpObj, Effect DpValue)
        {
            DpObj.SetValue(TipEffctProperty, DpValue);
        }
        public static readonly DependencyProperty TipEffctProperty =
            DependencyProperty.RegisterAttached("TipEffct", typeof(Effect), typeof(FrameworkElement),
            new PropertyMetadata(new DropShadowEffect() { ShadowDepth = 1.0, BlurRadius = 0.5, Color = Colors.Black, }));
        //依赖属性定义－提示文本内容
        public static string GetTipText(DependencyObject DpObj)
        {
            return (string)DpObj.GetValue(TipTextProperty);
        }
        public static void SetTipText(DependencyObject DpObj, string DpValue)
        {
            DpObj.SetValue(TipTextProperty, DpValue);
        }
        public static readonly DependencyProperty TipTextProperty =
            DependencyProperty.RegisterAttached(
            "TipText", typeof(string), typeof(FrameworkElement),
            new PropertyMetadata("", TipTextChanged));
        //提示文本内容变更通知
        private static void TipTextChanged(DependencyObject DpObj, DependencyPropertyChangedEventArgs DpArg)
        {
            if (NotifyPropertyChangeIf)
            {
                var ElOwner = DpObj as FrameworkElement;
                ElOwner.MouseEnter -= ElOwner_MouseEnter;
                ElOwner.MouseMove -= ElOwner_MouseMove;
                ElOwner.MouseLeave -= ElOwner_MouseLeave;
                ElOwner.MouseEnter += ElOwner_MouseEnter;
                ElOwner.MouseMove += ElOwner_MouseMove;
                ElOwner.MouseLeave += ElOwner_MouseLeave;
            }
        }
        //依赖对象－鼠标进入事件处理
        private static void ElOwner_MouseEnter(object sender, MouseEventArgs e)
        {
            var ElOwner = sender as FrameworkElement;
            TipPopup.IsOpen = false;
            TipPropertiesCheckAndAdjust(ElOwner);
            InitTips(ElOwner);
        }
        //依赖对象－鼠标移动事件处理
        private static void ElOwner_MouseMove(object sender, MouseEventArgs e)
        {
            var AppRoot = Application.Current.RootVisual as UserControl;
            var ElOwner = sender as FrameworkElement;
            var PosMouse = e.GetPosition(AppRoot);
            LayoutTips(ElOwner, PosMouse);
            TipPopup.IsOpen = true;
            TipTimerStart(ElOwner);
        }
        //依赖对象－鼠标离开事件处理
        private static void ElOwner_MouseLeave(object sender, MouseEventArgs e)
        {
            DeInitTips();
            TipTimerStop();
        }
        //依赖对象－提示初始化(与位置无关内容)
        private static void InitTips(FrameworkElement ElOwner)
        {
            TipTextBlock.Text = GetTipText(ElOwner);
            TipTextBlock.Margin = GetTipTextMargin(ElOwner);
            TipTextBlock.FontFamily = GetTipTextFontFamily(ElOwner);
            TipTextBlock.FontSize = GetTipTextFontSize(ElOwner);
            TipTextBlock.Foreground = GetTipTextForeground(ElOwner);
            TipCallout.Stroke = GetTipCalloutStroke(ElOwner);
            TipCallout.Fill = GetTipCalloutFill(ElOwner);
            TipCallout.CalloutStyle = (CalloutStyle)GetTipCalloutStyle(ElOwner);
            TipCallout.Effect = GetTipEffct(ElOwner);
        }
        //依赖对象－提示外观调整(与位置有关内容)，是整个类的核心
        private static void LayoutTips(FrameworkElement ElOwner, Point PosMouse)
        {
            //设置提示对象的大小
            var TipCalloutArrowHeight = GetTipCalloutArrowHeight(ElOwner);
            var TipTextSize = GetTipTextRequiredSize(ElOwner);
            var TipTextMargin = GetTipTextMargin(ElOwner);
            TipTextBlock.Margin = TipTextMargin;
            TipCallout.Width = TipTextSize.Width + TipTextMargin.Left + TipTextMargin.Right + 1;
            TipCallout.Height = TipTextSize.Height + TipTextMargin.Top + TipTextMargin.Bottom + 1;
            TipPopup.Width = TipCallout.Width + 1;
            TipPopup.Height = TipCallout.Height + TipCalloutArrowHeight + 1;
            //确定提示显示位置
            var AppRoot = Application.Current.RootVisual as UserControl;
            var LeftSpace = PosMouse.X;
            var RightSpace = AppRoot.ActualWidth - PosMouse.X;
            var TopSpace = PosMouse.Y;
            var BottomSpace = AppRoot.ActualHeight - PosMouse.Y;
            var TipPlacementX = "";
            if (RightSpace >= TipPopup.Width) { TipPlacementX = "R"; }
            else if (LeftSpace >= TipPopup.Width) { TipPlacementX = "L"; }
            else { TipPlacementX = RightSpace > LeftSpace ? "R" : "L"; }
            var TipPlacementY = "";
            if (BottomSpace >= TipPopup.Height) { TipPlacementY = "B"; }
            else if (TopSpace >= TipPopup.Height) { TipPlacementY = "T"; }
            else { TipPlacementY = BottomSpace >= TopSpace ? "B" : "T"; }
            var TipPlacement = TipPlacementX + TipPlacementY;
            //根据提示位置，调整提示布局，箭头方向
            var AnchorRatioY = TipCalloutArrowHeight / TipCallout.Height;
            var MouseToPopupX = 4.0; var MouseToPopupY = 4.0;
            if (TipPlacement == "RB")
            {
                TipCallout.AnchorPoint = new Point(0.0, 0.0 - AnchorRatioY);
                TipCallout.Margin = new Thickness(0.0, TipCalloutArrowHeight, 0.0, 0.0);
                TipPopup.HorizontalOffset = PosMouse.X + MouseToPopupX;
                TipPopup.VerticalOffset = PosMouse.Y + MouseToPopupY;
            }
            else if (TipPlacement == "RT")
            {
                TipCallout.AnchorPoint = new Point(0.0, 1.0 + AnchorRatioY);
                TipCallout.Margin = new Thickness(0.0, 0.0, 0.0, TipCalloutArrowHeight);
                TipPopup.HorizontalOffset = PosMouse.X + MouseToPopupX;
                TipPopup.VerticalOffset = PosMouse.Y - TipPopup.Height - MouseToPopupY;
            }
            else if (TipPlacement == "LB")
            {
                TipCallout.AnchorPoint = new Point(1.0, 0.0 - AnchorRatioY);
                TipCallout.Margin = new Thickness(0.0, TipCalloutArrowHeight, 0.0, 0.0);
                TipPopup.HorizontalOffset = PosMouse.X - TipCallout.Width - MouseToPopupX;
                TipPopup.VerticalOffset = PosMouse.Y + MouseToPopupY;
            }
            else if (TipPlacement == "LT")
            {
                TipCallout.AnchorPoint = new Point(1.0, 1.0 + AnchorRatioY);
                TipCallout.Margin = new Thickness(0.0, 0.0, 0.0, TipCalloutArrowHeight);
                TipPopup.HorizontalOffset = PosMouse.X - TipCallout.Width - MouseToPopupX;
                TipPopup.VerticalOffset = PosMouse.Y - TipPopup.Height - MouseToPopupY;
            }
            TipPopup.IsOpen = true;
        }
        //依赖对象－提示反初始化
        private static void DeInitTips()
        {
            TipTimerStop(); TipPopup.IsOpen = false;
        }
        //依赖对象－依赖属性值合理性检查，如不合理，自动调整或抛出异常。
        private static void TipPropertiesCheckAndAdjust(FrameworkElement ElOwner)
        {
            NotifyPropertyChangeIf = false;
            //这里以后有时间再写，以防治设置不合理的属性，导致出错。
            //暂时虽然未检查，但正常属性设置情况下，不会影响使用。
            //检查内容：主要是边距、最大、最小高宽等的逻辑合理性。
            NotifyPropertyChangeIf = true;
        }
        //依赖对象－计算提示文本所需空间
        private static Size GetTipTextRequiredSize(FrameworkElement ElOwner)
        {
            //采用的方法是通过建立一个临时的文本框来检测其所需空间。
            var Rlt = new Size();
            var TipTextMargin = GetTipTextMargin(ElOwner);
            var TipTextMaxSize = new Size()
            {
                Width = GetTipMaxWidth(ElOwner) - TipTextMargin.Left - TipTextMargin.Right,
                Height = GetTipMaxHeight(ElOwner) - TipTextMargin.Top - TipTextMargin.Bottom,
            };
            var TipTextMinSize = new Size()
            {
                Width = GetTipMinWidth(ElOwner) - TipTextMargin.Left - TipTextMargin.Right,
                Height = GetTipMinHeight(ElOwner) - TipTextMargin.Top - TipTextMargin.Bottom,
            };
            var TmpTextBlock = new TextBlock()
            {
                FontFamily = GetTipTextFontFamily(ElOwner),
                FontSize = GetTipTextFontSize(ElOwner),
                Width = TipTextMaxSize.Width,
                Height = TipTextMinSize.Height,
                Foreground = GetTipTextForeground(ElOwner),
                Text = GetTipText(ElOwner),
                TextWrapping = TextWrapping.Wrap,
            };
            Rlt.Width = TmpTextBlock.ActualWidth;
            if (Rlt.Width > TipTextMaxSize.Width) { Rlt.Width = TipTextMaxSize.Width; };
            if (Rlt.Width < TipTextMinSize.Width) { Rlt.Width = TipTextMinSize.Width; }
            Rlt.Height = TmpTextBlock.ActualHeight;
            if (Rlt.Height > TipTextMaxSize.Height) { Rlt.Height = TipTextMaxSize.Height; };
            if (Rlt.Height < TipTextMinSize.Height) { Rlt.Height = TipTextMinSize.Height; }
            return Rlt;
        }
    }
    /// <summary>
    /// 提示标注形状定义
    /// </summary>
    public enum T4TipCalloutStyle
    {
        Cloud = CalloutStyle.Cloud,                       //云形
        Oval = CalloutStyle.Oval,                         //椭圆性
        Rectangle = CalloutStyle.Rectangle,               //方形
        RoundedRectangle = CalloutStyle.RoundedRectangle  //圆角方形
    }
}
