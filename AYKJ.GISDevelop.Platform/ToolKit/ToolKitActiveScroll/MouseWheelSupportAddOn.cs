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
using System.Collections.Generic;
using System.Windows.Browser;

namespace AYKJ.GISDevelop.Platform
{
    public class MouseWheelSupportAddOn : IDisposable
    {
        private static Dictionary<UIElement, MouseWheelSupportAddOn> _addons = new Dictionary<UIElement, MouseWheelSupportAddOn>();
        private static ScrollViewer _scrollViewer;
        private static UIElement _scrollViewerOrHost;
        private static object _syncRoot = new object();

        static MouseWheelSupportAddOn()
        {
            HtmlPage.Window.AttachEvent("DOMMouseScroll", OnMouseScroll); // firefox
            HtmlPage.Window.AttachEvent("onmousewheel", OnMouseScroll);
            HtmlPage.Document.AttachEvent("onmousewheel", OnMouseScroll); // ie
        }

        private static void OnMouseScroll(object sender, HtmlEventArgs args)
        {
            if (_scrollViewer != null)
            {
                double delta = 0;
                ScriptObject e = args.EventObject; // safari & firefox
                if (e.GetProperty("detail") != null)
                {
                    delta = ((double)e.GetProperty("detail")) * -100;
                }
                else if (e.GetProperty("wheelDelta") != null) // ie && Opera
                {
                    delta = ((double)e.GetProperty("wheelDelta"));
                }
                _scrollViewer.ScrollToVerticalOffset(_scrollViewer.VerticalOffset + delta * -1 * 0.1);
            }
        }

        public static void Activate(UIElement element)
        {
            Activate(element, true);
        }

        public static void Activate(UIElement element, bool activateChildren)
        {
            _addons[element] = new MouseWheelSupportAddOn(element, activateChildren);
        }

        public static void Deactivate(UIElement element)
        {
            _addons[element].Dispose();
            _addons.Remove(element);
        }

        private UIElement _host;
        private bool _activateChildren;

        private MouseWheelSupportAddOn(UIElement element, bool activateChildren)
        {
            _host = element;
            _activateChildren = activateChildren;
            Subscribe(element, activateChildren);
        }

        private void Subscribe(UIElement element, bool activateChildren)
        {
            element.GotFocus += new RoutedEventHandler(element_GotFocus);

            if (activateChildren)
            {
                ContentControl contentControl = null;
                Panel panel = null;
                UserControl uc = null;

                if ((contentControl = element as ContentControl) != null)
                {
                    UIElement content = contentControl.Content as UIElement;
                    if (content != null)
                    {
                        Subscribe(content, activateChildren);
                    }
                }
                else if ((panel = element as Panel) != null)
                {
                    foreach (var child in panel.Children)
                    {
                        Subscribe(child, activateChildren);
                    }
                }
                else if ((uc = element as UserControl) != null)
                {
                }
            }
        }

        private void UnSubscribe(UIElement element, bool activateChildren)
        {
            element.GotFocus -= new RoutedEventHandler(element_GotFocus);

            if (_scrollViewerOrHost == element)
            {
                _scrollViewer = null;
            }

            if (activateChildren)
            {
                ContentControl contentControl = null;
                Panel panel = null;
                UserControl uc = null;

                if ((contentControl = element as ContentControl) != null)
                {
                    UIElement content = contentControl.Content as UIElement;
                    if (content != null)
                    {
                        UnSubscribe(content, activateChildren);
                    }
                }
                else if ((panel = element as Panel) != null)
                {
                    foreach (var child in panel.Children)
                    {
                        UnSubscribe(child, activateChildren);
                    }
                }
                else if ((uc = element as UserControl) != null)
                {
                }
            }
        }

        void element_GotFocus(object sender, RoutedEventArgs e)
        {
            lock (_syncRoot)
            {
                ScrollViewer sw;
                IScrollable scrollable;

                if ((sw = sender as ScrollViewer) != null)
                {
                    _scrollViewer = sw;
                    _scrollViewerOrHost = sw;
                }
                else if ((scrollable = sender as IScrollable) != null)
                {
                    _scrollViewer = scrollable.ScrollViewer;
                    _scrollViewerOrHost = scrollable as UIElement;
                }
                else
                {
                    //_scrollViewer = null;
                }
            }
        }

        public void Dispose()
        {
            UnSubscribe(_host, _activateChildren);
        }
    }
}
