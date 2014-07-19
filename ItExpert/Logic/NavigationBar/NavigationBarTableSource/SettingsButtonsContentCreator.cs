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
                button.SetTitleColor(UIColor.Black, UIControlState.Normal);
                button.TitleLabel.TextAlignment = UITextAlignment.Center;
                button.BackgroundColor = UIColor.White;
                button.Tag = i;

                button.TouchDown += (sender, e) => 
                {
                    var senderButton = (sender as UIButton);

                    senderButton.BackgroundColor = UIColor.FromRGB(160, 160, 160);
                };

                button.TouchUpInside += (sender, e) => 
                {
                    var senderButton = (sender as UIButton);

                    senderButton.BackgroundColor = UIColor.White;

                    _item.ButtonPushed(senderButton.Tag);
                };

                button.TouchUpOutside += (sender, e) => 
                {
                    var senderButton = (sender as UIButton);

                    senderButton.BackgroundColor = UIColor.White;
                };

                _buttons.Add(button);
            }
        }

        private void AddButtons(UIView contentView)
        {
            foreach (var button in _buttons)
            {
                contentView.Add(button);
            }
        }

        private List<UIButton> _buttons;
    }
}

