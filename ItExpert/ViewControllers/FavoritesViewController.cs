using System;
using MonoTouch.UIKit;
using System.Drawing;
using ItExpert.Enum;
using ItExpert.ServiceLayer;
using ItExpert.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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
					//Прокрутить список к position
				}
			}
			if (_firstLoad)
			{
				_currentOrientation = InterfaceOrientation;
				Initialize ();
				_firstLoad = false;
				UpdateViewsLayout ();
			}
		}

		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate(fromInterfaceOrientation);
			_currentOrientation = InterfaceOrientation;
			UpdateViewsLayout ();
		}

		#endregion

		#region Init

		public void Initialize()
		{
			View.BackgroundColor = ItExpertHelper.GetUIColorFromColor (ApplicationWorker.Settings.GetBackgroundColor ());
			View.AutosizesSubviews = true;
			AutomaticallyAdjustsScrollViewInsets = false;

			InitBottomToolbar ();
			InitLoadingProgress ();

			//Пустой список
			_articlesTableView = new UITableView(new RectangleF(0, 0, 0, 
				0), UITableViewStyle.Plain);
			_articlesTableView.ScrollEnabled = true; 
			_articlesTableView.UserInteractionEnabled = true;
			_articlesTableView.SeparatorInset = new UIEdgeInsets (0, 0, 0, 0);
			_articlesTableView.Bounces = true;
			_articlesTableView.SeparatorColor = UIColor.FromRGB(100, 100, 100);
			View.Add(_articlesTableView);

			LoadDataFromDb ();
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
			_addPreviousArticleButton = button;
		}

		#endregion

		#region Event handlers

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
						Toast.MakeText(this, "Больше нет избранных статей", ToastLength.Short).Show();
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
				NavigationController.PopToViewController (showController, true);
				showController.ShowFromAnotherScreen (Page.Trends);
			}
			else
			{
				showController = new NewsViewController (Page.Trends);
				NavigationController.PushViewController (showController, true);
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
				NavigationController.PopToViewController (showController, true);
				showController.ShowFromAnotherScreen (Page.News);
			}
			else
			{
				showController = new NewsViewController (Page.News);
				NavigationController.PushViewController (showController, true);
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
				NavigationController.PopToViewController (showController, true);
			}
			else
			{
				showController = new ArchiveViewController ();
				NavigationController.PushViewController (showController, true);
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
				NavigationController.PopToViewController (showController, true);
				showController.SetMagazineId (-1);
			}
			else
			{
				showController = new MagazineViewController (-1);
				NavigationController.PushViewController (showController, true);
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
				source = new ArticlesTableSource(articles, true, MagazineAction.NoAction);

				((ArticlesTableSource)source).PushDetailsView += OnPushArticleDetails;
			}
			else
			{
				source = new DoubleArticleTableSource(articles, true, MagazineAction.NoAction);

				((DoubleArticleTableSource)source).PushDetailsView += OnPushArticleDetails;
			}

			var tableViewTopOffset = NavigationController.NavigationBar.Frame.Height + ItExpertHelper.StatusBarHeight;
			_articlesTableView.Frame = new RectangleF(0, tableViewTopOffset, View.Bounds.Width, 
				View.Bounds.Height - tableViewTopOffset - _bottomBar.Frame.Height);

			_articlesTableView.ContentSize = new SizeF(_articlesTableView.Frame.Width, _articlesTableView.Frame.Height);

			_articlesTableView.Source = source;
			_articlesTableView.ReloadData();
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
				Toast.MakeText(this, "Нет избранных статей", ToastLength.Short).Show();
				_prevArticlesExists = false;
				//Пустой список
				UpdateTableView(new List<Article>());
				return;
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

