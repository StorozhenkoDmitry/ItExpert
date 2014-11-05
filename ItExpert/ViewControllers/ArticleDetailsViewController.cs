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
using BigTed;
using System.Text;

namespace ItExpert
{
	public class ArticleDetailsViewController: UIViewController
	{
		#region Fileds

		private UIButton _backButton;
		private UIBarButtonItem _backButtonBar;
		private SettingsView _settingsView;
		private UIButton _settingsButton;
		private UIBarButtonItem _settingsBarButton;
		private ShareView _shareView;
		private UIButton _shareButton;
		private UIBarButtonItem _shareBarButton;
		private bool _fromFavorite;
		private bool _loadMoreOperation = false;
		private FilterParameters _filterParams;
		private Rubric _searchRubric;
		private bool _canTransition = false;
		private Article _article;
		private MagazineAction _magazineAction;
		private List<int> _articlesId;
		private ArticleDetailContentView _currentView;
		private UISwipeGestureRecognizer _swipeLeftRecognizer;
		private UISwipeGestureRecognizer _swipeRightRecognizer;

		#endregion

		#region UIViewController members

		public ArticleDetailsViewController (Article article, List<int> articlesId, MagazineAction magazineAction)
		{
			_article = article;
			_magazineAction = magazineAction;
			_articlesId = articlesId;
			ApplicationWorker.SettingsChanged += OnSettingsChanged;
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
			Initialize();
		}

		public override void ViewDidDisappear(bool animated)
		{
			base.ViewDidDisappear(animated);
			ApplicationWorker.SettingsChanged -= OnSettingsChanged;
			ApplicationWorker.RemoteWorker.NewsGetted -= LoadMorePortalNewsGetted;
			ApplicationWorker.RemoteWorker.MagazineArticlesGetted -= LoadMoreMagazineArticlesGetted;
			ApplicationWorker.RemoteWorker.ArticleDetailGetted -= OnArticleDetailGetted;
			ApplicationWorker.RemoteWorker.MagazineArticleDetailGetted -= OnMagazineArticleDetailGetted;
		}

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			GC.Collect();
		}

		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate(fromInterfaceOrientation);
			var frame = View.Frame;
			if (_currentView != null)
			{
				_currentView.DidRotate(frame);
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			InvokeOnMainThread(() =>
			{
				_article = null;
				if (_articlesId != null)
				{
					_articlesId.Clear();
				}
				_articlesId = null;

				if (_currentView != null)
				{
					_currentView.Dispose();
				}
				_currentView = null;

				if (_swipeLeftRecognizer != null)
				{
					_swipeLeftRecognizer.Dispose();
				}
				_swipeLeftRecognizer = null;

				if (_swipeRightRecognizer != null)
				{
					_swipeRightRecognizer.Dispose();
				}
				_swipeRightRecognizer = null;

				BTProgressHUD.Dismiss();
				if (_backButton != null)
				{
					_backButton.RemoveFromSuperview();
					if (_backButton.ImageView != null && _backButton.ImageView.Image != null)
					{
						_backButton.ImageView.Image.Dispose();
						_backButton.ImageView.Image = null;
					}
					_backButton.TouchUpInside -= BackButtonTouchUp;
					_backButton.Dispose();
				}

				if (_backButtonBar != null)
				{
					_backButtonBar.Dispose();
				}

				_backButton = null;
				_backButtonBar = null;

				if (_settingsButton != null)
				{
					_settingsButton.RemoveFromSuperview();
					if (_settingsButton.ImageView != null && _settingsButton.ImageView.Image != null)
					{
						_settingsButton.ImageView.Image.Dispose();
						_settingsButton.ImageView.Image = null;
					}
					_settingsButton.TouchUpInside -= SettingsButtonTouchUp;
					_settingsButton.Dispose();
				}
				_settingsButton = null;

				if (_settingsBarButton != null)
				{
					_settingsBarButton.Dispose();
				}
				_settingsBarButton = null;

				if (_settingsView != null)
				{
					_settingsView.TapOutsideTableView -= ViewTapOutsideTableView;
					_settingsView.Dispose();
				}
				_settingsView = null;

				if (_shareButton != null)
				{
					_shareButton.RemoveFromSuperview();
					if (_shareButton.ImageView != null && _shareButton.ImageView.Image != null)
					{
						_shareButton.ImageView.Image.Dispose();
						_shareButton.ImageView.Image = null;
					}
					_shareButton.TouchUpInside -= ShareButtonTouchUp;
					_shareButton.Dispose();
				}
				_shareButton = null;

				if (_shareBarButton != null)
				{
					_shareBarButton.Dispose();
				}
				_shareBarButton = null;

				if (_shareView != null)
				{
					_shareView.TapOutsideTableView -= ViewTapOutsideTableView;
					_shareView.Dispose();
				}
				_shareView = null;

				if (_currentView != null)
				{
					_currentView.Dispose();
				}
				_currentView = null;

				ApplicationWorker.SettingsChanged -= OnSettingsChanged;
				ApplicationWorker.RemoteWorker.NewsGetted -= LoadMorePortalNewsGetted;
				ApplicationWorker.RemoteWorker.MagazineArticlesGetted -= LoadMoreMagazineArticlesGetted;
				ApplicationWorker.RemoteWorker.ArticleDetailGetted -= OnArticleDetailGetted;
				ApplicationWorker.RemoteWorker.MagazineArticleDetailGetted -= OnMagazineArticleDetailGetted;
			});
		}

		#endregion

		#region Server interaction

		void LoadMoreArticles()
		{
			var type = ArticleType.Portal;
			long lastTimestam = -1;
			var articles = ApplicationWorker.GetNewsList();
			if (articles != null && articles.Any())
			{
				type = articles.First().ArticleType;
				lastTimestam = articles.Min(x => x.Timespan);
			}
			if (type == ArticleType.Portal)
			{
				if (_filterParams != null)
				{
					_loadMoreOperation = true;
					BTProgressHUD.Show("Загрузка статей", -1, ProgressHUD.MaskType.None);
					ApplicationWorker.RemoteWorker.NewsGetted += LoadMorePortalNewsGetted;
					ThreadPool.QueueUserWorkItem(
						state =>
						ApplicationWorker.RemoteWorker.BeginGetNews(ApplicationWorker.Settings, -1, lastTimestam,
							_filterParams.BlockId, _filterParams.SectionId, _filterParams.AuthorId, _filterParams.Search));
				}
			}
			if (type == ArticleType.Magazine)
			{
				if (_filterParams != null)
				{
					_loadMoreOperation = true;
					BTProgressHUD.Show("Загрузка статей", -1, ProgressHUD.MaskType.None);
					_searchRubric = _filterParams.SearchRubric;
					ApplicationWorker.RemoteWorker.MagazineArticlesGetted += LoadMoreMagazineArticlesGetted;
					ThreadPool.QueueUserWorkItem(
						state =>
						ApplicationWorker.RemoteWorker.BeginGetMagazinesArticlesByRubric(
							ApplicationWorker.Settings, _searchRubric, Magazine.BlockId, lastTimestam));
				}
			}
		}

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
				ArticleDetailTextNotAvailable(article);
				SetCanTransitionTrue();
			}
		}

		void LoadMoreMagazineArticlesGetted (object sender, MagazineArticlesEventArgs e)
		{
			ApplicationWorker.RemoteWorker.MagazineArticlesGetted -= LoadMoreMagazineArticlesGetted;
			if (e.Abort)
			{
				return;
			}
			InvokeOnMainThread(() =>
			{
				var error = e.Error;
				if (!error)
				{
					if (e.Articles != null && e.Articles.Any())
					{
						e.Articles.ForEach(article =>
						{
							article.Rubrics.Add(_searchRubric);
							article.RubricsId = _searchRubric.Id.ToString("G");
						});
						ApplicationWorker.Db.SetPropertiesForArticles(e.Articles);
						ApplicationWorker.NormalizePreviewText(e.Articles);
						ThreadPool.QueueUserWorkItem(state => UpdateMagazineData(e));
						var sortedList = e.Articles.OrderByDescending(x => x.ActiveFrom).ToList();
						var firstArticle = sortedList.First();
						ApplicationWorker.AppendToNewsList(sortedList);
						_articlesId = ApplicationWorker.GetNewsList().Select(x=>x.Id).ToList();
						BTProgressHUD.Dismiss();
						_loadMoreOperation = false;
						_article = firstArticle;
						var oldView = _currentView;
						_currentView = new ArticleDetailContentView(firstArticle.Id, this, NavigationController.NavigationBar.Frame.Height, View.Frame);

						var options = UIViewAnimationOptions.CurveEaseIn;
						_currentView.Frame = new RectangleF(View.Frame.Right, 0, View.Frame.Width, View.Frame.Height);
						Add(_currentView);
						UIView.Animate(0.5, 0, options, () =>
						{
							_currentView.Frame = new RectangleF(View.Frame.X, 0, View.Frame.Width, View.Frame.Height);
						}, 
							() => _currentView.Frame = new RectangleF(View.Frame.X, 0, View.Frame.Width, View.Frame.Height));

						UIView.Animate(0.5, 0, options, () =>
						{
							oldView.Frame = new RectangleF(0 - View.Frame.Width, 0, View.Frame.Width, View.Frame.Height);
						}, () =>
						{
							oldView.RemoveFromSuperview();
							oldView.Dispose();
						});
					}
					else
					{
						BTProgressHUD.Dismiss();
						_loadMoreOperation = false;
						BTProgressHUD.ShowToast("Больше статей нет", ProgressHUD.MaskType.None, false, 2500);	
					}
				}
				else
				{
					BTProgressHUD.Dismiss();
					_loadMoreOperation = false;
					BTProgressHUD.ShowToast("Ошибка при загрузке", ProgressHUD.MaskType.None, false, 2500);	
				}
			});
		}

		void LoadMorePortalNewsGetted (object sender, ArticleEventArgs e)
		{
			ApplicationWorker.RemoteWorker.NewsGetted -= LoadMorePortalNewsGetted;
			if (e.Abort)
			{
				return;
			}
			InvokeOnMainThread(() =>
			{
				var error = e.Error;
				if (!error)
				{
					if (e.Articles != null && e.Articles.Any())
					{
						ApplicationWorker.Db.SetPropertiesForArticles(e.Articles);
						ApplicationWorker.NormalizePreviewText(e.Articles);
						ThreadPool.QueueUserWorkItem(state => UpdatePortalData(e));
						var sortedList = e.Articles.OrderByDescending(x => x.ActiveFrom).ToList();
						var firstArticle = sortedList.First();
						ApplicationWorker.AppendToNewsList(sortedList);
						_articlesId = ApplicationWorker.GetNewsList().Select(x=>x.Id).ToList();
						BTProgressHUD.Dismiss();
						_loadMoreOperation = false;
						_article = firstArticle;
						var oldView = _currentView;
						_currentView = new ArticleDetailContentView(firstArticle.Id, this, NavigationController.NavigationBar.Frame.Height, View.Frame);
						var options = UIViewAnimationOptions.CurveEaseIn;
						_currentView.Frame = new RectangleF(View.Frame.Right, 0, View.Frame.Width, View.Frame.Height);
						Add(_currentView);
						UIView.Animate(0.5, 0, options, () =>
						{
							_currentView.Frame = new RectangleF(View.Frame.X, 0, View.Frame.Width, View.Frame.Height);
						}, 
							() => _currentView.Frame = new RectangleF(View.Frame.X, 0, View.Frame.Width, View.Frame.Height));

						UIView.Animate(0.5, 0, options, () =>
						{
							oldView.Frame = new RectangleF(0 - View.Frame.Width, 0, View.Frame.Width, View.Frame.Height);
						}, () =>
						{
							oldView.RemoveFromSuperview();
							oldView.Dispose();
						});
					}
					else
					{
						BTProgressHUD.Dismiss();
						_loadMoreOperation = false;
						BTProgressHUD.ShowToast("Больше статей нет", ProgressHUD.MaskType.None, false, 2500);	
					}
				}
				else
				{
					BTProgressHUD.Dismiss();
					_loadMoreOperation = false;
					BTProgressHUD.ShowToast("Ошибка при загрузке", ProgressHUD.MaskType.None, false, 2500);	
				}
			});
		}

		private void OnMagazineArticleDetailGetted(object sender, MagazineArticleDetailEventArgs e)
		{
			ApplicationWorker.RemoteWorker.MagazineArticleDetailGetted -= OnMagazineArticleDetailGetted;
			if (e.Abort)
			{
				SetCanTransitionTrue();
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
					InvokeOnMainThread(() =>
					{
						if (_currentView != null)
						{
							_currentView.SetArticle(_article);
						}
					});
				}
			}
			else
			{
				SetCanTransitionTrue();
				InvokeOnMainThread(() => BTProgressHUD.ShowToast("Ошибка при загрузке", ProgressHUD.MaskType.None, false, 2500));
			}
		}

		private void OnArticleDetailGetted(object sender, ArticleDetailEventArgs e)
		{
			ApplicationWorker.RemoteWorker.ArticleDetailGetted -= OnArticleDetailGetted;
			if (e.Abort)
			{
				SetCanTransitionTrue();
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
					InvokeOnMainThread(() =>
					{
						if (_currentView != null)
						{
							_currentView.SetArticle(_article);
						}
					});
				}
			}
			else
			{
				SetCanTransitionTrue();
				InvokeOnMainThread(() => BTProgressHUD.ShowToast("Ошибка при загрузке", ProgressHUD.MaskType.None, false, 2500));
			}
		}

		#endregion

		#region Logic

		void Initialize()
		{
			InitNavigationBar();
			View.UserInteractionEnabled = true;

			_swipeLeftRecognizer = new UISwipeGestureRecognizer(()=>OnChangeArticle(SwipeDirection.Next));
			_swipeLeftRecognizer.Direction = UISwipeGestureRecognizerDirection.Left;
			View.AddGestureRecognizer(_swipeLeftRecognizer);

			_swipeRightRecognizer = new UISwipeGestureRecognizer(()=>OnChangeArticle(SwipeDirection.Previous));
			_swipeRightRecognizer.Direction = UISwipeGestureRecognizerDirection.Right;
			View.AddGestureRecognizer(_swipeRightRecognizer);

			_currentView = new ArticleDetailContentView(_article.Id, this, NavigationController.NavigationBar.Frame.Height, View.Frame);
			Add(_currentView);
		}

		private void InitNavigationBar()
		{
			UIBarButtonItem spaceForBack = new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace);

			spaceForBack.Width = -6;

			UIBarButtonItem spaceForLogo = new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace);

			spaceForLogo.Width = 20;

			_backButton = NavigationBarButton.GetButton("NavigationBar/Back.png", 2);
			_backButton.TouchUpInside += BackButtonTouchUp;
			_backButtonBar = new UIBarButtonItem(_backButton);

			NavigationItem.LeftBarButtonItems = new UIBarButtonItem[] { spaceForBack, _backButtonBar, spaceForLogo, NavigationBarButton.Logo };           

			UIBarButtonItem spaceForSettings = new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace);

			spaceForSettings.Width = -10;

			_settingsButton = NavigationBarButton.GetButton("NavigationBar/Settings.png", 4.1f);
			_settingsBarButton = new UIBarButtonItem(_settingsButton);
			_settingsView = new SettingsView(true);
			_settingsView.TapOutsideTableView += ViewTapOutsideTableView;
			_settingsButton.TouchUpInside += SettingsButtonTouchUp;


			_shareButton = NavigationBarButton.GetButton("NavigationBar/Share.png", 4.5f);
			_shareBarButton = new UIBarButtonItem(_shareButton);
			_shareView = new ShareView();
			_shareView.TapOutsideTableView += ViewTapOutsideTableView;
			_shareButton.TouchUpInside += ShareButtonTouchUp;

			NavigationItem.RightBarButtonItems = new UIBarButtonItem[] { spaceForSettings, _settingsBarButton, _shareBarButton };
		}

		void OnChangeArticle(SwipeDirection direction)
		{
			if (!_canTransition)
				return;
			if (_loadMoreOperation)
				return;
			var currentIndex = _articlesId.FindIndex(x => x == _article.Id);
			var searchElemIndex = -1;
			var options = UIViewAnimationOptions.CurveEaseIn;
			if (direction == SwipeDirection.Next)
			{
				searchElemIndex = currentIndex + 1;
			}
			if (direction == SwipeDirection.Previous)
			{
				searchElemIndex = currentIndex - 1;
			}
			if (searchElemIndex < 0)
			{
				UIView.Animate(0.35, 0, options, () =>
				{
					_currentView.Frame = new RectangleF(View.Frame.X + (View.Frame.Width * 0.12f), 0, View.Frame.Width, View.Frame.Height);
				}, 
					() => _currentView.Frame = new RectangleF(View.Frame.X, 0, View.Frame.Width, View.Frame.Height));

				return;
			}
			if (searchElemIndex > _articlesId.Count() - 1)
			{
				UIView.Animate(0.35, 0, options, () =>
				{
					_currentView.Frame = new RectangleF(View.Frame.X - (View.Frame.Width * 0.12f), 0, View.Frame.Width, View.Frame.Height);
				}, 
					() => _currentView.Frame = new RectangleF(View.Frame.X, 0, View.Frame.Width, View.Frame.Height)
				);
				if (!_fromFavorite && _filterParams != null)
				{
					ShowLoadMoreDialog();
				}
				return;
			}
			var articleId = _articlesId[searchElemIndex];
			_article = ApplicationWorker.GetArticle(articleId);
			_canTransition = false;
			var oldView = _currentView;
			_currentView = new ArticleDetailContentView(articleId, this, NavigationController.NavigationBar.Frame.Height, View.Frame);
			if (direction == SwipeDirection.Next)
			{
				_currentView.Frame = new RectangleF(View.Frame.Right, 0, View.Frame.Width, View.Frame.Height);
				Add(_currentView);
				UIView.Animate(0.5, 0, options, () =>
				{
					_currentView.Frame = new RectangleF(View.Frame.X, 0, View.Frame.Width, View.Frame.Height);
				}, 
				() => _currentView.Frame = new RectangleF(View.Frame.X, 0, View.Frame.Width, View.Frame.Height));

				UIView.Animate(0.5, 0, options, () =>
				{
					oldView.Frame = new RectangleF(0 - View.Frame.Width, 0, View.Frame.Width, View.Frame.Height);
				}, () =>
				{
					oldView.RemoveFromSuperview();
					oldView.Dispose();
				});
			}
			if (direction == SwipeDirection.Previous)
			{
				_currentView.Frame = new RectangleF(0 - View.Frame.Width, 0, View.Frame.Width, View.Frame.Height);
				Add(_currentView);
				UIView.Animate(0.5, 0, options, () =>
				{
					_currentView.Frame = new RectangleF(View.Frame.X, 0, View.Frame.Width, View.Frame.Height);
				}, 
					() => _currentView.Frame = new RectangleF(View.Frame.X, 0, View.Frame.Width, View.Frame.Height));

				UIView.Animate(0.5, 0, options, () =>
				{
					oldView.Frame = new RectangleF(View.Frame.Right, 0, View.Frame.Width, View.Frame.Height);
				}, () =>
				{
					oldView.RemoveFromSuperview();
					oldView.Dispose();
				});
			}

		}

		void BackButtonTouchUp(object sender, EventArgs e)
		{
			Exit();
		}

		void ViewTapOutsideTableView(object sender, EventArgs e)
		{
			NavigationBarButton.HideWindow();
		}

		void SettingsButtonTouchUp(object sender, EventArgs e)
		{
			NavigationBarButton.ShowWindow(_settingsView);
		}

		void ShareButtonTouchUp(object sender, EventArgs e)
		{
			NavigationBarButton.ShowWindow(_shareView);
			_shareView.Update();
		}

		void OnSettingsChanged (object sender, EventArgs e)
		{
			View.BackgroundColor = ItExpertHelper.GetUIColorFromColor (ApplicationWorker.Settings.GetBackgroundColor ());
			if (_currentView != null)
			{
				_currentView.OnSettingsChanged();
			}
		}

		public void SetCanTransitionTrue()
		{
			_canTransition = true;
		}

		public void SetFromFavorite(bool fromFavorite)
		{
			_fromFavorite = fromFavorite;
		}

		public void SetFilterParameters(FilterParameters filterParams)
		{
			_filterParams = filterParams;
		}

		void Exit()
		{
			NavigationController.PopViewControllerAnimated(true);
			Dispose();
		}

		#endregion

		#region Dialogs

		private void ShowLoadMoreDialog() 
		{
			BlackAlertView alertView = new BlackAlertView ("Загрузка", "Вы просмотрели последнюю статью. Загрузить следующие статьи?", "Нет", "Да");

			alertView.ButtonPushed += (sender, e) =>
			{
				if (e.ButtonIndex == 0)
				{
					alertView.Dispose();
				}
				if (e.ButtonIndex == 1)
				{
					LoadMoreArticles();
					alertView.Dispose();
				}
			};

			alertView.Show ();
		}

		public void ShowFilterSectionDialog(string sectionString)
		{
			if (!ApplicationWorker.Settings.OfflineMode && !_loadMoreOperation)
			{

				BlackAlertView alertView = new BlackAlertView(String.Format("Раздел: {0}", sectionString.Trim()), String.Format("Посмотреть все статьи из раздела: {0}?", sectionString.Trim()), "Нет", "Да");

				alertView.ButtonPushed += (sender, e) =>
				{
					if (e.ButtonIndex == 1)
					{
						FilterSection();
					}
					alertView.Dispose();
				};

				alertView.Show();
			}
		}

		public void ShowFilterAuthorDialog(List<Author> authors)
		{
			if (!ApplicationWorker.Settings.OfflineMode && !_loadMoreOperation)
			{
				var shortAuthors = authors.
					Select(x => x.Name.Split(new [] { "," }, 
						StringSplitOptions.RemoveEmptyEntries)[0].Trim()).ToArray();
				BlackAlertView alertView = new BlackAlertView("Выберете автора", "Отмена", shortAuthors, "Поиск");

				alertView.ButtonPushed += (sender, e) =>
				{
					if (e.ButtonIndex == 1)
					{
						var authorId = authors[e.SelectedRadioButton].Id;
						FilterAuthor(authorId);
					}
					alertView.Dispose();
				};

				alertView.Show();
			}
		}

		private void ShowLoadPdfDialog()
		{
			BlackAlertView alertView = new BlackAlertView ("Нет детальной статьи", "Отсутствует детальная статья. Скачать Pdf журнала?", "Нет", "Да");

			alertView.ButtonPushed += (sender, e) =>
			{
				if (e.ButtonIndex == 0)
				{
					Exit();
					alertView.Dispose();
				}
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
						Dispose();
					}
					else
					{
						Exit();
					}
					alertView.Dispose();
				}
			};

			alertView.Show ();
		}

		private void ShowShowPdfDialog()
		{
			BlackAlertView alertView = new BlackAlertView ("Нет детальной статьи", "Отсутствует детальная статья. Открыть Pdf журнала?", "Нет", "Да");

			alertView.ButtonPushed += (sender, e) =>
			{
				if (e.ButtonIndex == 0)
				{
					Exit();
					alertView.Dispose();
				}
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
						Dispose();
					}
					else
					{
						Exit();
					}
					alertView.Dispose();
				}
			};

			alertView.Show ();
		}

		void ShowNotFindArticleDialog()
		{
			var alert = new BlackAlertView ("Нет детальной статьи", "Не найдена детальная статья в кэше", "Ok");
			alert.ButtonPushed += (sender, e) =>
			{
				Exit();
				alert.Dispose();
			};
			alert.Show();
		}

	 	public void ArticleDetailTextNotAvailable(Article article)
		{
			SetCanTransitionTrue();
			if (article.ArticleType == ArticleType.Magazine && _magazineAction == MagazineAction.Open)
			{
				ShowShowPdfDialog();
				return;
			}
			if (article.ArticleType == ArticleType.Magazine && _magazineAction == MagazineAction.Download)
			{
				if (!ApplicationWorker.Settings.OfflineMode)
				{
					ShowLoadPdfDialog();
					return;
				}
			}
			ShowNotFindArticleDialog();
			return;
		}

		private void FilterSection()
		{
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
					NavigationController.PopToViewController (showController, false);
					showController.FilterSection (sectionId, blockId);
					Dispose();
				}
				else
				{
					var filterParams = new FilterParameters() 
					{
						Filter = Filter.Section,
						SectionId = sectionId,
						BlockId = blockId
					};
					NavigationController.DismissViewController(false, null);
					showController = new NewsViewController(filterParams);
					NavigationController.PushViewController(showController, false);
					Dispose();
				}
				return;
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
					NavigationController.PopToViewController(showController, false);
					showController.SearchRubric(sectionId);
					Dispose();
				}
				else
				{
					var filterParams = new FilterParameters() {
						Filter = Filter.Section,
						SectionId = sectionId,
					};
					NavigationController.DismissViewController(false, null);
					showController = new MagazineViewController(filterParams);
					NavigationController.PushViewController(showController, false);
					Dispose();
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
				NavigationController.PopToViewController(showController, false);
				showController.FilterAuthor(sectionId, blockId, authorId);
				Dispose();
			}
			else
			{
				var filterParams = new FilterParameters() 
				{
					Filter = Filter.Author,
					SectionId = sectionId,
					BlockId = blockId,
					AuthorId = authorId,
				};
				NavigationController.DismissViewController(false, null);
				showController = new NewsViewController(filterParams);
				NavigationController.PushViewController(showController, false);
				Dispose();
			}
		}

		#endregion

		#region DAL 

		private void UpdatePortalData(ArticleEventArgs e)
		{
			//Сохранение Блоков
			var newBlocks = e.Blocks;
			var oldBlocks = ApplicationWorker.Db.GetBlocks();
			ApplicationWorker.Db.InsertNewBlocks(oldBlocks, newBlocks);
			//Сохранение Секций
			var newSection = e.Sections;
			var oldSection = ApplicationWorker.Db.GetSections();
			ApplicationWorker.Db.InsertNewSections(oldSection, newSection);
			//Сохранение авторов
			var newAuthor = e.Authors;
			var oldAuthor = ApplicationWorker.Db.LoadAllAuthors();
			ApplicationWorker.Db.InsertNewAuthors(oldAuthor, newAuthor);
			//Обновление Статей
			foreach (var article in e.Articles)
			{
				var dbArticle = ApplicationWorker.Db.GetArticle(article.Id);
				if (dbArticle != null)
				{
					var changeBlock = dbArticle.IdBlock != article.IdBlock;
					var changeSections = dbArticle.SectionsId != article.SectionsId;
					if (changeBlock || changeSections)
					{
						var pictures = ApplicationWorker.Db.GetPicturesForParent(article.Id);
						foreach (var picture in pictures)
						{
							ApplicationWorker.Db.DeletePicture(picture.Id);
						}
						ApplicationWorker.Db.DeleteItemSectionsForArticle(article.Id);
						ApplicationWorker.Db.UpdateArticle(article);
						ApplicationWorker.Db.InsertItemSections(article.Sections);
						if (article.PreviewPicture != null)
						{
							ApplicationWorker.Db.InsertPicture(article.PreviewPicture);
						}
						if (article.DetailPicture != null)
						{
							ApplicationWorker.Db.InsertPicture(article.DetailPicture);
						}
						if (article.AwardsPicture != null)
						{
							ApplicationWorker.Db.InsertPicture(article.AwardsPicture);
						}
					}
				}
			}   
		}

		private void UpdateMagazineData(MagazineArticlesEventArgs e)
		{
			//Обновление рубрик
			var dbRubrics = ApplicationWorker.Db.LoadAllRubrics();
			var newRubrics = e.Rubrics;
			ApplicationWorker.Db.InsertNewRubrics(dbRubrics, newRubrics);
			//Сохранение авторов
			var newAuthor = e.Authors;
			var oldAuthor = ApplicationWorker.Db.LoadAllAuthors();
			ApplicationWorker.Db.InsertNewAuthors(oldAuthor, newAuthor);
			//Обновление Статей
			foreach (var article in e.Articles)
			{
				var dbArticle = ApplicationWorker.Db.GetArticle(article.Id);
				if (dbArticle != null)
				{
					var changeBlock = dbArticle.IdBlock != article.IdBlock;
					var changeSections = dbArticle.SectionsId != article.SectionsId;
					var changeRubrics = dbArticle.RubricsId != article.RubricsId;
					if (changeBlock || changeSections || changeRubrics)
					{
						var pictures = ApplicationWorker.Db.GetPicturesForParent(article.Id);
						foreach (var picture in pictures)
						{
							ApplicationWorker.Db.DeletePicture(picture.Id);
						}
						ApplicationWorker.Db.UpdateArticle(article);
						if (article.PreviewPicture != null)
						{
							ApplicationWorker.Db.InsertPicture(article.PreviewPicture);
						}
						if (article.DetailPicture != null)
						{
							ApplicationWorker.Db.InsertPicture(article.DetailPicture);
						}
						if (article.AwardsPicture != null)
						{
							ApplicationWorker.Db.InsertPicture(article.AwardsPicture);
						}
					}
				}
			}
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

		#endregion

		#region Helpers

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

