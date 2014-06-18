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
			_headerTextView = new UITextView ();

			_articleTextView = new UITextView ();
			_isReadedButton = new IsReadedButton ();

			_fontSize = 14;

			_padding = new UIEdgeInsets (8, 12, 8, 12);;

			ContentView.Add (_isReadedButton);
		}

		public void AddCellContent(Article article, float largestImageWidth)
		{
			CreateCellElements (article, largestImageWidth);

			ContentView.Add (_headerTextView);
			ContentView.Add (_articleTextView);
			ContentView.Add (_imageViewContainer);
		}

		public void UpdateCell (Article article, float largestImageWidth)
		{
			UpdateTextView (_headerTextView, ContentView.Bounds.Width - _padding.Right - _padding.Left,
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
			_headerTextView = ItManagerHelper.GetTextView (UIFont.BoldSystemFontOfSize (_fontSize), UIColor.Black, article.Name, 
				ContentView.Bounds.Width - _padding.Right - _padding.Left, new PointF (_padding.Left, _padding.Top));

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

		private int _fontSize;
		private UIEdgeInsets _padding;

		private UIView _imageViewContainer;
		private UIImageView _imageView;
		private UITextView _articleTextView;
		private UITextView _headerTextView;
		private IsReadedButton _isReadedButton;
	}
}

