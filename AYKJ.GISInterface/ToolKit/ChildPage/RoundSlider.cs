using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace AYKJ.GISInterface.ToolKit.ChildPage
{
    public class RoundSlider : Slider, INotifyPropertyChanged
    {
        bool _busy = false;
        double _discreteValue;
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public double RoundValue
        {
            get
            {
                return (SmallChange == 0 ? Value : Math.Round(Value / SmallChange) * SmallChange);
            }
        }

        protected override void OnValueChanged(double oldValue, double newValue)
        {
            if (!_busy)
            {
                _busy = true;
                if (SmallChange != 0)
                {
                    double newDiscreteValue = Math.Round(newValue / SmallChange) * SmallChange;
                    if (newDiscreteValue != _discreteValue)
                    {
                        Value = newDiscreteValue;
                        base.OnValueChanged(_discreteValue, newDiscreteValue);
                        _discreteValue = newDiscreteValue;
                    }
                }
                else
                {
                    base.OnValueChanged(oldValue, newValue);
                }
                _busy = false;
            }
            NotifyPropertyChanged("RoundValue");
        }
    }
}
