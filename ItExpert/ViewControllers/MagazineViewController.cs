﻿using System;
using MonoTouch.UIKit;
using System.Drawing;
using ItExpert.Enum;
using ItExpert.Model;
using System.Collections.Generic;
using ItExpert.ServiceLayer;
using System.Linq;
using System.Threading;

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
		private bool _toMenu = false;
		public static MagazineViewController Current;
		private UIView _extendedObject = null;
		private bool _lastMagazine = false;
		private UIView _addPreviousArticleButton = null;
		private bool _prevArticlesExists = true;
		private bool _headerAdded = false;
		private string _header = null;
		private BottomToolbarView _bottomBar = null;
		private int _magazineId = -1;

		#endregion

		#region UIViewController members

		static bool UserInterfaceIdiomIsPhone {
			get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
		}

		public MagazineViewController(int magazineId)
        {
			_magazineId = magazineId;
        }

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			Initialize ();
			// Perform any additional setup after loading the view, typically from a nib.
		}

		#endregion

		#region Init

		public void Initialize()
		{
			Current = this;
			View.BackgroundColor = ItExpertHelper.GetUIColorFromColor (ApplicationWorker.Settings.GetBackgroundColor ());

			InitBottomToolbar ();
			CreateExtendedObject();

			var screenWidth =
				ApplicationWorker.Settings.GetScreenWidthForScreen((int)UIScreen.MainScreen.Bounds.Size.Width);
			ApplicationWorker.RemoteWorker.BannerGetted += StartOnBannerGetted;
			var settings = new Settings()
			{
				ScreenWidth = screenWidth,
				ScreenResolution = ApplicationWorker.Settings.ScreenResolution
			};
			ThreadPool.QueueUserWorkItem(
				state => ApplicationWorker.RemoteWorker.BeginGetBanner(settings));
			_isLoadingData = true;
		}

		private void InitBottomToolbar()
		{
			float height = 66;

			_bottomBar = new BottomToolbarView ();
			_bottomBar.Frame = new RectangleF(0, View.Frame.Height - height, View.Frame.Width, height);
			_bottomBar.LayoutIfNeeded();
			_bottomBar.MagazineButton.SetState (true);	
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
				var bannerImage = image;
			}
			else
			{
				var koefScaling = screenWidth / picture.Width;
				var pictHeightScaling = picture.Height * koefScaling;
				if (pictHeightScaling > maxPictureHeight)
				{
					koefScaling = (float)maxPictureHeight / picture.Height;
				}
				var bannerGif = new BannerView (banner, koefScaling, screenWidth);
			}
			//Прикрепить обработчик клика по баннеру
		}

		private void CreateExtendedObject()
		{

		}

		private void InitData()
		{
			var itemId = _magazineId;
			var loadData = true;
			if (itemId != -1)
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
//						Toast.MakeText(this, "Нет доступных подключений, для указанных в настройках", ToastLength.Long)
//							.Show();
						return;
					}
					ApplicationWorker.RemoteWorker.MagazinesPriviewGetted += OnMagazinesPriviewGetted;
					_isLoadingData = true;
					ThreadPool.QueueUserWorkItem(
						state => ApplicationWorker.RemoteWorker.BeginGetMagazinesPreview(ApplicationWorker.Settings, -1));
				}
				else
				{
//					Magazine magazine = ApplicationWorker.Db.GetNewestSavedMagazine();
//					if (magazine != null)
//					{
//						_magazine = magazine;
//						InitMagazine(magazine);
//						LoadMagazineArticles();
//					}
				}
			}

		}


		#endregion

		#region Event handlers

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
//					Toast.MakeText(this, "Ошибка при загрузке Баннера", ToastLength.Short).Show();
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
//					Toast.MakeText(this, "Ошибка при загрузке", ToastLength.Short).Show();
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
//							Toast.MakeText(this, "Непрочитанных статей меньше 6, будут выведены некоторые прочитанные статьи", ToastLength.Short).Show();
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
						if (_extendedObject != null)
						{
//							if (_extendedObject.Parent != null)
//							{
//								((LinearLayout)_extendedObject.Parent).RemoveAllViews();
//							}
							_articles.Insert(0,
								new Article() { ArticleType = ArticleType.ExtendedObject, ExtendedObject = _extendedObject });
						}
						var action = MagazineAction.NoAction;
						if (!_isRubricSearch)
						{
							action = _magazine.Exists ? MagazineAction.Open : MagazineAction.Download;
						}
						//Загрузить _articles в список
					}
				}
				else
				{
//					Toast.MakeText(this, "Ошибка при загрузке", ToastLength.Short).Show();
				}
			});
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
				DestroyPdfLoader ();
				NavigationController.PopToViewController (showController, true);
				showController.ShowFromAnotherScreen (Page.Trends);
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
				DestroyPdfLoader ();
				NavigationController.PopToViewController (showController, true);
				showController.ShowFromAnotherScreen (Page.News);
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
			DestroyPdfLoader ();
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

		}

		private void ButFavoriteOnClick(object sender, EventArgs eventArgs)
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
			DestroyPdfLoader ();
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

		private void OnPdfGetted(object sender, PdfEventArgs e)
		{

		}

		#endregion

		#region Logic

		//Инициализация панели журнала
		private void InitMagazine(Magazine magazine)
		{
			if (magazine == null) return;

		}

		private void LoadMagazineArticles()
		{
			if (_magazine == null) return;
			if (!ApplicationWorker.Settings.OfflineMode)
			{
				var connectAccept = IsConnectionAccept();
				if (!connectAccept)
				{
//					Toast.MakeText(this, "Нет доступных подключений, для указанных в настройках", ToastLength.Long)
//						.Show();
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
//								Toast.MakeText(this, "Непрочитанных статей меньше 6, будут выведены некоторые прочитанные статьи", ToastLength.Short).Show();
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
							if (_extendedObject != null)
							{
//								if (_extendedObject.Parent != null)
//								{
//									((LinearLayout)_extendedObject.Parent).RemoveAllViews();
//								}
								_articles.Insert(0,
									new Article() { ArticleType = ArticleType.ExtendedObject, ExtendedObject = _extendedObject });
							}
							var action = MagazineAction.NoAction;
							if (!_isRubricSearch)
							{
								action = _magazine.Exists ? MagazineAction.Open : MagazineAction.Download;
							}
							//Загрузить _articles в список
						}
						return;
					}
				}
				_isLoadingData = true;
				ApplicationWorker.RemoteWorker.MagazineArticlesGetted += OnMagazineArticlesGetted;
				ThreadPool.QueueUserWorkItem(
					state =>
					ApplicationWorker.RemoteWorker.BeginGetMagazineArticles(ApplicationWorker.Settings, _magazine.Id));
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
						ApplicationWorker.Db.UpdateArticle(article);
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
//				if (IsDoubleRow())
//				{
//					returnLst.Add(new Article() {ArticleType = ArticleType.Placeholder});
//				}
				var rubricId = rubric.Id;
				var articles = lst.Where(x => x.Rubrics.First().Id == rubricId).OrderBy(x => x.Sort);
				returnLst.AddRange(articles);
//				if (IsDoubleRow() && articles.Count()%2 != 0)
//				{
//					returnLst.Add(new Article() {ArticleType = ArticleType.Placeholder});
//				}
			}
			return returnLst;
		}

		private void SetLoadingProgressVisible(bool visible)
		{
		}

		public void ShowLastMagazine()
		{

		}

		public void DestroyPdfLoader()
		{
			var view = _extendedObject;
			ApplicationWorker.PdfLoader.PdfGetted -= OnPdfGetted;
			ApplicationWorker.PdfLoader.AbortOperation();
			SetLoadingProgressVisible(false);
//			var button = view.FindViewById<Button>(Resource.Id.getMagazine);
//			button.Enabled = true;
		}

		private bool IsConnectionAccept()
		{
			var result = true;
			return result;
		}

		#endregion
    }
}

