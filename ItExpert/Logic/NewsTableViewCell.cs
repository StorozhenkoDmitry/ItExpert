using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using ItExpert.Model;

namespace ItExpert
{
	public class NewsTableViewCell: UITableViewCell
	{
		public NewsTableViewCell (string cellId) : base (UITableViewCellStyle.Default, cellId)
		{
			_fontSize = 14;

			_padding = new UIEdgeInsets (8, 12, 8, 12);
		}

		public void AddCellContent(Article article, float largestImageWidth)
        {
			CreateCellElements (article, largestImageWidth);

            ContentView.Add (_isReadedButton);
			ContentView.Add (_headerTextView);
			ContentView.Add (_articleTextView);
			ContentView.Add (_imageViewContainer);

            _isReadedButton.BringSubviewToFront(ContentView);
		}

		public void UpdateCell (Article article, float largestImageWidth)
		{
            _article = article;

            var buttonImage = new UIImage(GetIsReadedButtonImageData(_article.IsReaded), (float)2.5);

            _isReadedButtonImageView.Image = buttonImage;

            UpdateTextView (_headerTextView, ContentView.Bounds.Width - _padding.Right - _padding.Left - _isReadedButton.Frame.Width,
				UIFont.BoldSystemFontOfSize (_fontSize), UIColor.Black, article.Name, new PointF (_padding.Left, _padding.Top));

			if (_imageView != null && _imageView.Image != null)
			{
				if (_imageView.Image != null)
				{
					_imageView.Image.Dispose ();
					_imageView.Image = null;
				}

				_imageView.Image = ItManagerHelper.GetImageFromBase64String (article.PreviewPicture.Data);
				_imageView.Frame = new RectangleF (largestImageWidth / 2 - article.PreviewPicture.Width / 2, 
					0, article.PreviewPicture.Width, article.PreviewPicture.Height);

				_imageViewContainer.Frame = new RectangleF (ContentView.Bounds.Width - largestImageWidth - _padding.Right, 
					_headerTextView.Frame.Bottom + _padding.Top, largestImageWidth, article.PreviewPicture.Height);
			}

			UpdateTextView (_articleTextView, ContentView.Bounds.Width - _padding.Right - _padding.Left, 
				UIFont.SystemFontOfSize (_fontSize), UIColor.Black, article.PreviewText, 
				new PointF (_padding.Left, _headerTextView.Frame.Bottom + _padding.Top), _imageViewContainer);
		}

		public float GetHeightDependingOnContent(Article article, int largetsImageWidth)
		{
			CreateCellElements (article, largetsImageWidth);

			float height = _articleTextView.Frame.Bottom + _padding.Bottom;

			_headerTextView.Dispose ();
			_headerTextView = null;

			_articleTextView.Dispose ();
			_articleTextView = null;

			_imageViewContainer.Dispose ();
			_imageViewContainer = null;

			return height;
		}

		private void CreateCellElements(Article article, float largestImageWidth)
		{
            _article = article;

            _isReadedButton = new UIButton(new RectangleF(ContentView.Frame.Width - _padding.Right - 70, 2, 70, 60));
            _isReadedButton.TouchUpInside += OnReadedButtonTouchUpInside;

            var buttonImage = new UIImage(GetIsReadedButtonImageData(_article.IsReaded), (float)2.5);

            _isReadedButtonImageView = new UIImageView(new RectangleF(_isReadedButton.Frame.Width - buttonImage.Size.Width , 0, buttonImage.Size.Width, buttonImage.Size.Height));
            _isReadedButtonImageView.Image = buttonImage;
    
            _isReadedButton.Add(_isReadedButtonImageView);

			_headerTextView = ItManagerHelper.GetTextView (UIFont.BoldSystemFontOfSize (_fontSize), UIColor.Black, article.Name, 
                ContentView.Bounds.Width - _padding.Right - _padding.Left - _isReadedButton.Frame.Width, new PointF (_padding.Left, _padding.Top));

			_headerTextView.DataDetectorTypes = UIDataDetectorType.Link;

			_imageView = new UIImageView(new RectangleF (largestImageWidth/2 - article.PreviewPicture.Width / 2, 
				0, article.PreviewPicture.Width, article.PreviewPicture.Height));

			_imageView.Image = ItManagerHelper.GetImageFromBase64String(article.PreviewPicture.Data);

			_imageViewContainer = new UIView (new RectangleF (ContentView.Bounds.Width - largestImageWidth - _padding.Right, 
				_headerTextView.Frame.Bottom + _padding.Top, largestImageWidth, article.PreviewPicture.Height));

			_imageViewContainer.Add (_imageView);

			_articleTextView = ItManagerHelper.GetTextView (UIFont.SystemFontOfSize (_fontSize), UIColor.Black, article.PreviewText,
				ContentView.Bounds.Width - _padding.Right - _padding.Left, new PointF (_padding.Left, _headerTextView.Frame.Bottom + _padding.Top), _imageViewContainer);
		}

		private void UpdateTextView(UITextView textViewToUpdate, float updatedTextViewWidth, UIFont font, UIColor foregroundColor, string updatedText, PointF updatedTextViewLocation, UIView imageView = null)
		{
			var attributedString = ItManagerHelper.GetAttributedString (updatedText, font, foregroundColor);

			textViewToUpdate.TextStorage.SetString (attributedString);
			textViewToUpdate.AttributedText = attributedString;

			textViewToUpdate.TextContainer.Size = new Size ((int)updatedTextViewWidth, int.MaxValue);

			if (imageView != null)
			{
				var imageRectangle = ItManagerHelper.ConvertRectangleToSubviewCoordinates (imageView.Frame, updatedTextViewLocation);

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

        //Вызывается при взаимодействии с кнопкой IsReaded
        private void SetIsReadedForArticle(Article article, bool isReaded)
        {

        }

        private void OnReadedButtonTouchUpInside(object sender, EventArgs e)
        {
            _article.IsReaded = !_article.IsReaded;

            var buttonImage = new UIImage(GetIsReadedButtonImageData(_article.IsReaded), (float)2.5);

            _isReadedButtonImageView.Image = buttonImage;

            //SetIsReadedForArticle(_article, !_article.IsReaded);
        }

		private int _fontSize;
		private UIEdgeInsets _padding;
        private Article _article;

		private UIView _imageViewContainer;
		private UIImageView _imageView;
		private UITextView _articleTextView;
		private UITextView _headerTextView;
        private UIButton _isReadedButton;
        private UIImageView _isReadedButtonImageView;
	}
}

