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

namespace AYKJ.GISDevelop.Platform
{
    public delegate void SelectedColor(Color strColor);
    public partial class ColorPickerWin : ChildWindow
    {
        public event SelectedColor selectedColor;
        public ColorPickerWin()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            selectedColor(this.colorPicker1.Color);
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}

