using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreGraphics;

namespace ItExpert
{
    public class AlertView: UIView
    {
        public AlertView(RectangleF frame)
            : base(frame)
        {
            ContentMode = UIViewContentMode.Redraw;

            BackgroundColor = UIColor.Clear;

            _buttonHeight = 50;
        }

        public void SetViewText(string title, string message, float topOffset, UIEdgeInsets padding)
        {
            _topOffset = topOffset;

            var textMaxWidth = Frame.Width - padding.Left - padding.Right;

            if (_headerTextView != null)
            {                    
                _headerTextView.RemoveFromSuperview();
            }

            var attributedString = ItExpertHelper.GetAttributedString(title, UIFont.SystemFontOfSize(ApplicationWorker.Settings.HeaderSize),
                UIColor.FromRGB(51, 181, 229));

            _headerTextView = ItExpertHelper.GetTextView(attributedString, textMaxWidth, new PointF());

            _headerTextView.Frame = new RectangleF(Frame.Width / 2 - _headerTextView.Frame.Width / 2, topOffset, _headerTextView.Frame.Width, _headerTextView.Frame.Height);
            _headerTextView.TextAlignment = UITextAlignment.Center;
            _headerTextView.BackgroundColor = UIColor.Clear;

            Add(_headerTextView);

            if (_messageTextView != null)
            {
                _messageTextView.RemoveFromSuperview();
            }

            attributedString = ItExpertHelper.GetAttributedString(message, UIFont.SystemFontOfSize(ApplicationWorker.Settings.TextSize), UIColor.White);

            _messageTextView = ItExpertHelper.GetTextView(attributedString, textMaxWidth, new PointF());

            _messageTextView.Frame = new RectangleF(Frame.Width / 2 - _messageTextView.Frame.Width / 2, _headerTextView.Frame.Bottom + _topOffset + padding.Top,
                _messageTextView.Frame.Width, _messageTextView.Frame.Height);
            _messageTextView.TextAlignment = UITextAlignment.Center;
            _messageTextView.BackgroundColor = UIColor.Clear;

            Add(_messageTextView);

            SetNeedsDisplay();
        }

        public void SetButtons(string cancelButtonTitle, string confirmButtonTitle, float buttonHeight, Action<int> buttonPushed)
        {
            _buttonHeight = buttonHeight;

            _isConfirmButtonExists = confirmButtonTitle != null;

            float upperLineY = Bounds.Height - _buttonHeight;

            var cancelButton = new UIButton(new RectangleF(0, upperLineY, _isConfirmButtonExists ? Frame.Width / 2 : Frame.Width, buttonHeight));

            cancelButton.SetTitle(cancelButtonTitle, UIControlState.Normal);
            cancelButton.SetTitleColor(UIColor.White, UIControlState.Normal);
            cancelButton.TouchUpInside += (sender, e) => buttonPushed(0);

            Add(cancelButton);

            UIButton confirmButton = null;

            if (_isConfirmButtonExists)
            {
                confirmButton = new UIButton(new RectangleF(Frame.Width / 2, upperLineY, Frame.Width / 2, buttonHeight));

                confirmButton.SetTitle(confirmButtonTitle, UIControlState.Normal);
                confirmButton.SetTitleColor(UIColor.White, UIControlState.Normal);
                confirmButton.TouchUpInside += (sender, e) => buttonPushed(1);

                Add(confirmButton);
            }

            SetNeedsDisplay();
        }

        public void SetRadioButtons(string[] radioButtons)
        {

        }

        public override void Draw(RectangleF area)
        {
            base.Draw(area);

            var context = UIGraphics.GetCurrentContext();

            UIBezierPath roundedRect = UIBezierPath.FromRoundedRect(Bounds, 8);

            context.SetFillColor(UIColor.FromRGB(40, 40, 40).CGColor);
            context.AddPath(roundedRect.CGPath);
            context.FillPath();

            if (_headerTextView != null)
            {
                context.SetLineWidth(2);

                context.MoveTo(0, _headerTextView.Frame.Bottom + _topOffset);
                context.AddLineToPoint(Bounds.Width, _headerTextView.Frame.Bottom + _topOffset);
                context.SetStrokeColor(UIColor.FromRGB(51, 181, 229).CGColor);
                context.StrokePath();
            }

            float upperLineY = Bounds.Height - _buttonHeight;

            context.SetLineWidth(0.5f);
            context.MoveTo(0, upperLineY);
            context.AddLineToPoint(Bounds.Width, upperLineY);
            context.SetStrokeColor(UIColor.FromRGB(72, 72, 72).CGColor);
            context.StrokePath();

            if (_isConfirmButtonExists)
            {
                context.MoveTo(Bounds.Width / 2, upperLineY);
                context.AddLineToPoint(Bounds.Width / 2, Bounds.Bottom);
                context.StrokePath();
            }
        }

        private bool _isConfirmButtonExists;
        private float _topOffset;
        private float _buttonHeight;

        private UITextView _headerTextView;
        private UITextView _messageTextView;
    }
}

