﻿using System;
using MonoTouch.UIKit;
using System.Drawing;
using ItExpert.Enum;
using ItExpert.Model;
using System.Collections.Generic;
using ItExpert.ServiceLayer;
using System.Linq;
using System.Threading;
using System.IO;

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
		private UIView _banner = null;
		private bool _lastMagazine = false;
		private UIView _addPreviousArticleButton = null;
		private bool _prevArticlesExists = true;
		private bool _headerAdded = false;
		private string _header = null;
		private BottomToolbarView _bottomBar = null;
		private int _magazineId = -1;
        private UITableView _articlesTableView;
		private UIActivityIndicatorView _loadingIndicator;
		public bool IsLoadingPdf = false;

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

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			if (_articlesTableView != null)
			{
				_articlesTableView.DeselectRow(_articlesTableView.IndexPathForSelectedRow, true);
				_articlesTableView.ReloadData ();
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

		#endregion

		#region Init

		public void SetMagazineId(int magazineId)
		{
			_magazineId = magazineId;
			_isRubricSearch = false;
			_searchRubric = null;
			_headerAdded = false;
			_header = null;
			_prevArticlesExists = true;
			if (_articles != null)
			{
				_articles.Clear();
			}
			_articlesTableView.ReloadData ();
			InitData ();
		}

		public void Initialize()
		{
			Current = this;
			View.BackgroundColor = ItExpertHelper.GetUIColorFromColor (ApplicationWorker.Settings.GetBackgroundColor ());

			InitBottomToolbar ();
			InitLoadingProgress ();
			InitAddPreviousArticleButton ();

            var topOffset = NavigationController.NavigationBar.Frame.Height + ItExpertHelper.StatusBarHeight;

            _articlesTableView = new UITableView(new RectangleF(0, topOffset, View.Bounds.Width, 
                View.Bounds.Height- topOffset - _bottomBar.Frame.Height), UITableViewStyle.Plain);

            _articlesTableView.ScrollEnabled = true; 
            _articlesTableView.UserInteractionEnabled = true;
            _articlesTableView.SeparatorInset = new UIEdgeInsets (0, 0, 0, 0);
            _articlesTableView.Bounces = true;
            _articlesTableView.SeparatorColor = UIColor.FromRGB(100, 100, 100);

            View.Add(_articlesTableView);

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

		private void InitData()
		{
			SetLoadingImageVisible (true);
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
					Magazine magazine = ApplicationWorker.Db.GetNewestSavedMagazine();
					if (magazine != null)
					{
						_magazine = magazine;
						InitMagazine(magazine);
						LoadMagazineArticles();
					}
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

						var action = MagazineAction.NoAction;
						if (!_isRubricSearch)
						{
							action = _magazine.Exists ? MagazineAction.Open : MagazineAction.Download;
						}
						//Загрузить _articles в список

                        if (_articles != null && _articles.Any())
                        {
                            if (_articlesTableView.Source != null) 
                            {
                                (_articlesTableView.Source as ArticlesTableSource).PushDetailsView -= OnPushArticleDetails;
                                _articlesTableView.Source.Dispose();
                                _articlesTableView.Source = null;
                            }
							var source = new ArticlesTableSource(_articles, false, action);
                            source.PushDetailsView += OnPushArticleDetails;
                            _articlesTableView.Source = source;
                            _articlesTableView.ReloadData();
                        }
					}
				}
				else
				{
//					Toast.MakeText(this, "Ошибка при загрузке", ToastLength.Short).Show();
				}
				SetLoadingImageVisible (false);
			});
		}

		private void AddPreviousArticleOnClick(object sender, EventArgs eventArgs)
		{
			if ((!_isRubricSearch && _searchRubric == null) || ApplicationWorker.Settings.OfflineMode) return;
			if (_isLoadingData) return;
			var connectAccept = IsConnectionAccept();
			if (!connectAccept)
			{
//				Toast.MakeText(this, "Нет доступных подключений, для указанных в настройках", ToastLength.Long).Show();
				return;
			}
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
					if (!e.Articles.Any())
					{
//						Toast.MakeText(this, "Больше статей нет", ToastLength.Short).Show();
						_prevArticlesExists = false;
						if (_addPreviousArticleButton != null && _articles.Any())
						{
							_articles.RemoveAt(_articles.Count() - 1);
						}
						_articlesTableView.ReloadData ();
						return;
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
							var action = MagazineAction.NoAction;
							if (!_isRubricSearch)
							{
								action = _magazine.Exists ? MagazineAction.Open : MagazineAction.Download;
							}
							if (_articles != null && _articles.Any())
							{
								if (_articlesTableView.Source != null) 
								{
									(_articlesTableView.Source as ArticlesTableSource).PushDetailsView -= OnPushArticleDetails;
									_articlesTableView.Source.Dispose();
									_articlesTableView.Source = null;
								}
								var source = new ArticlesTableSource(_articles, false, action);
								source.PushDetailsView += OnPushArticleDetails;
								_articlesTableView.Source = source;
								_articlesTableView.ReloadData();
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
							if (_articlesTableView != null)
							{
								_articlesTableView.ReloadData();
							}
							if (ApplicationWorker.Settings.HideReaded)
							{
								//Прокрутить к position
							}
						}
					}
				}
				else
				{
//					Toast.MakeText(this, "Ошибка при загрузке", ToastLength.Short).Show();
				}
				SetLoadingImageVisible (false);
			});
		}


		private void OnPushArticleDetails(object sender, PushDetailsEventArgs e)
		{
			NavigationController.PushViewController (e.NewsDetailsView, true);
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
			DestroyPdfLoader ();
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
			DestroyPdfLoader ();
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
			if (ApplicationWorker.Settings.OfflineMode) return;
			if (_isRubricSearch && !_isLoadingData && _magazine != null)
			{
				_isRubricSearch = false;
				_searchRubric = null;
				_headerAdded = false;
				_header = null;
				_prevArticlesExists = true;
				LoadMagazineArticles();
			}
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
			ApplicationWorker.PdfLoader.PdfGetted -= OnPdfGetted;
			if (e.Abort)
			{
				InvokeOnMainThread(() =>
				{
					IsLoadingPdf = false;
					if (_articlesTableView != null)
					{
						_articlesTableView.ReloadData ();
					}
				});
				return;
			}
			var error = e.Error;
			InvokeOnMainThread(() =>
			{
				if (!error)
				{
					var folder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
					var dir = new DirectoryInfo(folder + Settings.PdfFolder);
					if (!dir.Exists)
					{
						dir.Create();
					}
					var fileName = _magazine.Id.ToString("G") + ".pdf";
					var path = Path.Combine(folder + Settings.PdfFolder, fileName);
					var fs = File.Create(path);
					fs.Write(e.Pdf, 0, e.Pdf.Length);
					fs.Flush();
					fs.Close();
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
							(_articlesTableView.Source as ArticlesTableSource).SetMagazineAction(MagazineAction.Open);
						}
					}
				}
				else
				{
//					Toast.MakeText(this, "Ошибка при запросе", ToastLength.Short).Show();
				}
				IsLoadingPdf = false;
				if (_articlesTableView != null)
				{
					_articlesTableView.ReloadData ();
				}
			});
		}

		#endregion

		#region Logic

		public void OpenMagazinePdf()
		{
			var folder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			var fileName = _magazine.Id.ToString("G") + ".pdf";
			var path = Path.Combine(folder + Settings.PdfFolder, fileName);
			if (!File.Exists(path))
			{
				//Toast.MakeText(this, "Файл не найден", ToastLength.Long).Show();
				return;
			}
			PdfViewController showController = null;
			var controllers = NavigationController.ViewControllers;
			foreach (var controller in controllers)
			{
				showController = controller as PdfViewController;
				if (showController != null)
				{
					break;
				}
			}
			DestroyPdfLoader ();
			if (showController != null)
			{
				NavigationController.PopToViewController (showController, true);
				showController.ShowPdf (path);
			}
			else
			{
				showController = new PdfViewController (path);
				NavigationController.PushViewController (showController, true);
			}
		}

		public void DownloadMagazinePdf()
		{
			if (ApplicationWorker.Settings.OfflineMode)
			{
//				Toast.MakeText(this, "Загрузка Pdf невозможна в оффлайн режиме", ToastLength.Long).Show();
				return;
			}
			if (string.IsNullOrWhiteSpace(_magazine.PdfFileSrc))
			{
//				Toast.MakeText(this, "Pdf файл недоступен", ToastLength.Long).Show();
				return;
			}
			if (!ApplicationWorker.PdfLoader.IsOperation())
			{
				var connectAccept = IsConnectionAccept();
				if (!connectAccept)
				{
//					Toast.MakeText(this, "Нет доступных подключений, для указанных в настройках", ToastLength.Long).Show();
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
//				Toast.MakeText(this, "Идет загрузка... Дождитесь завершения", ToastLength.Short).Show();
			}
		}

		public void SearchRubric(int rubricId)
		{
			if (rubricId != -1)
			{
				if (ApplicationWorker.Settings.OfflineMode)
				{
//					Toast.MakeText(this, "Поиск невозможен в оффлайн режиме", ToastLength.Long).Show();
					return;
				}
				var rubric =
					_articles.Where(
						x =>
						x.ArticleType == ArticleType.Magazine &&
						x.Rubrics != null)
						.SelectMany(x => x.Rubrics)
						.FirstOrDefault(x => x.Id == rubricId);
				if (rubric != null)
				{
					var connectAccept = IsConnectionAccept();
					if (!connectAccept)
					{
//						Toast.MakeText(this, "Нет доступных подключений, для указанных в настройках", ToastLength.Long)
//							.Show();
						return;
					}
					_headerAdded = true;
					_header = rubric.Name;
					_articles.Clear();
					_allArticles.Clear();
					if (_articlesTableView != null)
					{
						_articlesTableView.ReloadData ();
					}
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
				}
			}
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
			var folder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			var fileName = magazine.Id.ToString("G") + ".pdf";
			var path = System.IO.Path.Combine(folder + Settings.PdfFolder, fileName);
			var file = new FileInfo(path);
			magazine.Exists = file.Exists;
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
                            if (_banner != null)
							{
								_articles.Insert(0,
                                    new Article() { ArticleType = ArticleType.Banner, ExtendedObject = _banner });
							}
							if (ApplicationWorker.Magazine != null)
							{
								_articles.Insert(0, new Article() { ArticleType = ArticleType.MagazinePreview });
							}
							var action = MagazineAction.NoAction;
							if (!_isRubricSearch)
							{
								action = _magazine.Exists ? MagazineAction.Open : MagazineAction.Download;
							}
							//Загрузить _articles в список
							if (_articles != null && _articles.Any())
							{
								if (_articlesTableView.Source != null) 
								{
									(_articlesTableView.Source as ArticlesTableSource).PushDetailsView -= OnPushArticleDetails;
									_articlesTableView.Source.Dispose();
									_articlesTableView.Source = null;
								}
								var source = new ArticlesTableSource(_articles, false, action);
								source.PushDetailsView += OnPushArticleDetails;
								_articlesTableView.Source = source;
								_articlesTableView.ReloadData();
							}
							SetLoadingImageVisible (false);
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
			else
			{
				Action action = () =>
				{
					InvokeOnMainThread(() =>
					{
						SetLoadingImageVisible(true);
						var magAction = MagazineAction.NoAction;
						if (!_isRubricSearch)
						{
							magAction = _magazine.Exists ? MagazineAction.Open : MagazineAction.Download;
						}
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
								var magAction = MagazineAction.NoAction;
								if (!_isRubricSearch)
								{
									magAction = _magazine.Exists ? MagazineAction.Open : MagazineAction.Download;
								}
								if (_articles != null && _articles.Any())
								{
									if (_articlesTableView.Source != null) 
									{
										(_articlesTableView.Source as ArticlesTableSource).PushDetailsView -= OnPushArticleDetails;
										_articlesTableView.Source.Dispose();
										_articlesTableView.Source = null;
									}
									var source = new ArticlesTableSource(_articles, false, magAction);
									source.PushDetailsView += OnPushArticleDetails;
									_articlesTableView.Source = source;
									_articlesTableView.ReloadData();
								}
								SetLoadingImageVisible(false);
							});
						}
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



		public void ShowLastMagazine()
		{

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

