using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace ItExpert
{
    public class RadioButton : UIView
    {
        public RadioButton(RectangleF frame, string title, bool isCheked, int index, Action<int> buttonChecked)
        {
            Frame = frame;

            Index = index;

            _titleTextView = ItExpertHelper.GetTextView(ItExpertHelper.GetAttributedString(title, UIFont.SystemFontOfSize(ApplicationWorker.Settings.HeaderSize),
                UIColor.White), Frame.Width, new PointF());

            _titleTextView.Frame = new RectangleF(0, Bounds.Height / 2 - _titleTextView.Frame.Height / 2, _titleTextView.Frame.Width, _titleTextView.Frame.Height);
            _titleTextView.BackgroundColor = UIColor.Clear;

            var image = GetImage(isCheked);

            _chekedImageView = new UIImageView(new RectangleF(Frame.Width - image.Size.Width, Bounds.Height / 2 - image.Size.Height / 2, image.Size.Width, image.Size.Height));

            _chekedImageView.Image = image;

            Add(_titleTextView);
            Add(_chekedImageView);

            UITapGestureRecognizer tap = new UITapGestureRecognizer(() => buttonChecked(index));

            AddGestureRecognizer(tap);
        }

        public int Index
        {
            get;
            set;
        }

        public void ChangeState(bool isChecked)
        {
            _chekedImageView.Image = GetImage(isChecked);
        }

        private UIImage GetImage(bool isCheked)
        {
            if (isCheked)
            {
                if (_checkedImage == null)
                {
                    _checkedImage = new UIImage(NSData.FromFile("RadioButtonChecked.png"), 4.0f);
                }

                return _checkedImage;
            }
            else 
            {
                if (_uncheckedImage == null)
                {
                    _uncheckedImage = new UIImage(NSData.FromFile("RadioButtonUnchecked.png"), 4.0f);
                }

                return _uncheckedImage; 
            }
        }

        private UIImage _checkedImage;
        private UIImage _uncheckedImage;

        private UITextView _titleTextView;
        private UIImageView _chekedImageView;

    }
}

