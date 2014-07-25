using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace ItExpert
{
    public class SettingsSliderContentCreator : BaseNavigationBarContentCreator
    {
        public override float GetContentHeight(UIView cellContentView, NavigationBarItem item)
        {
            return _height;
        }

        protected override void Create(UITableViewCell cell, NavigationBarItem item)
        {
            CreateSlider(cell.ContentView.Frame.Size, item);

            cell.ContentView.Add(_textView);
            cell.ContentView.Add(_slider);
            cell.ContentView.Add(_valueLabel);
        }

        protected override void Update(UITableViewCell cell, NavigationBarItem item)
        {
            if (_slider == null)
            {
                CreateSlider(cell.ContentView.Frame.Size, item);
            }
            else
            {
                _textView.Dispose();
                _textView = null;

                CreateTextView(cell.ContentView.Frame.Size, item);
            }

            _slider.Value = (int)item.GetValue();
            _valueLabel.Text = _item.GetValue().ToString();

            _slider.Frame = new RectangleF(cell.ContentView.Frame.Width / 2 - _leftSliderOffset,  cell.ContentView.Frame.Height / 2 - _slider.Frame.Height / 2, 
                cell.ContentView.Frame.Width / 2 - _padding.Right - _labelSize.Width + _leftSliderOffset, _slider.Frame.Height);

            _valueLabel.Frame = new RectangleF(new PointF(cell.ContentView.Frame.Width - _labelSize.Width, cell.ContentView.Frame.Height / 2 - _labelSize.Height / 2), _labelSize);

            cell.ContentView.Add(_textView);
            cell.ContentView.Add(_slider);
            cell.ContentView.Add(_valueLabel);
        }

        private void CreateSlider(SizeF viewSize, NavigationBarItem item)
        {
            CreateTextView(viewSize, item);

            _labelSize = new SizeF(15, 20);
            _leftSliderOffset = 35;

            _slider = new UISlider();

            _slider.Frame = new RectangleF(viewSize.Width / 2 - _leftSliderOffset,  viewSize.Height / 2 - _slider.Frame.Height / 2, 
                viewSize.Width / 2 - _padding.Right - _labelSize.Width + _leftSliderOffset, _slider.Frame.Height);

			_slider.MinValue = 1;
			_slider.MaxValue = 6;
            _slider.Value = (int)_item.GetValue();;
			_slider.Continuous = false;

            _valueLabel = new UILabel();

            _valueLabel.Frame = new RectangleF(new PointF(viewSize.Width - _labelSize.Width, viewSize.Height / 2 - _labelSize.Height / 2), _labelSize);
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

        private float _leftSliderOffset;

        private SizeF _labelSize;

        private UISlider _slider;
        private UILabel _valueLabel;
    }
}

