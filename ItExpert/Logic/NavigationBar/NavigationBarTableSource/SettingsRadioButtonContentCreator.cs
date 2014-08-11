using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;
using System.Collections.Generic;

namespace ItExpert
{
    public class SettingsRadioButtonContentCreator : BaseNavigationBarContentCreator
    {
        public override float GetContentHeight(UIView cellContentView, NavigationBarItem item)
        {
            return _height;
        }

        protected override void Create(UITableViewCell cell, NavigationBarItem item)
        {
            CreateButton(cell.ContentView.Frame.Size, item);

            
			AddContents(cell.ContentView);
        }

        protected override void Update(UITableViewCell cell, NavigationBarItem item)
        {
            if (_button == null)
            {
                CreateButton(cell.ContentView.Frame.Size, item);
            }
            else
            {
                _textView.Dispose();
                _textView = null;

                CreateTextView(cell.ContentView.Frame.Size, item);
            }

			if (_button.ImageView != null && _button.ImageView.Image != null)
            {
                _button.ImageView.Image.Dispose();
                _button.ImageView.Image = null;
            }

            var image = new UIImage(GetButtonImageData((bool)item.GetValue()), 2);

            _button.SetImage(image, UIControlState.Normal);

            _button.Frame = new RectangleF(cell.ContentView.Frame.Width - image.Size.Width - _padding.Right, 
                cell.ContentView.Frame.Height / 2 - image.Size.Height / 2, image.Size.Width, image.Size.Height);

			AddContents(cell.ContentView);
        }

		public override void Dispose()
		{
			base.Dispose();
			if (_textView != null)
			{
				_textView.Dispose();
			}
			_textView = null;
			if (_button != null)
			{
				if (_button.ImageView != null && _button.ImageView.Image != null)
				{
					_button.ImageView.Image.Dispose();
					_button.ImageView.Image = null;
				}
				_button.TouchUpInside -= ButtonTouchUpInside;
				_button.Dispose();
			}
			_button = null;
		}

		void AddContents(UIView contentView)
		{
			var frame = contentView.Frame;
			var settingsButtonView = new SettingsRadioButtonView(frame, _button, _textView, ButtonTouchUpInside);
			contentView.Add(settingsButtonView);
		}

        private void CreateButton(SizeF viewSize, NavigationBarItem item)
        {
            CreateTextView(viewSize, item);

            _button = new UIButton();

            var image = new UIImage(GetButtonImageData((bool)item.GetValue()), 2);

            _button.SetImage(image, UIControlState.Normal);
            _button.AdjustsImageWhenHighlighted = false;

            _button.Frame = new RectangleF(viewSize.Width - image.Size.Width - _padding.Right, 
                viewSize.Height / 2 - image.Size.Height / 2, image.Size.Width, image.Size.Height);
				
        }

		void ButtonTouchUpInside(object sender, EventArgs e)
		{
			bool invertValue = !(bool)_item.GetValue();

			_button.ImageView.Image.Dispose();
			_button.ImageView.Image = null;

			_button.SetImage(new UIImage(GetButtonImageData(invertValue), 2), UIControlState.Normal);

			_item.SetValue(invertValue);
		}

        private NSData GetButtonImageData(bool isActive)
        {
            if (isActive)
            {
                return NSData.FromFile("ReadedButton.png");
            }
            else
            {
                return NSData.FromFile("NotReadedButton.png");
            }
        }

        private UIButton _button;
    }

	public class SettingsRadioButtonView : UIView, ICleanupObject
	{
		private UIButton _button;
		private UITextView _textView;
		private	EventHandler _buttonTouchUpInside;

		public SettingsRadioButtonView(RectangleF frame, UIButton button, UITextView textView,
			EventHandler buttonTouchUpInside): base(frame)
		{
			_button = button;
			_textView = textView;
			_buttonTouchUpInside = buttonTouchUpInside;
			_button.TouchUpInside += _buttonTouchUpInside;
			Add(_textView);
			Add(_button);
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
			if (_button != null)
			{
				_button.TouchUpInside -= _buttonTouchUpInside;
			}
			_button = null;
			_buttonTouchUpInside = null;
		}
	}
}

