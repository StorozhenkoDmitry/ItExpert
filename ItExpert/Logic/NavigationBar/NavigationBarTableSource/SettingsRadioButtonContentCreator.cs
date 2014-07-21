using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;

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

            cell.ContentView.Add(_textView);
            cell.ContentView.Add(_button);
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

            if (_button.ImageView.Image != null)
            {
                _button.ImageView.Image.Dispose();
                _button.ImageView.Image = null;
            }

            var image = new UIImage(GetButtonImageData((bool)item.GetValue()), 2);

            _button.SetImage(image, UIControlState.Normal);

            _button.Frame = new RectangleF(cell.ContentView.Frame.Width - image.Size.Width - _padding.Right, 
                cell.ContentView.Frame.Height / 2 - image.Size.Height / 2, image.Size.Width, image.Size.Height);

            cell.ContentView.Add(_textView);
            cell.ContentView.Add(_button);
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

            _button.TouchUpInside += (sender, e) => 
            {
                bool invertValue = !(bool)_item.GetValue();

                _button.ImageView.Image.Dispose();
                _button.ImageView.Image = null;

                _button.SetImage(new UIImage(GetButtonImageData(invertValue), 2), UIControlState.Normal);

                _item.SetValue(invertValue);
            };
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
}

