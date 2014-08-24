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
using BigTed;

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
		private UIButton _addPreviousArticleButton = null;
		private bool _prevArticlesExists = true;
		private bool _headerAdded = false;
		private string _header = null;
		private BottomToolbarView _bottomBar = null;
		private UIActivityIndicatorView _loadingIndicator;
		private bool _fromAnother = false;
		private bool _firstLoad = true;
		private Filter Filter = Filter.None;
		private UIInterfaceOrientation _currentOrientation;
		private UILabel _holdMessageView;
		private FilterParameters _filterParams = null;
		private MenuView _menu;
		private UIButton _menuButton;
		private UIBarButtonItem _menuBarButton;
		private SettingsView _settingsView;
		private UIButton _settingsButton;
		private UIBarButtonItem _settingsBarButton;
		private UIButton _refreshButton;
		private UIBarButtonItem _refreshBarButton;
		private UIButton _dumpCacheButton;
		private UIBarButtonItem _dumpCacheBarButton;

		#endregion

		#region UIViewController members

		public NewsViewController(){
		}

		public NewsViewController(Page page, string search) {
			_filterParams = null;
			_fromAnother = true;
			_currentPage = page;
			if (page == Page.None && !string.IsNullOrWhiteSpace(search))
			{
				_search = search;
			}
		}

		public NewsViewController(FilterParameters filterParams)
		{
			_filterParams = filterParams;
		}

		public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
			base.WillRotate(toInterfaceOrientation, duration);

			_bottomBar.Hidden = true;
			_articlesTableView.Hidden = true;
		}

        public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate(fromInterfaceOrientation);
			_currentOrientation = InterfaceOrientation;
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
			if (_allArticles != null & _allArticles.Any())
			{
				_articles.Clear();
				if (ApplicationWorker.Settings.HideReaded)
				{
					var buffer = _allArticles.Where(x => !x.IsReaded).ToList();
					if (buffer.Count() < 6)
					{
						var count = 6 - buffer.Count();
						buffer.AddRange(_allArticles.Where(x => x.IsReaded).Take(count));
						buffer = buffer.OrderByDescending(x => x.ActiveFrom).ToList();
					}
					_articles.AddRange(buffer);
				}
				else
				{
					_articles.AddRange(_allArticles.ToList());
				}
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
					_articles.Add(new Article() {
						ArticleType = ArticleType.PreviousArticlesButton,
						ExtendedObject = _addPreviousArticleButton
					});
				}
				if (_articlesTableView != null && _articlesTableView.Source != null)
				{
					if (_articlesTableView.Source is DoubleArticleTableSource)
					{
						(_articlesTableView.Source as DoubleArticleTableSource).UpdateSource();
					}
				}
			}

			UpdateViewsLayout();

			_bottomBar.Hidden = false;
			if (_articles != null && _articles.Any())
			{
				_articlesTableView.Hidden = false;
			}
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
			View.BackgroundColor = ItExpertHelper.GetUIColorFromColor (ApplicationWorker.Settings.GetBackgroundColor ());
			// Perform any additional setup after loading the view, typically from a nib.
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			if (!_firstLoad && _currentOrientation != InterfaceOrientation)
			{
				_currentOrientation = InterfaceOrientation;
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
				UpdateViewsLayout();
			}
			if (Filter == Filter.Section)
			{
				Filter = Filter.None;
				_bottomBar.TrendsButton.SetActiveState(false);
				_bottomBar.NewsButton.SetActiveState(true);
				if (!ApplicationWorker.Settings.OfflineMode)
				{
					var connectAccept = IsConnectionAccept();
					if (!connectAccept)
					{
						BTProgressHUD.ShowToast ("Нет доступных подключений, для указанных в настройках", ProgressHUD.MaskType.None, false, 2500);					
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
					//Пустой список
					if (_articles != null)
					{
						_articles.Clear();
					}
					if (_articlesTableView != null && _articlesTableView.Source != null)
					{
						if (_articlesTableView.Source is DoubleArticleTableSource)
						{
							(_articlesTableView.Source as DoubleArticleTableSource).UpdateSource();
						}
					}
					_articlesTableView.ReloadData();
					_articlesTableView.Hidden = true;
					_holdMessageView.Hidden = true;
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
					BTProgressHUD.ShowToast ("Поиск не доступен в оффлайн режиме", ProgressHUD.MaskType.None, false, 2500);	
					return;
				}
			}
			if (Filter == Filter.Author)
			{
				Filter = Filter.None;
				_bottomBar.TrendsButton.SetActiveState(false);
				_bottomBar.NewsButton.SetActiveState(true);
				if (!ApplicationWorker.Settings.OfflineMode)
				{
					var connectAccept = IsConnectionAccept();
					if (!connectAccept)
					{
						BTProgressHUD.ShowToast ("Нет доступных подключений, для указанных в настройках", ProgressHUD.MaskType.None, false, 2500);	
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
					//Пустой список
					if (_articles != null)
					{
						_articles.Clear();
					}
					if (_articlesTableView != null && _articlesTableView.Source != null)
					{
						if (_articlesTableView.Source is DoubleArticleTableSource)
						{
							(_articlesTableView.Source as DoubleArticleTableSource).UpdateSource();
						}
					}
					_articlesTableView.ReloadData();
					_articlesTableView.Hidden = true;
					_holdMessageView.Hidden = true;
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
					BTProgressHUD.ShowToast ("Поиск не доступен в оффлайн режиме", ProgressHUD.MaskType.None, false, 2500);	
					return;
				}
			}
			if (Filter == Filter.Page)
			{
				Filter = Filter.None;
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
						(_articlesTableView.Source as DoubleArticleTableSource).UpdateSource();
					}
				}
				//Пустой список
				_articlesTableView.ReloadData();
				_articlesTableView.Hidden = true;
				_holdMessageView.Hidden = true;
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
							//Пустой список
							UpdateTableView(new List<Article>());
							_articlesTableView.Hidden = true;
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
			if (Filter == Filter.Search)
			{
				Filter = Filter.None;
				//Пустой список
				if (_articles != null)
				{
					_articles.Clear();
				}
				if (_articlesTableView != null && _articlesTableView.Source != null)
				{
					if (_articlesTableView.Source is DoubleArticleTableSource)
					{
						(_articlesTableView.Source as DoubleArticleTableSource).UpdateSource();
					}
				}
				_articlesTableView.ReloadData();
				_articlesTableView.Hidden = true;
				_holdMessageView.Hidden = true;
				ApplicationWorker.RemoteWorker.NewsGetted += NewNewsGetted;
				ThreadPool.QueueUserWorkItem(
					state =>
					ApplicationWorker.RemoteWorker.BeginGetNews(ApplicationWorker.Settings, -1, -1, _blockId,
						_sectionId, -1, _search));
				SetLoadingImageVisible(true);
				return;
			}
			if (!_firstLoad && !_isLoadingData)
			{
				if (_allArticles == null || !_allArticles.Any())
					return;
				var newsList = ApplicationWorker.GetNewsList();
				if (newsList != null && newsList.Any())
				{
					_allArticles.Clear();
					_allArticles.AddRange(newsList);
				}
				var position = -1;
				Article selectArticle = null;
				var selectItemId = -1;
				if (_articlesTableView != null && _articlesTableView.Source != null)
				{
					if (_articlesTableView.Source is ArticlesTableSource)
					{
						selectItemId = (_articlesTableView.Source as ArticlesTableSource).GetSelectItemId();
						(_articlesTableView.Source as ArticlesTableSource).ResetSelectItem();
					}
					if (_articlesTableView.Source is DoubleArticleTableSource)
					{
						selectItemId = (_articlesTableView.Source as DoubleArticleTableSource).GetSelectItemId();
						(_articlesTableView.Source as DoubleArticleTableSource).ResetSelectItem();
					}
				}
				if (selectItemId != -1)
				{
					for (var i = 0; i < _articles.Count(); i++)
					{
						if (_articles[i].Id == selectItemId)
						{
							position = i;
							break;
						}
					}
					if (ApplicationWorker.Settings.HideReaded)
					{
						if (position > 0)
						{
							var isFound = false;
							while ( !isFound )
							{
								position--;
								if (position == 0)
								{
									break;
								}
								if ((_articles[position].ArticleType != ArticleType.Magazine &&
									_articles[position].ArticleType != ArticleType.Portal) &&
									!_articles[position].IsReaded)
								{
									isFound = true;
									selectArticle = _articles[position];
								}
							}
						}
					}
				}
				_articles.Clear();
				if (ApplicationWorker.Settings.HideReaded)
				{
					var buffer = _allArticles.Where(x => !x.IsReaded).ToList();
					if (buffer.Count() < 6)
					{
						var count = 6 - buffer.Count();
						buffer.AddRange(_allArticles.Where(x => x.IsReaded).Take(count));
						buffer = buffer.OrderByDescending(x => x.ActiveFrom).ToList();
					}
					_articles.AddRange(buffer);
					if (selectArticle != null && position > 0)
					{
						position = 0;
						for (var i = 0; i < _articles.Count(); i++)
						{
							if (_articles[i].Id == selectArticle.Id)
							{
								position = i;
								break;
							}
						}
					}
				}
				else
				{
					_articles.AddRange(_allArticles.ToList());
				}
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
					_articles.Add(new Article() {
						ArticleType = ArticleType.PreviousArticlesButton,
						ExtendedObject = _addPreviousArticleButton
					});
				}

				if (_articlesTableView != null && _articlesTableView.Source != null)
				{
					if (_articlesTableView.Source is DoubleArticleTableSource)
					{
						(_articlesTableView.Source as DoubleArticleTableSource).UpdateSource();
					}
				}
				UpdateViewsLayout();
				if (ApplicationWorker.Settings.HideReaded && position > 0)
				{
					if (!UserInterfaceIdiomIsPhone)
					{
						if (_banner != null && _addPreviousArticleButton != null)
						{
							position = (int)Math.Floor((double)(position - 2) / 2);
						}
						else
						{
							position = (int)Math.Floor((double)position / 2);
						}
					}
					var indexPath = NSIndexPath.FromItemSection(position, 0);
					_articlesTableView.ScrollToRow(indexPath, UITableViewScrollPosition.Middle, false);
				}
			}
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			if (_firstLoad)
			{
				_currentOrientation = InterfaceOrientation;
				_firstLoad = false;
				InitViews();
				Action action = () =>
				{
					Thread.Sleep(150);
					InvokeOnMainThread(()=>Initialize ());
				};
				ThreadPool.QueueUserWorkItem(state => action());
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

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			InvokeOnMainThread(() =>
			{
				ApplicationWorker.RemoteWorker.NewsGetted -= NewNewsGetted;
				ApplicationWorker.RemoteWorker.NewsGetted -= PreviousNewsGetted;
				ApplicationWorker.SettingsChanged -= OnSettingsChanged;
				if (_menuButton != null)
				{
					_menuButton.RemoveFromSuperview();
					if (_menuButton.ImageView != null && _menuButton.ImageView.Image != null)
					{
						_menuButton.ImageView.Image.Dispose();
						_menuButton.ImageView.Image = null;
					}
					_menuButton.TouchUpInside -= MenuButtonTouchUp;
					_menuButton.Dispose();
				}
				_menuButton = null;

				if (_menuBarButton != null)
				{
					_menuBarButton.Dispose();
				}
				_menuBarButton = null;

				if (_menu != null)
				{
					_menu.TapOutsideTableView -= ViewTapOutsideTableView;
					_menu.Dispose();
				}
				_menu = null;

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

				if (_refreshButton != null)
				{
					_refreshButton.RemoveFromSuperview();
					if (_refreshButton.ImageView != null && _refreshButton.ImageView.Image != null)
					{
						_refreshButton.ImageView.Image.Dispose();
						_refreshButton.ImageView.Image = null;
					}
					_refreshButton.TouchUpInside -= ButRefreshOnClick;
					_refreshButton.Dispose();
				}
				_refreshButton = null;

				if (_refreshBarButton != null)
				{
					_refreshBarButton.Dispose();
				}
				_refreshBarButton = null;

				if (_dumpCacheButton != null)
				{
					_dumpCacheButton.RemoveFromSuperview();
					if (_dumpCacheButton.ImageView != null && _dumpCacheButton.ImageView.Image != null)
					{
						_dumpCacheButton.ImageView.Image.Dispose();
						_dumpCacheButton.ImageView.Image = null;
					}
					_dumpCacheButton.TouchUpInside -= ButInCacheOnClick;
					_dumpCacheButton.Dispose();
				}
				_dumpCacheButton = null;

				if (_dumpCacheBarButton != null)
				{
					_dumpCacheBarButton.Dispose();
				}
				_dumpCacheBarButton = null;

				if (_bottomBar != null)
				{
					_bottomBar.RemoveFromSuperview();
					_bottomBar.NewsButton.ButtonClick -= ButNewsOnClick;
					_bottomBar.TrendsButton.ButtonClick -= ButTrendsOnClick;
					_bottomBar.MagazineButton.ButtonClick -= ButMagazineOnClick;
					_bottomBar.ArchiveButton.ButtonClick -= ButArchiveOnClick;
					_bottomBar.FavoritesButton.ButtonClick -= ButFavoriteOnClick;
					_bottomBar.Dispose();
				}
				_bottomBar = null;

				if (_addPreviousArticleButton != null)
				{
					_addPreviousArticleButton.RemoveFromSuperview();
					_addPreviousArticleButton.TouchUpInside -= AddPreviousArticleOnClick;
					_addPreviousArticleButton.TouchDown -= AddPreviousArticleTouchDown;
					_addPreviousArticleButton.TouchUpOutside -= AddPreviousArticleTouchUpOutside;
					_addPreviousArticleButton.Dispose();
				}
				_addPreviousArticleButton = null;

				if (_banner != null)
				{
					_banner.RemoveFromSuperview();
					_banner.Dispose();
				}
				_banner = null;

				if (_loadingIndicator != null)
				{
					_loadingIndicator.RemoveFromSuperview();
					_loadingIndicator.Dispose();
				}
				_loadingIndicator = null;

				if (_holdMessageView != null)
				{
					_holdMessageView.RemoveFromSuperview();
					_holdMessageView.Dispose();
				}
				_holdMessageView = null;

				if (_articlesTableView != null)
				{
					_articlesTableView.RemoveFromSuperview();
					if (_articlesTableView.Source != null)
					{
						if (_articlesTableView.Source is ArticlesTableSource)
						{
							((ArticlesTableSource)_articlesTableView.Source).PushDetailsView -= OnPushArticleDetails;
						}
						if (_articlesTableView.Source is DoubleArticleTableSource)
						{
							((DoubleArticleTableSource)_articlesTableView.Source).PushDetailsView -= OnPushArticleDetails;
						}
						_articlesTableView.Source.Dispose();
						_articlesTableView.Source = null;
					}
					_articlesTableView.Dispose();
				}
				_articlesTableView = null;

				if (_articles != null)
				{
					_articles.Clear();
				}
				_articles = null;

				if (_allArticles != null)
				{
					_allArticles.Clear();
				}
				_allArticles = null;
			});
		}

		#endregion

		#region Init

		void Initialize()
		{
			ClearCache ();

			if (string.IsNullOrWhiteSpace(ApplicationWorker.Css))
			{
				var documents = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
				var library = Path.Combine (documents, "..", "Library");
				var path = Path.Combine(library, "css.dt");
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
            
			ApplicationWorker.SettingsChanged += OnSettingsChanged;

            
			//Пустой список
            _articlesTableView = new UITableView(new RectangleF(0, 0, 0, 
                0), UITableViewStyle.Plain);
			_articlesTableView.BackgroundColor = ItExpertHelper.GetUIColorFromColor (ApplicationWorker.Settings.GetBackgroundColor ());
			_articlesTableView.ScrollEnabled = true; 
			_articlesTableView.UserInteractionEnabled = true;
			_articlesTableView.SeparatorInset = new UIEdgeInsets (0, 0, 0, 0);
			_articlesTableView.Bounces = false;
            _articlesTableView.SeparatorColor = UIColor.FromRGB(100, 100, 100);
			_articlesTableView.TableFooterView = new UIView();
			if (!UserInterfaceIdiomIsPhone)
			{
				_articlesTableView.AllowsSelection = false;
				_articlesTableView.AllowsMultipleSelection = false;
			}
			View.Add (_articlesTableView);
			_articlesTableView.Hidden = true;
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
			if (_filterParams != null)
			{
				if (_filterParams.Filter == Filter.Section)
				{
					_sectionId = _filterParams.SectionId;
					_blockId = _filterParams.BlockId;
					_filterParams = null;
					_authorId = -1;
					_currentPage = Page.News;
					Filter = Filter.None;
					_bottomBar.TrendsButton.SetActiveState(false);
					_bottomBar.NewsButton.SetActiveState(true);
					if (!ApplicationWorker.Settings.OfflineMode)
					{
						var connectAccept = IsConnectionAccept();
						if (!connectAccept)
						{
							BTProgressHUD.ShowToast ("Нет доступных подключений, для указанных в настройках", ProgressHUD.MaskType.None, false, 2500);					
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
						BTProgressHUD.ShowToast ("Поиск не доступен в оффлайн режиме", ProgressHUD.MaskType.None, false, 2500);	
					}
				}
				if (_filterParams.Filter == Filter.Author)
				{
					_sectionId = _filterParams.SectionId;
					_blockId = _filterParams.BlockId;
					_authorId = _filterParams.AuthorId;
					_filterParams = null;
					_currentPage = Page.News;
					Filter = Filter.None;
					_bottomBar.TrendsButton.SetActiveState(false);
					_bottomBar.NewsButton.SetActiveState(true);
					if (!ApplicationWorker.Settings.OfflineMode)
					{
						var connectAccept = IsConnectionAccept();
						if (!connectAccept)
						{
							BTProgressHUD.ShowToast ("Нет доступных подключений, для указанных в настройках", ProgressHUD.MaskType.None, false, 2500);	
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
						BTProgressHUD.ShowToast ("Поиск не доступен в оффлайн режиме", ProgressHUD.MaskType.None, false, 2500);	
						return;
					}
				}
				_filterParams = null;
			}
			if (!_fromAnother)
			{
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
				if (_currentPage == Page.None && !string.IsNullOrWhiteSpace(_search))
				{
					_currentPage = Page.News;
					Search(_search);
					return;
				}
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

		void InitViews()
		{
			InitNavigationBar();
			View.AutosizesSubviews = true;
			AutomaticallyAdjustsScrollViewInsets = false;
			InitAddPreviousArticleButton ();
			InitBottomToolbar();
			InitLoadingProgress ();
			InitHoldMessageView();
		}

		void InitHoldMessageView()
		{
			_holdMessageView = new UILabel(new RectangleF(0, 0, 0, 0));
			_holdMessageView.BackgroundColor = UIColor.Clear;
			_holdMessageView.Font = UIFont.BoldSystemFontOfSize(16);
			_holdMessageView.TextColor = ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetForeColor());
			_holdMessageView.Hidden = true;
			Add(_holdMessageView);
		}

		//Создание кнопки Загрузить еще
		private void InitAddPreviousArticleButton()
		{
			var button = new UIButton ();
			button.TitleLabel.Text = "Загрузить еще";
			button.TitleLabel.TextAlignment = UITextAlignment.Center;
			button.TitleLabel.BackgroundColor = ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetBackgroundColor());
			button.BackgroundColor = ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetBackgroundColor());
			button.TouchDown += AddPreviousArticleTouchDown;
			button.TouchUpOutside += AddPreviousArticleTouchUpOutside;
			button.TouchUpInside += AddPreviousArticleOnClick;
			_addPreviousArticleButton = button;
		}

		void AddPreviousArticleTouchDown(object sender, EventArgs e)
		{
			(sender as UIButton).SetTitleColor(UIColor.FromRGB(180, 180, 180), UIControlState.Normal);
		}

		void AddPreviousArticleTouchUpOutside(object sender, EventArgs e)
		{
			(sender as UIButton).SetTitleColor(UIColor.FromRGB(140, 140, 140), UIControlState.Normal);
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
			View.BringSubviewToFront(_bottomBar);
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
			_loadingIndicator.StartAnimating();
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
						InvokeOnMainThread(
							() => BTProgressHUD.Show ("Очищается кэш", -1, ProgressHUD.MaskType.Clear));
						ApplicationWorker.Db.ClearCache();
						InvokeOnMainThread(
							() =>BTProgressHUD.Dismiss());
					}
				};
				ThreadPool.QueueUserWorkItem(state => action());
			}
		}

        private void InitNavigationBar()
        {
			_menu = new MenuView(ButNewsOnClick, ButTrendsOnClick, ButMagazineOnClick, ButArchiveOnClick, ButFavoriteOnClick, AboutUsShow, Search);
			_menu.TapOutsideTableView += ViewTapOutsideTableView;
			_menuButton = NavigationBarButton.GetButton("NavigationBar/Menu.png", 2);
			_menuButton.TouchUpInside += MenuButtonTouchUp;
			_menuBarButton = new UIBarButtonItem(_menuButton);

			NavigationItem.LeftBarButtonItems = new UIBarButtonItem[] { _menuBarButton, NavigationBarButton.Logo };

            UIBarButtonItem space = new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace);

            space.Width = -10;

			_settingsButton = NavigationBarButton.GetButton("NavigationBar/Settings.png", 4.1f);
			_settingsBarButton = new UIBarButtonItem(_settingsButton);
			_settingsView = new SettingsView(false);
			_settingsView.TapOutsideTableView += ViewTapOutsideTableView;
			_settingsButton.TouchUpInside += SettingsButtonTouchUp;

			_refreshButton = NavigationBarButton.GetButton("NavigationBar/Refresh.png", 4.1f);
			_refreshButton.TouchUpInside += ButRefreshOnClick;
			_refreshBarButton = new UIBarButtonItem(_refreshButton);

			_dumpCacheButton = NavigationBarButton.GetButton("NavigationBar/DumpInCache.png", 4);
			_dumpCacheButton.TouchUpInside += ButInCacheOnClick;
			_dumpCacheBarButton = new UIBarButtonItem(_dumpCacheButton);

			NavigationItem.RightBarButtonItems = new UIBarButtonItem[] { space, _settingsBarButton, _refreshBarButton,
				_dumpCacheBarButton };
        }

		void ViewTapOutsideTableView(object sender, EventArgs e)
		{
			NavigationBarButton.HideWindow();
		}

		void MenuButtonTouchUp(object sender, EventArgs e)
		{
			NavigationBarButton.ShowWindow(_menu);
		}

		void SettingsButtonTouchUp(object sender, EventArgs e)
		{
			NavigationBarButton.ShowWindow(_settingsView);
		}

		#endregion

		#region Event Handlers

		void OnSettingsChanged (object sender, EventArgs e)
		{
			_articlesTableView.BackgroundColor = ItExpertHelper.GetUIColorFromColor (ApplicationWorker.Settings.GetBackgroundColor ());
			View.BackgroundColor = ItExpertHelper.GetUIColorFromColor (ApplicationWorker.Settings.GetBackgroundColor ());
			_loadingIndicator.BackgroundColor = ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetBackgroundColor());
			var button = _addPreviousArticleButton as UIButton;
			if (button != null)
			{
				button.TitleLabel.BackgroundColor = ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetBackgroundColor());
				button.BackgroundColor = ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetBackgroundColor());
			}
			if (_articlesTableView != null)
			{
				if (_allArticles != null && _allArticles.Any())
				{
					_articles.Clear();
					if (ApplicationWorker.Settings.HideReaded)
					{
						var buffer = _allArticles.Where(x => !x.IsReaded).ToList();
						if (buffer.Count() < 6)
						{
							var count = 6 - buffer.Count();
							buffer.AddRange(_allArticles.Where(x => x.IsReaded).Take(count));
							buffer = buffer.OrderByDescending(x => x.ActiveFrom).ToList();
						}
						_articles.AddRange(buffer);
					}
					else
					{
						_articles.AddRange(_allArticles.ToList());
					}
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
						_articles.Add(new Article() {
							ArticleType = ArticleType.PreviousArticlesButton,
							ExtendedObject = _addPreviousArticleButton
						});
					}

					if (_articlesTableView != null && _articlesTableView.Source != null)
					{
						if (_articlesTableView.Source is DoubleArticleTableSource)
						{
							(_articlesTableView.Source as DoubleArticleTableSource).UpdateSource();
						}
						var iUpdatatableSource = _articlesTableView.Source as IUpdatableSource;
						if (iUpdatatableSource != null)
						{
							iUpdatatableSource.UpdateProperties();
						}
					}
					_articlesTableView.ReloadData();
				}
			}
		}

		void AboutUsShow (object sender, EventArgs e)
		{
			ApplicationWorker.ClearNews();
			AboutUsViewController showController = null;
			var controllers = NavigationController.ViewControllers;
			foreach (var controller in controllers)
			{
				showController = controller as AboutUsViewController;
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
				showController = new AboutUsViewController ();
				NavigationController.PushViewController (showController, true);
			}
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
					BTProgressHUD.ShowToast ("Ошибка при загрузке", ProgressHUD.MaskType.None, false, 2500);	
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
					_articlesTableView.ReloadData();
					BTProgressHUD.ShowToast ("Ошибка при загрузке", ProgressHUD.MaskType.None, false, 2500);	
				}
			});
		}

		//Загрузка предыдущих статей
		private void AddPreviousArticleOnClick(object sender, EventArgs eventArgs)
		{
			(sender as UIButton).SetTitleColor(UIColor.FromRGB(140, 140, 140), UIControlState.Normal);

			if (_isLoadingData)return;
			Action addLoadingProgress = () =>
			{
				var button = sender as UIButton;
				if (button != null)
				{
					var loading = new UIActivityIndicatorView ( new RectangleF(0, 0, View.Bounds.Width, 40));
					loading.BackgroundColor = ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetBackgroundColor());
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
					BTProgressHUD.ShowToast ("Нет доступных подключений, для указанных в настройках", ProgressHUD.MaskType.None, false, 2500);	
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

		private void ButInCacheOnClick(object s, EventArgs ev)
		{
			if (_allArticles == null || !_allArticles.Any())
				return;
			Action inCache = () =>
			{
				var completeArticlesCount = _allArticles.Count(x => !string.IsNullOrEmpty(x.DetailText)); 
				var serverInteractionNeeded = ((double)completeArticlesCount / (double)_allArticles.Count()) < 0.5;
				if (!serverInteractionNeeded)
				{
					InvokeOnMainThread(() => BTProgressHUD.Show("Cброc в кэш..", -1, ProgressHUD.MaskType.Clear));
					ApplicationWorker.Db.SaveInCache(_allArticles.ToList());
					InvokeOnMainThread(() => BTProgressHUD.Dismiss());
				}
				else
				{
					InvokeOnMainThread(() => BTProgressHUD.Show("Получение полных статей...", -1, ProgressHUD.MaskType.Clear));
					var allArticlesCount = _allArticles.Count();
					var settingsClone = ApplicationWorker.Settings.Clone();
					settingsClone.LoadDetails = true;
					var serverInteractionCount = (int)Math.Ceiling((double)allArticlesCount/20);
					var serverOperationsCount = 0;
					var articles = new List<Article>();
					Action completeAction = ()=>{
						ApplicationWorker.RemoteWorker.ClearNewsEventHandler();
						ApplicationWorker.Db.SaveInCache(articles.ToList());
						InvokeOnMainThread(()=>{
							AddNewArticles(articles);
							BTProgressHUD.ShowToast("Полные статьи получены и сброшены в кеш", ProgressHUD.MaskType.None, false, 2500);	
							Action hideToastAction = ()=>{
								Thread.Sleep(1500);
								InvokeOnMainThread(()=>BTProgressHUD.Dismiss());
							};
							ThreadPool.QueueUserWorkItem(state => hideToastAction());
						});
					};
					ApplicationWorker.RemoteWorker.NewsGetted += (sender, e) =>
					{
						if (e.Abort || e.Error)
						{
							ApplicationWorker.RemoteWorker.ClearNewsEventHandler();
							InvokeOnMainThread(() =>
							{
								BTProgressHUD.ShowToast("Сбой при получении данных...Отмена сброса в кеш", ProgressHUD.MaskType.None, false, 2500);	;
								Action hideToastAction = ()=>{
									Thread.Sleep(1500);
									InvokeOnMainThread(()=>BTProgressHUD.Dismiss());
								};
								ThreadPool.QueueUserWorkItem(state => hideToastAction());
							});
						}
						else
						{
							serverOperationsCount++;
							if (e.Articles != null && e.Articles.Any())
							{
								ApplicationWorker.Db.SetPropertiesForArticles(e.Articles);
								ApplicationWorker.NormalizePreviewText(e.Articles);
								UpdateData(e);
								articles.AddRange(e.Articles);
								var transmissedArticlesCount = articles.Count();
								InvokeOnMainThread(()=>BTProgressHUD.Show("Получено статей: " + transmissedArticlesCount + " из " + allArticlesCount, -1, ProgressHUD.MaskType.Clear));
								if (serverOperationsCount != serverInteractionCount)
								{
									var datetimeMarker = e.Articles.Min(x=>x.Timespan);
									ApplicationWorker.RemoteWorker.BeginGetNews(settingsClone, -1, datetimeMarker, _blockId, _sectionId, _authorId, _search);
								}
								else
								{
									completeAction();
								}
							}
							else
							{
								completeAction();
							}
						}
					};
					ApplicationWorker.RemoteWorker.BeginGetNews(settingsClone, -1, -1, _blockId, _sectionId, _authorId, _search);
				}
			};
			ThreadPool.QueueUserWorkItem(state => inCache());
		}

		private void ButRefreshOnClick(object sender, EventArgs e)
		{
			if (!_isLoadingData)
			{
				if (!ApplicationWorker.Settings.OfflineMode)
				{
					var connectAccept = IsConnectionAccept();
					if (!connectAccept)
					{
						BTProgressHUD.ShowToast ("Нет доступных подключений, для указанных в настройках", ProgressHUD.MaskType.None, false, 2500);	
						return;
					}
				}
				_prevArticlesExists = true;
				_allArticles.Clear();
				//Пустой список
				if (_articles != null)
				{
					_articles.Clear ();
				}
				if (_articlesTableView != null && _articlesTableView.Source != null)
				{
					if (_articlesTableView.Source is DoubleArticleTableSource)
					{
						(_articlesTableView.Source as DoubleArticleTableSource).UpdateSource ();
					}
				}
				_articlesTableView.ReloadData ();
				_articlesTableView.Hidden = true;
				_holdMessageView.Hidden = true;
				if (!ApplicationWorker.Settings.OfflineMode)
				{
					_isLoadingData = true;
					ApplicationWorker.RemoteWorker.NewsGetted += NewNewsGetted;
					ThreadPool.QueueUserWorkItem(
						state =>
						ApplicationWorker.RemoteWorker.BeginGetNews(ApplicationWorker.Settings, -1, -1, _blockId,
							_sectionId, _authorId, _search));
					SetLoadingImageVisible(true);
				}
				else
				{
					SetLoadingImageVisible(true);
					Action getList = () =>
					{
						var isTrends = _blockId == 30;
						var lst = ApplicationWorker.Db.GetArticlesFromDb(0, 20, isTrends);
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

		private void ButNewsOnClick(object sender, EventArgs e)
		{
			ApplicationWorker.ClearNews();
			Filter = Filter.None;
			_headerAdded = false;
			_header = null;
			PageNewsActivate();
		}

		private void ButTrendsOnClick(object sender, EventArgs e)
		{
			ApplicationWorker.ClearNews();
			Filter = Filter.None;
			_headerAdded = false;
			_header = null;
			PageTrendsActivate();
		}

		private void ButArchiveOnClick(object sender, EventArgs e)
		{
			ApplicationWorker.ClearNews();
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
				NavigationController.PopToViewController (showController, false);
			}
			else
			{
				showController = new ArchiveViewController ();
				NavigationController.PushViewController (showController, false);
			}
		}

		private void ButMagazineOnClick(object sender, EventArgs e)
		{
			ApplicationWorker.ClearNews();
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
				NavigationController.PopToViewController (showController, false);
				showController.SetMagazineId (-1);
			}
			else
			{
				showController = new MagazineViewController(-1);
				NavigationController.PushViewController(showController, false);
			}
		}

		private void ButFavoriteOnClick(object sender, EventArgs e)
		{
			ApplicationWorker.ClearNews();
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
				NavigationController.PopToViewController (showController, false);
				showController.UpdateSource();
			}
			else
			{
				showController = new FavoritesViewController ();
				NavigationController.PushViewController (showController, false);
			}
		}

		private void OnPushArticleDetails(object sender, PushDetailsEventArgs e)
		{
			var controller = e.NewsDetailsView;
			controller.SetFromFavorite(false);
			var filterParams = new FilterParameters() {
				Search = _search,
				BlockId = _blockId,
				SectionId = _sectionId,
				AuthorId = _authorId
			};
			controller.SetFilterParameters(filterParams);
			NavigationController.PushViewController (controller, true);
		}

        private void UpdateViewsLayout()
        {
            if (_articlesTableView != null)
            {
				var tableViewTopOffset = NavigationController.NavigationBar.Frame.Height + ItExpertHelper.StatusBarHeight;
                _articlesTableView.Frame = new RectangleF(0, tableViewTopOffset, View.Bounds.Width, 
                    View.Bounds.Height - tableViewTopOffset - _bottomBar.Frame.Height);
				if (_articlesTableView.Source != null)
				{
					var iUpdatatableSource = _articlesTableView.Source as IUpdatableSource;
					if (iUpdatatableSource != null)
					{
						iUpdatatableSource.UpdateProperties();
					}
				}
                _articlesTableView.ReloadData();
            }

            if (_bottomBar != null)
            {
                _bottomBar.RemoveFromSuperview();
                _bottomBar.Dispose();
                _bottomBar = null;

            }

            InitBottomToolbar();

			if (_currentPage == Page.News)
			{
				_bottomBar.TrendsButton.SetActiveState(false);
				_bottomBar.NewsButton.SetActiveState(true);
			}
			else if (_currentPage == Page.Trends)
			{
				_bottomBar.TrendsButton.SetActiveState(true);
				_bottomBar.NewsButton.SetActiveState(false);
			}

			if (_loadingIndicator != null)
			{
				var height = 50;
				var bottomBarHeight = _bottomBar.Frame.Height;

				_loadingIndicator.Frame = new RectangleF(0, View.Bounds.Height - (height + bottomBarHeight), View.Bounds.Width, height);
			}

			if (_holdMessageView != null && !_holdMessageView.Hidden)
			{
				_holdMessageView.SizeToFit();
				_holdMessageView.Frame = new RectangleF((View.Bounds.Width - _holdMessageView.Frame.Width) / 2, View.Bounds.Height / 2, _holdMessageView.Frame.Width, _holdMessageView.Frame.Height);
				View.BringSubviewToFront(_holdMessageView);
			}
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
				alertView.Dispose();
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
						BTProgressHUD.ShowToast ("Ошибка при загрузке", ProgressHUD.MaskType.None, false, 2500);	
					}
					else
					{
						NewNewsGetted(null, e);
					}
					SetLoadingImageVisible(false);
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
						BTProgressHUD.ShowToast ("Ошибка при загрузке", ProgressHUD.MaskType.None, false, 2500);	
					}
					else
					{
						NewNewsGetted(null, e);
					}
					SetLoadingImageVisible(false);
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
				_currentPage = Page.None;
				ButMagazineOnClick(null, null);
			}
			if (page == Page.Archive)
			{
				_currentPage = Page.None;
				ButArchiveOnClick(null, null);
			}
			if (page == Page.Favorite)
			{
				_currentPage = Page.None;
				ButFavoriteOnClick(null, null);
			}
		}

		public void ShowFromAnotherScreen(Page pageToShow)
		{
			_headerAdded = false;
			_header = null;
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
					BTProgressHUD.ShowToast ("Нет доступных подключений, для указанных в настройках", ProgressHUD.MaskType.None, false, 2500);	
					return;
				}
			}
			if (sectionChange || blockChange || _search != null || _currentPage == Page.None)
			{
				Filter = Filter.Page;
			}
			_currentPage = pageToShow;
		}

		public void Search(string search)
		{
			if (!_isLoadingData && !string.IsNullOrWhiteSpace(search))
			{
				_bottomBar.NewsButton.SetActiveState (true);
				_bottomBar.TrendsButton.SetActiveState (false);
				_currentPage = Page.News;
				if (!ApplicationWorker.Settings.OfflineMode)
				{
					var connectAccept = IsConnectionAccept();
					if (!connectAccept)
					{
						BTProgressHUD.ShowToast ("Нет доступных подключений, для указанных в настройках", ProgressHUD.MaskType.None, false, 2500);	
						return;
					}
					_prevArticlesExists = true;
					_isLoadingData = true;
					_search = search;
					_blockId = -1;
					_sectionId = -1;
					Filter = Filter.Search;
					//Пустой список
					if (_articles != null)
					{
						_articles.Clear();
					}
					if (_articlesTableView != null && _articlesTableView.Source != null)
					{
						if (_articlesTableView.Source is DoubleArticleTableSource)
						{
							(_articlesTableView.Source as DoubleArticleTableSource).UpdateSource();
						}
					}
					_articlesTableView.ReloadData();
					_articlesTableView.Hidden = true;
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
					BTProgressHUD.ShowToast ("Поиск не доступен в оффлайн режиме", ProgressHUD.MaskType.None, false, 2500);	
					return;
				}
			}
		}

		public void SearchFromAnother(string search)
		{
			if (!_isLoadingData && !string.IsNullOrWhiteSpace(search))
			{
				_bottomBar.NewsButton.SetActiveState (true);
				_bottomBar.TrendsButton.SetActiveState (false);
				_currentPage = Page.News;
				if (!ApplicationWorker.Settings.OfflineMode)
				{
					var connectAccept = IsConnectionAccept();
					if (!connectAccept)
					{
						BTProgressHUD.ShowToast ("Нет доступных подключений, для указанных в настройках", ProgressHUD.MaskType.None, false, 2500);	
						return;
					}
					_prevArticlesExists = true;
					_isLoadingData = true;
					_search = search;
					_blockId = -1;
					_sectionId = -1;
					Filter = Filter.Search;
					return;
				}
				else
				{
					BTProgressHUD.ShowToast ("Поиск не доступен в оффлайн режиме", ProgressHUD.MaskType.None, false, 2500);	
					return;
				}
			}
		}

		public void FilterAuthor(int sectionId, int blockId, int authorId)
		{	
			_sectionId = sectionId;
			_blockId = blockId;
			_authorId = authorId;
			_currentPage = Page.News;
			Filter = Filter.Author;
		}

		public void FilterSection(int sectionId, int blockId)
		{
			_sectionId = sectionId;
			_blockId = blockId;
			_authorId = -1;
			_currentPage = Page.News;
			Filter = Filter.Section;
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
						BTProgressHUD.ShowToast ("Нет доступных подключений, для указанных в настройках", ProgressHUD.MaskType.None, false, 2500);	
						return;
					}
				}
				_blockId = -1;
				_sectionId = -1;
				_authorId = -1;
				_search = null;
				//Пустой список
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
				_articlesTableView.Hidden = true;
				_holdMessageView.Hidden = true;
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
							//Пустой список
							SetLoadingImageVisible(true);
                            UpdateTableView(new List<Article>());
							_articlesTableView.Hidden = true;
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
						BTProgressHUD.ShowToast ("Нет доступных подключений, для указанных в настройках", ProgressHUD.MaskType.None, false, 2500);	
						return;
					}
				}
				_blockId = 30;
				_sectionId = -1;
				_authorId = -1;
				_search = null;
				//Пустой список
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
				_articlesTableView.Hidden = true;
				_holdMessageView.Hidden = true;
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
							//Пустой список
							SetLoadingImageVisible(true);
                            UpdateTableView(new List<Article>());
							_articlesTableView.Hidden = true;
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
				ShowHoldMessage("Статей нет");
				_prevArticlesExists = false;
				return;
			}
			else
			{
				_holdMessageView.Hidden = true;
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
			if (_articles != null && _articles.Any())
			{
				_articlesTableView.Hidden = false;
			}
		}

		//Добавление последующих статей
		private void AddPreviousArticles(List<Article> lst)
		{
			if (lst == null || !lst.Any())
			{
				BTProgressHUD.ShowToast ("Больше статей нет", ProgressHUD.MaskType.None, false, 2500);	
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
				if (_articlesTableView.Source != null)
				{
					var iUpdatatableSource = _articlesTableView.Source as IUpdatableSource;
					if (iUpdatatableSource != null)
					{
						iUpdatatableSource.UpdateProperties();
					}
				}
			}
			_articlesTableView.ReloadData();
			if (ApplicationWorker.Settings.HideReaded && position > 0)
			{
				if (!UserInterfaceIdiomIsPhone)
				{
					if (_banner != null && _addPreviousArticleButton != null)
					{
						position = (int)Math.Floor((double)(position - 2) / 2);
					}
					else
					{
						position = (int)Math.Floor((double)position / 2);
					}
				}
				var indexPath = NSIndexPath.FromItemSection(position, 0);
				_articlesTableView.ScrollToRow(indexPath, UITableViewScrollPosition.Middle, false);
			}
			if (_articles != null && _articles.Any())
			{
				_articlesTableView.Hidden = false;
			}
		}

		void ShowHoldMessage(string message)
		{
			_holdMessageView.Hidden = false;
			_holdMessageView.Text = message;
			_holdMessageView.SizeToFit();
			_holdMessageView.Frame = new RectangleF((View.Bounds.Width - _holdMessageView.Frame.Width) / 2, View.Bounds.Height / 2, _holdMessageView.Frame.Width, _holdMessageView.Frame.Height);
			View.BringSubviewToFront(_holdMessageView);
		}

        private void UpdateTableView(List<Article> articles)
        {
            if (_articlesTableView.Source != null) 
            {
				if (_articlesTableView.Source is ArticlesTableSource)
				{
					((ArticlesTableSource)_articlesTableView.Source).PushDetailsView -= OnPushArticleDetails;
				}
				else if (_articlesTableView.Source is DoubleArticleTableSource)
				{
					((DoubleArticleTableSource)_articlesTableView.Source).PushDetailsView -= OnPushArticleDetails;
				}
                _articlesTableView.Source.Dispose();
                _articlesTableView.Source = null;
            }

            UITableViewSource source = null;

            if (UserInterfaceIdiomIsPhone)
            {
                source = new ArticlesTableSource(articles, MagazineAction.NoAction);

				((ArticlesTableSource)source).PushDetailsView += OnPushArticleDetails;
            }
            else
            {
                source = new DoubleArticleTableSource(articles, MagazineAction.NoAction);

				((DoubleArticleTableSource)source).PushDetailsView += OnPushArticleDetails;
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
			if (_loadingIndicator == null)
				return;
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

