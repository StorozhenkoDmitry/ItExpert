﻿using System;
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
	public partial class ItExpertViewController : UIViewController
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
		private UIView _extendedObject = null;
		private UIView _addPreviousArticleButton = null;
		private bool _prevArticlesExists = true;
		private bool _startPage = false;
		private bool _headerAdded = false;
		private string _header = null;

		#endregion

		#region UIViewController members

		static bool UserInterfaceIdiomIsPhone {
			get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
		}

		public ItExpertViewController (IntPtr handle) : base (handle)
		{
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
		}

		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
		}

		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear (animated);
		}

		#endregion

		void Initialize()
		{
			_startPage = true;
			InitAddPreviousArticleButton ();
			var screenWidth =
				ApplicationWorker.Settings.GetScreenWidthForScreen((int)UIScreen.MainScreen.Bounds.Size.Width);
			ApplicationWorker.Settings.ScreenWidth = screenWidth;
			ApplicationWorker.Settings.SaveSettings();
			ApplicationWorker.Settings.LoadDetails = false;
			ApplicationWorker.RemoteWorker.BannerGetted += BannerGetted;
			ThreadPool.QueueUserWorkItem (state => ApplicationWorker.RemoteWorker.BeginGetBanner (ApplicationWorker.Settings));
			_isLoadingData = true;
			View.AutosizesSubviews = true;

            var topOffset = NavigationController.NavigationBar.Frame.Height + ItExpertHelper.StatusBarHeight;

            _articlesTableView = new UITableView(new RectangleF(0, topOffset, View.Bounds.Width, View.Bounds.Height- topOffset), UITableViewStyle.Plain);
			_articlesTableView.ScrollEnabled = true; 
			_articlesTableView.UserInteractionEnabled = true;
			_articlesTableView.SeparatorInset = new UIEdgeInsets (0, 0, 0, 0);
			_articlesTableView.Bounces = true;

			View.Add (_articlesTableView);
		}

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
				SetLoadingImageVisible(false);
			});
		}

		//Загрузка предыдущих статей
		private void AddPreviousArticleOnClick(object sender, EventArgs eventArgs)
		{
			if (_isLoadingData) return;
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
				SetLoadingImageVisible(true);
			}
			else
			{
				SetLoadingImageVisible(true);
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


		private void OnPushNewsDetails(object sender, PushNewsDetailsEventArgs e)
		{
			NavigationController.PushViewController (e.NewsDetailsView, true);
		}

		#endregion

		#region Activity logic

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

		//Создание кнопки Загрузить еще
		private void InitAddPreviousArticleButton()
		{
			//			var button = new Button(this);
			//			button.LayoutParameters = new ViewGroup.LayoutParams(Resources.DisplayMetrics.WidthPixels, ViewGroup.LayoutParams.WrapContent);
			//			button.SetText("Загрузить еще", TextView.BufferType.Normal);
			//			button.SetTextSize(ComplexUnitType.Sp, ApplicationWorker.Settings.HeaderSize);
			//			button.SetTextColor(Color.Argb(255, 140, 140, 140));
			//			button.SetBackgroundColor(ApplicationWorker.Settings.GetBackgroundColor());
			//			button.Click += AddPreviousArticleOnClick;
			//			_addPreviousArticleButton = button;

            _addPreviousArticleButton = new UIButton();
            (_addPreviousArticleButton as UIButton).TitleLabel.Text = "Загрузить еще";
            (_addPreviousArticleButton as UIButton).TitleLabel.TextAlignment = UITextAlignment.Center;
            (_addPreviousArticleButton as UIButton).TitleLabel.BackgroundColor = ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetBackgroundColor());
            (_addPreviousArticleButton as UIButton).BackgroundColor = ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetBackgroundColor());
		}

		//Инициализация баннера
		private void InitBanner(Banner banner)
		{
			var maxPictureHeight = UIScreen.MainScreen.Bounds.Size.Height * 0.15;
			var picture = banner.Picture;
			UIImageView bannerView = null;
			//Если баннер не анимированный Gif
			if (picture.Extension != PictureExtension.Gif)
			{
				var pictHeight = picture.Height;
				var pictWidth = picture.Width;

				if (pictHeight > maxPictureHeight)
				{
					var koef = pictHeight / maxPictureHeight;
					pictHeight = (int)maxPictureHeight;
					pictWidth = (int)(pictWidth / koef);
				}
				
                bannerView = new UIImageView(ItExpertHelper.GetImageFromBase64String(picture.Data));
			}
			else
			{
				var koefScaling = UIScreen.MainScreen.Bounds.Size.Width / picture.Width;
				var pictHeightScaling = picture.Height * koefScaling;

				if (pictHeightScaling > maxPictureHeight)
				{
					koefScaling = (float)maxPictureHeight / picture.Height;
				}

                bannerView = AnimatedImageView.GetAnimatedImageView (NSData.FromArray(Convert.FromBase64String(picture.Data)));
                bannerView.Frame = new RectangleF(0, 0, koefScaling * (picture.Width), koefScaling * (picture.Height));
			}
			//Прикрепить обработчик клика по баннеру
			_extendedObject = bannerView;
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
					//					Toast.MakeText(this, "Непрочитанных статей меньше 6, будут выведены некоторые прочитанные статьи",
					//						ToastLength.Short).Show();
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
			if (_extendedObject != null)
			{
				//				if (_extendedObject.Parent != null)
				//				{
				//					((LinearLayout)_extendedObject.Parent).RemoveAllViews();
				//				}
				_articles.Insert(0,
					new Article() {ArticleType = ArticleType.ExtendedObject, ExtendedObject = _extendedObject});
			}
			//Добавление кнопки Загрузить еще
			if (_addPreviousArticleButton != null && _prevArticlesExists)
			{
				//				if (_addPreviousArticleButton.Parent != null)
				//				{
				//					((LinearLayout)_addPreviousArticleButton.Parent).RemoveAllViews();
				//				}
				_articles.Add(new Article()
				{
					ArticleType = ArticleType.ExtendedObject,
					ExtendedObject = _addPreviousArticleButton
				});
			}
			if (_articlesTableView.Source != null) 
			{
				(_articlesTableView.Source as ArticlesTableSource).PushNewsDetails -= OnPushNewsDetails;

				_articlesTableView.Source.Dispose();
				_articlesTableView.Source = null;
			}

			ArticlesTableSource source = new ArticlesTableSource(_articles, false, MagazineAction.NoAction);
			source.PushNewsDetails += OnPushNewsDetails;

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
				//Перерисовать список

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
					//					Toast.MakeText(this, "Непрочитанных статей меньше 6, будут выведены некоторые прочитанные статьи", ToastLength.Short).Show();
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
			if (_extendedObject != null)
			{
				//				if (_extendedObject.Parent != null)
				//				{
				//					((LinearLayout)_extendedObject.Parent).RemoveAllViews();
				//				}
				_articles.Insert(0,
					new Article() { ArticleType = ArticleType.ExtendedObject, ExtendedObject = _extendedObject });
			}
			//Добавление кнопки Загрузить еще
			if (_addPreviousArticleButton != null && _prevArticlesExists)
			{
				//				if (_addPreviousArticleButton.Parent != null)
				//				{
				//					((LinearLayout)_addPreviousArticleButton.Parent).RemoveAllViews();
				//				}
				_articles.Add(new Article()
				{
					ArticleType = ArticleType.ExtendedObject,
					ExtendedObject = _addPreviousArticleButton
				});
			}
			//Перерисовать список

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

		}

		#endregion
	}
}

