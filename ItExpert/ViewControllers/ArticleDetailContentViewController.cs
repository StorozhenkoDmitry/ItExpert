using System;
using MonoTouch.UIKit;
using System.Drawing;
using System.Linq;
using ItExpert.Model;
using ItExpert.Enum;
using System.Text;
using ItExpert.ServiceLayer;
using BigTed;
using System.Threading;

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
		private UIView _sectionWrapper;
		private UIView _authorWrapper;

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
			if (_articleTextWebView != null)
			{
				if (_articleTextWebView.Delegate != null)
				{
					var del = _articleTextWebView.Delegate as WebViewDelegate;
					if (del != null)
					{
						del.WebViewLoaded -= OnWebViewLoaded;
					}
					_articleTextWebView.Delegate.Dispose();
				}
			}
			if (_authorWrapper != null)
			{
				if (_authorWrapper.GestureRecognizers != null)
				{
					foreach (var gr in _authorWrapper.GestureRecognizers)
					{
						gr.Dispose();
					}
				}
			}
			if (_sectionWrapper != null)
			{
				if (_sectionWrapper.GestureRecognizers != null)
				{
					foreach (var gr in _sectionWrapper.GestureRecognizers)
					{
						gr.Dispose();
					}
				}
			}
			ItExpertHelper.RemoveSubviews(View);
			_splashScreen.Dispose();
			_splashScreen = null;
			UpdateScreen();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			InvokeOnMainThread(() =>
			{
				ApplicationWorker.RemoteWorker.ArticleDetailGetted -= OnArticleDetailGetted;
				ApplicationWorker.RemoteWorker.MagazineArticleDetailGetted -= OnMagazineArticleDetailGetted;

				if (_articleTextWebView != null)
				{
					_articleTextWebView.RemoveFromSuperview();
					if (_articleTextWebView.Delegate != null)
					{
						var del = _articleTextWebView.Delegate as WebViewDelegate;
						if (del != null)
						{
							del.WebViewLoaded -= OnWebViewLoaded;
						}
						_articleTextWebView.Delegate.Dispose();
					}
					_articleTextWebView.Dispose();
				}
				_articleTextWebView = null;

				if (_articleAuthorView != null)
				{
					_articleAuthorView.RemoveFromSuperview();
					_articleAuthorView.Dispose();
				}
				_articleAuthorView = null;

				if (_articleSectionView != null)
				{
					_articleSectionView.RemoveFromSuperview();
					_articleSectionView.Dispose();
				}
				_articleSectionView = null;

				if (_authorWrapper != null)
				{
					_authorWrapper.RemoveFromSuperview();
					if (_authorWrapper.GestureRecognizers != null)
					{
						foreach (var gr in _authorWrapper.GestureRecognizers)
						{
							gr.Dispose();
						}
					}
					_authorWrapper.Dispose();
				}
				_authorWrapper = null;

				if (_sectionWrapper != null)
				{
					_sectionWrapper.RemoveFromSuperview();
					if (_sectionWrapper.GestureRecognizers != null)
					{
						foreach (var gr in _sectionWrapper.GestureRecognizers)
						{
							gr.Dispose();
						}
					}
					_sectionWrapper.Dispose();
				}
				_sectionWrapper = null;

				if (_articleHeaderView != null)
				{
					_articleHeaderView.RemoveFromSuperview();
					_articleHeaderView.Dispose();
				}
				_articleHeaderView = null;

				if (_titleTextView != null)
				{
					_titleTextView.RemoveFromSuperview();
					_titleTextView.Dispose();
				}
				_titleTextView = null;

				if (_splashScreen != null)
				{
					_splashScreen.RemoveFromSuperview();
					_splashScreen.Dispose();
				}
				_splashScreen = null;

				if (_articleImageView != null)
				{
					_articleImageView.RemoveFromSuperview();
					if (_articleImageView.Image != null)
					{
						_articleImageView.Image.Dispose();
						_articleImageView.Image = null;
					}
					_articleImageView.Dispose();
				}
				_articleImageView = null;

				if (_articleAwardImageView != null)
				{
					_articleAwardImageView.RemoveFromSuperview();
					if (_articleAwardImageView.Image != null)
					{
						_articleAwardImageView.Image.Dispose();
						_articleAwardImageView.Image = null;
					}
					_articleAwardImageView.Dispose();
				}
				_articleAwardImageView = null;

				if (_articleImagesContainer != null)
				{
					_articleImagesContainer.RemoveFromSuperview();
					_articleImagesContainer.Dispose();
				}
				_articleImagesContainer = null;

				if (_scrollView != null)
				{
					_scrollView.RemoveFromSuperview();
					_scrollView.Dispose();
				}
				_scrollView = null;
			});
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
				GetArticleFromServer(_article);
			}
		}

		#endregion

		#region Worked with server

		public void GetArticleFromServer(Article article)
		{
			if (!ApplicationWorker.Settings.OfflineMode)
			{
				var connectAccept = IsConnectionAccept();
				if (!connectAccept)
				{
					BTProgressHUD.ShowToast ("Нет доступных подключений, для указанных в настройках", ProgressHUD.MaskType.None, false, 2500);
					return;
				}
				ApplicationWorker.RemoteWorker.ArticleDetailGetted -= OnArticleDetailGetted;
				ApplicationWorker.RemoteWorker.MagazineArticleDetailGetted -= OnMagazineArticleDetailGetted;
				ApplicationWorker.RemoteWorker.Abort();
				_article = article;
				if (_article.ArticleType == ArticleType.Portal)
				{
					ApplicationWorker.RemoteWorker.ArticleDetailGetted += OnArticleDetailGetted;
					ThreadPool.QueueUserWorkItem(
						state =>
						ApplicationWorker.RemoteWorker.BeginGetArticleDetail(ApplicationWorker.Settings, -1,
							-1,
							_article.Id));
				}
				if (_article.ArticleType == ArticleType.Magazine)
				{
					ApplicationWorker.RemoteWorker.MagazineArticleDetailGetted += OnMagazineArticleDetailGetted;
					ThreadPool.QueueUserWorkItem(
						state =>
						ApplicationWorker.RemoteWorker.BeginGetMagazineArticleDetail(
							ApplicationWorker.Settings, -1, -1, _article.Id));
				}
			}
			else
			{
				if (_parent != null)
				{
					_parent.ArticleDetailTextNotAvailable(article);
					return;
				}
			}
		}

		private void OnMagazineArticleDetailGetted(object sender, MagazineArticleDetailEventArgs e)
		{
			ApplicationWorker.RemoteWorker.MagazineArticleDetailGetted -= OnMagazineArticleDetailGetted;
			if (e.Abort)
			{
				return;
			}
			var error = e.Error;
			if (!error)
			{
				var article = e.Article;
				if (article != null)
				{
					article.IsReaded = true;
					if (_article != null)
					{
						_article.DetailText = article.DetailText;
						_article.IsReaded = article.IsReaded;
						_article.Authors = article.Authors;
						_article.AuthorsId = article.AuthorsId;
						_article.Video = article.Video;
						if (ApplicationWorker.Settings.LoadImages && article.DetailPicture != null)
						{
							_article.DetailPicture = article.DetailPicture;
						}
						if (article.AwardsPicture != null)
						{
							_article.AwardsPicture = article.AwardsPicture;
						}
						ThreadPool.QueueUserWorkItem(state => SaveArticle(_article));
					}
					else
					{
						_article = article;
					}
					InvokeOnMainThread(() => UpdateScreen());
				}
			}
			else
			{
				InvokeOnMainThread(() => BTProgressHUD.ShowToast("Ошибка при загрузке", ProgressHUD.MaskType.None, false, 2500));
			}
		}

		private void OnArticleDetailGetted(object sender, ArticleDetailEventArgs e)
		{
			ApplicationWorker.RemoteWorker.ArticleDetailGetted -= OnArticleDetailGetted;
			if (e.Abort)
			{
				return;
			}
			var error = e.Error;
			if (!error)
			{
				var article = e.Article;
				if (article != null)
				{
					article.IsReaded = true;
					if (_article != null)
					{
						_article.DetailText = article.DetailText;
						_article.IsReaded = article.IsReaded;
						_article.Authors = article.Authors;
						_article.AuthorsId = article.AuthorsId;
						_article.Video = article.Video;
						if (ApplicationWorker.Settings.LoadImages && article.DetailPicture != null)
						{
							_article.DetailPicture = article.DetailPicture;
						}
						if (article.AwardsPicture != null)
						{
							_article.AwardsPicture = article.AwardsPicture;
						}
						ThreadPool.QueueUserWorkItem(state => SaveArticle(_article));
					}
					else
					{
						_article = article;
					}
					InvokeOnMainThread(() => UpdateScreen());
				}
			}
			else
			{
				InvokeOnMainThread(() => BTProgressHUD.ShowToast("Ошибка при загрузке", ProgressHUD.MaskType.None, false, 2500));
			}
		}

		#endregion

		#region Article present level

		public int GetArticleId()
		{
			return _article.Id;
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
				_titleTextView.BackgroundColor = UIColor.Clear;
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
			if (_parent != null)
			{
				_parent.SetArticle(_article);
			}
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
				_articleSectionView.BackgroundColor = UIColor.Clear;

				_sectionWrapper = new UIView(new RectangleF(new PointF(_padding.Left, top), new SizeF(_articleSectionView.Frame.Width, _articleSectionView.Frame.Height + 8)));
				_sectionWrapper.BackgroundColor = UIColor.Clear;
				_sectionWrapper.Add(_articleSectionView);
				_sectionWrapper.UserInteractionEnabled = true;

				UITapGestureRecognizer tap = new UITapGestureRecognizer(() =>
				{
					if (_parent != null)
					{
						var sectionString = section.Split(new []{ "/" }, StringSplitOptions.RemoveEmptyEntries).Last();
						_parent.ShowFilterSectionDialog(sectionString);
					}
				});

				_sectionWrapper.AddGestureRecognizer (tap);
				_scrollView.Add(_sectionWrapper);

				return _sectionWrapper.Frame.Bottom + _padding.Bottom;
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
				_articleHeaderView.BackgroundColor = UIColor.Clear;
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
				_articleAuthorView.BackgroundColor = UIColor.Clear;

				_authorWrapper = new UIView(new RectangleF(new PointF(_padding.Left, top), new SizeF(_articleAuthorView.Frame.Width, _articleAuthorView.Frame.Height + 8)));
				_authorWrapper.BackgroundColor = UIColor.Clear;
				_authorWrapper.Add(_articleAuthorView);

				_authorWrapper.UserInteractionEnabled = true;

				UITapGestureRecognizer tap = new UITapGestureRecognizer (() =>
				{
					if (_parent != null)
					{
						var authors = _article.Authors;
						_parent.ShowFilterAuthorDialog(authors);
					}
				});

				_authorWrapper.AddGestureRecognizer (tap);
				_scrollView.Add(_authorWrapper);

				return _authorWrapper.Frame.Bottom + _padding.Bottom;
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

		private void SaveArticle(Article article)
		{
			var pictures = ApplicationWorker.Db.GetPicturesForParent(article.Id);
			if (pictures != null && pictures.Any())
			{
				var detailPicture = pictures.FirstOrDefault(x => x.Type == PictypeType.Detail);
				if (detailPicture != null)
				{
					ApplicationWorker.Db.DeletePicture(detailPicture.Id);
				}
				var awardsPicture = pictures.FirstOrDefault(x => x.Type == PictypeType.Awards);
				if (awardsPicture != null)
				{
					ApplicationWorker.Db.DeletePicture(awardsPicture.Id);
				}
				var previewPicture = pictures.FirstOrDefault(x => x.Type == PictypeType.Preview);
				if (previewPicture != null)
				{
					ApplicationWorker.Db.DeletePicture(previewPicture.Id);
				}
			}
			if (article.DetailPicture != null)
			{
				ApplicationWorker.Db.InsertPicture(article.DetailPicture);
			}
			if (article.AwardsPicture != null)
			{
				ApplicationWorker.Db.InsertPicture(article.AwardsPicture);
			}
			if (article.PreviewPicture != null)
			{
				ApplicationWorker.Db.InsertPicture(article.PreviewPicture);
			}
			var authors = article.Authors;
			if (authors != null && authors.Any())
			{
				foreach (var author in authors)
				{
					var dbAuthor = ApplicationWorker.Db.GetAuthor(author.Id);
					if (dbAuthor == null)
					{
						ApplicationWorker.Db.InsertAuthor(author);
					}
				}
			}
			var rubrics = article.Rubrics;
			if (rubrics != null && rubrics.Any())
			{
				foreach (var rubric in rubrics)
				{
					var dbRubric = ApplicationWorker.Db.GetRubric(rubric.Id);
					if (dbRubric == null)
					{
						ApplicationWorker.Db.InsertRubric(rubric);
					}
				}
			}
			ApplicationWorker.Db.DeleteItemSectionsForArticle(article.Id);
			if (article.Sections != null && article.Sections.Any())
			{
				ApplicationWorker.Db.InsertItemSections(article.Sections);
			}
			ApplicationWorker.Db.InsertArticle(article);
		}

		private bool IsConnectionAccept()
		{
			var result = true;
			var internetStatus = Reachability.InternetConnectionStatus();
			if (ApplicationWorker.Settings.NetworkMode == NetworkMode.WiFi)
			{
				if (internetStatus != NetworkStatus.ReachableViaWiFiNetwork)
				{
					result = false;
				}
			}
			if (ApplicationWorker.Settings.NetworkMode == NetworkMode.All)
			{
				if (internetStatus == NetworkStatus.NotReachable)
				{
					result = false;
				}
			}
			return result;
		}

		#endregion

	}
}

