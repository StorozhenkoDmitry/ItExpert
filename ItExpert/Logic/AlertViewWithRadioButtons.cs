using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreGraphics;

namespace ItExpert
{
    public class AlertViewWithRadioButtons: UIViewController
    {
        public AlertViewWithRadioButtons(string title, string message, string cancelButton, string confirmButton = null) 
        {
            var appDelegate = UIApplication.SharedApplication.Delegate;

            var window = appDelegate.Window;

            View.Frame = window.Frame;

            _backgroundView = new UIView(View.Frame);
           
            View.BackgroundColor = UIColor.Clear;

            _backgroundView.BackgroundColor = UIColor.FromRGBA(0, 0, 0, 80);

            var contentViewWidth = View.Frame.Width - 50;

            var padding = new UIEdgeInsets(8, 15, 8, 15);

            var topOffset = 15;

            var buttonHeight = 50;

            var titleHeight = ItExpertHelper.GetTextHeight(UIFont.SystemFontOfSize(ApplicationWorker.Settings.HeaderSize), title, 
                contentViewWidth - padding.Left - padding.Right);

            var textHeight = ItExpertHelper.GetTextHeight(UIFont.SystemFontOfSize(ApplicationWorker.Settings.TextSize), message, 
                contentViewWidth - padding.Left - padding.Right);

            var contentViewHeight = topOffset + titleHeight + topOffset + padding.Top + textHeight + padding.Bottom + buttonHeight;

            _alertView = new AlertView(new RectangleF(View.Frame.Width / 2 - contentViewWidth / 2, View.Frame.Height / 2 - contentViewHeight / 2, contentViewWidth, contentViewHeight));

            _alertView.SetViewText(title, message, topOffset, padding);
            _alertView.SetButtons(cancelButton, confirmButton, buttonHeight, (index) => OnButtonPushed(index));

            _backgroundView.Add(_alertView);

            View.Add(_backgroundView);
        }

        public event EventHandler<UIButtonEventArgs> ButtonPushed;

        public void Show()
        {
            var appDelegate = UIApplication.SharedApplication.Delegate;

            var window = appDelegate.Window;

            window.Add(View);
        }

        private void OnButtonPushed(int index)
        {
            View.RemoveFromSuperview();

            if (ButtonPushed != null)
            {
                ButtonPushed(this, new UIButtonEventArgs(index));
            }
        }

        private UIView _backgroundView;
        private AlertView _alertView;
    }
}

