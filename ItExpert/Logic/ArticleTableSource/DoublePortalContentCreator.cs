using System;
using ItExpert.Model;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;

namespace ItExpert
{
    public class DoublePortalContentCreator : BaseContentCreator
    {
        internal class Content: IDisposable
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
                    ImageView.Dispose();
                    ImageView = null;
                }

                if (ImageViewContainer != null)
                {
                    ImageViewContainer.Dispose();
                    ImageViewContainer = null;
                }

                if (HeaderTextView != null)
                {
                    HeaderTextView.Dispose();
                    HeaderTextView = null;
                }

                if (PreviewTextView != null)
                {
                    PreviewTextView.Dispose();
                    PreviewTextView = null;
                }

                if (IsReadedButtonImageView != null)
                {
                    IsReadedButtonImageView.Dispose();
                    IsReadedButtonImageView = null;
                }

                if (IsReadedButton != null)
                {
                    IsReadedButton.Dispose();
                    IsReadedButton = null;
                }

                if (MainContainer != null)
                {
                    MainContainer.Dispose();
                    MainContainer = null;
                }

				GC.Collect ();
            }
        }

        public event EventHandler<DoubleCellPushedEventArgs> CellPushed;

        public override float GetDoubleContentHeight(UIView cellContentView, DoubleArticle article)
        {
            var leftContent = new Content();

            leftContent.MainContainer = new UIView(new RectangleF(0, 0, cellContentView.Frame.Width / 2, cellContentView.Frame.Height));

            CreateContnentElements(leftContent.MainContainer, article.LeftContent, ItExpertHelper.LargestImageSizeInArticlesPreview, leftContent);

            Content rightContent = null;

            if (article.RightContent != null)
            {
                rightContent = new Content();

                rightContent.MainContainer = new UIView(new RectangleF(cellContentView.Frame.Width / 2, 0, cellContentView.Frame.Width / 2, cellContentView.Frame.Height));

                CreateContnentElements(rightContent.MainContainer, article.RightContent, ItExpertHelper.LargestImageSizeInArticlesPreview, rightContent);
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

            AddCellElements(cell.ContentView, _leftContent);

            if (article.RightContent != null)
            {
                _rightContent = new Content();

                FillContent(_rightContent, new RectangleF(cell.ContentView.Frame.Width / 2, 0, cell.ContentView.Frame.Width / 2, cell.ContentView.Frame.Height), Content.Position.Right, article.RightContent);

                maxHeight = GetMaxHeight(_leftContent, _rightContent);

                _rightContent.MainContainer.Frame = new RectangleF(cell.ContentView.Frame.Width / 2, 0, cell.ContentView.Frame.Width / 2, maxHeight);
                _rightContent.IsReadedButton.Tag = (int)Content.Position.Right;

                UITapGestureRecognizer rightTap = new UITapGestureRecognizer(() =>
                {
                    if (CellPushed != null)
                    {
                        CellPushed(_rightContent.MainContainer, new DoubleCellPushedEventArgs(_rightContent.Article));
                    }
                });

                _rightContent.MainContainer.AddGestureRecognizer(rightTap);

                AddCellElements(cell.ContentView, _rightContent);
            }

            _leftContent.MainContainer.Frame = new RectangleF(0, 0, cell.ContentView.Frame.Width / 2, maxHeight);

            UITapGestureRecognizer leftTap = new UITapGestureRecognizer(() =>
            {
                if (CellPushed != null)
                {
                    CellPushed(_leftContent.MainContainer, new DoubleCellPushedEventArgs(_leftContent.Article));
                }
            });

            _leftContent.MainContainer.AddGestureRecognizer(leftTap);
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

                AddCellElements(cell.ContentView, _leftContent);
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

                AddCellElements(cell.ContentView, _rightContent);
            }
        }

        private void CreateContnentElements(UIView mainContainer, Article article, float largestImageWidth, Content content)
        {
            content.Article = article;

            content.IsReadedButton = new UIButton(new RectangleF(mainContainer.Frame.Width - _padding.Right - 70, 2, 70, 60));
            content.IsReadedButton.TouchUpInside += OnReadedButtonTouchUpInside;

            var buttonImage = new UIImage(GetIsReadedButtonImageData(content.Article.IsReaded), (float)2.5);

            content.IsReadedButtonImageView = new UIImageView(new RectangleF(content.IsReadedButton.Frame.Width - buttonImage.Size.Width , 0, buttonImage.Size.Width, buttonImage.Size.Height));
            content.IsReadedButtonImageView.Image = buttonImage;

            content.IsReadedButton.Add(content.IsReadedButtonImageView);

			var headerFont = UIFont.BoldSystemFontOfSize (ApplicationWorker.Settings.HeaderSize);

			content.HeaderTextView = ItExpertHelper.GetTextView (ItExpertHelper.GetAttributedString(article.Name, headerFont, _forecolor), 
                mainContainer.Bounds.Width - _padding.Right - _padding.Left - content.IsReadedButton.Frame.Width, new PointF (_padding.Left, _padding.Top));

            content.HeaderTextView.UserInteractionEnabled = true;

            if (article.PreviewPicture != null && article.PreviewPicture.Data != null)
            {
                CreateImageView(content, article, largestImageWidth, mainContainer);
            }

			var previewFont = UIFont.SystemFontOfSize (ApplicationWorker.Settings.TextSize);

			content.PreviewTextView = ItExpertHelper.GetTextView(ItExpertHelper.GetAttributedString(article.PreviewText, previewFont, _forecolor), 
                mainContainer.Bounds.Width - _padding.Right - _padding.Left, new PointF (_padding.Left, content.HeaderTextView.Frame.Bottom + _padding.Top), content.ImageViewContainer);

            content.PreviewTextView.UserInteractionEnabled = true;
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

            content.IsReadedButton.Frame = new RectangleF(mainContainer.Frame.Width - _padding.Right - 70, 2, 70, 60);

			var headerFont = UIFont.BoldSystemFontOfSize (ApplicationWorker.Settings.HeaderSize);
            UpdateTextView (content.HeaderTextView, mainContainer.Bounds.Width - _padding.Right - _padding.Left - content.IsReadedButton.Frame.Width,
				headerFont, _forecolor, article.Name, new PointF (_padding.Left, _padding.Top));

            if (article.PreviewPicture != null && article.PreviewPicture.Data != null)
            {
                if (content.ImageView != null && content.ImageView.Image != null)
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
                content.ImageView.Image.Dispose();
                content.ImageView.Image = null;

                content.ImageView.Dispose();
                content.ImageView = null;

                content.ImageViewContainer.Dispose();
                content.ImageViewContainer = null;
            }

			var previewFont = UIFont.SystemFontOfSize (ApplicationWorker.Settings.TextSize);
            UpdateTextView (content.PreviewTextView, mainContainer.Bounds.Width - _padding.Right - _padding.Left, 
                previewFont, _forecolor, article.PreviewText, 
                new PointF (_padding.Left, content.HeaderTextView.Frame.Bottom + _padding.Top), content.ImageViewContainer);
        }

        private void AddCellElements(UIView cellContentView, Content content)
        {
            content.MainContainer.Add (content.IsReadedButton);
            content.MainContainer.Add (content.HeaderTextView);
            content.MainContainer.Add (content.PreviewTextView);
            content.MainContainer.Add (content.ImageViewContainer);

            content.IsReadedButton.BringSubviewToFront(content.MainContainer);

            cellContentView.Add(content.MainContainer);
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
            content.MainContainer.UserInteractionEnabled = true;

            CreateContnentElements(content.MainContainer, article, ItExpertHelper.LargestImageSizeInArticlesPreview, content);

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

            content.ImageView.UserInteractionEnabled = true;
            content.ImageViewContainer.UserInteractionEnabled = true;
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
}

