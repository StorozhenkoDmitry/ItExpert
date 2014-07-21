using System;
using MonoTouch.UIKit;
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.Foundation;

namespace ItExpert
{
    public class ShareView : UIViewController
    {
        internal class ViewWithButtons : UIView
        {
            enum ButtonsContainerType
            {
                Share,
                PutAside
            }

            public ViewWithButtons(RectangleF frame)
                :base(frame)
            {
                ContentMode = UIViewContentMode.Redraw;

                BackgroundColor = UIColor.Black;

                _padding = new UIEdgeInsets(10, 20, 10, 10);

                _shareTextView = ItExpertHelper.GetTextView(ItExpertHelper.GetAttributedString("Поделиться", UIFont.SystemFontOfSize(14), UIColor.White),
                    Frame.Width, new PointF(_padding.Left, _padding.Top));

                _shareTextView.BackgroundColor = UIColor.Clear;

                Add(_shareTextView);

                AddShareButtons();

                _putAsideTextView = ItExpertHelper.GetTextView(ItExpertHelper.GetAttributedString("Отложить для чтения", UIFont.SystemFontOfSize(14), UIColor.White),
                    Frame.Width, new PointF(_padding.Left, _shareButtonsContainer.Frame.Bottom + _padding.Top));

                _putAsideTextView.BackgroundColor = UIColor.Clear;

                Add(_putAsideTextView);

                AddPutAsideButtons();
            }

            public override void Draw(RectangleF rect)
            {
                base.Draw(rect);

                var context = UIGraphics.GetCurrentContext();

                context.SetLineWidth(2);

                if (_shareTextView != null)
                {
                    context.MoveTo(5, _shareTextView.Frame.Bottom + 2);
                    context.AddLineToPoint(Bounds.Width - 5, _shareTextView.Frame.Bottom + 2);
                    context.SetStrokeColor(UIColor.White.CGColor);
                    context.StrokePath();
                }

                if (_putAsideTextView != null)
                {
                    context.MoveTo(5, _putAsideTextView.Frame.Bottom + 2);
                    context.AddLineToPoint(Bounds.Width - 5, _putAsideTextView.Frame.Bottom + 2);
                    context.SetStrokeColor(UIColor.White.CGColor);
                    context.StrokePath();
                }
            }

            public void CorrectSubviewsFrames()
            {
                if (_shareButtonsContainer != null)
                {
                    _shareButtonsContainer.Frame = new RectangleF(new PointF(Frame.Width / 2 - _shareButtonsContainer.Frame.Width / 2, _shareButtonsContainer.Frame.Y), 
                        _shareButtonsContainer.Frame.Size);
                }

                if (_putAsideButtonsContainer != null)
                {
                    _putAsideButtonsContainer.Frame = new RectangleF(new PointF(Frame.Width / 2 - _putAsideButtonsContainer.Frame.Width / 2, _putAsideButtonsContainer.Frame.Y), 
                        _putAsideButtonsContainer.Frame.Size);
                }

                SetNeedsDisplay();
            }

            private void CreateEmailButton()
            {
                _emailButton = new UIView();

                UIImageView imageView = new UIImageView();

                var image = new UIImage(NSData.FromFile("NavigationBar/Mail.png"), 5);

                imageView.Image = image;

                UITextView textView = ItExpertHelper.GetTextView(ItExpertHelper.GetAttributedString("E-mail", UIFont.SystemFontOfSize(10), UIColor.White),
                    Frame.Width, new PointF(0, image.Size.Height));

                textView.BackgroundColor = UIColor.Clear;

                imageView.Frame = new RectangleF(new PointF(textView.Frame.Width / 2 - image.Size.Width / 2, 0), image.Size);

                _emailButton.Add(imageView);
                _emailButton.Add(textView);

                _emailButton.Frame = new RectangleF(0, 0, textView.Frame.Width, image.Size.Height + textView.Frame.Height);
            }

            private void AddShareButtons()
            {
                int buttonsCount = 5;

                _shareButtons = new List<UIButton>(buttonsCount);

                _shareButtonsContainer = new UIView();

                SizeF contentSize = new SizeF();

                float buttonsOffset = 15;

                CreateEmailButton();

                _shareButtonsContainer.Add(_emailButton);

                contentSize.Height = _emailButton.Frame.Height;
                contentSize.Width += _emailButton.Frame.Width + buttonsOffset + 13;

                for (int i = 0; i < buttonsCount; i++)
                {
                    _shareButtons.Add(GetButton(i, ButtonsContainerType.Share));

                    _shareButtons[i].Frame = new RectangleF(new PointF(contentSize.Width, _emailButton.Frame.Height / 2 - _shareButtons[i].Frame.Height / 2), 
                        _shareButtons[i].Frame.Size);

                    contentSize.Width += _shareButtons[i].Frame.Width + buttonsOffset;

                    _shareButtonsContainer.Add(_shareButtons[i]);
                }

                contentSize.Width -= buttonsOffset;

                _shareButtonsContainer.Frame = new RectangleF(new PointF(Frame.Width / 2 - contentSize.Width / 2, _shareTextView.Frame.Bottom + _padding.Top), 
                    contentSize);
                _shareButtonsContainer.BackgroundColor = UIColor.Clear;

                Add(_shareButtonsContainer);
            }

            private void CreateFavoriteButton()
            {
                _favoriteButton = new UIView();

                UIImageView imageView = new UIImageView();

                var image = new UIImage(NSData.FromFile("NavigationBar/Favorite.png"), 2.5f);

                imageView.Image = image;

                UITextView textView = ItExpertHelper.GetTextView(ItExpertHelper.GetAttributedString("Избранное", UIFont.SystemFontOfSize(10), UIColor.White),
                    Frame.Width, new PointF(0, image.Size.Height));

                textView.BackgroundColor = UIColor.Clear;

                imageView.Frame = new RectangleF(new PointF(textView.Frame.Width / 2 - image.Size.Width / 2, 0), image.Size);

                _favoriteButton.Add(imageView);
                _favoriteButton.Add(textView);

                _favoriteButton.Frame = new RectangleF(0, 0, textView.Frame.Width, image.Size.Height + textView.Frame.Height);
            }

            public void AddPutAsideButtons()
            {
                int buttonsCount = 4;

                _putAsideButtons = new List<UIButton>(buttonsCount);

                _putAsideButtonsContainer = new UIView();

                SizeF contentSize = new SizeF();

                float buttonsOffset = 20;

                CreateFavoriteButton();

                _putAsideButtonsContainer.Add(_favoriteButton);

                contentSize.Height = _favoriteButton.Frame.Height;
                contentSize.Width += _favoriteButton.Frame.Width + buttonsOffset + 13;

                for (int i = 0; i < buttonsCount; i++)
                {
                    _putAsideButtons.Add(GetButton(i, ButtonsContainerType.PutAside));

                    _putAsideButtons[i].Frame = new RectangleF(new PointF(contentSize.Width, contentSize.Height / 2 - _putAsideButtons[i].Frame.Height / 2), 
                        _putAsideButtons[i].Frame.Size);

                    contentSize.Width += _putAsideButtons[i].Frame.Width + buttonsOffset;

                    _putAsideButtonsContainer.Add(_putAsideButtons[i]);
                }

                contentSize.Width -= buttonsOffset;

                _putAsideButtonsContainer.Frame = new RectangleF(new PointF(Frame.Width / 2 - contentSize.Width / 2, _putAsideTextView.Frame.Bottom + _padding.Top + 5), 
                    contentSize);
                _putAsideButtonsContainer.BackgroundColor = UIColor.Clear;

                Add(_putAsideButtonsContainer);
            }

            private UIButton GetButton(int index, ButtonsContainerType containerType)
            {
                UIButton button = new UIButton();

                var image = new UIImage(GetButtonImageData(index, containerType), 2.5f);

                button.SetImage(image, UIControlState.Normal);
                button.AdjustsImageWhenHighlighted = false;

                button.Frame = new RectangleF(new PointF(), image.Size);

                return button;
            }

            private NSData GetButtonImageData(int index, ButtonsContainerType containerType)
            {
                if (containerType == ButtonsContainerType.Share)
                {
                    switch (index)
                    {
                        case 0:
                            return NSData.FromFile("NavigationBar/Facebook.png");

                        case 1:
                            return NSData.FromFile("NavigationBar/Vk.png");

                        case 2:
                            return NSData.FromFile("NavigationBar/Twitter.png");

                        case 3:
                            return NSData.FromFile("NavigationBar/Google.png");

                        case 4:
                            return NSData.FromFile("NavigationBar/Linkedin.png");

                        default:
                            throw new NotImplementedException("Wrong index for share button.");
                    }
                }
                else if (containerType == ButtonsContainerType.PutAside)
                {
                    switch (index)
                    {
                        case 0:
                            return NSData.FromFile("NavigationBar/Evernote.png");

                        case 1:
                            return NSData.FromFile("NavigationBar/Readability.png");

                        case 2:
                            return NSData.FromFile("NavigationBar/Pocket.png");

                        case 3:
                            return NSData.FromFile("NavigationBar/Instapaper.png");

                        default:
                            throw new NotImplementedException("Wrong index for share button.");
                    }
                }

                return null;
            }

            private UIEdgeInsets _padding;

            private UITextView _shareTextView;
            private UITextView _putAsideTextView;

            private List<UIButton> _shareButtons;
            private List<UIButton> _putAsideButtons;

            private UIView _shareButtonsContainer;
            private UIView _putAsideButtonsContainer;
            private UIView _emailButton;
            private UIView _favoriteButton;
        }

        public event EventHandler TapOutsideTableView;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            AutomaticallyAdjustsScrollViewInsets = false;

            _navigationController = (UIApplication.SharedApplication.KeyWindow.RootViewController as UINavigationController);

            AddHeaderView();

            _viewWithButtons = new ViewWithButtons(new RectangleF(0, ItExpertHelper.StatusBarHeight + _navigationController.NavigationBar.Frame.Height,
                View.Frame.Width, 175));

            View.Add(_viewWithButtons);

            AddTapView();
        }

        public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
        {
            base.DidRotate(fromInterfaceOrientation);

            var screenSize = ItExpertHelper.GetRealScreenSize();

            if (_viewWithButtons != null)
            {
                _viewWithButtons.Frame = new RectangleF(0, ItExpertHelper.StatusBarHeight + _navigationController.NavigationBar.Frame.Height,
                    screenSize.Width, 175);

                _viewWithButtons.CorrectSubviewsFrames();

                if (_tapableView != null)
                {
                    _tapableView.Frame = new RectangleF(0, _viewWithButtons.Frame.Bottom, screenSize.Width, screenSize.Height - _viewWithButtons.Frame.Bottom);
                }
            }

            if (_headerView != null)
            {
                _headerView.Frame = _navigationController.NavigationBar.Frame;

                _backButton.Frame = new RectangleF(new PointF(10, _headerView.Frame.Height / 2 - _backButton.Frame.Height / 2), _backButton.Frame.Size);

                _logoImageView.Frame = new RectangleF(new PointF(_backButton.Frame.Right + 20, _headerView.Frame.Height / 2 - _logoImageView.Frame.Height / 2), 
                    _logoImageView.Frame.Size);
            }
        }

        private void AddHeaderView()
        {
            _headerView = new UIView(_navigationController.NavigationBar.Frame);

            _headerView.BackgroundColor = UIColor.FromRGB(40, 40, 40);

            var image = new UIImage(NSData.FromFile("NavigationBar/Back.png"), 2);

            _backButton = new UIButton(new RectangleF(new PointF(10, _headerView.Frame.Height / 2 - image.Size.Height / 2), image.Size));

            _backButton.SetImage(image, UIControlState.Normal);
            _backButton.TouchUpInside += (sender, e) => OnTapOutsideTableView();

            _headerView.Add(_backButton);

            _logoImageView = new UIImageView(new UIImage(NSData.FromFile("NavigationBar/Logo.png"), 2));

            _logoImageView.Frame = new RectangleF(new PointF(_backButton.Frame.Right + 20, _headerView.Frame.Height / 2 - _logoImageView.Frame.Height / 2), 
                _logoImageView.Frame.Size);

            _headerView.Add(_logoImageView);

            View.Add(_headerView);
        }

        private void AddTapView()
        {
            _tapableView = new UIView(new RectangleF(0, _viewWithButtons.Frame.Bottom, View.Frame.Width, View.Frame.Height - _viewWithButtons.Frame.Bottom));

            _tapableView.BackgroundColor = UIColor.Clear;
            _tapableView.AddGestureRecognizer(new UITapGestureRecognizer(() =>
            {
                OnTapOutsideTableView();
            }));

            View.Add(_tapableView);
        }

        private void OnTapOutsideTableView()
        {
            if (TapOutsideTableView != null)
            {
                TapOutsideTableView(this, EventArgs.Empty);
            }
        }

        private ViewWithButtons _viewWithButtons;
        private UIImageView _logoImageView;
        private UIView _headerView;
        private UIButton _backButton;
        private UINavigationController _navigationController;
        private UIView _tapableView;
    }
}

