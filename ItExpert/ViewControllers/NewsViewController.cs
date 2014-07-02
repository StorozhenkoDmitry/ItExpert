using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Threading;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using ItExpert.Enum;
using ItExpert.Model;
using ItExpert.ServiceLayer;

namespace ItExpert
{
	public partial class NewsViewController : UIViewController
	{
		#region Fields

		private UITableView _articlesTableView;
		private bool _isLoadingData = false;
		private List<Article> _articles;
		private int _blockId = -1;
		private int _sectionId = -1;
		private int _authorId = -1;
		private List<Article> _allArticles = new List<Article>();
		private Page _currentPage;
		private string _search = null;
        private UIView _banner = null;
		private UIView _addPreviousArticleButton = null;
		private bool _prevArticlesExists = true;
		private bool _startPage = false;
		private bool _headerAdded = false;
		private string _header = null;
		private BottomToolbarView _bottomBar = null;
		private UIActivityIndicatorView _loadingIndicator;

		#endregion

		#region UIViewController members

		public NewsViewController(){
		}

		static bool UserInterfaceIdiomIsPhone {
			get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();

			// Release any cached data, images, etc that aren't in use.
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			Initialize ();
			// Perform any additional setup after loading the view, typically from a nib.
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

            if (_articlesTableView != null)
            {
                _articlesTableView.DeselectRow(_articlesTableView.IndexPathForSelectedRow, true);
            }
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			if (_articlesTableView != null)
			{
				_articlesTableView.ReloadData ();
			}
		}

		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
			ApplicationWorker.RemoteWorker.NewsGetted -= NewNewsGetted;
			ApplicationWorker.RemoteWorker.NewsGetted -= PreviousNewsGetted;
			SetLoadingImageVisible(false);
			_isLoadingData = false;
			if (_articlesTableView != null)
			{
				_articlesTableView.ReloadData ();
			}
		}

		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear (animated);
		}

		#endregion

		#region Init

		void Initialize()
		{
			_startPage = true;
			ClearCache ();

            float scale = 3.8f;
            TabBarItem = new UITabBarItem("News", new UIImage(NSData.FromFile("News.png"), scale), 0);
            View.AutosizesSubviews = true;
            var topOffset = NavigationController.NavigationBar.Frame.Height + ItExpertHelper.StatusBarHeight;
            AutomaticallyAdjustsScrollViewInsets = false;
			View.BackgroundColor = ItExpertHelper.GetUIColorFromColor (ApplicationWorker.Settings.GetBackgroundColor ());

            InitAddPreviousArticleButton ();
            InitBottomToolbar();
			InitLoadingProgress ();

            _articlesTableView = new UITableView(new RectangleF(0, topOffset, View.Bounds.Width, 
				View.Bounds.Height- topOffset - _bottomBar.Frame.Height), UITableViewStyle.Plain);
			_articlesTableView.ScrollEnabled = true; 
			_articlesTableView.UserInteractionEnabled = true;
			_articlesTableView.SeparatorInset = new UIEdgeInsets (0, 0, 0, 0);
			_articlesTableView.Bounces = true;
            _articlesTableView.SeparatorColor = UIColor.FromRGB(100, 100, 100);
			View.Add (_articlesTableView);

			var screenWidth =
				ApplicationWorker.Settings.GetScreenWidthForScreen((int)UIScreen.MainScreen.Bounds.Size.Width);
			ApplicationWorker.Settings.ScreenWidth = screenWidth;
			ApplicationWorker.Settings.SaveSettings();
			_isLoadingData = true;
			ApplicationWorker.RemoteWorker.BannerGetted += BannerGetted;
			ThreadPool.QueueUserWorkItem (state => ApplicationWorker.RemoteWorker.BeginGetBanner (ApplicationWorker.Settings));
		}

		//Создание кнопки Загрузить еще
		private void InitAddPreviousArticleButton()
		{
			var button = new UIButton ();
			button.TitleLabel.Text = "Загрузить еще";
			button.TitleLabel.TextAlignment = UITextAlignment.Center;
			button.TitleLabel.BackgroundColor = ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetBackgroundColor());
			button.BackgroundColor = ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetBackgroundColor());
			button.TouchUpInside += AddPreviousArticleOnClick;
			_addPreviousArticleButton = button;
		}

		//Инициализация панели навигации
		private void InitBottomToolbar()
		{
			var height = 66f;
			_bottomBar = new BottomToolbarView ();
			_bottomBar.Frame = new RectangleF(0, View.Frame.Height - height, View.Frame.Width, height);
			_bottomBar.LayoutIfNeeded();
			_bottomBar.NewsButton.SetState (true);	
			_bottomBar.NewsButton.ButtonClick += ButNewsOnClick;
			_bottomBar.TrendsButton.ButtonClick += ButTrendsOnClick;
			_bottomBar.MagazineButton.ButtonClick += ButMagazineOnClick;
			_bottomBar.ArchiveButton.ButtonClick += ButArchiveOnClick;
			_bottomBar.FavoritesButton.ButtonClick += ButFavoriteOnClick;
			View.Add(_bottomBar);
		}

		//Инициализация баннера
		private void InitBanner(Banner banner)
		{
			var maxPictureHeight = UIScreen.MainScreen.Bounds.Size.Height * 0.15;
			var screenWidth = UIScreen.MainScreen.Bounds.Size.Width;
			var picture = banner.Picture;
			//Если баннер не анимированный Gif
			if (picture.Extension != PictureExtension.Gif)
			{
				var koefScaling = screenWidth / picture.Width;
				var pictHeightScaling = picture.Height * koefScaling;
				if (pictHeightScaling > maxPictureHeight)
				{
					koefScaling = (float)maxPictureHeight / picture.Height;
				}
				var x = 0;
				if ((int)(koefScaling * (picture.Width)) < (int)screenWidth)
				{
					x = (int)(((int)screenWidth - (int)(koefScaling * (picture.Width))) / 2);
				}
				var image = new UIImageView(ItExpertHelper.GetImageFromBase64String(picture.Data));
				image.Frame = new RectangleF (x, 0, picture.Width * koefScaling, picture.Height * koefScaling);
                _banner = image;
			}
			else
			{
				var koefScaling = screenWidth / picture.Width;
				var pictHeightScaling = picture.Height * koefScaling;
				if (pictHeightScaling > maxPictureHeight)
				{
					koefScaling = (float)maxPictureHeight / picture.Height;
				}
                _banner = new BannerView (banner, koefScaling, screenWidth);
			}
			//Прикрепить обработчик клика по баннеру
		}

		void InitLoadingProgress()
		{
			var height = 50;
			var bottomBarHeight = _bottomBar.Frame.Height;
			_loadingIndicator = new UIActivityIndicatorView (
				new RectangleF (0, View.Bounds.Height - (height + bottomBarHeight), View.Bounds.Width, height));
			_loadingIndicator.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.WhiteLarge;
			_loadingIndicator.Color = UIColor.Blue;
			_loadingIndicator.BackgroundColor = ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetBackgroundColor());
			View.Add (_loadingIndicator);
			_loadingIndicator.Hidden = true;
		}

		private void ClearCache()
		{
			if (!ApplicationWorker.Settings.OfflineMode)
			{
				Action action = () =>
				{
					var dbSize = ApplicationWorker.Db.GetDbSize();
					var dbSizeLimit = ApplicationWorker.Settings.GetDbLimitSizeInMb()*(1024*1024);
					if (dbSize > dbSizeLimit)
					{
//						InvokeOnMainThread(
//							() => Toast.MakeText(this, "Очищается кэш, пожалуйста подождите", ToastLength.Short).Show());
						ApplicationWorker.Db.ClearCache();
//						InvokeOnMainThread(
//							() => Toast.MakeText(this, "Кэш очищен", ToastLength.Short).Show());
					}
				};
				ThreadPool.QueueUserWorkItem(state => action());
			}
		}

		#endregion

		#region Event Handlers

		void BannerGetted (object sender, BannerEventArgs e)
		{
			ApplicationWorker.RemoteWorker.BannerGetted -= BannerGetted;
			_isLoadingData = false;
			InvokeOnMainThread (() =>
			{
				if (!e.Error)
				{
					Banner banner = null;
					if (e.Banners != null && e.Banners.Any())
					{
						banner = e.Banners.FirstOrDefault();
					}
					if (banner != null)
					{
						InitBanner(banner);
					}
				}
				else
				{
					new UIAlertView ("Ошибка", "Ошибка при получении баннера", null, "OK", null).Show ();
				}
			});
			_isLoadingData = true;
			ApplicationWorker.RemoteWorker.NewsGetted += NewNewsGetted;
			ThreadPool.QueueUserWorkItem (state => ApplicationWorker.RemoteWorker.BeginGetNews (ApplicationWorker.Settings, -1, -1, -1, -1, -1, null));
		}

		void NewNewsGetted (object sender, ArticleEventArgs e)
		{
			ApplicationWorker.RemoteWorker.NewsGetted -= NewNewsGetted;
			_isLoadingData = false;
			if (e.Abort)
			{
				return;
			}
			InvokeOnMainThread(() =>
			{
				var error = e.Error;
				if (!error)
				{
					ApplicationWorker.Db.SetPropertiesForArticles(e.Articles);
					ApplicationWorker.NormalizePreviewText(e.Articles);
					ThreadPool.QueueUserWorkItem(state => UpdateData(e));
					//Обновление списка новостей для ListView
					AddNewArticles(e.Articles);
				}
				else
				{
					//Toast.MakeText(this, "Ошибка при загрузке", ToastLength.Short).Show();
				}
				SetLoadingImageVisible(false);
			});
		}

		private void PreviousNewsGetted(object sender, ArticleEventArgs e)
		{
			ApplicationWorker.RemoteWorker.NewsGetted -= PreviousNewsGetted;
			_isLoadingData = false;
			if (e.Abort)
			{
				return;
			}
			InvokeOnMainThread(() =>
			{
				var error = e.Error;
				if (!error)
				{
					ApplicationWorker.Db.SetPropertiesForArticles(e.Articles);
					ApplicationWorker.NormalizePreviewText(e.Articles);
					ThreadPool.QueueUserWorkItem(state => UpdateData(e));
					AddPreviousArticles(e.Articles);
				}
				else
				{
					//Toast.MakeText(this, "Ошибка при загрузке", ToastLength.Short).Show();
				}
			});
		}

		//Загрузка предыдущих статей
		private void AddPreviousArticleOnClick(object sender, EventArgs eventArgs)
		{
			if (_isLoadingData)return;
			Action addLoadingProgress = () =>
			{
				var button = sender as UIButton;
				if (button != null)
				{
					var loading = new UIActivityIndicatorView ( new RectangleF(0, 0, View.Bounds.Width, 40));
					loading.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.WhiteLarge;
					loading.Color = UIColor.Blue;
					loading.StartAnimating ();
					button.Superview.Add (loading);
					button.RemoveFromSuperview ();
				}
			};
			if (!ApplicationWorker.Settings.OfflineMode)
			{
				var connectAccept = IsConnectionAccept();
				if (!connectAccept)
				{
//					Toast.MakeText(this, "Нет доступных подключений, для указанных в настройках",
//						ToastLength.Short).Show();
					return;
				}
				_isLoadingData = true;
				var lastTimestam = _allArticles.Min(x => x.Timespan);
				ApplicationWorker.RemoteWorker.NewsGetted += PreviousNewsGetted;
				ThreadPool.QueueUserWorkItem(
					state =>
					ApplicationWorker.RemoteWorker.BeginGetNews(ApplicationWorker.Settings, -1, lastTimestam,
						_blockId, _sectionId, _authorId, _search));
				addLoadingProgress ();
			}
			else
			{
				addLoadingProgress ();
				Action getList = () =>
				{
					var count = _allArticles.Count();
					var isTrends = _blockId == 30;
					var lst = ApplicationWorker.Db.GetArticlesFromDb(count, 20, isTrends);
					if (lst.Count() < 20)
					{
						_prevArticlesExists = false;
					}
					InvokeOnMainThread(() =>
					{
						AddPreviousArticles(lst);
						SetLoadingImageVisible(false);
					});
				};
				ThreadPool.QueueUserWorkItem(state => getList());
			}
		}

		private void ButNewsOnClick(object sender, EventArgs e)
		{
			_headerAdded = false;
			_header = null;
			PageNewsActivate();
		}

		private void ButTrendsOnClick(object sender, EventArgs e)
		{
			_headerAdded = false;
			_header = null;
			PageTrendsActivate();
		}

		private void ButArchiveOnClick(object sender, EventArgs e)
		{
			ArchiveViewController showController = null;
			var controllers = NavigationController.ViewControllers;
			foreach (var controller in controllers)
			{
				showController = controller as ArchiveViewController;
				if (showController != null)
				{
					break;
				}
			}
			if (showController != null)
			{
				NavigationController.PopToViewController (showController, true);
			}
			else
			{
				showController = new ArchiveViewController ();
				NavigationController.PushViewController (showController, true);
			}
		}

		private void ButMagazineOnClick(object sender, EventArgs e)
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
				showController.SetMagazineId (-1);
			}
			else
			{
				showController = new MagazineViewController(-1);
				NavigationController.PushViewController(showController, true);
			}
		}

		private void ButFavoriteOnClick(object sender, EventArgs e)
		{
			FavoritesViewController showController = null;
			var controllers = NavigationController.ViewControllers;
			foreach (var controller in controllers)
			{
				showController = controller as FavoritesViewController;
				if (showController != null)
				{
					break;
				}
			}
			if (showController != null)
			{
				NavigationController.PopToViewController (showController, true);
			}
			else
			{
				showController = new FavoritesViewController ();
				NavigationController.PushViewController (showController, true);
			}
		}

		private void OnPushArticleDetails(object sender, PushDetailsEventArgs e)
		{
			NavigationController.PushViewController (e.NewsDetailsView, true);
		}

		#endregion

		#region Activity logic

		public void ShowFromAnotherScreen(Page pageToShow)
		{
			var sectionChange = false;
			var blockChange = false;
			var sectionId = 0;
			var blockId = 0;
			_bottomBar.NewsButton.SetState (false);
			_bottomBar.TrendsButton.SetState (false);
			if (pageToShow == Page.News)
			{
				sectionId = -1;
				blockId = -1;
				_bottomBar.NewsButton.SetState (true);
			}
			if (pageToShow == Page.Trends)
			{
				sectionId = -1;
				blockId = 30;
				_bottomBar.TrendsButton.SetState (true);
			}
			if (_sectionId != sectionId)
			{
				_sectionId = sectionId;
				sectionChange = true;
			}
			if (_blockId != blockId)
			{
				_blockId = blockId;
				blockChange = true;
			}
			if (!ApplicationWorker.Settings.OfflineMode)
			{
				var connectAccept = IsConnectionAccept();
				if (!connectAccept)
				{
//					Toast.MakeText(this, "Нет доступных подключений, для указанных в настройках",
//						ToastLength.Long).Show();
					return;
				}
			}
			if (sectionChange || blockChange || _search != null)
			{
				_search = null;
				if (_articles != null)
				{
					_articles.Clear();
					_allArticles.Clear();
				}
				_articlesTableView.ReloadData ();
				_prevArticlesExists = true;
				if (!ApplicationWorker.Settings.OfflineMode)
				{
					_isLoadingData = true;
					ApplicationWorker.RemoteWorker.NewsGetted += NewNewsGetted;
					ThreadPool.QueueUserWorkItem(
						state =>
						ApplicationWorker.RemoteWorker.BeginGetNews(ApplicationWorker.Settings, -1, -1,
							_blockId,
							_sectionId, -1, _search));
					SetLoadingImageVisible(true);
				}
				else
				{
					Action getList = () =>
					{
						InvokeOnMainThread(() =>
						{
							if (_articlesTableView.Source != null) 
							{
								(_articlesTableView.Source as ArticlesTableSource).PushDetailsView -= OnPushArticleDetails;

								_articlesTableView.Source.Dispose();
								_articlesTableView.Source = null;
							}
							var source = new ArticlesTableSource(new List<Article>(), false, MagazineAction.NoAction);
							_articlesTableView.Source = source;
							_articlesTableView.ReloadData();
						});
						var isTrends = _blockId == 30;
						var lst = ApplicationWorker.Db.GetArticlesFromDb(0, 20, isTrends);
						if (lst.Count() < 20)
						{
							_prevArticlesExists = false;
						}
						InvokeOnMainThread(() => AddNewArticles(lst));
					};
					ThreadPool.QueueUserWorkItem(state => getList());
				}
				return;
			}
			if (_articles != null && _articles.Any())
			{
				if (_articlesTableView.Source != null) 
				{
					(_articlesTableView.Source as ArticlesTableSource).PushDetailsView -= OnPushArticleDetails;

					_articlesTableView.Source.Dispose();
					_articlesTableView.Source = null;
				}

				ArticlesTableSource source = new ArticlesTableSource(_articles, false, MagazineAction.NoAction);
				source.PushDetailsView += OnPushArticleDetails;

				_articlesTableView.Source = source;
				_articlesTableView.ReloadData();
			}
		}

		public void FilterAuthor(int sectionId, int blockId, int authorId)
		{
			_sectionId = sectionId;
			_blockId = blockId;
			_authorId = authorId;
			_bottomBar.TrendsButton.SetState (false);
			_bottomBar.NewsButton.SetState (true);
			_currentPage = Page.News;
			if (!ApplicationWorker.Settings.OfflineMode)
			{
				var connectAccept = IsConnectionAccept();
				if (!connectAccept)
				{
//					Toast.MakeText(this, "Нет доступных подключений, для указанных в настройках",
//						ToastLength.Long).Show();
					return;
				}
				var author = ApplicationWorker.Db.GetAuthor(_authorId);
				if (author != null)
				{
					_headerAdded = true;
					_header = author.Name;
				}
				_prevArticlesExists = true;
				_isLoadingData = true;
				if (_articles != null)
				{
					_articles.Clear();
				}
				_articlesTableView.ReloadData ();
				_search = null;
				ApplicationWorker.RemoteWorker.NewsGetted += NewNewsGetted;
				ThreadPool.QueueUserWorkItem(
					state =>
					ApplicationWorker.RemoteWorker.BeginGetNews(ApplicationWorker.Settings, -1, -1, _blockId,
						_sectionId, _authorId, _search));
				SetLoadingImageVisible(true);
				return;
			}
			else
			{
//				Toast.MakeText(this, "Поиск не доступен в оффлайн режиме",
//					ToastLength.Long).Show();
				return;
			}
		}

		public void FilterSection(int sectionId, int blockId)
		{
			_sectionId = sectionId;
			_blockId = blockId;
			_bottomBar.TrendsButton.SetState (false);
			_bottomBar.NewsButton.SetState (true);
			_currentPage = Page.News;
			if (!ApplicationWorker.Settings.OfflineMode)
			{
				var connectAccept = IsConnectionAccept();
				if (!connectAccept)
				{
//					Toast.MakeText(this, "Нет доступных подключений, для указанных в настройках",
//						ToastLength.Long).Show();
					return;
				}
				var section = ApplicationWorker.Db.GetSection(_sectionId);
				if (section != null)
				{
					_headerAdded = true;
					_header = section.Name;
				}
				_prevArticlesExists = true;
				_isLoadingData = true;
				if (_articles != null)
				{
					_articles.Clear();
				}
				_articlesTableView.ReloadData ();
				_search = null;
				ApplicationWorker.RemoteWorker.NewsGetted += NewNewsGetted;
				ThreadPool.QueueUserWorkItem(
					state =>
					ApplicationWorker.RemoteWorker.BeginGetNews(ApplicationWorker.Settings, -1, -1, _blockId,
						_sectionId, -1, _search));
				SetLoadingImageVisible(true);
				return;
			}
			else
			{
//				Toast.MakeText(this, "Поиск не доступен в оффлайн режиме",
//					ToastLength.Long).Show();
				return;
			}
		}

		private void UpdateData(ArticleEventArgs e)
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
						ApplicationWorker.Db.DeleteItemSectionsForArticle(article.Id);
						ApplicationWorker.Db.UpdateArticle(article);
						ApplicationWorker.Db.InsertItemSections(article.Sections);
					}
				}
			}   
		}

		private void PageNewsActivate()
		{
			if (_blockId != -1 || _sectionId != -1 || _authorId != -1 || !string.IsNullOrWhiteSpace(_search) || _currentPage == Page.Trends)
			{
				_prevArticlesExists = true;
				_currentPage = Page.News;
				_bottomBar.TrendsButton.SetState (false);
				_bottomBar.NewsButton.SetState (true);
				if (_isLoadingData)
				{
					ApplicationWorker.RemoteWorker.NewsGetted -= NewNewsGetted;
					ApplicationWorker.RemoteWorker.NewsGetted -= PreviousNewsGetted;
					ApplicationWorker.RemoteWorker.Abort();
					SetLoadingImageVisible(false);
				}
				if (!ApplicationWorker.Settings.OfflineMode)
				{
					var connectAccept = IsConnectionAccept();
					if (!connectAccept)
					{
//						Toast.MakeText(this, "Нет доступных подключений, для указанных в настройках", ToastLength.Long)
//							.Show();
						return;
					}
				}
				_blockId = -1;
				_sectionId = -1;
				_authorId = -1;
				_search = null;
				if (_articles != null)
				{
					_articles.Clear();
				}
				_articlesTableView.ReloadData ();
				if (!ApplicationWorker.Settings.OfflineMode)
				{
					_isLoadingData = true;
					ApplicationWorker.RemoteWorker.NewsGetted += NewNewsGetted;
					ThreadPool.QueueUserWorkItem(
						state =>
						ApplicationWorker.RemoteWorker.BeginGetNews(ApplicationWorker.Settings, -1, -1, _blockId,
							_sectionId, -1, _search));
					SetLoadingImageVisible(true);
				}
				else
				{
					Action getList = () =>
					{
						InvokeOnMainThread(() =>
						{
							if (_articlesTableView.Source != null) 
							{
								(_articlesTableView.Source as ArticlesTableSource).PushDetailsView -= OnPushArticleDetails;

								_articlesTableView.Source.Dispose();
								_articlesTableView.Source = null;
							}
							var source = new ArticlesTableSource(new List<Article>(), false, MagazineAction.NoAction);
							_articlesTableView.Source = source;
							_articlesTableView.ReloadData();
							SetLoadingImageVisible(true);
						});
						var lst = ApplicationWorker.Db.GetArticlesFromDb(0, 20, false);
						if (lst.Count() < 20)
						{
							_prevArticlesExists = false;
						}
						InvokeOnMainThread(() =>
						{
							AddNewArticles(lst);
							SetLoadingImageVisible(false);
						});
					};
					ThreadPool.QueueUserWorkItem(state => getList());
				}
			}
		}

		private void PageTrendsActivate()
		{
			if (_blockId != 30 || _currentPage == Page.News)
			{
				_prevArticlesExists = true;
				_currentPage = Page.Trends;
				_bottomBar.TrendsButton.SetState (true);
				_bottomBar.NewsButton.SetState (false);
				if (_isLoadingData)
				{
					ApplicationWorker.RemoteWorker.NewsGetted -= NewNewsGetted;
					ApplicationWorker.RemoteWorker.NewsGetted -= PreviousNewsGetted;
					ApplicationWorker.RemoteWorker.Abort();
					SetLoadingImageVisible(false);
				}
				if (!ApplicationWorker.Settings.OfflineMode)
				{
					var connectAccept = IsConnectionAccept();
					if (!connectAccept)
					{
//						Toast.MakeText(this, "Нет доступных подключений, для указанных в настройках", ToastLength.Long)
//							.Show();
						return;
					}
				}
				_blockId = 30;
				_sectionId = -1;
				_authorId = -1;
				_search = null;
				if (_articles != null)
				{
					_articles.Clear();
				}
				_articlesTableView.ReloadData ();
				if (!ApplicationWorker.Settings.OfflineMode)
				{
					_isLoadingData = true;
					ApplicationWorker.RemoteWorker.NewsGetted += NewNewsGetted;
					ThreadPool.QueueUserWorkItem(
						state =>
						ApplicationWorker.RemoteWorker.BeginGetNews(ApplicationWorker.Settings, -1, -1, _blockId,
							_sectionId, -1, _search));
					SetLoadingImageVisible(true);
				}
				else
				{
					_isLoadingData = false;
					Action getList = () =>
					{
						InvokeOnMainThread(() =>
						{
							if (_articlesTableView.Source != null) 
							{
								(_articlesTableView.Source as ArticlesTableSource).PushDetailsView -= OnPushArticleDetails;

								_articlesTableView.Source.Dispose();
								_articlesTableView.Source = null;
							}
							var source = new ArticlesTableSource(new List<Article>(), false, MagazineAction.NoAction);
							_articlesTableView.Source = source;
							_articlesTableView.ReloadData();
							SetLoadingImageVisible(true);
						});
						var lst = ApplicationWorker.Db.GetArticlesFromDb(0, 20, true);
						if (lst.Count() < 20)
						{
							_prevArticlesExists = false;
						}
						InvokeOnMainThread(() =>
						{
							AddNewArticles(lst);
							SetLoadingImageVisible(false);
						});
					};
					ThreadPool.QueueUserWorkItem(state => getList());
				}
			}
		}

		//Добавление в начало списка статей
		private void AddNewArticles(List<Article> lst)
		{
			if (lst == null || !lst.Any())
			{
				//Toast.MakeText(this, "Статей нет", ToastLength.Short).Show();
				_prevArticlesExists = false;
				return;
			} 
			_allArticles = lst.OrderByDescending(x => x.ActiveFrom).ToList();
			if (ApplicationWorker.Settings.HideReaded)
			{
				_articles = _allArticles.Where(x => !x.IsReaded).ToList();
				if (_articles.Count() < 6)
				{
					var count = 6 - _articles.Count();
					_articles.AddRange(_allArticles.Where(x => x.IsReaded).Take(count));
					_articles = _articles.OrderByDescending(x => x.ActiveFrom).ToList();
				}
			}
			else
			{
				_articles = _allArticles.ToList();
			}
			//Добавление заголовка
			if (_headerAdded && !string.IsNullOrWhiteSpace(_header))
			{
				_articles.Insert(0,
					new Article() {ArticleType = ArticleType.Header, Name = _header});
			}
			//Добавление расширенного объекта: баннера
			if (_banner != null)
			{
				_articles.Insert(0,
                    new Article() {ArticleType = ArticleType.Banner, ExtendedObject = _banner});
			}
			//Добавление кнопки Загрузить еще
			if (_addPreviousArticleButton != null && _prevArticlesExists)
			{
				_articles.Add(new Article()
				{
                    ArticleType = ArticleType.PreviousArticlesButton,
					ExtendedObject = _addPreviousArticleButton
				});
			}
			if (_articlesTableView.Source != null) 
			{
				(_articlesTableView.Source as ArticlesTableSource).PushDetailsView -= OnPushArticleDetails;

				_articlesTableView.Source.Dispose();
				_articlesTableView.Source = null;
			}
			var source = new ArticlesTableSource(_articles, false, MagazineAction.NoAction);
			source.PushDetailsView += OnPushArticleDetails;
			_articlesTableView.Source = source;
			_articlesTableView.ReloadData();
		}

		//Добавление последующих статей
		private void AddPreviousArticles(List<Article> lst)
		{
			if (lst == null || !lst.Any())
			{
				//				Toast.MakeText(this, "Больше статей нет", ToastLength.Short).Show();
				_prevArticlesExists = false;
				if (_addPreviousArticleButton != null)
				{
					_articles.RemoveAt(_articles.Count() - 1);
				}
				_articlesTableView.ReloadData ();
				return;
			}
			var position = _articles.Count(x => !x.IsReaded) - 1;
			_articles.Clear();
			_allArticles.AddRange(lst.OrderByDescending(x => x.ActiveFrom).ToList());
			if (ApplicationWorker.Settings.HideReaded)
			{
				var buffer = _allArticles.Where(x => !x.IsReaded).ToList();
				if (buffer.Count() < 6)
				{
					var count = 6 - buffer.Count();
					buffer.AddRange(_allArticles.Where(x => x.IsReaded).Take(count));
					buffer = buffer.OrderByDescending(x => x.ActiveFrom).ToList();
					position = 6 - count;
				}
				_articles.AddRange(buffer);
			}
			else
			{
				_articles.AddRange(_allArticles.ToList());
			}
			//Добавление заголовка
			if (_headerAdded && !string.IsNullOrWhiteSpace(_header))
			{
				_articles.Insert(0,
					new Article() { ArticleType = ArticleType.Header, Name = _header });
			}
			//Добавление расширенного объекта: баннера
			if (_banner != null)
			{
				_articles.Insert(0,
                    new Article() { ArticleType = ArticleType.Banner, ExtendedObject = _banner });
			}
			//Добавление кнопки Загрузить еще
			if (_addPreviousArticleButton != null && _prevArticlesExists)
			{
				_articles.Add(new Article()
				{
                    ArticleType = ArticleType.PreviousArticlesButton,
					ExtendedObject = _addPreviousArticleButton
				});
			}
			//Перерисовать список
			_articlesTableView.ReloadData();
			if (ApplicationWorker.Settings.HideReaded)
			{
				//Прокрутить список к position
			}
		}



		#endregion

		#region Helpers method

		private bool IsConnectionAccept()
		{
			var result = true;
			return result;
		}

		private void SetLoadingImageVisible(bool visible)
		{
			if (visible)
			{
				_loadingIndicator.Hidden = false;
				_loadingIndicator.StartAnimating ();
				View.BringSubviewToFront (_loadingIndicator);
			}
			else
			{
				_loadingIndicator.Hidden = true;
				_loadingIndicator.StopAnimating ();
			}
		}

		#endregion
	}
}

