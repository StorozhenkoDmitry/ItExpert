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

        protected override void Create(UITableViewCell cell, NavigationBarItem item)
        {
            CreateSearchPanel(cell.ContentView.Frame.Size);

            cell.ContentView.Add(_searchTextField);
            cell.ContentView.Add(_searchButton);
        }

        protected override void Update(UITableViewCell cell, NavigationBarItem item)
        {
            if (_searchButton == null || _searchTextField == null)
            {
                CreateSearchPanel(cell.ContentView.Frame.Size);
            }

            cell.ContentView.Add(_searchTextField);
            cell.ContentView.Add(_searchButton);
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

            _searchTextField.ShouldReturn += (sender) =>
            {
                sender.ResignFirstResponder();

                return true;
            };

            _searchButton.TouchUpInside += (sender, e) => 
            {
                _item.SetValue(_searchTextField.Text);
            };
        }

        private UITextField _searchTextField;
        private UIButton _searchButton;
    }
}

