using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;

namespace ItExpert
{
    public class MenuSearchContentCreator : BaseNavigationBarContentCreator
    {
        public override float GetContentHeight(UIView cellContentView, NavigationBarItem item)
        {
            return _height;
        }

		public override void Dispose()
		{
			base.Dispose();
			if (_searchTextField != null)
			{
				_searchTextField.RemoveFromSuperview();
				_searchTextField.ShouldReturn -= TextFieldShouldReturn;
				_searchTextField.Dispose();
			}
			_searchTextField = null;

			if (_searchButton != null)
			{
				_searchButton.RemoveFromSuperview();
				if (_searchButton.ImageView != null && _searchButton.ImageView.Image != null)
				{
					_searchButton.ImageView.Image.Dispose();
					_searchButton.ImageView.Image = null;
				}
				_searchButton.TouchUpInside -= ButtonTouchUpInside;
				_searchButton.Dispose();
			}
			_searchButton = null;
		}

		void AddContents(UIView contentView)
		{
			var frame = contentView.Frame;
			var menuSearchView = new MenuSearchView(frame, _searchButton, _searchTextField, TextFieldShouldReturn, ButtonTouchUpInside);
			contentView.Add(menuSearchView);

		}

        protected override void Create(UITableViewCell cell, NavigationBarItem item)
        {
            CreateSearchPanel(cell.ContentView.Frame.Size);

			AddContents(cell.ContentView);
        }

        protected override void Update(UITableViewCell cell, NavigationBarItem item)
        {
            if (_searchButton == null || _searchTextField == null)
            {
                CreateSearchPanel(cell.ContentView.Frame.Size);
            }

			AddContents(cell.ContentView);
        }

        private void CreateSearchPanel(SizeF viewSize)
        {
            var image = new UIImage(NSData.FromFile("NavigationBar/Search.png"), 4);

            _searchButton = new UIButton();

            _searchButton.Frame = new RectangleF(viewSize.Width - image.Size.Width - _padding.Right, _height / 2 - image.Size.Height / 2,
                image.Size.Width, image.Size.Height);
            _searchButton.SetImage(image, UIControlState.Normal);

            _searchTextField = new UITextField();
            _searchTextField.Frame = new RectangleF(_padding.Left, _padding.Top, viewSize.Width - _padding.Left - _padding.Right - image.Size.Width,
                viewSize.Height - _padding.Top - _padding.Bottom);
            _searchTextField.BackgroundColor = UIColor.White;
        }

		bool TextFieldShouldReturn(UITextField sender)
		{
			sender.ResignFirstResponder();
			return true;
		}

		void ButtonTouchUpInside(object sender, EventArgs e)
		{
			_item.SetValue(_searchTextField.Text);
			_searchTextField.Text = string.Empty;
			_searchTextField.ResignFirstResponder();
		}

        private UITextField _searchTextField;
        private UIButton _searchButton;
    }

	public class MenuSearchView : UIView, ICleanupObject
	{
		private UITextField _searchTextField;
		private UIButton _searchButton;
		private EventHandler _buttonTouchUpInside;
		private UITextFieldCondition _textFieldShouldReturn;

		public MenuSearchView(RectangleF frame, UIButton searchButton, UITextField searchTextField, UITextFieldCondition textFieldShouldReturn, 
			EventHandler buttonTouchUpInside): base(frame)
		{
			_searchButton = searchButton;
			_searchTextField = searchTextField;
			_textFieldShouldReturn = textFieldShouldReturn;
			_buttonTouchUpInside = buttonTouchUpInside;
			_searchButton.TouchUpInside += _buttonTouchUpInside;
			_searchTextField.ShouldReturn += _textFieldShouldReturn;
			Add(_searchTextField);
			Add(_searchButton);
		}

		public void CleanUp()
		{
			foreach (var view in Subviews)
			{
				view.RemoveFromSuperview();
			}
			if (_searchButton != null)
			{
				_searchButton.TouchUpInside -= _buttonTouchUpInside;
			}
			_searchButton = null;
			if (_searchTextField != null)
			{
				_searchTextField.ShouldReturn -= _textFieldShouldReturn;
			}
			_searchTextField = null;
			_buttonTouchUpInside = null;
			_textFieldShouldReturn = null;
		}
	}
}

