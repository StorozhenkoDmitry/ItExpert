using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using ItExpert.Enum;
using ItExpert.Model;
using ItExpert.ServiceLayer;

namespace ItExpert
{
	public class ArticleDetailsViewController: UIViewController
	{
		public ArticleDetailsViewController (Article article, List<int> articlesId, bool fromFavorite, MagazineAction magazineAction)
		{
			_article = article;
			_fromFavorite = fromFavorite;
			_magazineAction = magazineAction;
			_articlesId = articlesId;
			ArticleChange += OnArticleChange;
		}

		static bool UserInterfaceIdiomIsPhone {
			get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

            AutomaticallyAdjustsScrollViewInsets = false;

			//Tim
			if (_fromFavorite)
			{
				//ссылки не интерактивны
			}

            ShowSplash(true);

			_padding = new UIEdgeInsets (8, 8, 8, 8);

            _maxWidth = View.Frame.Width - _padding.Left - _padding.Right;
            
			View.BackgroundColor = UIColor.White;

            UISwipeGestureRecognizer leftSwipeRecognizer = new UISwipeGestureRecognizer(() => OnArticleChange(this, new SwipeEventArgs() { Direction = SwipeDirection.Next }));

            leftSwipeRecognizer.Direction = UISwipeGestureRecognizerDirection.Left;

            View.AddGestureRecognizer(leftSwipeRecognizer);

            UISwipeGestureRecognizer rightSwipeRecognizer = new UISwipeGestureRecognizer(() => OnArticleChange(this, new SwipeEventArgs() { Direction = SwipeDirection.Previous }));

            rightSwipeRecognizer.Direction = UISwipeGestureRecognizerDirection.Right;

            View.AddGestureRecognizer(rightSwipeRecognizer);
		}

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            GetArticleData ();
        }

		#region Worked with server

		private void GetArticleData()
		{
			ShowSplash (true);

			var requestNeeded = false;
			var article = ApplicationWorker.GetArticle(_article.Id);
			if (article != null)
			{
				_authors.Clear();
				_authors.AddRange(article.Authors.Select(x => x.Name));
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
				_article = article;
				UpdateScreen();
			}
			else
			{
				if (!ApplicationWorker.Settings.OfflineMode)
				{
					var connectAccept = IsConnectionAccept();
					if (!connectAccept)
					{
//						Toast.MakeText(this, "Нет доступных подключений, для указанных в настройках",
//							ToastLength.Long).Show();
						return;
					}
					_article = article;
					_isLoading = true;
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
					if (_article.ArticleType  == ArticleType.Magazine && _magazineAction == MagazineAction.Open)
					{
						ShowShowPdfDialog ();
						return;
					}
					if (_article.ArticleType  == ArticleType.Magazine && _magazineAction == MagazineAction.Download)
					{
						ShowLoadPdfDialog ();
						return;
					}
//					Toast.MakeText(this, "Не найдена детальная статья в кэше", ToastLength.Short).Show();
					DismissViewController (true, null);
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
            BeginInvokeOnMainThread(() =>
			{
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
							_authors.Clear();
							_authors.AddRange(article.Authors.Select(x => x.Name));
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
						UpdateScreen();
					}
				}
				else
				{
//					Toast.MakeText(this, "Ошибка при загрузке", ToastLength.Short).Show();
					_isLoading = false;
				}
			});
		}

		private void OnArticleDetailGetted(object sender, ArticleDetailEventArgs e)
		{
			ApplicationWorker.RemoteWorker.ArticleDetailGetted -= OnArticleDetailGetted;
			if (e.Abort)
			{
				return;
			}
            BeginInvokeOnMainThread(() =>
			{
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
							_authors.Clear();
							_authors.AddRange(article.Authors.Select(x => x.Name));
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
						UpdateScreen();
					}
				}
				else
				{
//					Toast.MakeText(this, "Ошибка при загрузке", ToastLength.Short).Show();
					_isLoading = false;
				}
			});
		}

		#endregion

		#region Logic

		private void OnArticleChange(object sender, SwipeEventArgs e)
		{
			if (_isLoading) return;
            ItExpertHelper.RemoveSubviews(View);
			ShowSplash (true);
			Action actionChange = () =>
			{
				var currentIndex = _articlesId.FindIndex(x => x == _article.Id);
				if (currentIndex != -1)
				{
					var searchElemIndex = (e.Direction == SwipeDirection.Next) ? currentIndex + 1 : currentIndex - 1;
					if (searchElemIndex < 0)
					{
						searchElemIndex = _articlesId.Count() - 1;
					}
					if (searchElemIndex > _articlesId.Count() - 1)
					{
						searchElemIndex = 0;
					}
					var articleId = _articlesId[searchElemIndex];
					var requestNeeded = false;
					var article = ApplicationWorker.GetArticle(articleId);
					if (article != null)
					{
						_authors.Clear();
						_authors.AddRange(article.Authors.Select(x => x.Name));
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
						article.IsReaded = true;
						ThreadPool.QueueUserWorkItem(state => SaveArticle(article));
					}
					else
					{
						_article = null;
						requestNeeded = true;
					}
					if (!requestNeeded)
					{
						_article = article;
						InvokeOnMainThread(() => UpdateScreen());
					}
					else
					{
						if (!ApplicationWorker.Settings.OfflineMode)
						{
							var connectAccept = IsConnectionAccept();
							if (!connectAccept)
							{
//								Toast.MakeText(this, "Нет доступных подключений, для указанных в настройках",
//									ToastLength.Long).Show();
								return;
							}
							_article = article;
							ApplicationWorker.RemoteWorker.ArticleDetailGetted -= OnArticleDetailGetted;
							ApplicationWorker.RemoteWorker.MagazineArticleDetailGetted -= OnMagazineArticleDetailGetted;
							ApplicationWorker.RemoteWorker.Abort();
							_isLoading = true;
							if (_article.ArticleType == ArticleType.Portal)
							{
								ApplicationWorker.RemoteWorker.ArticleDetailGetted += OnArticleDetailGetted;
								ApplicationWorker.RemoteWorker.BeginGetArticleDetail(
									ApplicationWorker.Settings, -1, -1, articleId);
							}
							if (_article.ArticleType == ArticleType.Magazine)
							{
								ApplicationWorker.RemoteWorker.MagazineArticleDetailGetted += OnMagazineArticleDetailGetted;
								ApplicationWorker.RemoteWorker.BeginGetMagazineArticleDetail(
									ApplicationWorker.Settings, -1, -1, articleId);
							}
						}
						if (ApplicationWorker.Settings.OfflineMode)
						{
							InvokeOnMainThread(() =>
							{
								if (_article.ArticleType == ArticleType.Magazine && _magazineAction == MagazineAction.Open)
								{
									ShowShowPdfDialog ();
									return;
								}
								if (_article.ArticleType == ArticleType.Magazine && _magazineAction == MagazineAction.Download)
								{
									ShowLoadPdfDialog ();
									return;
								}
//								Toast.MakeText(this, "Не найдена детальная статья в кэше", ToastLength.Short).Show();
								DismissViewController(true, null);
							});
						}
					}
				}
			};
			ThreadPool.QueueUserWorkItem(state => actionChange());
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
			return result;
		}

		#endregion

		#region Filter articles

		private void FilterSection()
		{
			OnDestroy ();
			if (_article.ArticleType == ArticleType.Portal)
			{
				var sectionId = _article.Sections.OrderBy (x => x.DepthLevel).Select(x => x.Section.Id).Last();
				var blockId = _article.IdBlock;
				NewsViewController showController = null;
				var controllers = NavigationController.ViewControllers;
				foreach (var controller in controllers)
				{
					showController = controller as NewsViewController;
					if (showController != null)
					{
						break;
					}
				}
				if (showController != null)
				{
					NavigationController.PopToViewController (showController, true);
					showController.FilterSection (sectionId, blockId);
				}
			}
			if (_article.ArticleType == ArticleType.Magazine)
			{
				var sectionId = _article.Rubrics.Last().Id;
				MagazineViewController showController = null;
				var controllers = NavigationController.ViewControllers;
				foreach (var controller in controllers)
				{
					showController = controller as MagazineViewController;
					if (showController != null)
					{
						break;
					}
				}
				if (showController != null)
				{
					NavigationController.PopToViewController (showController, true);
					showController.SearchRubric (sectionId);
				}
			}
		}

		private void FilterAuthor(int authorId)
		{
			if (_article.ArticleType == ArticleType.Magazine && MagazineViewController.Current != null)
			{
				MagazineViewController.Current.DestroyPdfLoader ();
			}
			var sectionId = -1;
			if (_article.ArticleType == ArticleType.Magazine)
			{
				sectionId = _article.IdSection;
			}
			if (_article.ArticleType == ArticleType.Portal)
			{
				if (_article.Sections != null && _article.Sections.Any ())
				{
					sectionId = _article.Sections.OrderBy (x => x.DepthLevel).Select (x => x.Section.Id).Last ();
				}
			}
			var blockId = _article.IdBlock;
			OnDestroy ();
			NewsViewController showController = null;
			var controllers = NavigationController.ViewControllers;
			foreach (var controller in controllers)
			{
				showController = controller as NewsViewController;
				if (showController != null)
				{
					break;
				}
			}
			if (showController != null)
			{
				NavigationController.PopToViewController (showController, true);
				showController.FilterAuthor (sectionId, blockId, authorId);
			}
		}

		private void ShowLoadPdfDialog()
		{
			BlackAlertView alertView = new BlackAlertView ("Нет детальной статьи", "Отсутствует детальная статья. Скачать Pdf журнала?", "Нет", "Да");

			alertView.ButtonPushed += (sender, e) =>
			{
				if (e.ButtonIndex == 1)
				{
					MagazineViewController showController = null;
					var controllers = NavigationController.ViewControllers;
					foreach (var controller in controllers)
					{
						showController = controller as MagazineViewController;
						if (showController != null)
						{
							break;
						}
					}
					if (showController != null)
					{
						NavigationController.PopToViewController (showController, true);
						showController.DownloadMagazinePdf();
					}
				}
			};

			alertView.Show ();
		}

		private void ShowShowPdfDialog()
		{
			BlackAlertView alertView = new BlackAlertView ("Нет детальной статьи", "Отсутствует детальная статья. Открыть Pdf журнала?", "Нет", "Да");

			alertView.ButtonPushed += (sender, e) =>
			{
				if (e.ButtonIndex == 1)
				{
					MagazineViewController showController = null;
					var controllers = NavigationController.ViewControllers;
					foreach (var controller in controllers)
					{
						showController = controller as MagazineViewController;
						if (showController != null)
						{
							break;
						}
					}
					if (showController != null)
					{
						NavigationController.PopToViewController (showController, true);
						showController.OpenMagazinePdf();
					}
				}
			};

			alertView.Show ();
		}

		#endregion

		private void UpdateScreen()
		{
			var article = _article;
			if (string.IsNullOrWhiteSpace(article.DetailText))
			{
				_isLoading = false;
				if (_article.ArticleType == ArticleType.Magazine && _magazineAction == MagazineAction.Open)
				{
					ShowShowPdfDialog ();
					return;
				}
				if (_article.ArticleType == ArticleType.Magazine && _magazineAction == MagazineAction.Download)
				{
					ShowLoadPdfDialog ();
					return;
				}
//				Toast.MakeText(this, "Не найдена детальная статья в кэше", ToastLength.Short).Show();
				DismissViewController(true, null);
				return;
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
			var style = string.Empty;
			//sectionString - раздел , articleAuthors-авторы, если пустые строки то не отображать
			var html = "<html><head>" + css + "</head><body style='" + style + "'>" + text + video + "</body></html>";

			//После полного отображения выставить флаг и убрать сплаш
			//Прокрутить представление до самого верха
            AddContent(sectionString, articleAuthors, html);
			_isLoading = false;
            ShowSplash (false);
		}

		//Метод скрытия-отображения экрана заставки при загрузке
		private void ShowSplash(bool isVisible)
		{
            if (_splashScreen == null)
            {
                _splashScreen = new UIView(View.Bounds);

                var titleTextView = ItExpertHelper.GetTextView(ItExpertHelper.GetAttributedString("Загрузка статьи...", 
                                        UIFont.BoldSystemFontOfSize(ApplicationWorker.Settings.HeaderSize), 
                                        ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetForeColor())), View.Frame.Width, 
                                        new PointF(0, 0));

                titleTextView.Frame = new RectangleF(View.Frame.Width / 2 - titleTextView.Frame.Width / 2, 100, titleTextView.Frame.Width, titleTextView.Frame.Height);

                _splashScreen.Add(titleTextView);
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

        private void AddContent(string section, string author, string text)
        {
            _scrollView = new UIScrollView(new RectangleF(0, 0, _maxWidth, View.Frame.Height));
            _scrollView.UserInteractionEnabled = true;
            _scrollView.ScrollEnabled = true;

            View.Add(_scrollView);
            View.UserInteractionEnabled = true;

            float top = AddArticleSection(section, NavigationController.NavigationBar.Frame.Height + ItExpertHelper.StatusBarHeight + _padding.Top);
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
                    _maxWidth, new PointF(_padding.Left, top));
				if (!_fromFavorite)
				{
					_articleSectionView.UserInteractionEnabled = true;
					UITapGestureRecognizer tap = new UITapGestureRecognizer (() =>
					{
						var sectionString = section.Split(new []{"/"}, StringSplitOptions.RemoveEmptyEntries).Last();
						BlackAlertView alertView = new BlackAlertView (String.Format ("Раздел: {0}", sectionString.Trim ()), String.Format ("Посмотреть все статьи из раздела: {0}?", sectionString.Trim ()), "Нет", "Да");

						alertView.ButtonPushed += (sender, e) =>
						{
							if (e.ButtonIndex == 1)
							{
								FilterSection ();
							}
						};

						alertView.Show ();
					});

					_articleSectionView.AddGestureRecognizer (tap);
				}
                _scrollView.Add(_articleSectionView);

                return _articleSectionView.Frame.Bottom + _padding.Bottom;
            }

            return top;
		}

        private float AddArticlePicture(float top)
		{
            if (_article.DetailPicture != null)
            {
                float scale = 1;

                var picture = _article.DetailPicture;
                    
				if (picture.Width > (View.Frame.Width - _padding.Left - _padding.Right))
                {
					scale = picture.Width / (View.Frame.Width - _padding.Left - _padding.Right);
                }

                if (picture.Height > View.Frame.Height * 0.35)
                {
                    float heightScale = picture.Height / (View.Frame.Height * 0.35f);

                    if (heightScale > scale)
                    {
                        scale = heightScale;
                    }
                }

                var image = ItExpertHelper.GetImageFromBase64String(picture.Data, scale);

				_articleImageView = new UIImageView(new RectangleF((View.Frame.Width - image.Size.Width) / 2, 
                    top, image.Size.Width, image.Size.Height));

                _articleImageView.Image = image;

                _scrollView.Add(_articleImageView);

                return _articleImageView.Frame.Bottom + _padding.Bottom;
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
                    _maxWidth, new PointF(_padding.Left, top));
				if (!_fromFavorite)
				{
					_articleAuthorView.UserInteractionEnabled = true;

					UITapGestureRecognizer tap = new UITapGestureRecognizer (() =>
					{
						var authors = _article.Authors;
						var shortAuthors = authors.
						Select (x => x.Name.Split (new [] { "," }, StringSplitOptions.RemoveEmptyEntries) [0].Trim ()).ToArray ();
						BlackAlertView alertView = new BlackAlertView ("Выберете автора", "Отмена", shortAuthors, "Поиск");

						alertView.ButtonPushed += (sender, e) =>
						{
							if (e.ButtonIndex == 1)
							{
								var authorId = authors [e.SelectedRadioButton].Id;
								FilterAuthor (authorId);
							}
						};

						alertView.Show ();
					});

					_articleAuthorView.AddGestureRecognizer (tap);
				}
                _scrollView.Add(_articleAuthorView);

                return _articleAuthorView.Frame.Bottom + _padding.Bottom;
            }

            return top;
        }

        private void AddArticleText(string text, float top)
        {
            if (!String.IsNullOrWhiteSpace(text))
            {
                _articleTextWebView = new UIWebView(new RectangleF(_padding.Left,
                    top, _maxWidth, 1));                   

                var webViewDelegate = new WebViewDelegate(); 
                webViewDelegate.WebViewLoaded += OnWebViewLoaded;

                _articleTextWebView.Delegate = webViewDelegate;
                _articleTextWebView.ScrollView.ScrollEnabled = false;
                _articleTextWebView.ScrollView.Bounces = false;
                _articleTextWebView.LoadHtmlString(text, null);
                    
                _scrollView.Add(_articleTextWebView);
            }
        }

		private void OnDestroy()
		{
			ArticleChange -= OnArticleChange;
			ArticleChange = null;
//			ApplicationWorker.SettingsChanged -= ApplicationOnSettingsChanged;
			_articlesId = null;
			_authors = null;
		}

        private void OnWebViewLoaded(object sender, EventArgs e)
        {
            _scrollView.ContentSize = new SizeF(_maxWidth, _articleTextWebView.Frame.Bottom);
        }

        private float _maxWidth;
		private Article _article;
		private bool _fromFavorite;
		private bool _isLoading;
		private MagazineAction _magazineAction;
		private List<int> _articlesId;
		private List<string> _authors = new List<string>();
		private event EventHandler<SwipeEventArgs> ArticleChange;

		private UIEdgeInsets _padding;

        private UIScrollView _scrollView;
        private UITextView _articleSectionView;
		private UIImageView _articleImageView;
        private UITextView _articleHeaderView;
        private UITextView _articleAuthorView;
		private UIWebView _articleTextWebView;
        private UIView _splashScreen;
	}
}

