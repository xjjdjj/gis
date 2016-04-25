/// <summary>  
/// 作者：陈锋 
/// 时间：2012/5/21 15:54:19  
/// 公司:南京安元科技有限公司  
/// 版权：2012-2020  
/// CLR版本：4.0.30319.261  
/// CursorEx说明：
/// 唯一标识：cdcad416-b50b-43dd-ad66-448e9961999e  
/// </summary>

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace AYKJ.GISDevelop.Platform.ToolKit
{
    public class CursorEx
    {
        private static Popup _cursorPopup;
        private static DispatcherTimer _mouseLeaveTimer;
        private static object _syncRoot = new object();
        private static GeneralTransform _generalTransform;
        private static Point _mousePoint;
        private static UIElement _popupChild;
        private static FrameworkElement _shownElement;
        private static FrameworkElement _capturingElement;

        private static Popup CursorPopup
        {
            get
            {
                if (_cursorPopup == null)
                {
                    lock (_syncRoot)
                    {
                        if (_cursorPopup == null)
                        {
                            _cursorPopup = new Popup();
                            _cursorPopup.IsHitTestVisible = false;
                            _cursorPopup.IsOpen = true;
                        }
                    }
                }
                return _cursorPopup;
            }
        }

        /// <summary>
        /// MouseLeave启动
        /// </summary>
        private static DispatcherTimer MouseLeaveTimer
        {
            get
            {
                if (_mouseLeaveTimer == null)
                {
                    lock (_syncRoot)
                    {
                        if (_mouseLeaveTimer == null)
                        {
                            _mouseLeaveTimer = new DispatcherTimer();
                            _mouseLeaveTimer.Interval = TimeSpan.FromMilliseconds(10);
                            _mouseLeaveTimer.Tick += new EventHandler(OnMouseLeaveTimerTick);

                        }
                    }
                }
                return _mouseLeaveTimer;
            }
        }

        #region dependency property

        #region 自定义鼠标
        public static UIElement GetCustomCursor(DependencyObject obj) { return (UIElement)obj.GetValue(CustomCursorProperty); }

        public static void SetCustomCursor(DependencyObject obj, UIElement value) { obj.SetValue(CustomCursorProperty, value); }

        public static readonly DependencyProperty CustomCursorProperty =
            DependencyProperty.RegisterAttached("CustomCursor", typeof(FrameworkElement), typeof(CursorEx), new PropertyMetadata(OnCursorChanged));
        #endregion

        #region 是否使用默认鼠标
        public static bool GetUseOriginalCursor(DependencyObject obj)
        {
            return (bool)obj.GetValue(UseOriginalCursorProperty);
        }

        public static void SetUseOriginalCursor(DependencyObject obj, bool value)
        {
            obj.SetValue(UseOriginalCursorProperty, value);
        }

        public static readonly DependencyProperty UseOriginalCursorProperty =
            DependencyProperty.RegisterAttached("UseOriginalCursor", typeof(bool), typeof(CursorEx), new PropertyMetadata(OnUseOriginalCursorChanged));
        #endregion

        #region 本來的鼠标
        private static Cursor GetOriginalCursor(DependencyObject obj)
        {
            return (Cursor)obj.GetValue(OriginalCursorProperty);
        }

        private static void SetOriginalCursor(DependencyObject obj, Cursor value)
        {
            obj.SetValue(OriginalCursorProperty, value);
        }

        public static readonly DependencyProperty OriginalCursorProperty =
            DependencyProperty.RegisterAttached("OriginalCursor", typeof(Cursor), typeof(CursorEx), null);
        #endregion

        #endregion

        #region 响应事件

        private static void OnUseOriginalCursorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            FrameworkElement element = obj as FrameworkElement;
            if (element != null)
                SetCusorToUIElement(element);
        }


        private static void OnCursorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            FrameworkElement element = obj as FrameworkElement;
            if (element != null)
                SetCusorToUIElement(element);
        }

        private static void OnMouseMove(object sender, MouseEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            _generalTransform = element.TransformToVisual(PFApp.Root);
            OnMouseMove(element, _generalTransform.Transform(e.GetPosition(element)));
        }


        private static void OnMouseLeave(object sender, MouseEventArgs e)
        {
            var popup = CursorPopup;
            FrameworkElement element = sender as FrameworkElement;
            var child = GetCustomCursor(element);
            if (child != null)
                child.Visibility = Visibility.Collapsed;

            if (_capturingElement == element)
                MouseLeaveTimer.Start();
            else
                _shownElement = null;
        }

        private static void OnMouseLeaveTimerTick(object sender, EventArgs e)
        {
            if (_capturingElement == null || CheckIsCapturing(_capturingElement) == false)
            {
                MouseLeaveTimer.Stop();
                _shownElement = null;
                UpdateCurrentChild();
            }
        }
        #endregion


        private static void SetCusorToUIElement(FrameworkElement element)
        {
            var customCurosr = GetCustomCursor(element);
            var userOriginalCursor = GetUseOriginalCursor(element);
            if (customCurosr != null || userOriginalCursor)
            {
                if (customCurosr != null)
                {
                    customCurosr.IsHitTestVisible = false;
                    if (_shownElement == element && CursorPopup.Child != null)
                    {
                        customCurosr.Visibility = CursorPopup.Child.Visibility;
                        CursorPopup.Child = customCurosr;
                    }
                }
                if (userOriginalCursor == false)
                    element.Cursor = Cursors.None;
                DetachEvent(element);
                AttachEvent(element);
                if (_mousePoint != null && VisualTreeHelper.FindElementsInHostCoordinates(_mousePoint, element).Contains(element))
                    OnMouseMove(element, _mousePoint);
            }
            else
            {
                SetIsHandeld(element, false);
                element.Cursor = GetOriginalCursor(element);
                UpdateCurrentChild();
            }

            if (GetOriginalCursor(element) == null && element.Cursor != Cursors.None)
                SetOriginalCursor(element, element.Cursor);
        }

        private static void AttachEvent(FrameworkElement element)
        {
            element.MouseLeave += new MouseEventHandler(OnMouseLeave);
            element.MouseMove += new MouseEventHandler(OnMouseMove);
        }

        private static void DetachEvent(FrameworkElement element)
        {
            element.MouseLeave -= new MouseEventHandler(OnMouseLeave);
            element.MouseMove -= new MouseEventHandler(OnMouseMove);
        }


        private static bool CheckIsHandled(FrameworkElement element)
        {
            if (_shownElement == element)
                return false;
            FrameworkElement parent = _shownElement;
            while (parent != null && parent != element)
                parent = VisualTreeHelper.GetParent(parent) as FrameworkElement;
            if (parent != null)
                return true;
            else
                return false;
        }

        private static void SetIsHandeld(FrameworkElement element, bool value)
        {
            if (value)
                _shownElement = element;
            else
            {
                if (_shownElement == element)
                    _shownElement = null;
                DetachEvent(element);
            }
        }

        private static void OnMouseMove(FrameworkElement element, Point mousePoint)
        {
            _mousePoint = mousePoint;
            if (element == null || CheckIsHandled(element))
                return;
            SetIsHandeld(element, true);
            _popupChild = GetCustomCursor(element);

            if (_popupChild != CursorPopup.Child)
                CursorPopup.Child = _popupChild;
            if (_popupChild == null)
                return;
            if (_popupChild.Visibility != Visibility.Visible)
                _popupChild.Visibility = Visibility.Visible;
            CursorPopup.HorizontalOffset = _mousePoint.X;
            CursorPopup.VerticalOffset = _mousePoint.Y;

            if (CheckIsCapturing(element))
                _capturingElement = element;
            else if (_capturingElement == element)
                _capturingElement = null;
            GetToppestElement(_mousePoint);
        }


        private static void UpdateCurrentChild()
        {
            if (_mousePoint == null)
                return;
            var pointElement = GetToppestElement(_mousePoint);
            if (pointElement == null)
                CursorPopup.Child = null;
            else
                OnMouseMove(pointElement, _mousePoint);
        }

        private static FrameworkElement GetToppestElement(Point point)
        {

            var elements = VisualTreeHelper.FindElementsInHostCoordinates(point, PFApp.Root);
            return elements.Where(es => (es is FrameworkElement)
                && (GetCustomCursor(es as FrameworkElement) != null || GetUseOriginalCursor(es as FrameworkElement) == true))
                .ElementAtOrDefault(0) as FrameworkElement;
        }


        private static bool CheckIsCapturing(FrameworkElement element)
        {
            bool isRootCapturingMouse = PFApp.Root.CaptureMouse();
            PFApp.Root.ReleaseMouseCapture();
            if (isRootCapturingMouse)
                return false;
            else
            {
                if (element.CaptureMouse())
                    return true;
                else
                    return false;
            }
        }
    }
}
