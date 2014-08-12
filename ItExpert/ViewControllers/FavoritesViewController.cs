using System;
using MonoTouch.UIKit;
using System.Drawing;
using ItExpert.Enum;
using ItExpert.ServiceLayer;
using ItExpert.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BigTed;
using MonoTouch.Foundation;

namespace ItExpert
{
    public class FavoritesViewController: UIViewController
    {
		#region Fields

		private BottomToolbarView _bottomBar = null;
		private UIActivityIndicatorView _loadingIndicator = null;
		private UITableView _articlesTableView = null;
		private UIButton _addPreviousArticleButton = null;
		private bool _prevArticlesExists = true;
		private List<Article> _articles;
		private bool _firstLoad = true;
		private UIInterfaceOrientation _currentOrientation;
		private UILabel _holdMessageView;
		private bool _showFromAnotherScreen = false;
		private MenuView _menu;
		private UIButton _menuButton;
		private UIBarButtonItem _menuBarButton;
		private SettingsView _settingsView;
		private UIButton _settingsButton;
		private UIBarButtonItem _settingsBarButton;

		#endregion

		#region UIViewController members

		static bool UserInterfaceIdiomIsPhone {
			get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
		}

		public FavoritesViewController()
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			// Perform any additional setup after loading the view, typically from a nib.
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			if (!_firstLoad && _currentOrientation != InterfaceOrientation)
			{
				_currentOrientation = InterfaceOrientation;
				UpdateViewsLayout ();
			}
			if (_showFromAnotherScreen)
			{
				_showFromAnotherScreen = false;
				return;
			}
			if (!_firstLoad)
			{
				if (_articles == null || !_articles.Any()) return;
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
							if (_articles[position].IsFavorite)
							{
								isFound = true;
								selectArticle = _articles[position];
							}
						}
					}
				}
				var lst = _articles.Where(x => x.IsFavorite).ToList();
				_articles.Clear();
				_articles.AddRange(lst);
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
				//Добавление кнопки Загрузить еще
				if (_addPreviousArticleButton != null && _prevArticlesExists)
				{
					_articles.Add (new Article () {
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
				if (position > 0)
				{
					if (!UserInterfaceIdiomIsPhone)
					{
						position = (int)Math.Floor((double)position / 2);
					}
					var indexPath = NSIndexPath.FromItemSection(position, 0);
					_articlesTableView.ScrollToRow(indexPath, UITableViewScrollPosition.Middle, false);
				}
			}
		}

		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);
			if (_firstLoad)
			{
				_currentOrientation = InterfaceOrientation;
				_firstLoad = false;
				Initialize ();
				Action action = () =>
				{
					Thread.Sleep(150);
					InvokeOnMainThread(()=>LoadDataFromDb ());
				};
				ThreadPool.QueueUserWorkItem(state => action());
			}
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
			UpdateViewsLayout ();

			_articlesTableView.Hidden = false;
			_bottomBar.Hidden = false;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			InvokeOnMainThread(() =>
			{
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

				if (_articles != null)
				{
					_articles.Clear();
				}
				_articles = null;
			});
		}

		#endregion

		#region Init

		public void UpdateSource()
		{
			_showFromAnotherScreen = true;
			LoadDataFromDb ();
		}

		public void Initialize()
		{
			View.BackgroundColor = ItExpertHelper.GetUIColorFromColor (ApplicationWorker.Settings.GetBackgroundColor ());
			View.AutosizesSubviews = true;
			AutomaticallyAdjustsScrollViewInsets = false;

			ApplicationWorker.SettingsChanged += OnSettingsChanged;

			InitBottomToolbar ();
			InitLoadingProgress ();
			InitNavigationBar();
			InitHoldMessageView();
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

			View.Add(_articlesTableView);

			_articlesTableView.Hidden = true;
		}

		private void InitBottomToolbar()
		{
			float height = 66;

			_bottomBar = new BottomToolbarView ();
			_bottomBar.Frame = new RectangleF(0, View.Frame.Height - height, View.Frame.Width, height);
			_bottomBar.LayoutIfNeeded();
			_bottomBar.FavoritesButton.SetActiveState (true);	
			_bottomBar.NewsButton.ButtonClick += ButNewsOnClick;
			_bottomBar.TrendsButton.ButtonClick += ButTrendsOnClick;
			_bottomBar.MagazineButton.ButtonClick += ButMagazineOnClick;
			_bottomBar.ArchiveButton.ButtonClick += ButArchiveOnClick;
			_bottomBar.FavoritesButton.ButtonClick += ButFavoriteOnClick;
			View.Add(_bottomBar);
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

			NavigationItem.RightBarButtonItems = new UIBarButtonItem[] { space, _settingsBarButton };
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

		void InitHoldMessageView()
		{
			_holdMessageView = new UILabel(new RectangleF(0, 0, 0, 0));
			_holdMessageView.BackgroundColor = UIColor.Clear;
			_holdMessageView.Font = UIFont.BoldSystemFontOfSize(16);
			_holdMessageView.TextColor = ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetForeColor());
			_holdMessageView.Hidden = true;
			Add(_holdMessageView);
		}

		private void Search(string search)
		{
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

		void AboutUsShow (object sender, EventArgs e)
		{
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
				Dispose();
			}
			else
			{
				showController = new AboutUsViewController ();
				NavigationController.PushViewController (showController, true);
			}
		}

		private void AddPreviousArticleOnClick(object sender, EventArgs eventArgs)
		{
			Action updateList = () =>
			{
				var count = _articles.Count();
				var lst = ApplicationWorker.Db.GetFavorites(count, 20);
				InvokeOnMainThread(() =>
				{
					if (!lst.Any())
					{
						BTProgressHUD.ShowToast ("Больше нет избранных статей", ProgressHUD.MaskType.None, false, 2500);	
						_prevArticlesExists = false;
						_articles.RemoveAt(_articles.Count() - 1);
						if (_articlesTableView != null && _articlesTableView.Source != null)
						{
							if (_articlesTableView.Source is DoubleArticleTableSource)
							{
								(_articlesTableView.Source as DoubleArticleTableSource).UpdateSource ();
							}
							_articlesTableView.ReloadData();
						}
						return;
					}
					_articles.RemoveAt(_articles.Count() - 1);
					_articles.AddRange(lst);
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
				});
			};
			ThreadPool.QueueUserWorkItem(state => updateList());
		}

		private void ButTrendsOnClick(object sender, EventArgs eventArgs)
		{
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
				Dispose();
			}
			else
			{
				showController = new MagazineViewController (-1);
				NavigationController.PushViewController (showController, false);
			}
		}

		private void ButFavoriteOnClick(object sender, EventArgs eventArgs)
		{
			LoadDataFromDb();
		}

		private void OnPushArticleDetails(object sender, PushDetailsEventArgs e)
		{
			NavigationController.PushViewController (e.NewsDetailsView, true);
		}

		#endregion

		#region Logic

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

		private void UpdateTableView(List<Article> articles)
		{
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

		void ShowHoldMessage(string message)
		{
			_holdMessageView.Hidden = false;
			_holdMessageView.Text = message;
			_holdMessageView.SizeToFit();
			_holdMessageView.Frame = new RectangleF((View.Bounds.Width - _holdMessageView.Frame.Width) / 2, View.Bounds.Height / 2, _holdMessageView.Frame.Width, _holdMessageView.Frame.Height);
			View.BringSubviewToFront(_holdMessageView);
		}

		private void LoadDataFromDb()
		{
			_prevArticlesExists = true;
			var lst = ApplicationWorker.Db.GetFavorites(0, 20);
			if (lst.Count() < 20)
			{
				_prevArticlesExists = false;
			}
			_articles = lst;
			if (!_articles.Any())
			{
				ShowHoldMessage("Нет избранных статей");
				_prevArticlesExists = false;
				//Пустой список
				UpdateTableView(new List<Article>());
				_articlesTableView.Hidden = true;
				return;
			}
			else
			{
				_holdMessageView.Hidden = true;
			}
			if (_addPreviousArticleButton != null && _prevArticlesExists)
			{
				_articles.Add(new Article()
				{
					ArticleType = ArticleType.PreviousArticlesButton,
					ExtendedObject = _addPreviousArticleButton
				});
			}
			UpdateTableView(_articles);
			_articlesTableView.Hidden = false;
			Action reloadData = () =>
			{
				Thread.Sleep(250);
				InvokeOnMainThread(() =>
				{
					_articlesTableView.ReloadData();
				});
			};
			ThreadPool.QueueUserWorkItem(state => reloadData());
		}

		#endregion

		#region Helper methods

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

