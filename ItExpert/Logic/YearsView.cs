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

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (_buttons != null)
			{
				foreach (var button in _buttons)
				{
					button.RemoveFromSuperview();
					button.TouchUpInside -= ButtonTouchUpInside;
					button.Dispose();
				}
				_buttons.Clear();
			}
			_buttons = null;
			if (_scrollView != null)
			{
				_scrollView.RemoveFromSuperview();
				_scrollView.Dispose();
			}
			_scrollView = null;
		}

        public override RectangleF Frame
        {
            get
            {
                return base.Frame;
            }
            set
            {
                base.Frame = value;

                if (_scrollView != null)
                {
                    _scrollView.Frame = Bounds;
                    _scrollView.ContentSize = new SizeF (GetButtonsWidth(), Frame.Height);
                }
            }
        }

        public float GetButtonsWidth()
        {
            if (_scrollView != null && _buttons != null)
            {
                float totalWidth = 0;

                foreach (var button in _buttons)
                {
                    totalWidth += button.Frame.Width;
                }

                return totalWidth;
            }

            return 0;
        }

        public void AddButtons(List<UIButton> buttons)
		{
			if (_buttons != null)
			{
				foreach (var button in _buttons)
				{
					button.RemoveFromSuperview();
					button.TouchUpInside -= ButtonTouchUpInside;
					button.Dispose();
				}
				_buttons.Clear();
			}

			if (buttons.Count == 0)
			{
				return;
			}

			if (_buttons == null)
			{
				_buttons = new List<UIButton> ();
			}

			_buttons.Clear ();
			_buttons.AddRange (buttons);

			float buttonWidth = 60;
			float totalWidth = 0;

			for (int i = 0; i < _buttons.Count; i++)
			{
				var button = _buttons [i];

				button.Frame = new RectangleF (buttonWidth * i, 0, buttonWidth, Frame.Height);

				button.SetTitle (button.Title (UIControlState.Normal), UIControlState.Normal);
				button.SetTitleColor (UIColor.White, UIControlState.Normal);
				button.BackgroundColor = UIColor.Black;

				if (i == 0)
				{
					button.BackgroundColor = UIColor.FromRGB (160, 160, 160);
				}

				button.TouchUpInside += ButtonTouchUpInside;

				_scrollView.Add (buttons [i]);

				totalWidth += buttonWidth;
			}

			_scrollView.ContentSize = new SizeF (totalWidth, Frame.Height);
			if (Frame.Width > totalWidth)
			{
				Frame = new RectangleF ((Frame.Width - totalWidth) / 2, Frame.Y, totalWidth, Frame.Height);
			}
		}

		void ButtonTouchUpInside(object sender, EventArgs e)
		{
			foreach (var button in _buttons)
			{
				button.BackgroundColor = UIColor.Black;
			}
			(sender as UIButton).BackgroundColor = UIColor.FromRGB (160, 160, 160);
		}

        private UIScrollView _scrollView;
        private List<UIButton> _buttons;
    }
}

