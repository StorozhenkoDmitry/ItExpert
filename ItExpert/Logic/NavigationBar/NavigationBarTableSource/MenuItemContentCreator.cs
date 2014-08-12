using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace ItExpert
{
    public class MenuItemContentCreator : BaseNavigationBarContentCreator
    {
        public MenuItemContentCreator()
        {
            _height = 35;
        }

		public override void Dispose()
		{
			base.Dispose();
			if (_button != null)
			{
				_button.RemoveFromSuperview();
				_button.TouchDown -= ButtonTouchDown;
				_button.TouchUpInside -= ButtonTouchUpInside;
				_button.Dispose();
			}
			_button = null;
		}

		void AddContents(UIView contentView)
		{
			var frame = contentView.Frame;
			var menuItemView = new MenuItemView(frame, _button, ButtonTouchDown, ButtonTouchUpInside);
			contentView.Add(menuItemView);
		}

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

			AddContents(cell.ContentView);
        }

        private void CreateButton(SizeF viewSize, NavigationBarItem item)
        {
            _button = new UIButton(new RectangleF(0, 0, viewSize.Width, _height));

            _button.SetTitle(item.Title, UIControlState.Normal);
            _button.Font = UIFont.BoldSystemFontOfSize(16);
            _button.TitleEdgeInsets = _padding;
            _button.BackgroundColor = UIColor.Clear;
            _button.TitleLabel.TextColor = UIColor.White;
            _button.HorizontalAlignment = UIControlContentHorizontalAlignment.Left;

            
        }

		void ButtonTouchDown(object sender, EventArgs e)
		{
			var button = sender as UIButton;
			button.TitleLabel.TextColor = UIColor.FromRGB(160, 160, 160);
		}

		void ButtonTouchUpInside(object sender, EventArgs e)
		{
			var button = sender as UIButton;
			button.TitleLabel.TextColor = UIColor.White;
			_item.ButtonPushed(0);
		}

        private UIButton _button;
    }

	public class MenuItemView : UIView, ICleanupObject
	{
		private UIButton _button;
		private EventHandler _buttonTouchDown;
		private EventHandler _buttonTouchUpInside;

		public MenuItemView(RectangleF frame, UIButton button, EventHandler buttonTouchDown, 
			EventHandler buttonTouchUpInside): base(frame)
		{
			_button = button;
			_buttonTouchDown = buttonTouchDown;
			_buttonTouchUpInside = buttonTouchUpInside;
			_button.TouchDown += _buttonTouchDown;
			_button.TouchUpInside += _buttonTouchUpInside;

			Add(_button);
		}

		public void CleanUp()
		{
			foreach (var view in Subviews)
			{
				view.RemoveFromSuperview();
			}
			if (_button != null)
			{
				_button.TouchDown -= _buttonTouchDown;
				_button.TouchUpInside -= _buttonTouchUpInside;
			}
			_button = null;
			_buttonTouchDown = null;
			_buttonTouchUpInside = null;
		}
	}
}

