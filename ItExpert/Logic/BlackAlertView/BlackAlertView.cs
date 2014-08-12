using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreGraphics;

namespace ItExpert
{
    public class BlackAlertView: UIViewController
    {
        public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
        {
            base.DidRotate(fromInterfaceOrientation);

            if (_alertView != null)
            {
                _backgroundView.Frame = ItExpertHelper.GetRealScreenSize();

                _alertView.Frame = new RectangleF(_backgroundView.Frame.Width / 2 - _contentViewWidth / 2, 0, _contentViewWidth, 0);
                _alertView.CorrectHeight();
            }
        }

        public BlackAlertView(string title, string message, string cancelButton, string confirmButton = null) 
        {
            AddAlertView();

            _alertView.SetViewText(title, message);
            _alertView.SetButtons(cancelButton, confirmButton);
        }

        public BlackAlertView(string title, string cancelButton, string[] radioButtons, string confirmButton = null)
        {
            AddAlertView();

            _alertView.SetViewText(title);
            _alertView.SetRadioButtons(radioButtons);
            _alertView.SetButtons(cancelButton, confirmButton);
        }


		public void SetRadionButtonActive(int index)
		{
			_alertView.SetRadioButtonActive(index);
		}

        public event EventHandler<BlackAlertViewButtonEventArgs> ButtonPushed;

        public void Show()
        {
            _alertWindow = new UIWindow(UIScreen.MainScreen.Bounds);

            _oldKeyWindow = UIApplication.SharedApplication.KeyWindow;

            _alertWindow.WindowLevel = UIWindowLevel.Alert;
            _alertWindow.RootViewController = this;
            _alertWindow.MakeKeyAndVisible();
        }

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (_alertView != null)
			{
				_alertView.ButtonPushed -= OnButtonPushed;
				_alertView.RemoveFromSuperview();
				_alertView.Dispose();
			}
			_alertView = null;

			if (_backgroundView != null)
			{
				_backgroundView.RemoveFromSuperview();
				_backgroundView.Dispose();
			}
			_backgroundView = null;

			ButtonPushed = null;
		}

        private void AddAlertView()
        {
            View.Frame = ItExpertHelper.GetRealScreenSize();

            View.BackgroundColor = UIColor.Clear;

            _backgroundView = new UIView(View.Frame);

            _backgroundView.BackgroundColor = UIColor.FromRGBA(0, 0, 0, 80);

            _contentViewWidth = 270;

            _alertView = new AlertView(new RectangleF(View.Frame.Width / 2 - _contentViewWidth / 2, 0, _contentViewWidth, 0));

            _alertView.ButtonPushed += OnButtonPushed;

            _backgroundView.Add(_alertView);

            View.Add(_backgroundView);
        }

        private void OnButtonPushed(object sender, BlackAlertViewButtonEventArgs e)
        {
            if (_alertWindow != null)
            {
                _alertWindow.ResignKeyWindow();
                _alertWindow.Alpha = 0;

                if (_oldKeyWindow != null)
                {
                    _oldKeyWindow.MakeKeyWindow();
                }

                _alertWindow.Dispose();
				if (_alertView != null)
				{
					_alertView.ButtonPushed -= OnButtonPushed;
					_alertView.RemoveFromSuperview();
					_alertView.Dispose();
				}
				_alertView = null;

				if (_backgroundView != null)
				{
					_backgroundView.RemoveFromSuperview();
					_backgroundView.Dispose();
				}
				_backgroundView = null;
            }

            if (ButtonPushed != null)
            {
                ButtonPushed(sender, e);
            }
        }

        private float _contentViewWidth;

        private UIView _backgroundView;
        private AlertView _alertView;
        private UIWindow _alertWindow;
        private UIWindow _oldKeyWindow;
    }
}

