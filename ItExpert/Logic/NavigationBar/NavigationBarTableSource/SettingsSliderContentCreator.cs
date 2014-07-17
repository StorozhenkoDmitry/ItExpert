using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace ItExpert
{
    public class SettingsSliderContentCreator : BaseNavigationBarContentCreator
    {
        protected override void Create(UITableViewCell cell, NavigationBarItem item)
        {
            CreateSlider(cell.ContentView.Frame.Size);

            cell.ContentView.Add(_slider);
            cell.ContentView.Add(_valueLabel);
        }

        protected override void Update(UITableViewCell cell, NavigationBarItem item)
        {
            if (_slider == null)
            {
                CreateSlider(cell.ContentView.Frame.Size);
            }

            _slider.Value = (int)item.GetValue();
            _valueLabel.Text = _item.GetValue().ToString();

            cell.ContentView.Add(_slider);
            cell.ContentView.Add(_valueLabel);
        }

        private void CreateSlider(SizeF viewSize)
        {
            SizeF labelSize = new SizeF(15, 20);
            float leftSliderOffset = 35;

            _slider = new UISlider();

            _slider.Frame = new RectangleF(viewSize.Width / 2 - leftSliderOffset,  viewSize.Height / 2 - _slider.Frame.Height / 2, 
                viewSize.Width / 2 - _padding.Right - labelSize.Width + leftSliderOffset, _slider.Frame.Height);

            _slider.MinValue = 0;
            _slider.MaxValue = 5;
            _slider.Value = (int)_item.GetValue();;
            _slider.Continuous = false;

            _valueLabel = new UILabel();

            _valueLabel.Frame = new RectangleF(new PointF(viewSize.Width - labelSize.Width, viewSize.Height / 2 - labelSize.Height / 2), labelSize);
            _valueLabel.BackgroundColor = UIColor.Clear;
            _valueLabel.TextColor = UIColor.White;
            _valueLabel.Text = _item.GetValue().ToString();

            _slider.ValueChanged += (sender, e) => 
            {
                var value = Convert.ToInt32(_slider.Value);

                _valueLabel.Text = value.ToString();

                _item.SetValue(value);
            };
        }

        private UISlider _slider;
        private UILabel _valueLabel;
    }
}

