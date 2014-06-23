using System;
using MonoTouch.UIKit;
using System.Drawing;
using System.Collections.Generic;
using MonoTouch.Foundation;

namespace ItExpert
{
    public class BottomToolbarButton: UIView
    {
        public BottomToolbarButton(RectangleF frame, UIImage image, string text, NSAction tapAction)
        {
            UserInteractionEnabled = true;

            Frame = frame;

            _imageView = new UIImageView(new RectangleF(frame.Width / 2 - image.Size.Width / 2, 0, image.Size.Width, image.Size.Height));

            _imageView.Image = image;

            _label = new UILabel();
            _label.Text = text;
            _label.TextColor = UIColor.FromRGB(140, 140, 140);
            _label.Font = UIFont.BoldSystemFontOfSize(10);

            _label.SizeToFit();

            _label.Frame = new RectangleF(frame.Width / 2 - _label.Frame.Width / 2, _imageView.Frame.Bottom, _label.Frame.Width, _label.Frame.Height);

            Add(_imageView);
            Add(_label);

            _tapGestureRecognizer = new UITapGestureRecognizer(tapAction);

            AddGestureRecognizer(_tapGestureRecognizer);
        }

        public UIColor Forecolor
        {
            get
            {
                return _label.TextColor;
            }
            set
            {
                _label.TextColor = value;
            }
        }

        public UIFont Font
        {
            get
            {
                return _label.Font;
            }
            set
            {
                _label.Font = value;
            }
        }

        private UIImageView _imageView;
        private UILabel _label;
        private UIGestureRecognizer _tapGestureRecognizer;
    }
}

