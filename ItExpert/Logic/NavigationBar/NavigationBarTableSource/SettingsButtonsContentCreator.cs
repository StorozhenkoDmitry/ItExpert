using System;
using MonoTouch.UIKit;
using System.Collections.Generic;
using System.Drawing;

namespace ItExpert
{
    public class SettingsButtonsContentCreator: BaseNavigationBarContentCreator
    {
        public SettingsButtonsContentCreator()
        {
            _height = 44;
        }

		public override void Dispose()
		{
			base.Dispose();
			if (_buttons != null)
			{
				foreach (var button in _buttons)
				{
					button.TouchDown -= ButtonTouchDown;

					button.TouchUpInside -= ButtonTouchUpInside;

					button.TouchUpOutside -= ButtonTouchUpOutside;
					button.Dispose();
				}

				_buttons.Clear();
			}
			_buttons = null;
		}

        public override float GetContentHeight(UIView cellContentView, NavigationBarItem item)
        {
            return _height;
        }

        protected override void Create(UITableViewCell cell, NavigationBarItem item)
        {
            _buttons = new List<UIButton>();

            CreateButtons(item, cell.ContentView.Frame.Size);

            AddButtons(cell.ContentView);
        }

        protected override void Update(UITableViewCell cell, NavigationBarItem item)
        {
            if (_buttons == null)
            {
                _buttons = new List<UIButton>();
            }
            else
            {
                foreach (var button in _buttons)
                {
					button.TouchDown -= ButtonTouchDown;

					button.TouchUpInside -= ButtonTouchUpInside;

					button.TouchUpOutside -= ButtonTouchUpOutside;
                    button.Dispose();
                }

                _buttons.Clear();
            }

            CreateButtons(item, cell.ContentView.Frame.Size);

            AddButtons(cell.ContentView);
        }

        private void CreateButtons(NavigationBarItem item, SizeF viewSize)
        {
            for (int i = 0; i < item.Buttons.Length; i++)
            {
                UIButton button = new UIButton();

                button.Frame = new RectangleF(viewSize.Width / item.Buttons.Length * i + _padding.Left, _padding.Top, 
                    viewSize.Width / item.Buttons.Length - _padding.Left - _padding.Right, 
                    _height - _padding.Top - _padding.Bottom);

                button.SetTitle(item.Buttons[i], UIControlState.Normal);
                button.SetTitleColor(ItExpertHelper.ButtonTextColor, UIControlState.Normal);
                button.TitleLabel.TextAlignment = UITextAlignment.Center;
                button.BackgroundColor = ItExpertHelper.ButtonColor;
                button.Tag = i;

                _buttons.Add(button);
            }
        }

		void ButtonTouchDown(object sender, EventArgs e)
		{
			var senderButton = (sender as UIButton);

			senderButton.BackgroundColor = ItExpertHelper.ButtonPushedColor;
		}

		void ButtonTouchUpInside(object sender, EventArgs e)
		{
			var senderButton = (sender as UIButton);

			senderButton.BackgroundColor = ItExpertHelper.ButtonColor;

			_item.ButtonPushed(senderButton.Tag);
		}

		void ButtonTouchUpOutside(object sender, EventArgs e)
		{
			var senderButton = (sender as UIButton);

			senderButton.BackgroundColor = ItExpertHelper.ButtonColor;
		}

        private void AddButtons(UIView contentView)
        {
			var frame = contentView.Frame;
			var settingsButtonView = new SettingsButtonView(frame, _buttons, ButtonTouchDown, ButtonTouchUpInside, ButtonTouchUpOutside);
			contentView.Add(settingsButtonView);
        }

        private List<UIButton> _buttons;
    }

	public class SettingsButtonView : UIView, ICleanupObject
	{
		private List<UIButton> _buttons;
		private	EventHandler _buttonTouchDown; 
		private	EventHandler _buttonTouchUpInside;
		private	EventHandler _buttonTouchUpOutside;

		public SettingsButtonView(RectangleF frame, List<UIButton> buttons, EventHandler buttonTouchDown,
			EventHandler buttonTouchUpInside, EventHandler buttonTouchUpOutside): base(frame)
		{
			_buttons = buttons;
			_buttonTouchDown = buttonTouchDown; 
			_buttonTouchUpInside = buttonTouchUpInside;
			_buttonTouchUpOutside = buttonTouchUpOutside;
			foreach (var button in _buttons)
			{
				button.TouchDown += _buttonTouchDown;
				button.TouchUpInside += _buttonTouchUpInside;
				button.TouchUpOutside += _buttonTouchUpOutside;
				Add(button);
			}
		}

		public void CleanUp()
		{
			foreach (var view in Subviews)
			{
				view.RemoveFromSuperview();
			}
			if (_buttons != null)
			{
				foreach (var button in _buttons)
				{
					button.TouchDown -= _buttonTouchDown;
					button.TouchUpInside -= _buttonTouchUpInside;
					button.TouchUpOutside -= _buttonTouchUpOutside;
					button.Dispose();
				}

				_buttons.Clear();
			}
			_buttons = null;
			_buttonTouchDown = null; 
			_buttonTouchUpInside = null;
			_buttonTouchUpOutside = null;
		}
	}
}

