using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace ItExpert
{
    public class CacheSliderContentCreator : BaseNavigationBarContentCreator
    {
        public override float GetContentHeight(UIView cellContentView, NavigationBarItem item)
        {
            _item = item;

            CreateSlider(cellContentView.Frame.Size, item);

            _height = _slider.Frame.Bottom + _padding.Bottom;

            _textView.Dispose();
            _textView = null;

            _rightTextView.Dispose();
            _rightTextView = null;

            _slider.Dispose();
            _slider = null;

            return _height;
        }

        protected override void Create(UITableViewCell cell, NavigationBarItem item)
        {
            CreateSlider(cell.ContentView.Frame.Size, item);

            cell.ContentView.Add(_textView);
            cell.ContentView.Add(_rightTextView);
            cell.ContentView.Add(_slider);
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

            cell.ContentView.Add(_textView);
            cell.ContentView.Add(_rightTextView);
            cell.ContentView.Add(_slider);
        }

        private void CreateSlider(SizeF viewSize, NavigationBarItem item)
        {
            CreateTextView(viewSize, item);

            _rightTextView = ItExpertHelper.GetTextView(ItExpertHelper.GetAttributedString(item.GetValue().ToString(), UIFont.SystemFontOfSize(12), UIColor.White),
                viewSize.Width, new PointF());

            _rightTextView.Frame = new RectangleF(new PointF(viewSize.Width - _rightTextView.Frame.Width - _padding.Right, viewSize.Height / 2 - _rightTextView.Frame.Height / 2), 
                _rightTextView.Frame.Size);
            _rightTextView.BackgroundColor = UIColor.Clear;

            float sliderHorizontalOffset = 10;
            float sliderVerticalOffset = 5;

            _slider = new UISlider();

            _slider.Frame = new RectangleF(sliderHorizontalOffset + _padding.Left,  _textView.Frame.Bottom + _padding.Bottom + sliderVerticalOffset, 
                viewSize.Width - _padding.Right - _padding.Left - sliderHorizontalOffset * 2, _slider.Frame.Height);

            _slider.MinValue = 0;
            _slider.MaxValue = 7;
            _slider.Value = (int)_item.GetValue();
            _slider.Continuous = false;

            _slider.ValueChanged += (sender, e) => 
            {
                var value = Convert.ToInt32(_slider.Value);

                _item.SetValue(value);
            };
        }

        private UITextView _rightTextView;
        private UISlider _slider;
    }
}

