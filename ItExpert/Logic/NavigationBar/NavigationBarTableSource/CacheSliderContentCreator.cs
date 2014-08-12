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

		public override void Dispose()
		{
			base.Dispose();

			if (_textView != null)
			{
				_textView.RemoveFromSuperview();
				_textView.Dispose();
			}
			_textView = null;

			if (_rightTextView != null)
			{
				_rightTextView.RemoveFromSuperview();
				_rightTextView.Dispose();
			}
			_rightTextView = null;

			if (_slider != null)
			{
				_slider.RemoveFromSuperview();
				_slider.InvokeOnMainThread(() => _slider.ValueChanged -= SliderValueChanged);
				_slider.Dispose();
			}
			_slider = null;
		}

		void AddContents(UIView contentView)
		{
			var frame = contentView.Frame;
			var cacheSliderView = new CacheSliderView(frame, _slider, _textView, _rightTextView, SliderValueChanged);
			contentView.Add(cacheSliderView);
		}

        protected override void Create(UITableViewCell cell, NavigationBarItem item)
        {
            CreateSlider(cell.ContentView.Frame.Size, item);
			AddContents(cell.ContentView);
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

			float sliderHorizontalOffset = 10;
			float sliderVerticalOffset = 5;

			_rightTextView.Frame = new RectangleF(
				new PointF(cell.ContentView.Frame.Width - _rightTextView.Frame.Width - _padding.Right, 
				cell.ContentView.Frame.Height / 2 - _rightTextView.Frame.Height / 2), 
				_rightTextView.Frame.Size);

			_slider.Frame = new RectangleF(sliderHorizontalOffset + _padding.Left,  
				_textView.Frame.Bottom + _padding.Bottom + sliderVerticalOffset, 
				cell.ContentView.Frame.Width - _padding.Right - _padding.Left - sliderHorizontalOffset * 2, 
				_slider.Frame.Height);

			AddContents(cell.ContentView);
        }

        private void CreateSlider(SizeF viewSize, NavigationBarItem item)
        {
            CreateTextView(viewSize, item);

			_rightTextView = ItExpertHelper.GetTextView(ItExpertHelper.GetAttributedString(ApplicationWorker.Settings.GetDbLimitSizeInMb().ToString() + " Мб", UIFont.SystemFontOfSize(12), UIColor.White),
                viewSize.Width, new PointF());

            _rightTextView.Frame = new RectangleF(new PointF(viewSize.Width - _rightTextView.Frame.Width - _padding.Right, viewSize.Height / 2 - _rightTextView.Frame.Height / 2), 
                _rightTextView.Frame.Size);
            _rightTextView.BackgroundColor = UIColor.Clear;

            float sliderHorizontalOffset = 10;
            float sliderVerticalOffset = 5;

            _slider = new UISlider();

            _slider.Frame = new RectangleF(sliderHorizontalOffset + _padding.Left,  _textView.Frame.Bottom + _padding.Bottom + sliderVerticalOffset, 
                viewSize.Width - _padding.Right - _padding.Left - sliderHorizontalOffset * 2, _slider.Frame.Height);

			_slider.MinValue = 1;
			_slider.MaxValue = 8;
            _slider.Value = (int)_item.GetValue();
			_slider.Continuous = true;
        }

		void SliderValueChanged(object sender, EventArgs e)
		{
			var value = Convert.ToInt32(_slider.Value);
			ApplicationWorker.Settings.SetDbLimitSize(value);
			var dbLimit = ApplicationWorker.Settings.GetDbLimitSizeInMb();
			var dbLimitString = ItExpertHelper.GetAttributedString(dbLimit + " Мб", UIFont.SystemFontOfSize(12), UIColor.White);
			_rightTextView.AttributedText = dbLimitString;
			_item.SetValue(value);
		}

        private UITextView _rightTextView;
        private UISlider _slider;
    }

	public class CacheSliderView : UIView, ICleanupObject
	{
		private UISlider _slider;
		private UITextView _rightTextView;
		private UITextView _textView;
		private EventHandler _sliderValueChanged;

		public CacheSliderView(RectangleF frame, UISlider slider, UITextView textView, UITextView rightTextView,
			EventHandler sliderValueChanged): base(frame)
		{
			_slider = slider;
			_textView = textView;
			_rightTextView = rightTextView;
			_sliderValueChanged = sliderValueChanged;
			_slider.ValueChanged += _sliderValueChanged;
			Add(_textView);
			Add(_rightTextView);
			Add(_slider);
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
			_rightTextView = null;
			_sliderValueChanged = null;
		}
	}
}

