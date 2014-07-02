using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreGraphics;

namespace ItExpert
{
    public class BlackAlertView: UIViewController
    {
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

        public event EventHandler<BlackAlertViewButtonEventArgs> ButtonPushed;

        public void Show()
        {
            var appDelegate = UIApplication.SharedApplication.Delegate;

            var window = appDelegate.Window;

            window.Add(View);
        }

        private void AddAlertView()
        {
            var appDelegate = UIApplication.SharedApplication.Delegate;

            var window = appDelegate.Window;

            View.Frame = window.Frame;

            View.BackgroundColor = UIColor.Clear;

            _backgroundView = new UIView(View.Frame);

            _backgroundView.BackgroundColor = UIColor.FromRGBA(0, 0, 0, 80);

            _contentViewWidth = View.Frame.Width - 50;

            _alertView = new AlertView(new RectangleF(View.Frame.Width / 2 - _contentViewWidth / 2, 0, _contentViewWidth, 0));

            _alertView.ButtonPushed += OnButtonPushed;

            _backgroundView.Add(_alertView);

            View.Add(_backgroundView);
        }

        private void OnButtonPushed(object sender, BlackAlertViewButtonEventArgs e)
        {
            View.RemoveFromSuperview();

            if (ButtonPushed != null)
            {
                ButtonPushed(sender, e);
            }
        }

        private float _contentViewWidth;

        private UIView _backgroundView;
        private AlertView _alertView;
    }
}

