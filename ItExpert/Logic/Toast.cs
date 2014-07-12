using System;
using MonoTouch.UIKit;
using System.Timers;
using System.Drawing;

namespace ItExpert
{
    public class Toast: UIView
    {
        public static Toast MakeText(UIViewController view, string text, int duration)
        {
            Toast toast = new Toast(view, text, duration);

            return toast;
        }

        public Toast(UIViewController view, string text, int duration)
        {
            BackgroundColor = UIColor.Black;

            _padding = new UIEdgeInsets(3, 3, 3, 3);

            _parentView = view;
            _location = new PointF(0, view.View.Frame.Height - view.View.Frame.Height / 4);
            _locationSet = false;

            _textView = ItExpertHelper.GetTextView(ItExpertHelper.GetAttributedString(text, UIFont.SystemFontOfSize(14), UIColor.White), 
                view.View.Frame.Width / 3 * 2, new PointF(_padding.Left, _padding.Top));

            _textView.BackgroundColor = UIColor.Clear;

            Add(_textView);
            _disappearTimer = new Timer(duration);

            _disappearTimer.Elapsed += OnDisappearTimerTick;
        }

        public PointF Location
        {
            get
            {
                return _location;
            }
            set
            {
                _locationSet = true;
                _location = value;
            }
        }

        public void Show()
        {
            var size = new SizeF(_textView.Frame.Width + _padding.Left + _padding.Right, _textView.Frame.Height + _padding.Top + _padding.Bottom);

            if (_locationSet)
            {
                Frame = new RectangleF(_location, size);
            }
            else
            {
                Frame = new RectangleF(new PointF(_parentView.View.Frame.Width / 2 - _textView.Frame.Width / 2, _location.Y), size);
            }

            _disappearTimer.Start();

            _parentView.Add(this);
        }

        private void OnDisappearTimerTick(object sender, ElapsedEventArgs e)
        {
            _disappearTimer.Stop();
            _disappearTimer.Dispose();
            _disappearTimer = null;

			InvokeOnMainThread (() =>
			{
				RemoveFromSuperview();
				_textView.Dispose();
				_textView = null;
				Dispose();
			});
        }

        private UIEdgeInsets _padding;
        private Timer _disappearTimer;
        private UIViewController _parentView;
        private UITextView _textView;
        private PointF _location;
        private bool _locationSet;
    }
}

