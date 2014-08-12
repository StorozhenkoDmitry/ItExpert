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

		public override void Dispose()
		{
			base.Dispose();
			if (_textView != null)
			{
				_textView.RemoveFromSuperview();
				_textView.Dispose();
			}
			_textView = null;
			if (_slider != null)
			{
				_slider.RemoveFromSuperview();
				_slider.ValueChanged -= SliderValueChanged;
				_slider.Dispose();
			}
			_slider = null;
			if (_valueLabel != null)
			{
				_valueLabel.RemoveFromSuperview();
				_valueLabel.Dispose();
			}
			_valueLabel = null;
		}

		public void AddContent(UIView contentView)
		{
			var frame = contentView.Frame;
			var settingsSliderView = new SettingsSliderView(frame, _slider, _textView, _valueLabel, SliderValueChanged);
			contentView.Add(settingsSliderView);
		}
			
        protected override void Create(UITableViewCell cell, NavigationBarItem item)
        {
            CreateSlider(cell.ContentView.Frame.Size, item);

			AddContent(cell.ContentView);
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

			AddContent(cell.ContentView);
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
        }

		void SliderValueChanged(object sender, EventArgs e)
		{
			var value = Convert.ToInt32(_slider.Value);

			_valueLabel.Text = value.ToString();
			_item.SetValue(value);
		}

        private float _leftSliderOffset;

        private SizeF _labelSize;

        private UISlider _slider;
        private UILabel _valueLabel;
    }

	public class SettingsSliderView : UIView, ICleanupObject
	{
		private UISlider _slider;
		private UILabel _valueLabel;
		private UITextView _textView;
		private EventHandler _sliderValueChanged;

		public SettingsSliderView(RectangleF frame, UISlider slider, UITextView textView, UILabel valueLabel,
			EventHandler sliderValueChanged): base(frame)
		{
			_slider = slider;
			_textView = textView;
			_valueLabel = valueLabel;
			_sliderValueChanged = sliderValueChanged;
			_slider.ValueChanged += _sliderValueChanged;
			Add(_textView);
			Add(_slider);
			Add(_valueLabel);
		}

		public void CleanUp()
		{
			foreach (var view in Subviews)
			{
				view.RemoveFromSuperview();
			}
			if (_textView != null)
			{
				_textView.Dispose();
			}
			_textView = null;
			if (_slider != null)
			{
				_slider.ValueChanged -= _sliderValueChanged;
			}
			_slider = null;
			_valueLabel = null;
			_sliderValueChanged = null;
		}
	}
}

