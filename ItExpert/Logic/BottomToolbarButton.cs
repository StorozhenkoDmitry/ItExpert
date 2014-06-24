using System;
using MonoTouch.UIKit;
using System.Drawing;
using System.Collections.Generic;
using MonoTouch.Foundation;
using System.Threading;

namespace ItExpert
{
    public class BottomToolbarButton: UIView
    {
		public event EventHandler<EventArgs> ButtonClick;

        public BottomToolbarButton(RectangleF frame, UIImage image, string text)
        {
            UserInteractionEnabled = true;
			BackgroundColor = UIColor.Black;
            Frame = frame;

            _imageView = new UIImageView(new RectangleF(frame.Width / 2 - image.Size.Width / 2, 0, image.Size.Width, image.Size.Height));

            _imageView.Image = image;

            _label = new UILabel();
            _label.Text = text;
			_label.TextColor = UIColor.FromRGB(160, 160, 160);
            _label.Font = UIFont.BoldSystemFontOfSize(10);

            _label.SizeToFit();

            _label.Frame = new RectangleF(frame.Width / 2 - _label.Frame.Width / 2, _imageView.Frame.Bottom, _label.Frame.Width, _label.Frame.Height);

            Add(_imageView);
            Add(_label);

			_tapGestureRecognizer = new UITapGestureRecognizer (OnButtonClick);

            AddGestureRecognizer(_tapGestureRecognizer);
        }

		public void OnButtonClick()
		{
			var handler = Interlocked.CompareExchange(ref ButtonClick, null, null);
			if (handler != null)
			{
				handler(this, new EventArgs());
			}
		}

		public void SetState(bool isActive)
		{
			if (isActive)
			{
				_label.TextColor = UIColor.Black;
				BackgroundColor = UIColor.DarkGray;
			}
			else
			{
				_label.TextColor = UIColor.FromRGB(160, 160, 160);
				BackgroundColor = UIColor.Black;
			}
		}

        private UIImageView _imageView;
        private UILabel _label;
        private UIGestureRecognizer _tapGestureRecognizer;
    }
}

