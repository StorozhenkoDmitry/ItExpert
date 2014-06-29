using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MonoTouch.UIKit;
using ItExpert.Model;

namespace ItExpert
{
    public class YearsView: UIView
    {
        public YearsView(RectangleF frame)
        {
            Frame = frame;

            UserInteractionEnabled = true;

            _scrollView = new UIScrollView(Bounds);
            _scrollView.UserInteractionEnabled = true;
            _scrollView.ScrollEnabled = true;
            _scrollView.UserInteractionEnabled = true;
            _scrollView.Bounces = false;

            Add(_scrollView);
        }

        public void AddButtons(List<UIButton> buttons)
        {
            ItExpertHelper.RemoveSubviews(_scrollView);

            if (buttons.Count == 0)
            {
                return;
            }

            if (_buttons == null)
            {
                _buttons = new List<UIButton>();
            }

            _buttons.Clear();
            _buttons.AddRange(buttons);

            float buttonWidth = 60;
            float totalWidth = 0;

            Action removeButtonsHighliting = () =>
            {
                foreach (var button in _buttons)
                {
                    button.BackgroundColor = UIColor.Black;
                }
            };

            for (int i = 0; i < _buttons.Count; i++)
            {
                var button = _buttons[i];

                button.Frame = new RectangleF(buttonWidth * i, 0, buttonWidth, Frame.Height);

                button.SetTitle(button.Title(UIControlState.Normal), UIControlState.Normal);
                button.SetTitleColor(UIColor.White, UIControlState.Normal);
                button.BackgroundColor = UIColor.Black;

                if (i == 0)
                {
                    button.BackgroundColor = UIColor.FromRGB(160, 160, 160);
                }

                button.TouchUpInside += (sender, e) =>
                {
                    removeButtonsHighliting();

                    (sender as UIButton).BackgroundColor = UIColor.FromRGB(160, 160, 160);
                };

                _scrollView.Add(buttons[i]);

                totalWidth += buttonWidth;
            }

            _scrollView.ContentSize = new SizeF(totalWidth, Frame.Height);
        }

        private UIScrollView _scrollView;
        private List<UIButton> _buttons;
    }
}

