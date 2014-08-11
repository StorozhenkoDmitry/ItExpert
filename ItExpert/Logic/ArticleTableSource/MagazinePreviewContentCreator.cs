﻿using System;
using MonoTouch.UIKit;
using ItExpert.Model;
using MonoTouch.Foundation;
using System.Drawing;

namespace ItExpert
{
    public class MagazinePreviewContentCreator : BaseContentCreator
    {
        public override float GetContentHeight(UIView cellContentView, Article article)
        {
            UIImage image = ItExpertHelper.GetImageFromBase64String(ApplicationWorker.Magazine.PreviewPicture.Data);

            return image.Size.Height + _padding.Top + _padding.Bottom;
        }

        protected override void Create(UITableViewCell cell, Article article)
        {
            cell.ContentView.BackgroundColor = UIColor.Black;

            AddImageView(cell);
            AddHeader(cell);
            AddButton(cell);
			AddProgress (cell);
			if (MagazineViewController.Current.IsLoadingPdf)
			{
				_progress.StartAnimating ();
				_button.Enabled = false;
			}
			else
			{
				_progress.StopAnimating ();
				_button.Enabled = true;
			}
        }

        protected override void Update(UITableViewCell cell, Article article)
        {
            cell.ContentView.BackgroundColor = UIColor.Black;

            AddImageView(cell);
            AddHeader(cell);
            AddButton(cell);
			AddProgress (cell);
			if (MagazineViewController.Current.IsLoadingPdf)
			{
				_progress.StartAnimating ();
				_button.Enabled = false;
			}
			else
			{
				_progress.StopAnimating ();
				_button.Enabled = true;
			}
        }

        private void AddImageView(UITableViewCell cell)
        {
			UIImage image = null;

			if (ApplicationWorker.Magazine.PreviewPicture != null && !String.IsNullOrWhiteSpace(ApplicationWorker.Magazine.PreviewPicture.Data))
			{
				image = ItExpertHelper.GetImageFromBase64String(ApplicationWorker.Magazine.PreviewPicture.Data);
			}
			else
			{
				image = UIImage.FromFile("MagazineSplash.png");
			}

            if (_imageView == null)
            {
                _imageView = new UIImageView();
            }

            _imageView.Frame = new RectangleF(_padding.Left, _padding.Top, image.Size.Width, image.Size.Height);
            _imageView.Image = image;

            cell.ContentView.Add(_imageView);
        }

        private void AddHeader(UITableViewCell cell)
        {
            _leftHeaderOffset = 30;

            float headerX = (_imageView != null ? _imageView.Frame.Right : _padding.Left) + _leftHeaderOffset;

			if (_headerTextView != null)
			{
				_headerTextView.Dispose();
				_headerTextView = null;
			}

            _headerTextView = ItExpertHelper.GetTextView(ItExpertHelper.GetAttributedString(ApplicationWorker.Magazine.Name, UIFont.BoldSystemFontOfSize(16), 
                UIColor.FromRGB(160, 160, 160)), cell.ContentView.Frame.Width, new PointF(headerX, _padding.Top * 2));

            _headerTextView.BackgroundColor = UIColor.Clear;

            cell.ContentView.Add(_headerTextView);
        }

        private void AddButton(UITableViewCell cell)
        {
            if (_button == null)
            {
                _leftButtonOffset = 35;
                _spaceBetweenHeaderAndButton = 10;
                float buttonWidth = 80;
                float buttonHeight = 28;
                float buttonX = (_imageView != null ? _imageView.Frame.Right : _padding.Left) + _leftButtonOffset;
                float buttonY = (_headerTextView != null ? _headerTextView.Frame.Bottom + _spaceBetweenHeaderAndButton : _padding.Top * 2);

                _button = new UIButton(UIButtonType.RoundedRect);

                _button.Frame = new RectangleF(buttonX, buttonY, buttonWidth, buttonHeight);
                _button.SetTitleColor(UIColor.FromRGB(180, 180, 180), UIControlState.Normal);
                _button.Font = UIFont.SystemFontOfSize(16);
                _button.BackgroundColor = UIColor.FromRGB(30, 30, 30);
                _button.Layer.CornerRadius = 3;
                _button.TouchUpInside += OnButtonPush;
            }

            _button.SetTitle(ApplicationWorker.Magazine.Exists ? "Открыть" : "Скачать", UIControlState.Normal);

            cell.ContentView.Add(_button);

			if (ApplicationWorker.Magazine.Exists)
			{
				if (_deleteButton == null)
				{
					_deleteButton = new UIButton(UIButtonType.RoundedRect);
					_deleteButton.SetTitle("Удалить", UIControlState.Normal);
					_deleteButton.Frame = new RectangleF(_button.Frame.X, _button.Frame.Bottom + 8, 80, 28);
					_deleteButton.SetTitleColor(UIColor.FromRGB(180, 180, 180), UIControlState.Normal);
					_deleteButton.Font = UIFont.SystemFontOfSize(16);
					_deleteButton.BackgroundColor = UIColor.FromRGB(30, 30, 30);
					_deleteButton.Layer.CornerRadius = 3;
					_deleteButton.TouchUpInside += OnDeleteButtonPush;
				}
				cell.ContentView.Add(_deleteButton);
			}
        }

		private void AddProgress(UITableViewCell cell)
		{
			if (_progress == null)
			{
				_progress = new UIActivityIndicatorView (
					new RectangleF (_button.Frame.X, _button.Frame.Bottom + 10, 80, 50));
				_progress.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.WhiteLarge;
				_progress.Color = UIColor.LightGray;
				_progress.BackgroundColor = UIColor.Black;
			}
			cell.ContentView.Add(_progress);
		}

		private void OnDeleteButtonPush(object sender, EventArgs e)
        {
			MagazineViewController.Current.DeleteMagazinePdf ();
        }

		private void OnButtonPush(object sender, EventArgs e)
		{
			if (ApplicationWorker.Magazine.Exists)
			{
				MagazineViewController.Current.OpenMagazinePdf ();
			}
			else
			{
				MagazineViewController.Current.DownloadMagazinePdf ();
			}
		}

        private float _leftHeaderOffset;
        private float _spaceBetweenHeaderAndButton;
        private float _leftButtonOffset;

        private UIImageView _imageView;
        private UITextView _headerTextView;
        private UIButton _button;
		private UIButton _deleteButton;
		private UIActivityIndicatorView _progress;
    }
}

