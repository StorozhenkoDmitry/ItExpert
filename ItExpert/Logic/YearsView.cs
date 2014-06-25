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

            for (int i = 0; i < _buttons.Count; i++)
            {
                _buttons[i].Frame = new RectangleF(buttonWidth * i, 0, buttonWidth, Frame.Height);

                _buttons[i].SetTitle(_buttons[i].Title(UIControlState.Normal), UIControlState.Normal);
                _buttons[i].SetTitleColor(UIColor.Black, UIControlState.Normal);

                _scrollView.Add(buttons[i]);

                totalWidth += buttonWidth;
            }

            _scrollView.ContentSize = new SizeF(totalWidth, Frame.Height);
        }

        private UIScrollView _scrollView;
        private List<UIButton> _buttons;
    }
}

