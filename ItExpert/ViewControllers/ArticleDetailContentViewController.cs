using System;
using MonoTouch.UIKit;
using System.Drawing;
using System.Linq;
using ItExpert.Model;
using ItExpert.Enum;
using System.Text;

namespace ItExpert
{
	public class ArticleDetailContentViewController: UIViewController
	{
		#region Fields

		private Article _article;
		private int _articleId;
		private ArticleDetailsViewController _parent;
		private float _maxWidth;
		private float _navigationBarHeight;
		private RectangleF _initalFrame;
		private bool _firstLoad = true;

		private UIEdgeInsets _padding;
		private UIScrollView _scrollView;
		private UITextView _articleSectionView;
		private UIView _articleImagesContainer;
		private UIImageView _articleImageView;
		private UIImageView _articleAwardImageView;
		private UITextView _articleHeaderView;
		private UITextView _articleAuthorView;
		private UIWebView _articleTextWebView;
		private UIView _splashScreen;
		private UITextView _titleTextView;

		#endregion

		public ArticleDetailContentViewController(int articleId, ArticleDetailsViewController parent, float navigationBarHeight, RectangleF initalFrame)
		{
			_articleId = articleId;
			_parent = parent;
			_navigationBarHeight = navigationBarHeight;
			_initalFrame = initalFrame;
		}

		static bool UserInterfaceIdiomIsPhone {
			get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			AutomaticallyAdjustsScrollViewInsets = false;
			View.BackgroundColor = ItExpertHelper.GetUIColorFromColor (ApplicationWorker.Settings.GetBackgroundColor ());
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			ItExpertHelper.RemoveSubviews(View);
			ShowSplash(true);
			Initialize();
		}

		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate(fromInterfaceOrientation);
			_initalFrame = View.Frame;
			_maxWidth = _initalFrame.Width - _padding.Left - _padding.Right;
			ItExpertHelper.RemoveSubviews(View);
			_splashScreen.Dispose();
			_splashScreen = null;
			UpdateScreen();
		}

		void Initialize()
		{
			_padding = new UIEdgeInsets (8, 8, 8, 8);
			if (_firstLoad)
			{
				_firstLoad = false;
			}
			else
			{
				_initalFrame = View.Frame;
			}
			_maxWidth = _initalFrame.Width - _padding.Left - _padding.Right;
			GetArticleData();
		}

		public void OnSettingsChanged()
		{
			ItExpertHelper.RemoveSubviews(View);
			View.BackgroundColor = ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetBackgroundColor());
			if (_splashScreen != null)
			{
				_splashScreen.BackgroundColor = ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetBackgroundColor());
				if (_titleTextView != null)
				{
					_titleTextView.TextColor = ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetForeColor());
				}
			}
			UpdateScreen();
		}

		#region Server interaction

		private void GetArticleData()
		{
			var requestNeeded = false;
			var article = ApplicationWorker.GetArticle(_articleId);
			if (article != null)
			{
				_article = article;
				if (!ApplicationWorker.Settings.OfflineMode)
				{
					if ((ApplicationWorker.Settings.LoadImages && article.DetailPicture == null) ||
						string.IsNullOrWhiteSpace(article.DetailText))
					{
						requestNeeded = true;
					}
				}
				else
				{
					if (string.IsNullOrWhiteSpace(article.DetailText))
					{
						requestNeeded = true;
					}
				}
			}
			else
			{
				_article = null;
				requestNeeded = true;
			}
			if (!requestNeeded)
			{
				UpdateScreen();
			}
			else
			{
				if (_parent != null)
				{
					_parent.GetArticleFromServer(_article);
				}
			}
		}

		#endregion

		#region Article present level

		public int GetArticleId()
		{
			return _article.Id;
		}

		public void SetArticle(Article article)
		{
			_article = article;
			UpdateScreen();
		}

		private void ShowSplash(bool isVisible)
		{
			if (_splashScreen == null)
			{
				_splashScreen = new UIView(_initalFrame);
				_splashScreen.BackgroundColor = ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetBackgroundColor());
				_titleTextView = ItExpertHelper.GetTextView(ItExpertHelper.GetAttributedString("Загрузка статьи...", 
					UIFont.BoldSystemFontOfSize(ApplicationWorker.Settings.HeaderSize), 
					ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetForeColor())), _initalFrame.Width, 
					new PointF(0, 0));

				_titleTextView.Frame = new RectangleF(_initalFrame.Width / 2 - _titleTextView.Frame.Width / 2, 150, _titleTextView.Frame.Width, _titleTextView.Frame.Height);

				_splashScreen.Add(_titleTextView);
			}

			if (isVisible)
			{
				if (!View.Subviews.Any(s => s == _splashScreen))
				{
					View.Add(_splashScreen);
				}
			}
			else
			{
				_splashScreen.RemoveFromSuperview();
			}
		}

		private void UpdateScreen()
		{
			ApplicationWorker.SharedArticle = _article;
			var article = _article;
			if (string.IsNullOrWhiteSpace(article.DetailText))
			{
				if (_parent != null)
				{
					_parent.ArticleDetailTextNotAvailable(article);
					return;
				}
			}
			var sectionString = string.Empty;
			if (article.ArticleType == ArticleType.Portal)
			{
				if (article.Sections != null && article.Sections.Any())
				{
					var section = string.Join(@"/",
						article.Sections.OrderBy(x => x.DepthLevel).Select(x => x.Section.Name));
					if (!string.IsNullOrWhiteSpace(section))
					{
						sectionString = section;
					}
				}
			}
			if (article.ArticleType == ArticleType.Magazine)
			{
				var rubricsId = article.RubricsId;
				if (!string.IsNullOrWhiteSpace(rubricsId))
				{
					var rubrics = article.Rubrics;
					var rubricText = string.Join("/", rubrics.Select(x => x.Name));
					sectionString = rubricText;
				}
			}
			var articleAuthors = string.Empty;
			if (article.Authors != null && article.Authors.Any())
			{
				articleAuthors = string.Join("/", article.Authors.Select(x => x.Name));
			}
			var text = ApplicationWorker.NormalizeDetailText(article, (int)_maxWidth);
			var video = string.Empty;
			if (!string.IsNullOrWhiteSpace(article.Video))
			{
				video = "<br/><a href='" + article.Video + "'>Видео</a>";
			}
			var css = string.Empty;
			if (!string.IsNullOrWhiteSpace(ApplicationWorker.Css))
			{
				css = "<style>" + ApplicationWorker.Css + "</style>";
			}
			//В style я передаю строку стиля HTML с цветом и размером текста и цветом фона
			var foreColor = ColorToCssRgb(ApplicationWorker.Settings.GetForeColor());
			var backgroundColor = ColorToCssRgb(ApplicationWorker.Settings.GetBackgroundColor());
			var fontSize = ((int)(ApplicationWorker.Settings.DetailTextSize * 1.35f)).ToString("G");

			var names = UIFont.FamilyNames;
			names = names.OrderBy(x => x).ToArray();
			var optionsSb = new StringBuilder();
			foreach (var name in names)
			{
				var selected = (name == "Verdana") ? "selected" : string.Empty;
				optionsSb.Append("<option value='"+name+"' " +selected + ">" + name + "</option>");
			}
			var fontSelectorSb = new StringBuilder();
			fontSelectorSb.Append("<select size='1' onchange='changeFont(this.options[this.selectedIndex].value)'>");
			fontSelectorSb.Append(optionsSb.ToString());
			fontSelectorSb.Append("</select>");
			var changeFontScript = @"<script>
				function changeFont(font){
					var body = document.getElementsByTagName('body')[0];
					body.style.fontFamily = font;
				}
				</script>";

			var style = "background-color: " + backgroundColor + "; color: " + foreColor + "; font-size: " + fontSize +
				"px; font-family: Verdana;";
			//sectionString - раздел , articleAuthors-авторы, если пустые строки то не отображать
			var html = "<html><head>" + css + changeFontScript + "</head><body style='" + style + "'>" + fontSelectorSb + text + video + "</body></html>";

			//После полного отображения выставить флаг и убрать сплаш
			//Прокрутить представление до самого верха
			AddContent(sectionString, articleAuthors, html);
			ShowSplash (false);
		}

		private void AddContent(string section, string author, string text)
		{
			_scrollView = new UIScrollView(new RectangleF(0, 0, _initalFrame.Width, _initalFrame.Height));
			_scrollView.UserInteractionEnabled = true;
			_scrollView.ScrollEnabled = true;
			_scrollView.Bounces = false;

			View.Add(_scrollView);
			View.UserInteractionEnabled = true;

			float top = AddArticleSection(section, _navigationBarHeight + ItExpertHelper.StatusBarHeight + _padding.Top);
			top = AddArticlePicture(top);
			top = AddArticleHeader(top);
			top = AddArticleAuthor(author, top);
			AddArticleText(text, top);

			if (_articleTextWebView == null)
			{
				_scrollView.ContentSize = new SizeF(_maxWidth, top);
			}
		}

		private float AddArticleSection (string section, float top)
		{
			if (!String.IsNullOrWhiteSpace(section))
			{
				_articleSectionView = ItExpertHelper.GetTextView(
					ItExpertHelper.GetAttributedString(section, UIFont.BoldSystemFontOfSize(ApplicationWorker.Settings.DetailHeaderSize), UIColor.Blue, true),
					_maxWidth, new PointF(0, 4));
				var viewWrapper = new UIView(new RectangleF(new PointF(_padding.Left, top), new SizeF(_articleSectionView.Frame.Width, _articleSectionView.Frame.Height + 8)));
				viewWrapper.Add(_articleSectionView);
				viewWrapper.UserInteractionEnabled = true;
				UITapGestureRecognizer tap = new UITapGestureRecognizer(() =>
				{
					if (_parent != null)
					{
						var sectionString = section.Split(new []{ "/" }, StringSplitOptions.RemoveEmptyEntries).Last();
						_parent.ShowFilterSectionDialog(sectionString);
					}
				});

				viewWrapper.AddGestureRecognizer (tap);
				_scrollView.Add(viewWrapper);

				return viewWrapper.Frame.Bottom + _padding.Bottom;
			}

			return top;
		}

		private float AddArticlePicture(float top)
		{
			if (ApplicationWorker.Settings.LoadImages && _article.DetailPicture != null && _article.DetailPicture.Data != null)
			{
				float scale = 1;

				var picture = _article.DetailPicture;

				if (picture.Width > (_initalFrame.Width - _padding.Left - _padding.Right))
				{
					scale = picture.Width / (_initalFrame.Width - _padding.Left - _padding.Right);
				}

				if (picture.Height > _initalFrame.Height * 0.35)
				{
					float heightScale = picture.Height / (_initalFrame.Height * 0.35f);

					if (heightScale > scale)
					{
						scale = heightScale;
					}
				}

				if (_article.AwardsPicture != null && _article.AwardsPicture.Data != null)
				{
					UIImage awardImage = ItExpertHelper.GetImageFromBase64String(_article.AwardsPicture.Data);

					_articleAwardImageView = new UIImageView(new RectangleF(0, 0, awardImage.Size.Width, awardImage.Size.Height));

					_articleAwardImageView.Image = awardImage;
				}

				UIImage previewImage = ItExpertHelper.GetImageFromBase64String(picture.Data, scale);

				_articleImageView = new UIImageView(new RectangleF(0, 0, previewImage.Size.Width, previewImage.Size.Height));

				_articleImageView.Image = previewImage;

				_articleImagesContainer = new UIView(new RectangleF(_initalFrame.Width / 2 - _articleImageView.Frame.Width / 2,
					top, _articleImageView.Frame.Width, _articleImageView.Frame.Height));

				_articleImagesContainer.Add(_articleImageView);

				if (_articleAwardImageView != null)
				{
					_articleImagesContainer.Add(_articleAwardImageView);
				}

				_scrollView.Add(_articleImagesContainer);

				return _articleImagesContainer.Frame.Bottom + _padding.Bottom;
			}

			return top;
		}

		private float AddArticleHeader(float top)
		{
			if (!String.IsNullOrWhiteSpace(_article.Name))
			{
				_articleHeaderView = ItExpertHelper.GetTextView(
					ItExpertHelper.GetAttributedString(_article.Name, UIFont.BoldSystemFontOfSize(ApplicationWorker.Settings.DetailHeaderSize), 
						ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetForeColor())), _maxWidth, new PointF(_padding.Left, top));

				_scrollView.Add(_articleHeaderView);

				return _articleHeaderView.Frame.Bottom + _padding.Bottom;
			}

			return top;
		}

		private float AddArticleAuthor(string author, float top)
		{
			if (!String.IsNullOrWhiteSpace(author))
			{
				_articleAuthorView = ItExpertHelper.GetTextView(
					ItExpertHelper.GetAttributedString(author, UIFont.BoldSystemFontOfSize(ApplicationWorker.Settings.DetailHeaderSize), UIColor.Blue, true),
					_maxWidth, new PointF(0, 4));
				var viewWrapper = new UIView(new RectangleF(new PointF(_padding.Left, top), new SizeF(_articleAuthorView.Frame.Width, _articleAuthorView.Frame.Height + 8)));
				viewWrapper.Add(_articleAuthorView);

				viewWrapper.UserInteractionEnabled = true;

				UITapGestureRecognizer tap = new UITapGestureRecognizer (() =>
				{
					if (_parent != null)
					{
						var authors = _article.Authors;
						_parent.ShowFilterAuthorDialog(authors);
					}
				});

				viewWrapper.AddGestureRecognizer (tap);
				_scrollView.Add(viewWrapper);

				return viewWrapper.Frame.Bottom + _padding.Bottom;
			}

			return top;
		}

		private void AddArticleText(string text, float top)
		{
			if (!String.IsNullOrWhiteSpace(text))
			{
				_articleTextWebView = new UIWebView(new RectangleF(_padding.Left,
					top - 4, _maxWidth, 1));                   

				var webViewDelegate = new WebViewDelegate(); 
				webViewDelegate.WebViewLoaded += OnWebViewLoaded;

				_articleTextWebView.Delegate = webViewDelegate;
				_articleTextWebView.ScrollView.ScrollEnabled = false;
				_articleTextWebView.ScrollView.Bounces = false;
				_articleTextWebView.LoadHtmlString(text, null);
				_scrollView.Add(_articleTextWebView);
			}
		}

		private string ColorToCssRgb(Color color)
		{
			var sb = new StringBuilder();
			sb.Append("rgb(");
			sb.Append(color.R.ToString("G"));
			sb.Append(", ");
			sb.Append(color.G.ToString("G"));
			sb.Append(", ");
			sb.Append(color.B.ToString("G"));
			sb.Append(")");
			return sb.ToString();
		}

		private void OnWebViewLoaded(object sender, EventArgs e)
		{
			_scrollView.ContentSize = new SizeF(_maxWidth, _articleTextWebView.Frame.Bottom);
		}

		#endregion

	}
}

