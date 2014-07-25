using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreGraphics;

namespace ItExpert
{
    public class AlertView: UIView
    {
        #region nested types

        internal static class ComponentHeight
        {
            public static float TitleHeight;
            public static float MessageHeight;
            public static float RadioButtonsHeight;
            public static float BottomButtonsHeight;

            public static float VerticalOffset;
            public static UIEdgeInsets Padding;

            public static float GetTotalHeight()
            {
                float height = 0;

                if (TitleHeight > 0)
                {
                    height += VerticalOffset + TitleHeight + VerticalOffset;
                }

                if (MessageHeight > 0)
                {
                    height += Padding.Top + MessageHeight + Padding.Bottom;
                }

                if (RadioButtonsHeight > 0)
                {
                    height += Padding.Top + RadioButtonsHeight + Padding.Bottom;
                }

                if (BottomButtonsHeight > 0)
                {
                    height += BottomButtonsHeight;
                }

                return height;
            }

            public static void Reset()
            {
                TitleHeight = 0;
                MessageHeight = 0;
                RadioButtonsHeight = 0;
                BottomButtonsHeight = 0;
                VerticalOffset = 0;
                Padding = new UIEdgeInsets();
            }
        }

        #endregion

        #region public constructors

        public AlertView(RectangleF frame)
            : base(frame)
        {
            ComponentHeight.Reset();

            ContentMode = UIViewContentMode.Redraw;

            BackgroundColor = UIColor.Clear;

            ButtonHeight = 50;
            Padding = new UIEdgeInsets(8, 15, 8, 15);
            VerticalOffset = 15;
        }

        #endregion

        #region public events

        public event EventHandler<BlackAlertViewButtonEventArgs> ButtonPushed;

        #endregion

        #region public properties

        public UIEdgeInsets Padding
        {
            get
            {
                return ComponentHeight.Padding;
            }
            set
            {
                ComponentHeight.Padding = value;
            }
        }

        public float VerticalOffset
        {
            get
            {
                return ComponentHeight.VerticalOffset;
            }
            set
            {
                ComponentHeight.VerticalOffset = value;
            }
        }

        public float ButtonHeight
        {
            get
            {
                return ComponentHeight.BottomButtonsHeight;
            }
            set
            {
                ComponentHeight.BottomButtonsHeight = value;
            }
        }

        #endregion

        #region public methods

        public void SetViewText(string title, string message = null)
        {
            var textMaxWidth = Frame.Width - Padding.Left - Padding.Right;

            if (_titleTextView != null)
            {                    
                _titleTextView.RemoveFromSuperview();
            }

            var attributedString = ItExpertHelper.GetAttributedString(title, UIFont.SystemFontOfSize(ApplicationWorker.Settings.HeaderSize),
                UIColor.FromRGB(51, 181, 229));

            _titleTextView = ItExpertHelper.GetTextView(attributedString, textMaxWidth, new PointF());

            _titleTextView.Frame = new RectangleF(Frame.Width / 2 - _titleTextView.Frame.Width / 2, VerticalOffset, _titleTextView.Frame.Width, _titleTextView.Frame.Height);
            _titleTextView.TextAlignment = UITextAlignment.Center;
            _titleTextView.BackgroundColor = UIColor.Clear;

            ComponentHeight.TitleHeight = _titleTextView.Frame.Height;

            Add(_titleTextView);

            if (_messageTextView != null)
            {
                _messageTextView.RemoveFromSuperview();
            }

            if (message != null)
            {
                attributedString = ItExpertHelper.GetAttributedString(message, UIFont.SystemFontOfSize(ApplicationWorker.Settings.TextSize), UIColor.White);

                _messageTextView = ItExpertHelper.GetTextView(attributedString, textMaxWidth, new PointF());

                _messageTextView.Frame = new RectangleF(Frame.Width / 2 - _messageTextView.Frame.Width / 2, 
                    _titleTextView.Frame.Bottom + ComponentHeight.VerticalOffset + Padding.Top,

                    _messageTextView.Frame.Width, _messageTextView.Frame.Height);
                _messageTextView.TextAlignment = UITextAlignment.Center;
                _messageTextView.BackgroundColor = UIColor.Clear;

                ComponentHeight.MessageHeight = _messageTextView.Frame.Height;

                Add(_messageTextView);
            }

            CorrectHeight();
        }

        public void SetButtons(string cancelButtonTitle, string confirmButtonTitle)
        {
            if (_cancelButton != null)
            {
                _cancelButton.RemoveFromSuperview();
            }

            _isConfirmButtonExists = confirmButtonTitle != null;

            float upperLineY = Bounds.Height - ComponentHeight.BottomButtonsHeight;

            _cancelButton = new UIButton(new RectangleF(0, upperLineY, _isConfirmButtonExists ? Frame.Width / 2 : Frame.Width, ButtonHeight));

            _cancelButton.SetTitle(cancelButtonTitle, UIControlState.Normal);
            _cancelButton.SetTitleColor(UIColor.White, UIControlState.Normal);
            _cancelButton.TouchUpInside += (sender, e) => OnButtonPushed(0);

            Add(_cancelButton);

            if (_isConfirmButtonExists)
            {
                if (_confirmButton != null)
                {
                    _confirmButton.RemoveFromSuperview();
                }

                _confirmButton = new UIButton(new RectangleF(Frame.Width / 2, upperLineY, Frame.Width / 2, ButtonHeight));

                _confirmButton.SetTitle(confirmButtonTitle, UIControlState.Normal);
                _confirmButton.SetTitleColor(UIColor.White, UIControlState.Normal);
                _confirmButton.TouchUpInside += (sender, e) => OnButtonPushed(1);

                Add(_confirmButton);
            }

            CorrectHeight();
        }

        public void SetRadioButtons(string[] radioButtons)
        {
            var radioButtonMaxWidth = Frame.Width - Padding.Left - Padding.Right;

            if (_radioButtonsGroup != null)
            {
                _radioButtonsGroup.RemoveFromSuperview();
            }

            _radioButtonsGroup = new RadioButtonGroup(radioButtonMaxWidth, radioButtons);

            ComponentHeight.RadioButtonsHeight = _radioButtonsGroup.Frame.Height;

            Add(_radioButtonsGroup);

            CorrectHeight();
        }

        public override void Draw(RectangleF area)
        {
            base.Draw(area);

            var context = UIGraphics.GetCurrentContext();

            UIBezierPath roundedRect = UIBezierPath.FromRoundedRect(Bounds, 8);

            context.SetFillColor(UIColor.FromRGB(40, 40, 40).CGColor);
            context.AddPath(roundedRect.CGPath);
            context.FillPath();

            if (_titleTextView != null)
            {
                context.SetLineWidth(2);

                context.MoveTo(0, _titleTextView.Frame.Bottom + ComponentHeight.VerticalOffset);
                context.AddLineToPoint(Bounds.Width, _titleTextView.Frame.Bottom + ComponentHeight.VerticalOffset);
                context.SetStrokeColor(UIColor.FromRGB(51, 181, 229).CGColor);
                context.StrokePath();
            }

            float upperLineY = ComponentHeight.GetTotalHeight() - ComponentHeight.BottomButtonsHeight;

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

		public void SetRadioButtonActive(int index)
		{
			_radioButtonsGroup.OnButtonChecked(index);
		}

        #endregion

        #region private methods

        public void CorrectHeight()
        {
            var height = ComponentHeight.GetTotalHeight();

            Frame = new RectangleF(Frame.X, ItExpertHelper.GetRealScreenSize().Height / 2 - height / 2, Frame.Width, height);

            if (_radioButtonsGroup != null)
            {
                if (_titleTextView != null)
                {

                    _radioButtonsGroup.Location = new PointF(Padding.Left, _titleTextView.Frame.Bottom + VerticalOffset + Padding.Top);
                }
                else
                {
                    _radioButtonsGroup.Location = new PointF(Padding.Left, VerticalOffset);
                }
            }

            float upperLineY = Bounds.Height - ButtonHeight;

            if (_cancelButton != null)
            {
                _cancelButton.Frame = new RectangleF(_cancelButton.Frame.X, upperLineY, _cancelButton.Frame.Width, ButtonHeight);
            }

            if (_confirmButton != null)
            {
                _confirmButton.Frame = new RectangleF(_confirmButton.Frame.X, upperLineY, _confirmButton.Frame.Width, ButtonHeight);
            }

            SetNeedsDisplay();
        }

        public void OnButtonPushed(int index)
        {
            if (ButtonPushed != null)
            {
                int radioButtonIndex = 0;

                if (_radioButtonsGroup != null)
                {
                    radioButtonIndex = _radioButtonsGroup.SelectedIndex;
                }

                ButtonPushed(this, new BlackAlertViewButtonEventArgs(index, radioButtonIndex));
            }
        }

        #endregion

        #region private fields

        private bool _isConfirmButtonExists;

        private UITextView _titleTextView;
        private UITextView _messageTextView;

        private RadioButtonGroup _radioButtonsGroup;

        private UIButton _cancelButton;
        private UIButton _confirmButton;

        #endregion
    }
}

