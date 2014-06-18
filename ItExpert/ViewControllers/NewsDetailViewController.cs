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

            AutomaticallyAdjustsScrollViewInsets = false;

			_padding = new UIEdgeInsets (8, 8, 8, 8);

			View.BackgroundColor = UIColor.White;
            View.UserInteractionEnabled = true;

            _contentScrollView = new UIScrollView(new RectangleF(0, NavigationController.NavigationBar.Frame.Height + ItManagerHelper.StatusBarHeight, 
                View.Frame.Width, View.Frame.Height));

            _contentScrollView.ScrollEnabled = true;
            _contentScrollView.UserInteractionEnabled = true;

            View.Add(_contentScrollView);

			AddArticleSection ();
		}

		private void AddArticleSection ()
        {
            var section = string.Join(@"/", _article.Sections.OrderBy(x => x.DepthLevel).Select(x => x.Section.Name));

			if (!String.IsNullOrEmpty(section))
			{
                _artcleSectionWebView = new UIWebView (new RectangleF (_padding.Left, _padding.Top, 
                    View.Frame.Width - _padding.Left - _padding.Right, 1));

                _artcleSectionWebView.Delegate = new WebViewDelegate();

                var htmlSectionName = String.Format ("<body><p style = \"font-size: {0};\"><a href=\"URL\">{1}</a><p></body>", ApplicationWorker.Settings.DetailHeaderSize, section);

                _artcleSectionWebView.LoadHtmlString (htmlSectionName, null);

                _contentScrollView.Add (_artcleSectionWebView);
			}
		}

		private void AddArticlePicture ()
		{
            if (ApplicationWorker.Settings.LoadImages && _article.DetailPicture != null)
            {
                var screenSize = UIScreen.MainScreen.ApplicationFrame.Size;

                var image = ItManagerHelper.GetImageFromBase64String(_article.DetailPicture.Data);

            }
		}

		private void AddArticleHeader ()
		{

		}

		private void AddArticleText ()
		{
		}

		private Article _article;
		private UIEdgeInsets _padding;

        private UIScrollView _contentScrollView;
        private UIWebView _artcleSectionWebView;
		private UIImageView _articleImageView;
		private UILabel _articleHeaderLabel;
		private UILabel _articleAuthorLabel;
		private UIWebView _articleTextWebView;
	}
}

