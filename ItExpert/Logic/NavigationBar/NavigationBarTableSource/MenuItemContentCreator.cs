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

        public override float GetContentHeight(UIView cellContentView, NavigationBarItem item)
        {
            return _height;
        }

        protected override void Create(UITableViewCell cell, NavigationBarItem item)
        {
            //CreateMenuItem(cell.ContentView.Frame.Size, item);
            CreateButton(cell.ContentView.Frame.Size, item);

            cell.ContentView.Add(_button);

//            cell.ContentView.Add(_textView);
//            cell.ContentView.Add(_tappableView);
        }

        protected override void Update(UITableViewCell cell, NavigationBarItem item)
        {
//            if (_tappableView == null)
//            {
//                CreateMenuItem(cell.ContentView.Frame.Size, item);
//            }

            if (_button == null)
            {
                CreateButton(cell.ContentView.Frame.Size, item);
            }

            cell.ContentView.Add(_button);

//            cell.ContentView.Add(_textView);
//            cell.ContentView.Add(_tappableView);
        }

        private void CreateMenuItem(SizeF viewSize, NavigationBarItem item)
        {
            _textFont = UIFont.BoldSystemFontOfSize(16);

            CreateTextView(viewSize, item);

            _tappableView = new UIView(new RectangleF(new PointF(0, 0), viewSize));

            _tappableView.BackgroundColor = UIColor.Clear;
            _tappableView.AddGestureRecognizer(new UITapGestureRecognizer(() =>
            {
                _item.ButtonPushed(0);
            }));
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

            _button.TouchDown += (sender, e) => 
            {
                _button.TitleLabel.TextColor = UIColor.FromRGB(160, 160, 160);
            };

            _button.TouchUpInside += (sender, e) => 
            {
                _button.TitleLabel.TextColor = UIColor.White;
                _item.ButtonPushed(0);
            };
        }

        private UIView _tappableView;
        private UIButton _button;
    }
}

