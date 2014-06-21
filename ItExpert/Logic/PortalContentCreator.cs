using System;
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

			float height = _previewTextView.Frame.Bottom + _padding.Bottom;

			_headerTextView.Dispose ();
			_headerTextView = null;

			_previewTextView.Dispose ();
			_previewTextView = null;

			_imageViewContainer.Dispose ();
			_imageViewContainer = null;

			return height;
		}

        protected override void Create(UITableViewCell cell, Article article)
        {
            cell.SelectionStyle = UITableViewCellSelectionStyle.Default;

            CreateCellElements (cell.ContentView, article, ItExpertHelper.LargestImageSizeInArticlesPreview);

            AddCellElements(cell.ContentView);
        }
       
        protected override void Update(UITableViewCell cell, Article article)
        {
            cell.SelectionStyle = UITableViewCellSelectionStyle.Default;

            _article = article;

            var buttonImage = new UIImage(GetIsReadedButtonImageData(_article.IsReaded), (float)2.5);

            _isReadedButtonImageView.Image = buttonImage;

            UpdateTextView (_headerTextView, cell.ContentView.Bounds.Width - _padding.Right - _padding.Left - _isReadedButton.Frame.Width,
                _previewHeaderFont, _forecolor, article.Name, new PointF (_padding.Left, _padding.Top));

            if (_imageView != null && _imageView.Image != null)
            {
                if (_imageView.Image != null)
                {
                    _imageView.Image.Dispose ();
                    _imageView.Image = null;
                }

                _imageView.Image = ItExpertHelper.GetImageFromBase64String (article.PreviewPicture.Data);
                _imageView.Frame = new RectangleF (ItExpertHelper.LargestImageSizeInArticlesPreview / 2 - article.PreviewPicture.Width / 2, 
                    0, article.PreviewPicture.Width, article.PreviewPicture.Height);

                _imageViewContainer.Frame = new RectangleF (cell.ContentView.Bounds.Width - ItExpertHelper.LargestImageSizeInArticlesPreview - _padding.Right, 
                    _headerTextView.Frame.Bottom + _padding.Top, ItExpertHelper.LargestImageSizeInArticlesPreview, article.PreviewPicture.Height);
            }

            UpdateTextView (_previewTextView, cell.ContentView.Bounds.Width - _padding.Right - _padding.Left, 
                _previewTextFont, _forecolor, article.PreviewText, 
                new PointF (_padding.Left, _headerTextView.Frame.Bottom + _padding.Top), _imageViewContainer);

            AddCellElements(cell.ContentView);
        }

        private void CreateCellElements(UIView cellContentView, Article article, float largestImageWidth)
		{
            _article = article;

            _isReadedButton = new UIButton(new RectangleF(cellContentView.Frame.Width - _padding.Right - 70, 2, 70, 60));
            _isReadedButton.TouchUpInside += OnReadedButtonTouchUpInside;

            var buttonImage = new UIImage(GetIsReadedButtonImageData(_article.IsReaded), (float)2.5);

            _isReadedButtonImageView = new UIImageView(new RectangleF(_isReadedButton.Frame.Width - buttonImage.Size.Width , 0, buttonImage.Size.Width, buttonImage.Size.Height));
            _isReadedButtonImageView.Image = buttonImage;
    
            _isReadedButton.Add(_isReadedButtonImageView);

            _headerTextView = ItExpertHelper.GetTextView (_previewHeaderFont, _forecolor, article.Name, 
                cellContentView.Bounds.Width - _padding.Right - _padding.Left - _isReadedButton.Frame.Width, new PointF (_padding.Left, _padding.Top));

			_headerTextView.DataDetectorTypes = UIDataDetectorType.Link;

			_imageView = new UIImageView(new RectangleF (largestImageWidth/2 - article.PreviewPicture.Width / 2, 
				0, article.PreviewPicture.Width, article.PreviewPicture.Height));

			_imageView.Image = ItExpertHelper.GetImageFromBase64String(article.PreviewPicture.Data);

            _imageViewContainer = new UIView (new RectangleF (cellContentView.Bounds.Width - largestImageWidth - _padding.Right, 
				_headerTextView.Frame.Bottom + _padding.Top, largestImageWidth, article.PreviewPicture.Height));

			_imageViewContainer.Add (_imageView);

            _previewTextView = ItExpertHelper.GetTextView(_previewTextFont, _forecolor , article.PreviewText,
                cellContentView.Bounds.Width - _padding.Right - _padding.Left, new PointF (_padding.Left, _headerTextView.Frame.Bottom + _padding.Top), _imageViewContainer);
		}

        private void AddCellElements(UIView cellContentView)
        {
            cellContentView.Add (_isReadedButton);
            cellContentView.Add (_headerTextView);
            cellContentView.Add (_previewTextView);
            cellContentView.Add (_imageViewContainer);

            _isReadedButton.BringSubviewToFront(cellContentView);
        }

		private void UpdateTextView(UITextView textViewToUpdate, float updatedTextViewWidth, UIFont font, UIColor foregroundColor, string updatedText, PointF updatedTextViewLocation, UIView imageView = null)
		{
			var attributedString = ItExpertHelper.GetAttributedString (updatedText, font, foregroundColor);

			textViewToUpdate.TextStorage.SetString (attributedString);
			textViewToUpdate.AttributedText = attributedString;

			textViewToUpdate.TextContainer.Size = new Size ((int)updatedTextViewWidth, int.MaxValue);

			if (imageView != null)
			{
				var imageRectangle = ItExpertHelper.ConvertRectangleToSubviewCoordinates (imageView.Frame, updatedTextViewLocation);

				textViewToUpdate.TextContainer.ExclusionPaths = new UIBezierPath[] { UIBezierPath.FromRect (imageRectangle) };
			}

			var range = textViewToUpdate.LayoutManager.GetGlyphRange (textViewToUpdate.TextContainer);
			var containerSize = textViewToUpdate.LayoutManager.BoundingRectForGlyphRange (range, textViewToUpdate.TextContainer);

			textViewToUpdate.Frame = new RectangleF (updatedTextViewLocation, new SizeF (containerSize.Width, containerSize.Height));
		}

        public NSData GetIsReadedButtonImageData(bool isReaded)
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

		private UIView _imageViewContainer;
		private UIImageView _imageView;
		private UITextView _previewTextView;
		private UITextView _headerTextView;
        private UIButton _isReadedButton;
        private UIImageView _isReadedButtonImageView;
	}
}

