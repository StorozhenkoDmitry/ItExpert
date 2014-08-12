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
            _magazine = magazine;

			UIImage image = null;

			if (magazine.PreviewPicture != null && !String.IsNullOrWhiteSpace(magazine.PreviewPicture.Data))
			{
				image = ItExpertHelper.GetImageFromBase64String(magazine.PreviewPicture.Data);
			}
			else
			{
				image = UIImage.FromFile("MagazineSplash.png");
			}

			AddHeader(magazine.Name, Math.Max(120, image.Size.Width));
            AddImage(image);
            AddButtons(magazine.Exists);

            float totalHeight = magazine.Exists ? _deleteButton.Frame.Bottom : _downloadButton.Frame.Bottom + _downloadButton.Frame.Height + _spaceBetweenButtons;

			Frame = new RectangleF(0, 0, Math.Max(_headerTextView.Frame.Width, _imageView.Frame.Width), totalHeight);
        }

        public event EventHandler MagazineImagePushed;
		public event EventHandler MagazineOpen;
		public event EventHandler MagazineDelete;
		public event EventHandler MagazineDownload;

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

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (_deleteButton != null)
			{
				_deleteButton.TouchUpInside -= OnMagazinePdfDelete;
				_deleteButton.RemoveFromSuperview ();
				_deleteButton.Dispose();
			}
			_deleteButton = null;

			if (_openButton != null)
			{
				_openButton.TouchUpInside -= OnMagazinePdfOpen;
				_openButton.RemoveFromSuperview ();
				_openButton.Dispose();
			}
			_openButton = null;

			if (_downloadButton != null)
			{
				_downloadButton.TouchUpInside -= OnMagazinePdfDownload;
				_downloadButton.RemoveFromSuperview ();
				_downloadButton.Dispose();
			}
			_downloadButton = null;

			if (_imageView != null)
			{
				if (_imageView.Image != null)
				{
					_imageView.Image.Dispose();
					_imageView.Image = null;
				}
				if (_imageView.GestureRecognizers != null)
				{
					foreach (var gr in _imageView.GestureRecognizers)
					{
						gr.Dispose();
					}
				}
				_imageView.RemoveFromSuperview();
				_imageView.Dispose();
			}
			_imageView = null;

			if (_headerTextView != null)
			{
				_headerTextView.RemoveFromSuperview();
				_headerTextView.Dispose();
			}
			_headerTextView = null;

			MagazineImagePushed = null;
			MagazineOpen = null;
			MagazineDelete = null;
			MagazineDownload = null;
		}

        private void AddHeader(string header, float maxTextWidth)
        {
            if (String.IsNullOrEmpty(header))
            {
                header = " ";
            }

            _headerTextView = ItExpertHelper.GetTextView(
				ItExpertHelper.GetAttributedString(header, UIFont.BoldSystemFontOfSize(16), 
                    ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetForeColor())), maxTextWidth, new PointF()); 
			_headerTextView.BackgroundColor = UIColor.Clear;
            Add(_headerTextView);
        }

        private void AddImage(UIImage image)
        {
            _imageView = new UIImageView(new RectangleF(0, _headerTextView.Frame.Bottom, image.Size.Width, image.Size.Height));

            _imageView.Layer.BorderColor = UIColor.Black.CGColor;
            _imageView.Layer.BorderWidth = 1;
            _imageView.Image = image;
            _imageView.UserInteractionEnabled = true;

            UITapGestureRecognizer tap = new UITapGestureRecognizer(() =>
            {
                ApplicationWorker.Magazine = _magazine;

                OnMagazineImagePushed();
            });

            _imageView.AddGestureRecognizer(tap);

            Add(_imageView);
        }

        private void AddButtons(bool isExists)
        {
            float buttonWidth = 70;
            float buttonHeight = 20;

            if (isExists)
            {
                _openButton = CreateButton("Открыть", new RectangleF(0, _imageView.Frame.Bottom + 3, buttonWidth, buttonHeight));
				_openButton.TouchUpInside += OnMagazinePdfOpen;
                Add(_openButton);

                _deleteButton = CreateButton("Удалить", new RectangleF(0, _openButton.Frame.Bottom + _spaceBetweenButtons, buttonWidth, buttonHeight));
				_deleteButton.TouchUpInside += OnMagazinePdfDelete;
                Add(_deleteButton);
            }
            else
            {
                _downloadButton = CreateButton("Скачать", new RectangleF(0, _imageView.Frame.Bottom + 3, buttonWidth, buttonHeight));
				_downloadButton.TouchUpInside += OnMagazinePdfDownload;
                Add(_downloadButton);
            }
        }

        private UIButton CreateButton(string title, RectangleF frame)
        {
            UIButton button = new UIButton(UIButtonType.RoundedRect);

            button.Frame = frame;
            button.SetTitle(title, UIControlState.Normal);
            button.SetTitleColor(UIColor.White, UIControlState.Normal);
            button.Font = UIFont.SystemFontOfSize(16);
            button.BackgroundColor = UIColor.FromRGB(160, 160, 160);
            button.Layer.CornerRadius = 3;

            return button;
        }

		public void UpdateMagazineExists(bool exists)
		{
			float buttonWidth = 70;
			float buttonHeight = 20;
			if (_deleteButton != null)
			{
				_deleteButton.TouchUpInside -= OnMagazinePdfDelete;
				_deleteButton.RemoveFromSuperview ();
				_deleteButton.Dispose();
			}
			if (_openButton != null)
			{
				_openButton.TouchUpInside -= OnMagazinePdfOpen;
				_openButton.RemoveFromSuperview ();
				_openButton.Dispose();
			}
			if (_downloadButton != null)
			{
				_downloadButton.TouchUpInside -= OnMagazinePdfDownload;
				_downloadButton.RemoveFromSuperview ();
				_downloadButton.Dispose();
			}
			if (exists)
			{
				_openButton = CreateButton("Открыть", new RectangleF(0, _imageView.Frame.Bottom + 3, buttonWidth, buttonHeight));
				_openButton.TouchUpInside += OnMagazinePdfOpen;
				Add(_openButton);

				_deleteButton = CreateButton("Удалить", new RectangleF(0, _openButton.Frame.Bottom + _spaceBetweenButtons, buttonWidth, buttonHeight));
				_deleteButton.TouchUpInside += OnMagazinePdfDelete;
				Add(_deleteButton);
			}
			else
			{
				_downloadButton = CreateButton("Скачать", new RectangleF(0, _imageView.Frame.Bottom + 3, buttonWidth, buttonHeight));
				_downloadButton.TouchUpInside += OnMagazinePdfDownload;
				Add(_downloadButton);
			}
		}

		void OnMagazinePdfDelete(object sender, EventArgs e)
		{
			if (MagazineDelete != null)
			{
				MagazineDelete(this, e);
			}
		}

		void OnMagazinePdfOpen(object sender, EventArgs e)
		{
			if (MagazineOpen != null)
			{
				MagazineOpen(this, e);
			}
		}

		void OnMagazinePdfDownload(object sender, EventArgs e)
		{
			if (MagazineDownload != null)
			{
				MagazineDownload(this, e);
			}
		}

        private void OnMagazineImagePushed()
        {
            if (MagazineImagePushed != null)
            {
                MagazineImagePushed(this, EventArgs.Empty);
            }
        }

		public Magazine Magazine
		{
			get { return _magazine; }
		}

        private float _spaceBetweenButtons;

        private Magazine _magazine;

        private UITextView _headerTextView;
        private UIImageView _imageView;
        private UIButton _downloadButton;
        private UIButton _openButton;
        private UIButton _deleteButton;
    }
}

