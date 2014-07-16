using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;

namespace ItExpert
{
    public class SettingsRadioButtonContentCreator : BaseSettingsItemContentCreator
    {
        protected override void Create(UITableViewCell cell, SettingsItem item)
        {
            CreateButton(cell.ContentView.Frame.Size, item);

            cell.ContentView.Add(_button);
        }

        protected override void Update(UITableViewCell cell, SettingsItem item)
        {
            if (_button == null)
            {
                CreateButton(cell.ContentView.Frame.Size, item);
            }

            var image = new UIImage(GetButtonImageData((bool)item.GetValue()), 2);

            _button.SetImage(image, UIControlState.Normal);

            cell.ContentView.Add(_button);
        }

        private void CreateButton(SizeF viewSize, SettingsItem item)
        {
            _button = new UIButton();

            var image = new UIImage(GetButtonImageData((bool)item.GetValue()), 2);

            _button.SetImage(image, UIControlState.Normal);
            _button.AdjustsImageWhenHighlighted = false;

            _button.Frame = new RectangleF(viewSize.Width - image.Size.Width - _padding.Right, 
                viewSize.Height / 2 - image.Size.Height / 2, image.Size.Width, image.Size.Height);

            _button.TouchUpInside += (sender, e) => 
            {
                bool invertValue = !(bool)_item.GetValue();

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

