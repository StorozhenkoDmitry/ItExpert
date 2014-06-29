using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MonoTouch.UIKit;
using ItExpert.Model;

namespace ItExpert
{
    public class MagazineView : UIView
    {
        public MagazineView(Magazine magazine)
        {
            _spaceBetweenButtons = 5;

            UIImage image = ItExpertHelper.GetImageFromBase64String(magazine.PreviewPicture.Data);

            AddHeader(magazine.Name, image.Size.Width);
            AddImage(image);
            AddButtons(magazine.Exists);

            float totalHeight = magazine.Exists ? _deleteButton.Frame.Bottom : _downloadButton.Frame.Bottom + _downloadButton.Frame.Height + _spaceBetweenButtons;

            Frame = new RectangleF(0, 0, _imageView.Frame.Width, totalHeight);
        }

        public PointF Location
        {
            get
            {
                return Frame.Location;
            }
            set
            {
                Frame = new RectangleF(value, Frame.Size);
            }
        }

        private void AddHeader(string header, float maxTextWidth)
        {
            if (String.IsNullOrEmpty(header))
            {
                header = " ";
            }

            _headerTextView = ItExpertHelper.GetTextView(
                ItExpertHelper.GetAttributedString(header, UIFont.BoldSystemFontOfSize(ApplicationWorker.Settings.HeaderSize), 
                    ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetForeColor())), maxTextWidth, new PointF()); 

            Add(_headerTextView);
        }

        private void AddImage(UIImage image)
        {
            _imageView = new UIImageView(new RectangleF(0, _headerTextView.Frame.Bottom, image.Size.Width, image.Size.Height));

            _imageView.Layer.BorderColor = UIColor.Black.CGColor;
            _imageView.Layer.BorderWidth = 1;
            _imageView.Image = image;

            Add(_imageView);
        }

        private void AddButtons(bool isExists)
        {
            float buttonWidth = 70;
            float buttonHeight = 20;

            if (isExists)
            {
                _openButton = CreateButton("Открыть", new RectangleF(0, _imageView.Frame.Bottom + 3, buttonWidth, buttonHeight));

                Add(_openButton);

                _deleteButton = CreateButton("Удалить", new RectangleF(0, _openButton.Frame.Bottom + _spaceBetweenButtons, buttonWidth, buttonHeight));

                Add(_deleteButton);
            }
            else
            {
                _downloadButton = CreateButton("Скачать", new RectangleF(0, _imageView.Frame.Bottom + 3, buttonWidth, buttonHeight));

                Add(_downloadButton);
            }
        }

        private UIButton CreateButton(string title, RectangleF frame)
        {
            UIButton button = new UIButton(UIButtonType.RoundedRect);

            button.Frame = frame;
            button.SetTitle(title, UIControlState.Normal);
            button.SetTitleColor(UIColor.White, UIControlState.Normal);
            button.Font = UIFont.SystemFontOfSize(ApplicationWorker.Settings.TextSize);
            button.BackgroundColor = UIColor.FromRGB(160, 160, 160);
            button.Layer.CornerRadius = 3;

            return button;
        }

        private float _spaceBetweenButtons;

        private UITextView _headerTextView;
        private UIImageView _imageView;
        private UIButton _downloadButton;
        private UIButton _openButton;
        private UIButton _deleteButton;
    }
}

