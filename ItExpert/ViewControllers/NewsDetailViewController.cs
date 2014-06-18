using System;
using System.Drawing;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using ItExpert.Model;

namespace ItExpert
{
	public class NewsDetailsViewController: UIViewController
	{
		public NewsDetailsViewController (Article article)
		{
			_article = article;
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();



			_padding = new UIEdgeInsets (8, 8, 8, 8);

			View.BackgroundColor = UIColor.White;
			AddArticleSection ();
		}

		private void AddArticleSection ()
		{
			var section = _article.Sections.LastOrDefault ();

			if (section != null && !String.IsNullOrEmpty(section.Section.Name))
			{
				//				var textView = ItManagerHelper.GetTextView (UIFont.BoldSystemFontOfSize (ApplicationWorker.Settings.DetailHeaderSize), 
				//					               ItManagerHelper.GetUIColorFromColor (ApplicationWorker.Settings.GetForeColor ()), section.Section.Name,
				//					               View.Frame.Width - _padding.Left - _padding.Right, new PointF (0, 0));
				var textView = new UITextView(new RectangleF(20,20, 300, 200));
				textView.Editable = false;
				View.AddSubview (textView);
				textView.AttributedText = new NSAttributedString (section.Section.Name, new UIStringAttributes {
					ForegroundColor = UIColor.Blue,
					Font = UIFont.FromName ("Courier", 18f),
					UnderlineStyle = NSUnderlineStyle.Single
				});
				//				float textHeight = textView.Frame.Height;
				//
				//				textView.Dispose ();
				//				textView = null;

				//				_articleSectionLabel = new UILabel(new RectangleF(_padding.Left, 
				//					NavigationController.NavigationBar.Frame.Height + _padding.Top + ItManagerHelper.StatusBarHeight, 
				//					View.Frame.Width - _padding.Left - _padding.Right, textHeight));
				//
				//				_articleSectionLabel.AttributedText = ItManagerHelper.GetAttributedString(section.Section.Name, 
				//					UIFont.BoldSystemFontOfSize (ApplicationWorker.Settings.DetailHeaderSize), UIColor.Blue, true);
				//
				//				View.AddSubview (_articleSectionLabel);

				//				UIWebView sectionWebView = new UIWebView (new RectangleF (_padding.Left, 
				//					NavigationController.NavigationBar.Frame.Height + _padding.Top + ItManagerHelper.StatusBarHeight, 
				//					View.Frame.Width - _padding.Left - _padding.Right, 
				//					textHeight + 20));
				////				{
				////					BackgroundColor = UIColor.White
				////				};
				//				AutomaticallyAdjustsScrollViewInsets = false;
				//				sectionWebView.ScrollView.ScrollEnabled = false;
				//				sectionWebView.ScrollView.Bounces = false;
				//
				//				var htmlSectionName = String.Format ("<body><a href=\"URL\">olololo!!!</a></body>", section.Section.Name);
				//
				//				sectionWebView.LoadHtmlString (htmlSectionName, null);
				//
				//				View.Add (sectionWebView);
			}
		}

		private void AddArticlePicture ()
		{
		}

		private void AddArticleHeader ()
		{

		}

		private void AddArticleText ()
		{
		}

		private Article _article;
		private UIEdgeInsets _padding;

		private UILabel _articleSectionLabel;
		private UIImageView _articleImageView;
		private UILabel _articleHeaderLabel;
		private UILabel _articleAuthorLabel;
		private UIWebView _articleTextWebView;
	}
}

