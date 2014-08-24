using System;
using MonoTouch.UIKit;
using System.Drawing;
using ItExpert.Enum;
using ItExpert.Model;
using System.Collections.Generic;
using ItExpert.ServiceLayer;
using System.Linq;
using System.Threading;
using System.IO;
using MonoTouch.Foundation;
using mTouchPDFReader.Library.Views.Core;
using BigTed;

namespace ItExpert
{
    public class MagazineViewController: UIViewController
    {
		#region Fields

		private Magazine _magazine;
		private List<Article> _articles;
		private List<Article> _allArticles = new List<Article>();
		private bool _isLoadingData;
		private bool _isRubricSearch = false;
		private Rubric _searchRubric = null;
		public static MagazineViewController Current;
		private UIView _banner = null;
		private bool _lastMagazine = false;
		private UIButton _addPreviousArticleButton = null;
		private bool _prevArticlesExists = true;
		private bool _headerAdded = false;
		private string _header = null;
		private BottomToolbarView _bottomBar = null;
		private int _magazineId = -1;
        private UITableView _articlesTableView;
		private UIActivityIndicatorView _loadingIndicator;
		public bool IsLoadingPdf = false;
		private bool _firstLoad = true;
		private UIInterfaceOrientation _currentOrientation;
		private int _rubricId = -1;
		private bool _showFromAnotherScreen = false;
		private UILabel _holdMessageView;
		private FilterParameters _filterParams;
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

		static bool UserInterfaceIdiomIsPhone {
			get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
		}

		public MagazineViewController(int magazineId)
        {
			_magazineId = magazineId;
        }

		public MagazineViewController(FilterParameters filterParams)
		{
			_filterParams = filterParams;
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			// Perform any additional setup after loading the view, typically from a nib.
		}

        public override void ViewWillAppear(bool animated)
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
				InitMagazine (_magazine);
				UpdateViewsLayout ();
			}
			if (_showFromAnotherScreen)
			{
				_showFromAnotherScreen = false;
				_rubricId = -1;
				_isRubricSearch = false;
				_searchRubric = null;
				_headerAdded = false;
				_header = null;
				_prevArticlesExists = true;
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
				InitData ();
				return;
			}
			if (_rubricId != -1)
			{
				if (ApplicationWorker.Settings.OfflineMode)
				{
					BTProgressHUD.ShowToast ("Поиск невозможен в оффлайн режиме", ProgressHUD.MaskType.None, false, 2500);
					_rubricId = -1;
					return;
				}
				var rubric =
					_articles.Where(
						x =>
						x.ArticleType == ArticleType.Magazine &&
						x.Rubrics != null)
						.SelectMany(x => x.Rubrics)
						.FirstOrDefault(x => x.Id == _rubricId);
				_rubricId = -1;
				if (rubric != null)
				{
					var connectAccept = IsConnectionAccept();
					if (!connectAccept)
					{
						BTProgressHUD.ShowToast ("Нет доступных подключений, для указанных в настройках", ProgressHUD.MaskType.None, false, 2500);
						return;
					}
					_headerAdded = true;
					_header = rubric.Name;
					//Пустой список
					_articles.Clear();
					_allArticles.Clear();
					if (_articlesTableView != null && _articlesTableView.Source != null)
					{
						if (_articlesTableView.Source is DoubleArticleTableSource)
						{
							(_articlesTableView.Source as DoubleArticleTableSource).UpdateSource ();
						}
					}
					if (_articlesTableView != null)
					{
						_articlesTableView.ReloadData ();
					}
					_articlesTableView.Hidden = true;
					_holdMessageView.Hidden = true;
					_prevArticlesExists = true;
					_articles = null;
					_searchRubric = rubric;
					_isLoadingData = true;
					_isRubricSearch = true;
					ApplicationWorker.RemoteWorker.MagazineArticlesGetted += SearchRubricOnMagazineArticlesGetted;
					ThreadPool.QueueUserWorkItem(
						state =>
						ApplicationWorker.RemoteWorker.BeginGetMagazinesArticlesByRubric(
							ApplicationWorker.Settings, rubric, Magazine.BlockId, -1));
					SetLoadingImageVisible (true);
					return;
				}
			}
			if (!_firstLoad && !_isLoadingData)
			{
				if (_allArticles == null || !_allArticles.Any ())
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
						selectItemId = (_articlesTableView.Source as ArticlesTableSource).GetSelectItemId ();
						(_articlesTableView.Source as ArticlesTableSource).ResetSelectItem ();
					}
					if (_articlesTableView.Source is DoubleArticleTableSource)
					{
						selectItemId = (_articlesTableView.Source as DoubleArticleTableSource).GetSelectItemId ();
						(_articlesTableView.Source as DoubleArticleTableSource).ResetSelectItem ();
					}
				}
				if (selectItemId != -1)
				{
					for (var i = 0; i < _articles.Count (); i++)
					{
						if (_articles [i].Id == selectItemId)
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
							while (!isFound)
							{
								position--;
								if (position == 0)
								{
									break;
								}
								if ((_articles [position].ArticleType != ArticleType.Magazine &&
									_articles [position].ArticleType != ArticleType.Portal) &&
									!_articles [position].IsReaded)
								{
									isFound = true;
									selectArticle = _articles [position];
								}
							}
						}
					}
				}
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
					if (!_isRubricSearch)
					{
						buffer = SortAndAddHeader(buffer);
					}
					_articles.AddRange (buffer);
					if (selectArticle != null && position > 0)
					{
						position = 0;
						for (var i = 0; i < _articles.Count (); i++)
						{
							if (_articles [i].Id == selectArticle.Id)
							{
								position = i;
								break;
							}
						}
					}
				}
				else
				{
					var lst = _allArticles.ToList();
					if (!_isRubricSearch)
					{
						lst = SortAndAddHeader(lst);
					}
					_articles.AddRange(lst);
				}
				if (_isRubricSearch && _headerAdded && !string.IsNullOrWhiteSpace (_header))
				{
					_articles.Insert (0,
						new Article () { ArticleType = ArticleType.Header, Name = _header });
				}
				if (_banner != null)
				{
					_articles.Insert(0,
						new Article() { ArticleType = ArticleType.Banner, ExtendedObject = _banner });
				}
				if (!_isRubricSearch && ApplicationWorker.Magazine != null)
				{
					_articles.Insert(0, new Article() { ArticleType = ArticleType.MagazinePreview });
				}

				if (_isRubricSearch && _addPreviousArticleButton != null && _prevArticlesExists)
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
				UpdateViewsLayout ();
				if (ApplicationWorker.Settings.HideReaded && position > 0)
				{
					if (!UserInterfaceIdiomIsPhone)
					{
						if (_banner != null && _addPreviousArticleButton != null)
						{
							position = (int)Math.Floor ((double)(position - 2) / 2);
						}
						else
						{
							position = (int)Math.Floor ((double)position / 2);
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

			ApplicationWorker.RemoteWorker.BannerGetted -= StartOnBannerGetted;
			ApplicationWorker.RemoteWorker.MagazinesPriviewGetted -= OnMagazinesPriviewGetted;
			ApplicationWorker.RemoteWorker.MagazineArticlesGetted -= OnMagazineArticlesGetted;
			ApplicationWorker.RemoteWorker.MagazineArticlesGetted -= SearchRubricOnMagazineArticlesGetted;
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
				DestroyPdfLoader();
				ApplicationWorker.RemoteWorker.BannerGetted -= StartOnBannerGetted;
				ApplicationWorker.RemoteWorker.MagazinesPriviewGetted -= OnMagazinesPriviewGetted;
				ApplicationWorker.RemoteWorker.MagazineArticlesGetted -= OnMagazineArticlesGetted;
				ApplicationWorker.RemoteWorker.MagazineArticlesGetted -= SearchRubricOnMagazineArticlesGetted;
				ApplicationWorker.SettingsChanged -= OnSettingsChanged;
				ApplicationWorker.AllPdfFilesDeleted -= OnAllPdfFilesDeleted;
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

				Current = null;
			});
		}

		public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
			base.WillRotate(toInterfaceOrientation, duration);

			_articlesTableView.Hidden = true;
			_bottomBar.Hidden = true;
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
			if (_allArticles != null && _allArticles.Any ())
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
					if (!_isRubricSearch)
					{
						buffer = SortAndAddHeader (buffer);
					}
					_articles.AddRange (buffer);
				}
				else
				{
					var lst = _allArticles.ToList ();
					if (!_isRubricSearch)
					{
						lst = SortAndAddHeader (lst);
					}
					_articles.AddRange (lst);
				}

				if (_isRubricSearch && _headerAdded && !string.IsNullOrWhiteSpace (_header))
				{
					_articles.Insert (0,
						new Article () { ArticleType = ArticleType.Header, Name = _header });
				}
				if (_banner != null)
				{
					_articles.Insert(0,
						new Article() { ArticleType = ArticleType.Banner, ExtendedObject = _banner });
				}
				if (!_isRubricSearch && ApplicationWorker.Magazine != null)
				{
					_articles.Insert(0, new Article() { ArticleType = ArticleType.MagazinePreview });
				}

				if (_isRubricSearch && _addPreviousArticleButton != null && _prevArticlesExists)
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

			_articlesTableView.Hidden = false;
			_bottomBar.Hidden = false;
        }

		#endregion

		#region Init

		public void SetMagazineId(int magazineId)
		{
			_magazineId = magazineId;
			_showFromAnotherScreen = true;
		}

		void InitViews()
		{
			Current = this;
			View.AutosizesSubviews = true;
			AutomaticallyAdjustsScrollViewInsets = false;
			View.BackgroundColor = ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetBackgroundColor());
			InitBottomToolbar();
			InitLoadingProgress();
			InitAddPreviousArticleButton();
			InitNavigationBar();
			InitHoldMessageView();
		}

		public void Initialize()
		{
			//Пустой список
			_articlesTableView = new UITableView(new RectangleF(0, 0, 0, 
				0), UITableViewStyle.Plain);
			_articlesTableView.BackgroundColor = ItExpertHelper.GetUIColorFromColor (ApplicationWorker.Settings.GetBackgroundColor ());
			_articlesTableView.ScrollEnabled = true; 
			_articlesTableView.UserInteractionEnabled = true;
			_articlesTableView.SeparatorInset = new UIEdgeInsets(0, 0, 0, 0);
			_articlesTableView.Bounces = false;
			_articlesTableView.SeparatorColor = UIColor.FromRGB(100, 100, 100);
			_articlesTableView.TableFooterView = new UIView();
			if (!UserInterfaceIdiomIsPhone)
			{
				_articlesTableView.AllowsSelection = false;
				_articlesTableView.AllowsMultipleSelection = false;
			}

			View.Add(_articlesTableView);
			_articlesTableView.Hidden = true;

			ApplicationWorker.SettingsChanged += OnSettingsChanged;
			ApplicationWorker.AllPdfFilesDeleted += OnAllPdfFilesDeleted;

			var screenWidth =
				ApplicationWorker.Settings.GetScreenWidthForScreen((int)View.Bounds.Width);
			var loadBanners = true;
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
							loadBanners = false;
						}
					}
				}
			}
				
			if (loadBanners)
			{
				if (!ApplicationWorker.Settings.OfflineMode)
				{
					var connectAccept = IsConnectionAccept();
					if (!connectAccept)
					{
						BTProgressHUD.ShowToast ("Нет доступных подключений, для указанных в настройках", ProgressHUD.MaskType.None, false, 2500);
						return;
					}
					ApplicationWorker.RemoteWorker.BannerGetted += StartOnBannerGetted;
					var settings = new Settings() {
						ScreenWidth = screenWidth,
						ScreenResolution = ApplicationWorker.Settings.ScreenResolution
					};
					ThreadPool.QueueUserWorkItem(
						state => ApplicationWorker.RemoteWorker.BeginGetBanner(settings));
					_isLoadingData = true;
				}
				else
				{
					InitData();
				}
			}
			else
			{
				InitBanner(banner);
				InitData();
			}
		}

		private void InitBottomToolbar()
		{
			float height = 66;

			_bottomBar = new BottomToolbarView ();
			_bottomBar.Frame = new RectangleF(0, View.Frame.Height - height, View.Frame.Width, height);
			_bottomBar.LayoutIfNeeded();
			_bottomBar.MagazineButton.SetActiveState (true);	
			_bottomBar.NewsButton.ButtonClick += ButNewsOnClick;
			_bottomBar.TrendsButton.ButtonClick += ButTrendsOnClick;
			_bottomBar.MagazineButton.ButtonClick += ButMagazineOnClick;
			_bottomBar.ArchiveButton.ButtonClick += ButArchiveOnClick;
			_bottomBar.FavoritesButton.ButtonClick += ButFavoriteOnClick;
			View.Add(_bottomBar);
			View.BringSubviewToFront(_bottomBar);
		}

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

		//Создание кнопки Загрузить еще
		private void InitAddPreviousArticleButton()
		{
			var button = new UIButton ();
			button.TitleLabel.Text = "Загрузить еще";
			button.TitleLabel.TextAlignment = UITextAlignment.Center;
			button.TitleLabel.BackgroundColor = ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetBackgroundColor());
			button.BackgroundColor = ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetBackgroundColor());
			button.TouchUpInside += AddPreviousArticleOnClick;
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

		private void InitData()
		{
			SetLoadingImageVisible (true);
			if (_filterParams != null)
			{
				if (_filterParams.Filter == Filter.Section)
				{
					_rubricId = _filterParams.SectionId;
					_filterParams = null;
					if (ApplicationWorker.Settings.OfflineMode)
					{
						BTProgressHUD.ShowToast ("Поиск невозможен в оффлайн режиме", ProgressHUD.MaskType.None, false, 2500);
						_rubricId = -1;
						return;
					}
					var rubric = ApplicationWorker.Db.GetRubric(_rubricId);
					_rubricId = -1;
					if (rubric != null)
					{
						var connectAccept = IsConnectionAccept();
						if (!connectAccept)
						{
							BTProgressHUD.ShowToast ("Нет доступных подключений, для указанных в настройках", ProgressHUD.MaskType.None, false, 2500);
							return;
						}
						_headerAdded = true;
						_header = rubric.Name;
						_prevArticlesExists = true;
						_articles = null;
						_searchRubric = rubric;
						_isLoadingData = true;
						_isRubricSearch = true;
						ApplicationWorker.RemoteWorker.MagazineArticlesGetted += SearchRubricOnMagazineArticlesGetted;
						ThreadPool.QueueUserWorkItem(
							state =>
							ApplicationWorker.RemoteWorker.BeginGetMagazinesArticlesByRubric(
								ApplicationWorker.Settings, rubric, Magazine.BlockId, -1));
						return;
					}
				}
				_filterParams = null;
			}
			var lastMagazineId = -1;
			if (ApplicationWorker.LastMagazine != null)
			{
				lastMagazineId = ApplicationWorker.LastMagazine.Id;
			}
			var itemId = _magazineId;
			var loadData = true;
			if (itemId != -1 && (lastMagazineId == -1 || itemId != lastMagazineId))
			{
				_lastMagazine = false;
				var magazine = ApplicationWorker.Magazine;
				if (magazine.Id != itemId)
				{
					magazine = ApplicationWorker.Db.GetMagazine(itemId, true);
				}
				if (magazine != null)
				{
					_magazine = magazine;
					InitMagazine(magazine);
					loadData = false;
					LoadMagazineArticles();
				}
			}
			if (loadData)
			{
				_lastMagazine = true;
				if (!ApplicationWorker.Settings.OfflineMode)
				{
					var magazine = ApplicationWorker.LastMagazine;
					if (magazine != null)
					{
						_magazine = magazine;
						InitMagazine(magazine);
						LoadMagazineArticles();
						return;
					}
					var connectAccept = IsConnectionAccept();
					if (!connectAccept)
					{
						BTProgressHUD.ShowToast ("Нет доступных подключений, для указанных в настройках", ProgressHUD.MaskType.None, false, 2500);
						return;
					}
					ApplicationWorker.RemoteWorker.MagazinesPriviewGetted += OnMagazinesPriviewGetted;
					_isLoadingData = true;
					ThreadPool.QueueUserWorkItem(
						state => ApplicationWorker.RemoteWorker.BeginGetMagazinesPreview(ApplicationWorker.Settings, -1));
				}
				else
				{
					Magazine magazine = ApplicationWorker.Db.GetNewestSavedMagazine();
					if (magazine != null)
					{
						_magazine = magazine;
						InitMagazine(magazine);
						LoadMagazineArticles();
					}
					else
					{
						SetLoadingImageVisible (false);
					}
				}
			}

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

		private void Search(string search)
		{
			ApplicationWorker.ClearNews();
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
			_isLoadingData = false;
			DestroyPdfLoader();
			if (showController != null)
			{
				NavigationController.PopToViewController (showController, false);
				showController.SearchFromAnother (search);
				Dispose();
			}
			else
			{
				showController = new NewsViewController (Page.None, search);
				NavigationController.PushViewController (showController, false);
			}
		}

		#endregion

		#region Event handlers

		void OnAllPdfFilesDeleted (object sender, EventArgs e)
		{
			if (_magazine != null)
			{
				UpdateMagazinesPdfExists(_magazine);
				_articlesTableView.ReloadData();
			}
		}

		void OnSettingsChanged (object sender, EventArgs e)
		{
			_articlesTableView.BackgroundColor = ItExpertHelper.GetUIColorFromColor (ApplicationWorker.Settings.GetBackgroundColor ());
			View.BackgroundColor = ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetBackgroundColor());
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
						if (!_isRubricSearch)
						{
							buffer = SortAndAddHeader(buffer);
						}
						_articles.AddRange(buffer);
					}
					else
					{
						var lst = _allArticles.ToList();
						if (!_isRubricSearch)
						{
							lst = SortAndAddHeader(lst);
						}
						_articles.AddRange(lst);
					}
					if (_isRubricSearch && _headerAdded && !string.IsNullOrWhiteSpace(_header))
					{
						_articles.Insert(0,
							new Article() { ArticleType = ArticleType.Header, Name = _header });
					}
					if (_banner != null)
					{
						_articles.Insert(0,
							new Article() { ArticleType = ArticleType.Banner, ExtendedObject = _banner });
					}
					if (!_isRubricSearch && ApplicationWorker.Magazine != null)
					{
						_articles.Insert(0, new Article() { ArticleType = ArticleType.MagazinePreview });
					}

					if (_isRubricSearch && _addPreviousArticleButton != null && _prevArticlesExists)
					{
						_articles.Add(new Article() {
							ArticleType = ArticleType.PreviousArticlesButton,
							ExtendedObject = _addPreviousArticleButton
						});
					}
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
			_isLoadingData = false;
			DestroyPdfLoader();
			if (showController != null)
			{
				NavigationController.PopToViewController (showController, true);
				Dispose();
			}
			else
			{
				showController = new AboutUsViewController ();
				NavigationController.PushViewController (showController, true);
			}
		}

		private void StartOnBannerGetted(object sender, BannerEventArgs e)
		{
			ApplicationWorker.RemoteWorker.BannerGetted -= StartOnBannerGetted;
			InvokeOnMainThread(() =>
			{
				var error = e.Error;
				if (!error)
				{
					var banner = e.Banners.FirstOrDefault();
					if (banner != null)
					{
						if (!banner.Url.StartsWith("http"))
						{
							banner.Url = Settings.Domen + banner.Url;
						}
						ApplicationWorker.Db.InsertBanner(banner);
						var picture = banner.Picture;
						if (picture != null)
						{
							ApplicationWorker.Db.InsertPicture(picture);
						}
						InitBanner(banner);
					}
				}
				else
				{
					BTProgressHUD.ShowToast ("Ошибка при загрузке", ProgressHUD.MaskType.None, false, 2500);
				}
				InitData();
			});
		}

		private void OnMagazinesPriviewGetted(object sender, MagazinesPreviewEventArgs e)
		{
			ApplicationWorker.RemoteWorker.MagazinesPriviewGetted -= OnMagazinesPriviewGetted;
			_isLoadingData = false;
			if (e.Abort)
			{
				return;
			}
			var error = e.Error;
			InvokeOnMainThread(() =>
			{
				if (!error)
				{
					var magazine = e.Magazines.FirstOrDefault();
					if (magazine != null)
					{
						_magazine = magazine;
						if (_lastMagazine)
						{
							ApplicationWorker.LastMagazine = magazine;
						}
						InitMagazine(magazine);
					}
				}
				else
				{
					BTProgressHUD.ShowToast ("Ошибка при загрузке", ProgressHUD.MaskType.None, false, 2500);
				}
				LoadMagazineArticles();
			});
		}

		private void OnMagazineArticlesGetted(object sender, MagazineArticlesEventArgs e)
		{
			ApplicationWorker.RemoteWorker.MagazineArticlesGetted -= OnMagazineArticlesGetted;
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
					if (e.Articles != null && e.Articles.Any())
					{
						_holdMessageView.Hidden = true;
						e.Articles.ForEach(article => article.IdMagazine = _magazine.Id);
						ApplicationWorker.Db.SetPropertiesForArticles(e.Articles);
						ApplicationWorker.NormalizePreviewText(e.Articles);
						if (_lastMagazine)
						{
							ApplicationWorker.LastMagazineArticles = e.Articles;
						}
						ThreadPool.QueueUserWorkItem(state => UpdateData(e));
						_allArticles = e.Articles.OrderByDescending(x => x.ActiveFrom).ToList();
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
						var lst = SortAndAddHeader(_articles);
						//Обновление списка новостей для ListView
						if (lst != null && lst.Any())
						{
							_articles = lst.ToList();
							if (_banner != null)
							{
								_articles.Insert(0,
									new Article() { ArticleType = ArticleType.Banner, ExtendedObject = _banner });
							}
							if (ApplicationWorker.Magazine != null)
							{
								_articles.Insert(0, new Article() { ArticleType = ArticleType.MagazinePreview });
							}
							//Загрузить _articles в список

							if (_articles != null && _articles.Any())
							{
								UpdateTableView(_articles);	
								_articlesTableView.Hidden = false;
							}
						}
					}
					else
					{
						ShowHoldMessage("Статей нет");
					}
				}
				else
				{
					BTProgressHUD.ShowToast("Ошибка при загрузке", ProgressHUD.MaskType.None, false, 2500);
				}
				SetLoadingImageVisible(false);
			});
		}

		private void AddPreviousArticleOnClick(object sender, EventArgs eventArgs)
		{
			if ((!_isRubricSearch && _searchRubric == null) || ApplicationWorker.Settings.OfflineMode) return;
			if (_isLoadingData) return;
			var connectAccept = IsConnectionAccept();
			if (!connectAccept)
			{
				BTProgressHUD.ShowToast ("Нет доступных подключений, для указанных в настройках", ProgressHUD.MaskType.None, false, 2500);
				return;
			}
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
			_isLoadingData = true;
			var lastDateTime =
				_articles.Where(
					x =>
					x.ArticleType == ArticleType.Magazine)
					.Min(x => x.Timespan);
			ApplicationWorker.RemoteWorker.MagazineArticlesGetted += SearchRubricOnMagazineArticlesGetted;
			ThreadPool.QueueUserWorkItem(
				state =>
				ApplicationWorker.RemoteWorker.BeginGetMagazinesArticlesByRubric(
					ApplicationWorker.Settings, _searchRubric, Magazine.BlockId, lastDateTime));

		}

		private void SearchRubricOnMagazineArticlesGetted(object sender, MagazineArticlesEventArgs e)
		{
			ApplicationWorker.RemoteWorker.MagazineArticlesGetted -= SearchRubricOnMagazineArticlesGetted;
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
					if (e.Articles == null || !e.Articles.Any())
					{
						if (_articles == null || !_articles.Any())
						{
							ShowHoldMessage("Статей нет");
						}
						else 
						{
							BTProgressHUD.ShowToast ("Больше статей нет", ProgressHUD.MaskType.None, false, 2500);
						}
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
					else
					{
						_holdMessageView.Hidden = true;
					}
					e.Articles.ForEach(article =>
					{
						article.Rubrics.Add(_searchRubric);
						article.RubricsId = _searchRubric.Id.ToString("G");
					});
					ApplicationWorker.Db.SetPropertiesForArticles(e.Articles);
					ApplicationWorker.NormalizePreviewText(e.Articles);
					ThreadPool.QueueUserWorkItem(state => UpdateData(e));
					//Обновление списка новостей для ListView
					if (_articles == null)
					{
						if (e.Articles != null && e.Articles.Any())
						{
							_allArticles = e.Articles.OrderByDescending(x => x.ActiveFrom).ToList();
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
							if (_isRubricSearch && _headerAdded && !string.IsNullOrWhiteSpace(_header))
							{
								_articles.Insert(0,
									new Article() { ArticleType = ArticleType.Header, Name = _header });
							}
							if (_banner != null)
							{
								_articles.Insert(0,
									new Article() { ArticleType = ArticleType.Banner, ExtendedObject = _banner });
							}
							if (_addPreviousArticleButton != null && _prevArticlesExists)
							{
								_articles.Add(new Article()
								{
									ArticleType = ArticleType.PreviousArticlesButton,
									ExtendedObject = _addPreviousArticleButton
								});
							}
							if (_articles != null && _articles.Any())
							{
								UpdateTableView(_articles);
								_articlesTableView.Hidden = false;
							}
						}
					}
					else
					{
						if (e.Articles != null && e.Articles.Any())
						{
							var position = _articles.Count(x => !x.IsReaded) - 1;
							_articles.Clear();
							_allArticles.AddRange(e.Articles.OrderByDescending(x => x.ActiveFrom).ToList());
							if (ApplicationWorker.Settings.HideReaded)
							{
								var buffer = _allArticles.Where(x => !x.IsReaded).ToList();
								if (buffer.Count() < 6)
								{
									var count = 6 - buffer.Count();
									buffer.AddRange(_allArticles.Where(x => x.IsReaded).Take(count));
									buffer = buffer.OrderBy(x => x.Timespan).ToList();
									position = 6 - count;
								}
								_articles.AddRange(buffer);
							}
							else
							{
								_articles.AddRange(_allArticles.ToList());
							}
							if (_isRubricSearch && _headerAdded && !string.IsNullOrWhiteSpace(_header))
							{
								_articles.Insert(0,
									new Article() { ArticleType = ArticleType.Header, Name = _header });
							}
							if (_banner != null)
							{
								_articles.Insert(0,
									new Article() { ArticleType = ArticleType.Banner, ExtendedObject = _banner });
							}
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
							if (_articlesTableView != null)
							{
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
							if (_articles != null && _articles.Any())
							{
								_articlesTableView.Hidden = false;
								if (ApplicationWorker.Settings.HideReaded)
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
					}
				}
				else
				{
					if (_articlesTableView != null)
					{
						_articlesTableView.ReloadData();
					}
					BTProgressHUD.ShowToast ("Ошибка при загрузке", ProgressHUD.MaskType.None, false, 2500);
				}
				SetLoadingImageVisible (false);
			});
		}


		private void OnPushArticleDetails(object sender, PushDetailsEventArgs e)
		{
			var controller = e.NewsDetailsView;
			controller.SetFromFavorite(false);
			if (_searchRubric != null)
			{
				var filterParams = new FilterParameters() {
					SearchRubric = _searchRubric,
				};
				controller.SetFilterParameters(filterParams);
			}
			NavigationController.PushViewController(controller, true);
		}

		private void ButInCacheOnClick(object s, EventArgs ev)
		{
			if (_allArticles == null || !_allArticles.Any() || _magazine == null) return;
			if (_isRubricSearch)
			{
				BTProgressHUD.ShowToast ("Результаты по рубрикам нельзя сбросит в кэш", ProgressHUD.MaskType.None, false, 2500);
				return;
			}
			//отобразить модальный индикатор прогресса;
			Action inCache = () =>
			{
				InvokeOnMainThread(() =>
				{
					BTProgressHUD.Show("Cброc в кэш..", -1, ProgressHUD.MaskType.Clear);
				});
				_magazine.InCache = true;
				ApplicationWorker.Db.InsertMagazine(_magazine);
				var pictures = ApplicationWorker.Db.GetPicturesForParent(_magazine.Id);
				if (pictures != null && pictures.Any())
				{
					var magazinePicture = pictures.FirstOrDefault(x => x.Type == PictypeType.Magazine);
					if (magazinePicture != null)
					{
						ApplicationWorker.Db.DeletePicture(magazinePicture.Id);
					}
				}
				if (_magazine.PreviewPicture != null)
				{
					ApplicationWorker.Db.InsertPicture(_magazine.PreviewPicture);
				}
				var completeArticlesCount = _allArticles.Count(x => !string.IsNullOrEmpty(x.DetailText)); 
				var serverInteractionNeeded = ((double)completeArticlesCount / (double)_allArticles.Count()) < 0.5;
				if (!serverInteractionNeeded)
				{
					ApplicationWorker.Db.SaveInCache(_allArticles.ToList());
					InvokeOnMainThread(()=>{
						BTProgressHUD.ShowToast("Данные сброшены в кеш", ProgressHUD.MaskType.None, false, 2500);
						Action hideToastAction = ()=>{
							Thread.Sleep(1500);
							InvokeOnMainThread(()=>BTProgressHUD.Dismiss());
						};
						ThreadPool.QueueUserWorkItem(state => hideToastAction());
					});
				}
				else
				{
					InvokeOnMainThread(() =>
					{
						BTProgressHUD.Show("Получение полных статей...", -1, ProgressHUD.MaskType.Clear);
					});
					var settingsClone = ApplicationWorker.Settings.Clone();
					settingsClone.LoadDetails = true;
					ApplicationWorker.RemoteWorker.MagazineArticlesGetted += (sender, e) => {
						ApplicationWorker.RemoteWorker.ClearMagazineArticlesEventHandler();
						if (e.Abort || e.Error)
						{
							InvokeOnMainThread(() =>
							{
								BTProgressHUD.ShowToast("Сбой при получении данных...Отмена сброса в кеш", ProgressHUD.MaskType.None, false, 2500);
								Action hideToastAction = ()=>{
									Thread.Sleep(1500);
									InvokeOnMainThread(()=>BTProgressHUD.Dismiss());
								};
								ThreadPool.QueueUserWorkItem(state => hideToastAction());
							});
						}
						else
						{
							if (e.Articles != null && e.Articles.Any())
							{
								e.Articles.ForEach(article => article.IdMagazine = _magazine.Id);
								ApplicationWorker.Db.SetPropertiesForArticles(e.Articles);
								ApplicationWorker.NormalizePreviewText(e.Articles);
								if (_lastMagazine)
								{
									ApplicationWorker.LastMagazineArticles = e.Articles;
								}
								UpdateData(e);
								_allArticles = e.Articles.OrderByDescending(x => x.ActiveFrom).ToList();
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
								var lst = SortAndAddHeader(_articles);
								//Обновление списка новостей для ListView
								if (lst != null && lst.Any())
								{
									_articles = lst.ToList();
									if (_banner != null)
									{
										_articles.Insert(0,
											new Article() { ArticleType = ArticleType.Banner, ExtendedObject = _banner });
									}
									if (ApplicationWorker.Magazine != null)
									{
										_articles.Insert(0, new Article() { ArticleType = ArticleType.MagazinePreview });
									}
								}
								ApplicationWorker.Db.SaveInCache(_allArticles.ToList());
								InvokeOnMainThread(()=>{
									UpdateTableView(_articles);	
									_articlesTableView.Hidden = false;
									BTProgressHUD.ShowToast("Полные статьи получены и сброшены в кеш", ProgressHUD.MaskType.None, false, 2500);
									Action hideToastAction = ()=>{
										Thread.Sleep(1500);
										InvokeOnMainThread(()=>BTProgressHUD.Dismiss());
									};
									ThreadPool.QueueUserWorkItem(state => hideToastAction());
								});
							}
							else
							{
								InvokeOnMainThread(()=>{
									BTProgressHUD.ShowToast("Не удалось получить данные. Сброс в кеш отменен", ProgressHUD.MaskType.None, false, 2500);
									Action hideToastAction = ()=>{
										Thread.Sleep(1500);
										InvokeOnMainThread(()=>BTProgressHUD.Dismiss());
									};
									ThreadPool.QueueUserWorkItem(state => hideToastAction());
								});
							}
						}
					};
					ApplicationWorker.RemoteWorker.BeginGetMagazineArticles(settingsClone, _magazine.Id);
				}

			};
			ThreadPool.QueueUserWorkItem(state => inCache());
		}

		private void ButRefreshOnClick(object sender, EventArgs e)
		{
			if (!_isLoadingData && !ApplicationWorker.Settings.OfflineMode)
			{
				var connectAccept = IsConnectionAccept();
				if (!connectAccept)
				{
					BTProgressHUD.ShowToast ("Нет доступных подключений, для указанных в настройках", ProgressHUD.MaskType.None, false, 2500);
					return;
				}
				_prevArticlesExists = true;
				//Пустой список
				_articles.Clear();
				_allArticles.Clear();
				if (_articlesTableView != null && _articlesTableView.Source != null)
				{
					if (_articlesTableView.Source is DoubleArticleTableSource)
					{
						(_articlesTableView.Source as DoubleArticleTableSource).UpdateSource ();
					}
				}
				if (_articlesTableView != null)
				{
					_articlesTableView.ReloadData ();
				}
				_articlesTableView.Hidden = true;
				_holdMessageView.Hidden = true;
				if (_searchRubric != null && _isRubricSearch)
				{
					_isLoadingData = true;
					ApplicationWorker.RemoteWorker.MagazineArticlesGetted += SearchRubricOnMagazineArticlesGetted;
					ThreadPool.QueueUserWorkItem(
						state =>
						ApplicationWorker.RemoteWorker.BeginGetMagazinesArticlesByRubric(
							ApplicationWorker.Settings, _searchRubric, Magazine.BlockId, -1));
				}
				else
				{
					_searchRubric = null;
					_isRubricSearch = false;
					LoadMagazineArticles();   
				}
			}
		}

		private void ButTrendsOnClick(object sender, EventArgs eventArgs)
		{
			ApplicationWorker.ClearNews();
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
			DestroyPdfLoader ();
			_isLoadingData = false;
			if (showController != null)
			{
				NavigationController.PopToViewController (showController, false);
				showController.ShowFromAnotherScreen (Page.Trends);
				Dispose();
			}
			else
			{
				showController = new NewsViewController (Page.Trends, null);
				NavigationController.PushViewController (showController, false);
			}
		}

		private void ButNewsOnClick(object sender, EventArgs eventArgs)
		{
			ApplicationWorker.ClearNews();
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
			DestroyPdfLoader ();
			_isLoadingData = false;
			if (showController != null)
			{
				NavigationController.PopToViewController (showController, false);
				showController.ShowFromAnotherScreen (Page.News);
				Dispose();
			}
			else
			{
				showController = new NewsViewController (Page.News, null);
				NavigationController.PushViewController (showController, false);
			}
		}

		private void ButArchiveOnClick(object sender, EventArgs eventArgs)
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
			DestroyPdfLoader ();
			if (showController != null)
			{
				NavigationController.PopToViewController (showController, false);
				Dispose();
			}
			else
			{
				showController = new ArchiveViewController ();
				NavigationController.PushViewController (showController, false);
			}
		}

		private void ButMagazineOnClick(object sender, EventArgs eventArgs)
		{
			ApplicationWorker.ClearNews();
			if (ApplicationWorker.Settings.OfflineMode) return;
			if (_isRubricSearch && !_isLoadingData && _magazine != null)
			{
				_isRubricSearch = false;
				_searchRubric = null;
				_headerAdded = false;
				_header = null;
				_prevArticlesExists = true;
				_rubricId = -1;
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
					source = new ArticlesTableSource(new List<Article>(), MagazineAction.NoAction);
				}
				else
				{
					source = new DoubleArticleTableSource(new List<Article>(), MagazineAction.NoAction);
				}

				var tableViewTopOffset = NavigationController.NavigationBar.Frame.Height + ItExpertHelper.StatusBarHeight;
				_articlesTableView.Frame = new RectangleF(0, tableViewTopOffset, View.Bounds.Width, 
					View.Bounds.Height - tableViewTopOffset - _bottomBar.Frame.Height);

				_articlesTableView.ContentSize = new SizeF(_articlesTableView.Frame.Width, _articlesTableView.Frame.Height);

				_articlesTableView.Source = source;
				_articlesTableView.ReloadData();
				_articlesTableView.Hidden = true;
				LoadMagazineArticles();
				return;
			}
			if (_isRubricSearch && !_isLoadingData && _magazine == null)
			{
				_magazineId = -1;
				InitData();
			}
		}

		private void ButFavoriteOnClick(object sender, EventArgs eventArgs)
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
			DestroyPdfLoader ();
			if (showController != null)
			{
				NavigationController.PopToViewController (showController, false);
				showController.UpdateSource();
				Dispose();
			}
			else
			{
				showController = new FavoritesViewController ();
				NavigationController.PushViewController (showController, false);
			}
		}

		private void OnPdfGetted(object sender, PdfEventArgs e)
		{
			ApplicationWorker.PdfLoader.PdfGetted -= OnPdfGetted;
			if (e.Abort)
			{
				InvokeOnMainThread(() =>
				{
					IsLoadingPdf = false;
					if (_articlesTableView != null)
					{
						_articlesTableView.ReloadData();
					}
				});
				return;
			}
			var error = e.Error;
			InvokeOnMainThread(() =>
			{
				if (!error)
				{
					var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
					var fileName = _magazine.Id.ToString("G") + ".pdf";
					var path = Path.Combine(folder, fileName);
					using (var fs = File.Create(path))
					{
						fs.Write(e.Pdf, 0, e.Pdf.Length);
						fs.Flush();
					}
					_magazine.Exists = true;
					var dbModel = ApplicationWorker.Db.GetMagazine(_magazine.Id, false);
					if (dbModel != null)
					{
						ApplicationWorker.Db.UpdateMagazine(_magazine);
					}
					else
					{
						ApplicationWorker.Db.InsertMagazine(_magazine);
						ApplicationWorker.Db.InsertPicture(_magazine.PreviewPicture);
					}
					if (_magazine.Exists)
					{
						if (_articlesTableView != null)
						{
							if (_articlesTableView.Source is ArticlesTableSource)
							{
								(_articlesTableView.Source as ArticlesTableSource).SetMagazineAction(MagazineAction.Open);
							}
							if (_articlesTableView.Source is DoubleArticleTableSource)
							{
								(_articlesTableView.Source as DoubleArticleTableSource).SetMagazineAction(MagazineAction.Open);
							}
						}
					}
				}
				else
				{
					BTProgressHUD.ShowToast("Ошибка при запросе", ProgressHUD.MaskType.None, false, 2500);
				}
				IsLoadingPdf = false;
				if (_articlesTableView != null)
				{
					_articlesTableView.ReloadData();
				}
			});
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

		#region Logic

		public void DeleteMagazinePdf()
		{
			if (_magazine == null)
				return;
			var alertView = new BlackAlertView("Удаление", String.Format("Удалить журнал: {0}?", _magazine.Name), "Нет", "Да");

			alertView.ButtonPushed += (s, ev) => 
			{
				if (ev.ButtonIndex == 1)
				{
					var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
					var fileName = _magazine.Id.ToString("G") + ".pdf";
					var path = Path.Combine(folder, fileName);
					File.Delete(path);
					_magazine.Exists = false;
					ApplicationWorker.Db.UpdateMagazine(_magazine);
					if (_articlesTableView != null)
					{
						_articlesTableView.ReloadData ();
					}
					BTProgressHUD.ShowToast ("Файл удален", ProgressHUD.MaskType.None, false, 2500);
				}
				alertView.Dispose();
			};

			alertView.Show();
		}

		public void OpenMagazinePdf()
		{
			var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			var fileName = _magazine.Id.ToString("G") + ".pdf";
			var path = Path.Combine(folder, fileName);
			if (!File.Exists(path))
			{
				BTProgressHUD.ShowToast ("Файл не найден", ProgressHUD.MaskType.None, false, 2500);
				return;
			}
			var file = new FileInfo (path);
			var docViewController = new DocumentViewController(file.Name, file.FullName);
			PresentViewController(docViewController, true, null);
		}

		public void DownloadMagazinePdf()
		{
			if (_magazine.Exists || IsLoadingPdf)
				return;
			if (ApplicationWorker.Settings.OfflineMode)
			{
				BTProgressHUD.ShowToast ("Загрузка Pdf невозможна в оффлайн режиме", ProgressHUD.MaskType.None, false, 2500);
				return;
			}
			if (string.IsNullOrWhiteSpace(_magazine.PdfFileSrc))
			{
				BTProgressHUD.ShowToast ("Pdf файл недоступен", ProgressHUD.MaskType.None, false, 2500);
				return;
			}
			if (!ApplicationWorker.PdfLoader.IsOperation())
			{
				var connectAccept = IsConnectionAccept();
				if (!connectAccept)
				{
					BTProgressHUD.ShowToast ("Нет доступных подключений, для указанных в настройках", ProgressHUD.MaskType.None, false, 2500);
					return;
				}
				IsLoadingPdf = true;
				ApplicationWorker.PdfLoader.PdfGetted += OnPdfGetted;
				ThreadPool.QueueUserWorkItem(state => ApplicationWorker.PdfLoader.BeginGetMagazinePdf(_magazine.PdfFileSrc));
				if (_articlesTableView != null)
				{
					_articlesTableView.ReloadData ();
				}
			}
			else
			{
				BTProgressHUD.ShowToast ("Идет загрузка Pdf... Дождитесь завершения", ProgressHUD.MaskType.None, false, 2500);
			}
		}

		public void SearchRubric(int rubricId)
		{
			_rubricId = rubricId;
		}

		//Инициализация панели журнала
		private void InitMagazine(Magazine magazine)
		{
			if (magazine == null) return;
			_magazine = magazine;
            ApplicationWorker.Magazine = magazine;
			UpdateMagazinesPdfExists(magazine);
		}

		private void UpdateMagazinesPdfExists(Magazine magazine)
		{
			if (magazine == null) return;
			var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			var fileName = magazine.Id.ToString("G") + ".pdf";
			var path = System.IO.Path.Combine(folder, fileName);
			var file = new FileInfo(path);
			magazine.Exists = file.Exists;
		}

		private void LoadMagazineArticles()
		{
			if (_magazine == null) return;
			_holdMessageView.Hidden = true;
			if (!ApplicationWorker.Settings.OfflineMode)
			{
				var connectAccept = IsConnectionAccept();
				if (!connectAccept)
				{
					BTProgressHUD.ShowToast ("Нет доступных подключений, для указанных в настройках", ProgressHUD.MaskType.None, false, 2500);
					return;
				}
				if (_lastMagazine)
				{
					var articles = ApplicationWorker.LastMagazineArticles;
					if (articles != null)
					{
						_allArticles = articles.OrderByDescending(x => x.ActiveFrom).ToList();
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
						var lst = SortAndAddHeader(_articles);
						//Обновление списка новостей для ListView
						if (lst != null && lst.Any())
						{
							_articles = lst.ToList();
                            if (_banner != null)
							{
								_articles.Insert(0,
                                    new Article() { ArticleType = ArticleType.Banner, ExtendedObject = _banner });
							}
							if (ApplicationWorker.Magazine != null)
							{
								_articles.Insert(0, new Article() { ArticleType = ArticleType.MagazinePreview });
							}
							//Загрузить _articles в список
							if (_articles != null && _articles.Any())
							{
								UpdateTableView(_articles);
								_articlesTableView.Hidden = false;
							}
							SetLoadingImageVisible (false);
						}
						return;
					}
				}
				SetLoadingImageVisible (true);
				_isLoadingData = true;
				ApplicationWorker.RemoteWorker.MagazineArticlesGetted += OnMagazineArticlesGetted;
				ThreadPool.QueueUserWorkItem(
					state =>
					ApplicationWorker.RemoteWorker.BeginGetMagazineArticles(ApplicationWorker.Settings, _magazine.Id));
			}
			else
			{
				Action action = () =>
				{
					InvokeOnMainThread(() =>
					{
						SetLoadingImageVisible(true);
						UpdateTableView(new List<Article>());
						_articlesTableView.Hidden = true;
					});
					var lst = ApplicationWorker.Db.GetMagazineArticlesFromDb(_magazine.Id);
					if (lst != null && lst.Any())
					{
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
						lst = SortAndAddHeader(_articles);
						//Обновление списка новостей для ListView
						if (lst != null && lst.Any())
						{
							_articles = lst.ToList();
							if (_banner != null)
							{
								_articles.Insert(0,
									new Article() { ArticleType = ArticleType.Banner, ExtendedObject = _banner });
							}
							if (ApplicationWorker.Magazine != null)
							{
								_articles.Insert(0, new Article() { ArticleType = ArticleType.MagazinePreview });
							}
							InvokeOnMainThread(() =>
							{
								if (_articles != null && _articles.Any())
								{
									UpdateTableView(_articles);	
									_articlesTableView.Hidden = false;
								}
								SetLoadingImageVisible(false);
							});
						}
					}
					else
					{
						InvokeOnMainThread(() =>
						{
							SetLoadingImageVisible(false);
						});
					}
				};
				ThreadPool.QueueUserWorkItem(state => action());
			}
		}

		private void UpdateData(MagazineArticlesEventArgs e)
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

		private List<Article> SortAndAddHeader(List<Article> lst)
		{
			if (lst == null || !lst.Any()) return null;
			var returnLst = new List<Article>();
			var rubrics =
				lst.OrderBy(x => x.Sort)
					.Select(x => x.Rubrics.First())
					.Distinct(new RubricComparer())
					.ToList();
			foreach (var rubric in rubrics)
			{
				returnLst.Add(new Article() {ArticleType = ArticleType.Header, Name = rubric.Name});
				var rubricId = rubric.Id;
				var articles = lst.Where(x => x.Rubrics.First().Id == rubricId).OrderBy(x => x.Sort);
				returnLst.AddRange(articles);
			}
			return returnLst;
		}

		private void UpdateTableView(List<Article> articles)
        {
			var action = MagazineAction.NoAction;
			if (!_isRubricSearch)
			{
				action = _magazine.Exists ? MagazineAction.Open : MagazineAction.Download;
			}
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
				source = new ArticlesTableSource(articles, action);

                (source as ArticlesTableSource).PushDetailsView += OnPushArticleDetails;
            }
            else
            {
				source = new DoubleArticleTableSource(articles, action);

                (source as DoubleArticleTableSource).PushDetailsView += OnPushArticleDetails;
            }

            var tableViewTopOffset = NavigationController.NavigationBar.Frame.Height + ItExpertHelper.StatusBarHeight;
            _articlesTableView.Frame = new RectangleF(0, tableViewTopOffset, View.Bounds.Width, 
                View.Bounds.Height - tableViewTopOffset - _bottomBar.Frame.Height);

            _articlesTableView.ContentSize = new SizeF(_articlesTableView.Frame.Width, _articlesTableView.Frame.Height);

            _articlesTableView.Source = source;
            _articlesTableView.ReloadData();
        }

		void ShowHoldMessage(string message)
		{
			_holdMessageView.Hidden = false;
			_holdMessageView.Text = message;
			_holdMessageView.SizeToFit();
			_holdMessageView.Frame = new RectangleF((View.Bounds.Width - _holdMessageView.Frame.Width) / 2, View.Bounds.Height / 2, _holdMessageView.Frame.Width, _holdMessageView.Frame.Height);
			View.BringSubviewToFront(_holdMessageView);
		}

		public void DestroyPdfLoader()
		{
			ApplicationWorker.PdfLoader.PdfGetted -= OnPdfGetted;
			ApplicationWorker.PdfLoader.AbortOperation();
			if (_articlesTableView != null)
			{
				_articlesTableView.ReloadData ();
			}
			IsLoadingPdf = false;
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

