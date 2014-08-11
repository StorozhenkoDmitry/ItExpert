﻿using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using ItExpert.Model;
using ItExpert.Enum;
using System.Threading;

namespace ItExpert
{
    public sealed class PortalContentCreator: BaseContentCreator
	{
        public override float GetContentHeight(UIView cellContentView, Article article)
		{
            CreateCellElements (cellContentView, article, ItExpertHelper.LargestImageSizeInArticlesPreview);

			var imageContainerHeight = (_imageViewContainer != null) ? _imageViewContainer.Frame.Bottom : 0;

            float height = Math.Max(_previewTextView.Frame.Bottom, imageContainerHeight);

			_headerTextView.Dispose ();
			_headerTextView = null;

			_previewTextView.Dispose ();
			_previewTextView = null;

			if (_imageViewContainer != null)
			{
				_imageViewContainer.Dispose();
				_imageViewContainer = null;
			}
            return height + _padding.Bottom;
		}

        protected override void Create(UITableViewCell cell, Article article)
        {
			cell.ContentView.BackgroundColor = 
				ItExpertHelper.GetUIColorFromColor (ApplicationWorker.Settings.GetBackgroundColor ());

            cell.SelectionStyle = UITableViewCellSelectionStyle.Default;

            CreateCellElements (cell.ContentView, article, ItExpertHelper.LargestImageSizeInArticlesPreview);

            AddCellElements(cell.ContentView);
        }
       
        protected override void Update(UITableViewCell cell, Article article)
        {
			cell.ContentView.BackgroundColor = 
				ItExpertHelper.GetUIColorFromColor (ApplicationWorker.Settings.GetBackgroundColor ());

            cell.SelectionStyle = UITableViewCellSelectionStyle.Default;

            _article = article;

            var buttonImage = new UIImage(GetIsReadedButtonImageData(_article.IsReaded), (float)2.5);

            if (_isReadedButtonImageView.Image != null)
            {
                _isReadedButtonImageView.Image.Dispose();
                _isReadedButtonImageView.Image = null;
            }

            _isReadedButtonImageView.Image = buttonImage;

            _isReadedButton.Frame =new RectangleF(cell.ContentView.Frame.Width - _padding.Right - 70, 2, 70, 70);

			var headerFont = UIFont.BoldSystemFontOfSize (ApplicationWorker.Settings.HeaderSize);
			var foreColor = ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetForeColor());
            UpdateTextView (_headerTextView, cell.ContentView.Bounds.Width - _padding.Right - _padding.Left - _isReadedButton.Frame.Width,
				headerFont, foreColor, article.Name, new PointF (_padding.Left, _padding.Top));

			if (ApplicationWorker.Settings.LoadImages && article.PreviewPicture != null && article.PreviewPicture.Data != null)
			{
				if (_imageView != null)
				{
					if (_imageView.Image != null)
					{
						_imageView.Image.Dispose();
						_imageView.Image = null;
					}

					_imageView.Image = ItExpertHelper.GetImageFromBase64String(article.PreviewPicture.Data);
					_imageView.Frame = new RectangleF(ItExpertHelper.LargestImageSizeInArticlesPreview / 2 - article.PreviewPicture.Width / 2, 
						0, article.PreviewPicture.Width, article.PreviewPicture.Height);

					_imageViewContainer.Frame = new RectangleF(cell.ContentView.Bounds.Width - ItExpertHelper.LargestImageSizeInArticlesPreview - _padding.Right, 
						_headerTextView.Frame.Bottom + _padding.Top, ItExpertHelper.LargestImageSizeInArticlesPreview, article.PreviewPicture.Height);
				}
				else
				{
					CreateImageView(article, ItExpertHelper.LargestImageSizeInArticlesPreview, cell.ContentView);
				}
			}
			else
			{
				if (_imageView != null)
				{
					_imageView.Dispose();
					_imageView = null;
				}
				if (_imageViewContainer != null)
				{
					_imageViewContainer.Dispose();
					_imageViewContainer = null;
				}
			}
			var previewFont = UIFont.SystemFontOfSize (ApplicationWorker.Settings.TextSize);
            UpdateTextView (_previewTextView, cell.ContentView.Bounds.Width - _padding.Right - _padding.Left, 
				previewFont, foreColor, article.PreviewText, 
                new PointF (_padding.Left, _headerTextView.Frame.Bottom + _padding.Top), _imageViewContainer);

            AddCellElements(cell.ContentView);
        }

        private void CreateCellElements(UIView cellContentView, Article article, float largestImageWidth)
		{
            _article = article;

            _isReadedButton = new UIButton(new RectangleF(cellContentView.Frame.Width - _padding.Right - 70, 2, 70, 70));
            _isReadedButton.TouchUpInside += OnReadedButtonTouchUpInside;
            _isReadedButton.AdjustsImageWhenHighlighted = false;

            var buttonImage = new UIImage(GetIsReadedButtonImageData(_article.IsReaded), (float)2.5);

            _isReadedButtonImageView = new UIImageView(new RectangleF(_isReadedButton.Frame.Width - buttonImage.Size.Width , 0, buttonImage.Size.Width, buttonImage.Size.Height));
            _isReadedButtonImageView.Image = buttonImage;
    
            _isReadedButton.Add(_isReadedButtonImageView);

			var foreColor = ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetForeColor());
			var headerFont = UIFont.BoldSystemFontOfSize (ApplicationWorker.Settings.HeaderSize);
			_headerTextView = ItExpertHelper.GetTextView (ItExpertHelper.GetAttributedString(article.Name, headerFont, foreColor), 
                cellContentView.Bounds.Width - _padding.Right - _padding.Left - _isReadedButton.Frame.Width, new PointF (_padding.Left, _padding.Top));
			_headerTextView.BackgroundColor = ItExpertHelper.GetUIColorFromColor (ApplicationWorker.Settings.GetBackgroundColor ());
			if (ApplicationWorker.Settings.LoadImages && article.PreviewPicture != null && article.PreviewPicture.Data != null)
			{
				CreateImageView(article, largestImageWidth, cellContentView);
			}
			else
			{
				if (_imageViewContainer != null)
				{
					_imageViewContainer.Dispose();
					_imageViewContainer = null;
				}
			}
			var previewFont = UIFont.SystemFontOfSize (ApplicationWorker.Settings.TextSize);
			_previewTextView = ItExpertHelper.GetTextView(ItExpertHelper.GetAttributedString(article.PreviewText, previewFont, foreColor), 
                cellContentView.Bounds.Width - _padding.Right - _padding.Left, new PointF (_padding.Left, _headerTextView.Frame.Bottom + _padding.Top), _imageViewContainer);
			_previewTextView.BackgroundColor = ItExpertHelper.GetUIColorFromColor (ApplicationWorker.Settings.GetBackgroundColor ());
		}

        private void AddCellElements(UIView cellContentView)
        {
            cellContentView.Add (_isReadedButton);
            cellContentView.Add (_headerTextView);
            cellContentView.Add (_previewTextView);
			if (_imageViewContainer != null)
			{
				cellContentView.Add(_imageViewContainer);
			}
            _isReadedButton.BringSubviewToFront(cellContentView);
        }

		private void UpdateTextView(UITextView textViewToUpdate, float updatedTextViewWidth, UIFont font, UIColor foregroundColor, string updatedText, PointF updatedTextViewLocation, UIView imageView = null)
		{
            var attributedString = ItExpertHelper.GetAttributedString (updatedText, font, foregroundColor);

			textViewToUpdate.TextStorage.SetString (attributedString);
			textViewToUpdate.AttributedText = attributedString;

			textViewToUpdate.TextContainer.Size = new Size ((int)updatedTextViewWidth, int.MaxValue);

			textViewToUpdate.BackgroundColor = ItExpertHelper.GetUIColorFromColor (ApplicationWorker.Settings.GetBackgroundColor ());

			if (imageView != null)
			{
				var imageRectangle = ItExpertHelper.ConvertRectangleToSubviewCoordinates(imageView.Frame, updatedTextViewLocation);

				textViewToUpdate.TextContainer.ExclusionPaths = new UIBezierPath[] { UIBezierPath.FromRect(imageRectangle) };
			}
			else
			{
				textViewToUpdate.TextContainer.ExclusionPaths = new UIBezierPath[0];
			}
			var range = textViewToUpdate.LayoutManager.GetGlyphRange (textViewToUpdate.TextContainer);
			var containerSize = textViewToUpdate.LayoutManager.BoundingRectForGlyphRange (range, textViewToUpdate.TextContainer);

			textViewToUpdate.Frame = new RectangleF (updatedTextViewLocation, new SizeF (containerSize.Width, containerSize.Height));
		}

        private NSData GetIsReadedButtonImageData(bool isReaded)
        {
            if (isReaded)
            {
                return NSData.FromFile("ReadedButton.png");
            }
            else
            {
                return NSData.FromFile("NotReadedButton.png");
            }
        }

        private void OnReadedButtonTouchUpInside(object sender, EventArgs e)
        {
            _article.IsReaded = !_article.IsReaded;

            var buttonImage = new UIImage(GetIsReadedButtonImageData(_article.IsReaded), (float)2.5);

            _isReadedButtonImageView.Image = buttonImage;

			ArticlesTableSource.SetIsReadedForArticle(_article);
        }

        private void CreateImageView(Article article, float largestImageWidth, UIView cellContentView)
        {
            _imageView = new UIImageView(new RectangleF(largestImageWidth / 2 - article.PreviewPicture.Width / 2, 
                0, article.PreviewPicture.Width, article.PreviewPicture.Height));

            _imageView.Image = ItExpertHelper.GetImageFromBase64String(article.PreviewPicture.Data);

            _imageViewContainer = new UIView(new RectangleF(cellContentView.Bounds.Width - largestImageWidth - _padding.Right, 
                _headerTextView.Frame.Bottom + _padding.Top, largestImageWidth, article.PreviewPicture.Height));

            _imageViewContainer.Add(_imageView);
        }

		private UIView _imageViewContainer;
		private UIImageView _imageView;
		private UITextView _previewTextView;
		private UITextView _headerTextView;
        private UIButton _isReadedButton;
        private UIImageView _isReadedButtonImageView;
	}
}

