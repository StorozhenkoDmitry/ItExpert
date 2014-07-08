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
using System.IO;
using System.Text;

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
		private bool _fromAnother = false;
		private bool _firstLoad = true;

		#endregion

		#region UIViewController members

		public NewsViewController(){
		}

		public NewsViewController(Page page) {
			_fromAnother = true;
			_currentPage = page;
		}

        public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
        {
            base.DidRotate(fromInterfaceOrientation);
			var screenWidth =
				ApplicationWorker.Settings.GetScreenWidthForScreen(
					(int)View.Bounds.Width);
			var banners = ApplicationWorker.Db.LoadBanners();
			Banner banner = null;
			if (banners != null && banners.Any())
			{
				banner = banners.FirstOrDefault(x => x.ScreenWidth == screenWidth);
				if (banner != null)
				{
					var pictures = ApplicationWorker.Db.GetPicturesForParent(banner.Id);
					if (pictures != null && pictures.Any())
					{
						var picture = pictures.FirstOrDefault(x => x.Type == PictypeType.Banner);
						if (picture != null)
						{
							banner.Picture = picture;
						}
					}
				}
			}
			if (banner != null)
			{
				InitBanner(banner);
			}
			if (_allArticles != null & _allArticles.Any ())
			{
				_articles.Clear ();
				if (ApplicationWorker.Settings.HideReaded)
				{
					var buffer = _allArticles.Where (x => !x.IsReaded).ToList ();
					if (buffer.Count () < 6)
					{
						var count = 6 - buffer.Count ();
						buffer.AddRange (_allArticles.Where (x => x.IsReaded).Take (count));
						buffer = buffer.OrderByDescending (x => x.ActiveFrom).ToList ();
					}
					_articles.AddRange (buffer);
				}
				else
				{
					_articles.AddRange (_allArticles.ToList ());
				}
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
				if (_articlesTableView != null && _articlesTableView.Source != null)
				{
					if (_articlesTableView.Source is DoubleArticleTableSource)
					{
						(_articlesTableView.Source as DoubleArticleTableSource).UpdateSource ();
					}
				}
			}
			UpdateViewsLayout ();
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
			NavigationController.NavigationBarHidden = false;

			// Perform any additional setup after loading the view, typically from a nib.
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			if (_firstLoad)
			{
				Initialize ();
				_firstLoad = false;
			}
			UpdateViewsLayout ();
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
			if (string.IsNullOrWhiteSpace(ApplicationWorker.Css))
			{
				var folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
				var path = Path.Combine(folder, "css.dt");
				var fileInfo = new FileInfo(path);
				if (fileInfo.Exists)
				{
					using (var sr = new StreamReader(path, Encoding.UTF8))
					{
						ApplicationWorker.Css = sr.ReadToEnd();
					} 
				}
			}

            float scale = 3.8f;
            TabBarItem = new UITabBarItem("News", new UIImage(NSData.FromFile("News.png"), scale), 0);
            View.AutosizesSubviews = true;
            AutomaticallyAdjustsScrollViewInsets = false;
			View.BackgroundColor = ItExpertHelper.GetUIColorFromColor (ApplicationWorker.Settings.GetBackgroundColor ());

            InitAddPreviousArticleButton ();
            InitBottomToolbar();
			InitLoadingProgress ();

            _articlesTableView = new UITableView(new RectangleF(0, 0, 0, 
                0), UITableViewStyle.Plain);
			_articlesTableView.ScrollEnabled = true; 
			_articlesTableView.UserInteractionEnabled = true;
			_articlesTableView.SeparatorInset = new UIEdgeInsets (0, 0, 0, 0);
			_articlesTableView.Bounces = true;
            _articlesTableView.SeparatorColor = UIColor.FromRGB(100, 100, 100);
			View.Add (_articlesTableView);

			if (!_fromAnother)
			{
				var width = View.Bounds.Width;
				var screenWidth =
					ApplicationWorker.Settings.GetScreenWidthForScreen ((int)width);
				var banners = ApplicationWorker.Db.LoadBanners();
				Banner banner = null;
				if (banners != null && banners.Any())
				{
					banner = banners.FirstOrDefault(x => x.ScreenWidth == screenWidth);
					if (banner != null)
					{
						var pictures = ApplicationWorker.Db.GetPicturesForParent(banner.Id);
						if (pictures != null && pictures.Any())
						{
							var picture = pictures.FirstOrDefault(x => x.Type == PictypeType.Banner);
							if (picture != null)
							{
								banner.Picture = picture;
							}
						}
					}
				}
				if (banner != null)
				{
					InitBanner(banner);
				}
				var connectAccept = IsConnectionAccept();
				if (!connectAccept)
				{
					ShowOfflineModeDialog ();
				}
				else
				{
					SelectStartSection(ApplicationWorker.Settings.Page);
				}
			}
			else
			{
				_fromAnother = false;
				if (_currentPage == Page.News)
				{
					_currentPage = Page.Trends;
					PageNewsActivate ();
				}
				if (_currentPage == Page.Trends)
				{
					_currentPage = Page.News;
					PageTrendsActivate ();
				}
			}
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
			_bottomBar.NewsButton.SetActiveState (true);	
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
			var maxPictureHeight = View.Bounds.Height * 0.15;
			var screenWidth = View.Bounds.Width;
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
				var image = new BannerImageView(ItExpertHelper.GetImageFromBase64String(picture.Data), banner);
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
                _banner = new BannerGifView (banner, koefScaling, screenWidth);
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

        private void UpdateViewsLayout()
        {
            if (_articlesTableView != null)
            {
				var tableViewTopOffset = NavigationController.NavigationBar.Frame.Height + ItExpertHelper.StatusBarHeight;
                _articlesTableView.Frame = new RectangleF(0, tableViewTopOffset, View.Bounds.Width, 
                    View.Bounds.Height - tableViewTopOffset - _bottomBar.Frame.Height);
                _articlesTableView.ReloadData();
            }

            if (_bottomBar != null)
            {
                _bottomBar.RemoveFromSuperview();
                _bottomBar.Dispose();
                _bottomBar = null;

            }

            InitBottomToolbar();
        }

		#endregion

		#region Activity logic

		void ShowOfflineModeDialog()
		{
			var alertView = new BlackAlertView ("Оффлайн режим", "Нет доступных подключений. Перевести приложение в режим Оффлайн?", "Нет", "Да");

			alertView.ButtonPushed += (sender, e) =>
			{
				if (e.ButtonIndex == 0)
				{
					ToOfflineModeNo();
				}
				if (e.ButtonIndex == 1)
				{
					ToOfflineModeYes();
				}
			};

			alertView.Show ();
		}

		private void ToOfflineModeYes()
		{
			ApplicationWorker.Settings.OfflineMode = true;
			ToOfflineModeNo();
		}

		private void ToOfflineModeNo()
		{
			ClearCache();
			_isLoadingData = false;
			var screenWidth =
				ApplicationWorker.Settings.GetScreenWidthForScreen(
					(int)View.Bounds.Width);
			var bannerFound = false;
			var banners = ApplicationWorker.Db.LoadBanners();
			Banner banner = null;
			if (banners != null && banners.Any())
			{
				banner = banners.FirstOrDefault(x => x.ScreenWidth == screenWidth);
				if (banner != null)
				{
					var pictures = ApplicationWorker.Db.GetPicturesForParent(banner.Id);
					if (pictures != null && pictures.Any())
					{
						var picture = pictures.FirstOrDefault(x => x.Type == PictypeType.Banner);
						if (picture != null)
						{
							banner.Picture = picture;
							bannerFound = true;
						}
					}
				}
			}
			if (bannerFound)
			{
				InitBanner(banner);
			}
			SelectStartSection(ApplicationWorker.Settings.Page);
		}

		private void SelectStartSection(Page page)
		{
			var e = ApplicationWorker.StartArticlesEventArgs;
			if (page == Page.News)
			{
				_currentPage = Page.News;
				_prevArticlesExists = true;
				_bottomBar.TrendsButton.SetActiveState (false);
				_bottomBar.NewsButton.SetActiveState (true);
				if (!ApplicationWorker.Settings.OfflineMode)
				{
					if (e == null)
					{
//						Toast.MakeText(this, "Ошибка при загрузке", ToastLength.Short).Show();
					}
					else
					{
						NewNewsGetted(null, e);
					}
				}
				else
				{
					SetLoadingImageVisible(true);
					Action getList = () =>
					{
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
			if (page == Page.Trends)
			{
				_prevArticlesExists = true;
				_currentPage = Page.Trends;
				_bottomBar.TrendsButton.SetActiveState (true);
				_bottomBar.NewsButton.SetActiveState (false);
				if (!ApplicationWorker.Settings.OfflineMode)
				{
					if (e == null)
					{
//						Toast.MakeText(this, "Ошибка при загрузке", ToastLength.Short).Show();
					}
					else
					{
						NewNewsGetted(null, e);
					}
				}
				else
				{
					SetLoadingImageVisible(true);
					Action getList = () =>
					{
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
			if (page == Page.Magazine)
			{
				ButMagazineOnClick(null, null);
			}
			if (page == Page.Archive)
			{
				ButArchiveOnClick(null, null);
			}
			if (page == Page.Favorite)
			{
				ButFavoriteOnClick(null, null);
			}
		}

		public void ShowFromAnotherScreen(Page pageToShow)
		{
			var sectionChange = false;
			var blockChange = false;
			var sectionId = 0;
			var blockId = 0;
			_bottomBar.NewsButton.SetActiveState (false);
			_bottomBar.TrendsButton.SetActiveState (false);
			if (pageToShow == Page.News)
			{
				sectionId = -1;
				blockId = -1;
				_bottomBar.NewsButton.SetActiveState (true);
			}
			if (pageToShow == Page.Trends)
			{
				sectionId = -1;
				blockId = 30;
				_bottomBar.TrendsButton.SetActiveState (true);
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
				if (_articlesTableView != null && _articlesTableView.Source != null)
				{
					if (_articlesTableView.Source is DoubleArticleTableSource)
					{
						(_articlesTableView.Source as DoubleArticleTableSource).UpdateSource ();
					}
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
                            UpdateTableView(new List<Article>());
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
                UpdateTableView(_articles);
			}
		}

		public void FilterAuthor(int sectionId, int blockId, int authorId)
		{
			_sectionId = sectionId;
			_blockId = blockId;
			_authorId = authorId;
			_bottomBar.TrendsButton.SetActiveState (false);
			_bottomBar.NewsButton.SetActiveState (true);
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
				if (_articlesTableView != null && _articlesTableView.Source != null)
				{
					if (_articlesTableView.Source is DoubleArticleTableSource)
					{
						(_articlesTableView.Source as DoubleArticleTableSource).UpdateSource ();
					}
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
			_bottomBar.TrendsButton.SetActiveState (false);
			_bottomBar.NewsButton.SetActiveState (true);
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
				if (_articlesTableView != null && _articlesTableView.Source != null)
				{
					if (_articlesTableView.Source is DoubleArticleTableSource)
					{
						(_articlesTableView.Source as DoubleArticleTableSource).UpdateSource ();
					}
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
				_bottomBar.TrendsButton.SetActiveState (false);
				_bottomBar.NewsButton.SetActiveState (true);
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
				if (_articlesTableView != null && _articlesTableView.Source != null)
				{
					if (_articlesTableView.Source is DoubleArticleTableSource)
					{
						(_articlesTableView.Source as DoubleArticleTableSource).UpdateSource ();
					}
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
							SetLoadingImageVisible(true);
                            UpdateTableView(new List<Article>());
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
				_bottomBar.TrendsButton.SetActiveState (true);
				_bottomBar.NewsButton.SetActiveState (false);
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
				if (_articlesTableView != null && _articlesTableView.Source != null)
				{
					if (_articlesTableView.Source is DoubleArticleTableSource)
					{
						(_articlesTableView.Source as DoubleArticleTableSource).UpdateSource ();
					}
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
							SetLoadingImageVisible(true);
                            UpdateTableView(new List<Article>());
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

            UpdateTableView(_articles);
		}

		//Добавление последующих статей
		private void AddPreviousArticles(List<Article> lst)
		{
			if (lst == null || !lst.Any())
			{
				//				Toast.MakeText(this, "Больше статей нет", ToastLength.Short).Show();
				_prevArticlesExists = false;
				if (_addPreviousArticleButton != null && _articles.Any())
				{
					_articles.RemoveAt(_articles.Count() - 1);
				}
				if (_articlesTableView != null && _articlesTableView.Source != null)
				{
					if (_articlesTableView.Source is DoubleArticleTableSource)
					{
						(_articlesTableView.Source as DoubleArticleTableSource).UpdateSource ();
					}
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
			if (_articlesTableView != null && _articlesTableView.Source != null)
			{
				if (_articlesTableView.Source is DoubleArticleTableSource)
				{
					(_articlesTableView.Source as DoubleArticleTableSource).UpdateSource ();
				}
			}
			_articlesTableView.ReloadData();
			if (ApplicationWorker.Settings.HideReaded)
			{
				//Прокрутить список к position
			}
		}

        private void UpdateTableView(List<Article> articles)
        {
            if (_articlesTableView.Source != null) 
            {
				if (_articlesTableView.Source is ArticlesTableSource)
				{
					(_articlesTableView.Source as ArticlesTableSource).PushDetailsView -= OnPushArticleDetails;
				}
				if (_articlesTableView.Source is DoubleArticleTableSource)
				{
					(_articlesTableView.Source as DoubleArticleTableSource).PushDetailsView -= OnPushArticleDetails;
				}
                _articlesTableView.Source.Dispose();
                _articlesTableView.Source = null;
            }

            UITableViewSource source = null;

            if (UserInterfaceIdiomIsPhone)
            {
                source = new ArticlesTableSource(articles, false, MagazineAction.NoAction);

                (source as ArticlesTableSource).PushDetailsView += OnPushArticleDetails;
            }
            else
            {
                source = new DoubleArticleTableSource(articles, false, MagazineAction.NoAction);

                (source as DoubleArticleTableSource).PushDetailsView += OnPushArticleDetails;
            }

            var tableViewTopOffset = NavigationController.NavigationBar.Frame.Height + ItExpertHelper.StatusBarHeight;
            _articlesTableView.Frame = new RectangleF(0, tableViewTopOffset, View.Bounds.Width, 
                View.Bounds.Height - tableViewTopOffset - _bottomBar.Frame.Height);

            _articlesTableView.ContentSize = new SizeF(_articlesTableView.Frame.Width, _articlesTableView.Frame.Height);

            _articlesTableView.Source = source;
            _articlesTableView.ReloadData();
        }

		#endregion

		#region Helpers method

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

