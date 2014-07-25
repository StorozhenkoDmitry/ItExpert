using System;
using MonoTouch.UIKit;
using ItExpert.Model;
using System.Drawing;

namespace ItExpert
{
    public class HeaderContentCreator : BaseContentCreator
    {
        public override float GetContentHeight(UIView cellContentView, Article article)
        {
            var textViewHeight = ItExpertHelper.GetTextHeight(UIFont.BoldSystemFontOfSize(ApplicationWorker.Settings.HeaderSize), article.Name, 
                                     cellContentView.Bounds.Width - _padding.Right - _padding.Left);

            return _padding.Top + textViewHeight + _padding.Bottom;
        }

        protected override void Create(UITableViewCell cell, Article article)
        {
            _headerTextView = ItExpertHelper.GetTextView (
                ItExpertHelper.GetAttributedString(article.Name, UIFont.BoldSystemFontOfSize(ApplicationWorker.Settings.HeaderSize), 
					UIColor.Black), 
                cell.ContentView.Bounds.Width - _padding.Right - _padding.Left, new PointF (_padding.Left, _padding.Top));

            _headerTextView.BackgroundColor = UIColor.Clear;

            _backgroundView = new UIView(new RectangleF(0, 0, cell.ContentView.Bounds.Width, _padding.Top + _headerTextView.Frame.Height + _padding.Bottom));

            _backgroundView.BackgroundColor = UIColor.FromRGB(160, 160, 160);

            _backgroundView.Add(_headerTextView);

            cell.ContentView.Add(_backgroundView);

            cell.UserInteractionEnabled = false;
        }

        protected override void Update(UITableViewCell cell, Article article)
        {
            var attributedString = ItExpertHelper.GetAttributedString (article.Name, UIFont.BoldSystemFontOfSize(ApplicationWorker.Settings.HeaderSize), 
				UIColor.Black);

            _headerTextView.TextStorage.SetString (attributedString);
            _headerTextView.AttributedText = attributedString;

            _headerTextView.TextContainer.Size = new Size ((int)(cell.ContentView.Bounds.Width - _padding.Right - _padding.Left), int.MaxValue);

            var range = _headerTextView.LayoutManager.GetGlyphRange (_headerTextView.TextContainer);
            var containerSize = _headerTextView.LayoutManager.BoundingRectForGlyphRange (range, _headerTextView.TextContainer);


            _headerTextView.Frame = new RectangleF(_padding.Left, _padding.Top, containerSize.Width, containerSize.Height);
			_backgroundView.Frame = new RectangleF (0, 0, cell.ContentView.Bounds.Width, _padding.Top + _headerTextView.Frame.Height + _padding.Bottom);
            cell.ContentView.Add(_backgroundView);

            cell.UserInteractionEnabled = false;
        }

        private UIView _backgroundView;
        private UITextView _headerTextView;
    }
}

