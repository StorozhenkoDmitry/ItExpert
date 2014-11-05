using System;
using ItExpert.Model;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;
using System.Threading;

namespace ItExpert
{
    public class DoublePortalContentCreator : BaseContentCreator
    {
        public class Content: IDisposable
        {
            public enum Position
            {
                Left,
                Right
            }

            public Article Article { get; set; }
            public UIView MainContainer { get; set; } 
            public UIView ImageViewContainer { get; set; }
            public UIImageView ImageView { get; set; }
            public UITextView PreviewTextView { get; set; }
            public UITextView HeaderTextView { get; set; }
            public UIButton IsReadedButton { get; set; }
            public UIImageView IsReadedButtonImageView { get; set; }

            public void Dispose()
            {
                if (ImageView != null)
                {
					ImageView.RemoveFromSuperview();
					if (ImageView.Image != null)
					{
						ImageView.Image.Dispose();
						ImageView.Image = null;
					}
                    ImageView.Dispose();
                    ImageView = null;
                }

                if (ImageViewContainer != null)
                {
					ImageViewContainer.RemoveFromSuperview();
                    ImageViewContainer.Dispose();
                    ImageViewContainer = null;
                }

                if (HeaderTextView != null)
                {
					HeaderTextView.RemoveFromSuperview();
					if (HeaderTextView.GestureRecognizers != null)
					{
						foreach (var gr in HeaderTextView.GestureRecognizers)
						{
							gr.Dispose();
						}
						HeaderTextView.GestureRecognizers = new UIGestureRecognizer[0];
					}
                    HeaderTextView.Dispose();
                    HeaderTextView = null;
                }

                if (PreviewTextView != null)
                {
					PreviewTextView.RemoveFromSuperview();
					if (PreviewTextView.GestureRecognizers != null)
					{
						foreach (var gr in PreviewTextView.GestureRecognizers)
						{
							gr.Dispose();
						}
						PreviewTextView.GestureRecognizers = new UIGestureRecognizer[0];
					}
                    PreviewTextView.Dispose();
                    PreviewTextView = null;
                }

                if (IsReadedButtonImageView != null)
                {
					IsReadedButtonImageView.RemoveFromSuperview();
					if (IsReadedButtonImageView.Image != null)
					{
						IsReadedButtonImageView.Image.Dispose();
						IsReadedButtonImageView.Image = null;
					}
                    IsReadedButtonImageView.Dispose();
                    IsReadedButtonImageView = null;
                }

                if (IsReadedButton != null)
                {
					IsReadedButton.RemoveFromSuperview();
                    IsReadedButton.Dispose();
                    IsReadedButton = null;
                }

                if (MainContainer != null)
                {
					MainContainer.RemoveFromSuperview();
                    MainContainer.Dispose();
                    MainContainer = null;
                }
            }
        }

		public override void Dispose()
		{
			base.Dispose();
			CellPushed = null;
			if (_leftContent != null)
			{
				_leftContent.Dispose();
			}
			if (_rightContent != null)
			{
				_rightContent.Dispose();
			}
			_leftContent = null;
			_rightContent = null;
		}

        public event EventHandler<DoubleCellPushedEventArgs> CellPushed;

        public override float GetDoubleContentHeight(UIView cellContentView, DoubleArticle article)
        {
            var leftContent = new Content();

            leftContent.MainContainer = new UIView(new RectangleF(0, 0, cellContentView.Frame.Width / 2, cellContentView.Frame.Height));

            CreateContentElements(leftContent.MainContainer, article.LeftContent, ItExpertHelper.LargestImageSizeInArticlesPreview, leftContent);

            Content rightContent = null;

            if (article.RightContent != null)
            {
                rightContent = new Content();

                rightContent.MainContainer = new UIView(new RectangleF(cellContentView.Frame.Width / 2, 0, cellContentView.Frame.Width / 2, cellContentView.Frame.Height));

                CreateContentElements(rightContent.MainContainer, article.RightContent, ItExpertHelper.LargestImageSizeInArticlesPreview, rightContent);
            }

            var maxHeight = GetMaxHeight(leftContent, rightContent);

            leftContent.Dispose();

            if (rightContent != null)
            {
                rightContent.Dispose();
            }

            return maxHeight;
        }

        protected override void CreateDouble(UITableViewCell cell, DoubleArticle article)
        {
            cell.ContentView.BackgroundColor = 
                ItExpertHelper.GetUIColorFromColor (ApplicationWorker.Settings.GetBackgroundColor ());

            cell.SelectionStyle = UITableViewCellSelectionStyle.Default;

            _leftContent = new Content();

            FillContent(_leftContent, new RectangleF(0, 0, cell.ContentView.Frame.Width / 2, cell.ContentView.Frame.Height), Content.Position.Left, article.LeftContent);

            var maxHeight = GetMaxHeight(_leftContent, _rightContent);
			_leftContent.MainContainer.Frame = new RectangleF(0, 0, cell.ContentView.Frame.Width / 2, maxHeight);

            AddCellElements(cell, _leftContent);

            if (article.RightContent != null)
            {
                _rightContent = new Content();

                FillContent(_rightContent, new RectangleF(cell.ContentView.Frame.Width / 2, 0, cell.ContentView.Frame.Width / 2, cell.ContentView.Frame.Height), Content.Position.Right, article.RightContent);

                maxHeight = GetMaxHeight(_leftContent, _rightContent);

                _rightContent.MainContainer.Frame = new RectangleF(cell.ContentView.Frame.Width / 2, 0, cell.ContentView.Frame.Width / 2, maxHeight);
                _rightContent.IsReadedButton.Tag = (int)Content.Position.Right;

                AddCellElements(cell, _rightContent);
            }

            _leftContent.MainContainer.Frame = new RectangleF(0, 0, cell.ContentView.Frame.Width / 2, maxHeight);
        }

        protected override void UpdateDouble(UITableViewCell cell, DoubleArticle article)
        {
            cell.ContentView.BackgroundColor = 
                ItExpertHelper.GetUIColorFromColor (ApplicationWorker.Settings.GetBackgroundColor ());

            cell.SelectionStyle = UITableViewCellSelectionStyle.Default;

            if (article.LeftContent != null)
            {
                if (_leftContent == null)
                {
                    _leftContent = new Content();

                    FillContent(_leftContent, new RectangleF(0, 0, cell.ContentView.Frame.Width / 2, cell.ContentView.Frame.Height), Content.Position.Left, article.LeftContent);
                }
                else
                {
                    _leftContent.MainContainer.Frame = new RectangleF(0, 0, cell.ContentView.Frame.Width / 2, cell.ContentView.Frame.Height);

                    UpdateContentElements(_leftContent.MainContainer, article.LeftContent, ItExpertHelper.LargestImageSizeInArticlesPreview, _leftContent);
                }
				var maxHeight = GetMaxHeight(_leftContent, null);
				_leftContent.MainContainer.Frame = new RectangleF(0, 0, cell.ContentView.Frame.Width / 2, maxHeight);
                AddCellElements(cell, _leftContent);
            }

            if (article.RightContent != null)
            {
                if (_rightContent == null)
                {
                    _rightContent = new Content();

                    FillContent(_rightContent, new RectangleF(cell.ContentView.Frame.Width / 2, 0, cell.ContentView.Frame.Width / 2, cell.ContentView.Frame.Height), Content.Position.Right, article.RightContent);
                }
                else
                {
                    _rightContent.MainContainer.Frame = new RectangleF(cell.ContentView.Frame.Width / 2, 0, cell.ContentView.Frame.Width / 2, cell.ContentView.Frame.Height);

                    UpdateContentElements(_rightContent.MainContainer, article.RightContent, ItExpertHelper.LargestImageSizeInArticlesPreview, _rightContent);
                }
				var maxHeight = GetMaxHeight(_leftContent, _rightContent);
				_rightContent.MainContainer.Frame = new RectangleF(cell.ContentView.Frame.Width / 2, 0, cell.ContentView.Frame.Width / 2, maxHeight);
                AddCellElements(cell, _rightContent);
            }
			var maxHeight2 = GetMaxHeight(_leftContent, _rightContent);
			_leftContent.MainContainer.Frame = new RectangleF(0, 0, cell.ContentView.Frame.Width / 2, maxHeight2);

        }

        private void CreateContentElements(UIView mainContainer, Article article, float largestImageWidth, Content content)
        {
            content.Article = article;

            content.IsReadedButton = new UIButton(new RectangleF(mainContainer.Frame.Width - _padding.Right - 70, 2, 70, 70));
            

            var buttonImage = new UIImage(GetIsReadedButtonImageData(content.Article.IsReaded), (float)2.5);

            content.IsReadedButtonImageView = new UIImageView(new RectangleF(content.IsReadedButton.Frame.Width - buttonImage.Size.Width , 0, buttonImage.Size.Width, buttonImage.Size.Height));
            content.IsReadedButtonImageView.Image = buttonImage;

            content.IsReadedButton.Add(content.IsReadedButtonImageView);

			var headerFont = UIFont.BoldSystemFontOfSize (ApplicationWorker.Settings.HeaderSize);
			var foreColor = ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetForeColor());
			content.HeaderTextView = ItExpertHelper.GetTextView (ItExpertHelper.GetAttributedString(article.Name, headerFont, foreColor), 
                mainContainer.Bounds.Width - _padding.Right - _padding.Left - content.IsReadedButton.Frame.Width, new PointF (_padding.Left, _padding.Top));
				
			content.HeaderTextView.BackgroundColor = ItExpertHelper.GetUIColorFromColor (ApplicationWorker.Settings.GetBackgroundColor ());
			content.HeaderTextView.Selectable = false;

			if (ApplicationWorker.Settings.LoadImages && article.PreviewPicture != null && article.PreviewPicture.Data != null)
			{
				CreateImageView(content, article, largestImageWidth, mainContainer);
			}
			else
			{
				if (content.ImageView != null)
				{
					content.ImageView.Dispose();
					content.ImageView = null;
				}
				if (content.ImageViewContainer != null)
				{
					content.ImageViewContainer.Dispose();
					content.ImageViewContainer = null;
				}
			}
			var previewFont = UIFont.SystemFontOfSize (ApplicationWorker.Settings.TextSize);

			content.PreviewTextView = ItExpertHelper.GetTextView(ItExpertHelper.GetAttributedString(article.PreviewText, previewFont, foreColor), 
                mainContainer.Bounds.Width - _padding.Right - _padding.Left, new PointF (_padding.Left, content.HeaderTextView.Frame.Bottom + _padding.Top), content.ImageViewContainer);
				
			content.PreviewTextView.BackgroundColor = ItExpertHelper.GetUIColorFromColor (ApplicationWorker.Settings.GetBackgroundColor ());
			content.PreviewTextView.Selectable = false;
		}

        private void UpdateContentElements(UIView mainContainer, Article article, float largestImageWidth, Content content)
        {
            content.Article = article;

            if (content.IsReadedButtonImageView.Image != null)
            {
                content.IsReadedButtonImageView.Image.Dispose();
                content.IsReadedButtonImageView.Image = null;
            }

            content.IsReadedButtonImageView.Image = new UIImage(GetIsReadedButtonImageData(content.Article.IsReaded), (float)2.5);

            content.IsReadedButton.Frame = new RectangleF(mainContainer.Frame.Width - _padding.Right - 70, 2, 70, 70);

			var foreColor = ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetForeColor());
			var headerFont = UIFont.BoldSystemFontOfSize(ApplicationWorker.Settings.HeaderSize);
            UpdateTextView (content.HeaderTextView, mainContainer.Bounds.Width - _padding.Right - _padding.Left - content.IsReadedButton.Frame.Width,
				headerFont, foreColor, article.Name, new PointF (_padding.Left, _padding.Top));
			content.HeaderTextView.Selectable = false;
			if (ApplicationWorker.Settings.LoadImages && article.PreviewPicture != null && article.PreviewPicture.Data != null)
			{
				if (content.ImageView != null)
				{
					if (content.ImageView.Image != null)
					{
						content.ImageView.Image.Dispose();
						content.ImageView.Image = null;
					}

					content.ImageView.Image = ItExpertHelper.GetImageFromBase64String(article.PreviewPicture.Data);
					content.ImageView.Frame = new RectangleF(ItExpertHelper.LargestImageSizeInArticlesPreview / 2 - article.PreviewPicture.Width / 2, 
						0, article.PreviewPicture.Width, article.PreviewPicture.Height);

					content.ImageViewContainer.Frame = new RectangleF(mainContainer.Bounds.Width - largestImageWidth - _padding.Right, 
						content.HeaderTextView.Frame.Bottom + _padding.Top, largestImageWidth, article.PreviewPicture.Height);
				}
				else
				{
					CreateImageView(content, article, largestImageWidth, mainContainer);
				}
			}
			else
			{
				if (content.ImageView != null)
				{
					content.ImageView.Dispose();
					content.ImageView = null;
				}
				if (content.ImageViewContainer != null)
				{
					content.ImageViewContainer.Dispose();
					content.ImageViewContainer = null;
				}
			}

			var previewFont = UIFont.SystemFontOfSize (ApplicationWorker.Settings.TextSize);
            UpdateTextView (content.PreviewTextView, mainContainer.Bounds.Width - _padding.Right - _padding.Left, 
				previewFont, foreColor, article.PreviewText, 
                new PointF (_padding.Left, content.HeaderTextView.Frame.Bottom + _padding.Top), content.ImageViewContainer);
			content.PreviewTextView.Selectable = false;
		}

        private void UpdateTextView(UITextView textViewToUpdate, float updatedTextViewWidth, UIFont font, UIColor foregroundColor, string updatedText, PointF updatedTextViewLocation, UIView imageView = null)
        {
			textViewToUpdate.BackgroundColor = ItExpertHelper.GetUIColorFromColor (ApplicationWorker.Settings.GetBackgroundColor ());

            var attributedString = ItExpertHelper.GetAttributedString (updatedText, font, foregroundColor);

            textViewToUpdate.TextStorage.SetString (attributedString);
            textViewToUpdate.AttributedText = attributedString;

            textViewToUpdate.TextContainer.Size = new Size ((int)updatedTextViewWidth, int.MaxValue);

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
            var button = (sender as UIButton);

            Content content = null;

            if (button.Tag == (int) Content.Position.Left && _leftContent != null)
            {
                content = _leftContent;
            }
            else if (_rightContent != null)
            {
                content = _rightContent;
            }

            content.Article.IsReaded = !content.Article.IsReaded;

            content.IsReadedButtonImageView.Image = new UIImage(GetIsReadedButtonImageData(content.Article.IsReaded), (float)2.5);

            ArticlesTableSource.SetIsReadedForArticle(content.Article);
        }

        private float GetMaxHeight(Content leftContent, Content rightContent)
        {
            float maxLeftContentHeight = 0;

            if (leftContent.ImageViewContainer != null)
            {
                maxLeftContentHeight = Math.Max(leftContent.PreviewTextView.Frame.Bottom, leftContent.ImageViewContainer.Frame.Bottom);
            }
            else
            {
                maxLeftContentHeight = leftContent.PreviewTextView.Frame.Bottom;
            }

            float maxRightContentHeight = 0;

            if (rightContent != null)
            {
                if (rightContent.ImageViewContainer != null)
                {
                    maxRightContentHeight = Math.Max(rightContent.PreviewTextView.Frame.Bottom, rightContent.ImageViewContainer.Frame.Bottom);
                }
                else
                {
                    maxRightContentHeight = rightContent.PreviewTextView.Frame.Bottom;
                }
            }

            return Math.Max(maxLeftContentHeight + _padding.Bottom, maxRightContentHeight + _padding.Bottom);
        }

        private void FillContent(Content content, RectangleF frame, Content.Position contentPosition, Article article)
        {
            content.MainContainer = new UIView(frame);

            CreateContentElements(content.MainContainer, article, ItExpertHelper.LargestImageSizeInArticlesPreview, content);

            content.IsReadedButton.Tag = (int)contentPosition;
        }

        private void CreateImageView(Content content, Article article, float largestImageWidth, UIView mainContainer)
        {
            content.ImageView = new UIImageView(new RectangleF (largestImageWidth/2 - article.PreviewPicture.Width / 2, 
                0, article.PreviewPicture.Width, article.PreviewPicture.Height));

            content.ImageView.Image = ItExpertHelper.GetImageFromBase64String(article.PreviewPicture.Data);

            content.ImageViewContainer = new UIView (new RectangleF (mainContainer.Bounds.Width - largestImageWidth - _padding.Right, 
                content.HeaderTextView.Frame.Bottom + _padding.Top, largestImageWidth, article.PreviewPicture.Height));

            content.ImageViewContainer.Add (content.ImageView);
        }

		private void AddCellElements(UITableViewCell cell, Content content)
		{
			ItExpertHelper.RemoveSubviews(content.MainContainer);

			var headerTap = new UITapGestureRecognizer(() =>
			{
				var handler = Interlocked.CompareExchange(ref CellPushed, null, null);
				if (handler != null)
				{
					handler(content.MainContainer, new DoubleCellPushedEventArgs(content.Article));
				}
			});
			var previewTap = new UITapGestureRecognizer(() =>
			{
				if (CellPushed != null)
				{
					CellPushed(content.MainContainer, new DoubleCellPushedEventArgs(content.Article));
				}
			});
			var frame = content.MainContainer.Frame;
			var doublePortalView = new DoublePortalView(frame, content, OnReadedButtonTouchUpInside, headerTap, previewTap);
			cell.ContentView.Add(doublePortalView);
			cell.ContentView.BringSubviewToFront(doublePortalView);
		}

        public override float GetContentHeight(UIView cellContentView, Article article)
        {
            throw new NotImplementedException();
        }

        protected override void Create(UITableViewCell cell, Article article)
        {
            throw new NotImplementedException();
        }

        protected override void Update(UITableViewCell cell, Article article)
        {
            throw new NotImplementedException();
        }

        private Content _leftContent;
        private Content _rightContent;
    }

	public class DoublePortalView : UIView, ICleanupObject
	{
		private DoublePortalContentCreator.Content _content;
		private EventHandler _isReaderClick;
		private UITapGestureRecognizer _headerTap;
		private UITapGestureRecognizer _previewTap;

		public DoublePortalView(RectangleF frame, DoublePortalContentCreator.Content content,
			EventHandler isReadedClick, UITapGestureRecognizer headerTap, UITapGestureRecognizer previewTap): base(frame)
		{
			_headerTap = headerTap;
			_previewTap = previewTap;
			_content = content;
			_isReaderClick = isReadedClick;
			_content.IsReadedButton.TouchUpInside += _isReaderClick;
			Add (_content.IsReadedButton);
			Add (_content.HeaderTextView);
			Add (_content.PreviewTextView);
			if (content.ImageViewContainer != null)
			{
				Add(content.ImageViewContainer);
			}
			_content.HeaderTextView.UserInteractionEnabled = true;
			_content.PreviewTextView.UserInteractionEnabled = true;
			_content.HeaderTextView.AddGestureRecognizer(_headerTap);
			_content.PreviewTextView.AddGestureRecognizer(_previewTap);
			BringSubviewToFront(_content.IsReadedButton);
		}

		public void CleanUp()
		{
			foreach (var view in Subviews)
			{
				view.RemoveFromSuperview();
			}
			if (_content != null)
			{
				var content = _content;
				content.IsReadedButton.TouchUpInside -= _isReaderClick;
			}
			if (_content.PreviewTextView != null && _content.PreviewTextView.GestureRecognizers != null)
			{
				foreach (var gr in _content.PreviewTextView.GestureRecognizers)
				{
					gr.Dispose();
				}
				_content.PreviewTextView.GestureRecognizers = new UIGestureRecognizer[0];
			}
			if (_content.HeaderTextView != null && _content.HeaderTextView.GestureRecognizers != null)
			{
				foreach (var gr in _content.HeaderTextView.GestureRecognizers)
				{
					gr.Dispose();
				}
				_content.HeaderTextView.GestureRecognizers = new UIGestureRecognizer[0];
			}
			_headerTap = null;
			_previewTap = null;
			_content = null;
			_isReaderClick = null;
		}
	}
}

