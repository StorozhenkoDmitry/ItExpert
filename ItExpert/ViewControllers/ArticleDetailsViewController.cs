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
		private UIPageViewController _pageController;

		public ArticleDetailsViewController (Article article, List<int> articlesId, MagazineAction magazineAction)
		{
			_article = article;
			_magazineAction = magazineAction;
			_articlesId = articlesId;
			_authors.Clear();
			_authors.AddRange(article.Authors.Select(x => x.Name));
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
		}

		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate(fromInterfaceOrientation);
			_pageController.View.Frame = View.Frame;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			ApplicationWorker.SettingsChanged -= OnSettingsChanged;
		}

		void OnSettingsChanged (object sender, EventArgs e)
		{
			var currentController = _pageController.ViewControllers[0] as ArticleDetailContentViewController;
			if (currentController != null)
			{
				currentController.OnSettingsChanged();
			}
		}

		void Exit()
		{
			if (UIApplication.SharedApplication.KeyWindow.RootViewController is UINavigationController)
			{
				(UIApplication.SharedApplication.KeyWindow.RootViewController as UINavigationController).PopViewControllerAnimated(true);
			}
			Dispose();
		}

		void Initialize()
		{
            InitNavigationBar();

			_pageController = new UIPageViewController(UIPageViewControllerTransitionStyle.Scroll, UIPageViewControllerNavigationOrientation.Horizontal, UIPageViewControllerSpineLocation.Min);
			_pageController.View.BackgroundColor = ItExpertHelper.GetUIColorFromColor (ApplicationWorker.Settings.GetBackgroundColor ());
			_pageController.View.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			_pageController.View.Frame = View.Frame;
			_pageController.GetNextViewController = GetNextPageViewController;
			_pageController.GetPreviousViewController = GetPreviousPageViewController;
			AddChildViewController(_pageController);
			View.Add(_pageController.View);
			var startController = new ArticleDetailContentViewController(_article.Id, this, NavigationController.NavigationBar.Frame.Height, View.Frame);
			_pageController.SetViewControllers(
				new UIViewController[] { startController }, 
				UIPageViewControllerNavigationDirection.Forward, 
				false,
				null);
		}

		private UIViewController GetPreviousPageViewController(UIPageViewController pageController, UIViewController referenceViewController)
		{		
			var controller = referenceViewController as ArticleDetailContentViewController;
			if (controller != null)
			{
				var currentIndex = _articlesId.FindIndex(x => x == controller.GetArticleId());
				var searchElemIndex = currentIndex - 1;
				if (searchElemIndex < 0)
				{
					searchElemIndex = _articlesId.Count() - 1;
				}
				if (searchElemIndex > _articlesId.Count() - 1)
				{
					searchElemIndex = 0;
				}
				var articleId = _articlesId[searchElemIndex];
				_article = ApplicationWorker.GetArticle(articleId);
				return new ArticleDetailContentViewController(articleId, this, NavigationController.NavigationBar.Frame.Height, View.Frame);
			}
			return null;
		}

		private UIViewController GetNextPageViewController(UIPageViewController pageController, UIViewController referenceViewController)
		{	
			var controller = referenceViewController as ArticleDetailContentViewController;
			if (controller != null)
			{
				var currentIndex = _articlesId.FindIndex(x => x == controller.GetArticleId());
				var searchElemIndex = currentIndex + 1;
				if (searchElemIndex < 0)
				{
					searchElemIndex = _articlesId.Count() - 1;
				}
				if (searchElemIndex > _articlesId.Count() - 1)
				{
					searchElemIndex = 0;
				}
				var articleId = _articlesId[searchElemIndex];
				_article = ApplicationWorker.GetArticle(articleId);
				return new ArticleDetailContentViewController(articleId, this, NavigationController.NavigationBar.Frame.Height, View.Frame);
			}
			return null;
		}

        private void InitNavigationBar()
        {
            UIBarButtonItem spaceForBack = new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace);

            spaceForBack.Width = -6;

            UIBarButtonItem spaceForLogo = new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace);

            spaceForLogo.Width = 20;

            NavigationItem.LeftBarButtonItems = new UIBarButtonItem[] { spaceForBack, NavigationBarButton.Back, spaceForLogo, NavigationBarButton.Logo };           

            UIBarButtonItem spaceForSettings = new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace);

            spaceForSettings.Width = -10;

			NavigationItem.RightBarButtonItems = new UIBarButtonItem[] { spaceForSettings, NavigationBarButton.GetSettingsButton(true), NavigationBarButton.Share };
        }

		#region Worked with server

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
				if (_article.ArticleType  == ArticleType.Magazine && _magazineAction == MagazineAction.Open)
				{
					ShowShowPdfDialog ();
					return;
				}
				ShowNotFindArticleDialog();
			}
		}

		private void OnMagazineArticleDetailGetted(object sender, MagazineArticleDetailEventArgs e)
		{
			ApplicationWorker.RemoteWorker.MagazineArticleDetailGetted -= OnMagazineArticleDetailGetted;
			if (e.Abort)
			{
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
						_authors.Clear();
						_authors.AddRange(article.Authors.Select(x => x.Name));
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
						var currentController = _pageController.ViewControllers[0] as ArticleDetailContentViewController;
						if (currentController != null)
						{
							currentController.SetArticle(_article);
						}
					});
				}
			}
			else
			{
				InvokeOnMainThread(() => BTProgressHUD.ShowToast("Ошибка при загрузке", ProgressHUD.MaskType.None, false, 2500));
			}
		}

		private void OnArticleDetailGetted(object sender, ArticleDetailEventArgs e)
		{
			ApplicationWorker.RemoteWorker.ArticleDetailGetted -= OnArticleDetailGetted;
			if (e.Abort)
			{
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
						_authors.Clear();
						_authors.AddRange(article.Authors.Select(x => x.Name));
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
						var currentController = _pageController.ViewControllers[0] as ArticleDetailContentViewController;
						if (currentController != null)
						{
							currentController.SetArticle(_article);
						}
					});
				}
			}
			else
			{
				InvokeOnMainThread(() => BTProgressHUD.ShowToast("Ошибка при загрузке", ProgressHUD.MaskType.None, false, 2500));
			}
		}

		#endregion

		#region Logic

		#region Filter articles

		public void ShowFilterSectionDialog(string sectionString)
		{
			if (!ApplicationWorker.Settings.OfflineMode)
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
			if (!ApplicationWorker.Settings.OfflineMode)
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

		private void FilterSection()
		{
			OnDestroy ();
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
					NavigationController.PopToViewController (showController, true);
					showController.FilterSection (sectionId, blockId);
				}
				else
				{
					var filterParams = new FilterParameters() 
					{
						Filter = Filter.Section,
						SectionId = sectionId,
						BlockId = blockId
					};
					showController = new NewsViewController(filterParams);
					NavigationController.PushViewController(showController, true);
				}
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
					NavigationController.PopToViewController(showController, true);
					showController.SearchRubric(sectionId);
				}
				else
				{
					var filterParams = new FilterParameters() {
						Filter = Filter.Section,
						SectionId = sectionId,
					};
					showController = new MagazineViewController(filterParams);
					NavigationController.PushViewController(showController, true);
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
			OnDestroy ();
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
				NavigationController.PopToViewController(showController, true);
				showController.FilterAuthor(sectionId, blockId, authorId);
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
				showController = new NewsViewController(filterParams);
				NavigationController.PushViewController(showController, true);
			}
		}

		public void ArticleDetailTextNotAvailable(Article article)
		{
			if (article.ArticleType == ArticleType.Magazine && _magazineAction == MagazineAction.Open)
			{
				ShowShowPdfDialog();
				return;
			}
			if (article.ArticleType == ArticleType.Magazine && _magazineAction == MagazineAction.Download)
			{
				ShowLoadPdfDialog();
				return;
			}
			ShowNotFindArticleDialog();
			return;
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
					}
					alertView.Dispose();
				}
			};

			alertView.Show ();
		}

		private void ShowNotFindArticleDialog()
		{
			var alert = new BlackAlertView ("Нет детальной статьи", "Не найдена детальная статья в кэше", "Ok");
			alert.ButtonPushed += (sender, e) =>
			{
				Exit();
				alert.Dispose();
			};
			alert.Show();
		}

		#endregion


		private void OnDestroy()
		{
			ApplicationWorker.SettingsChanged -= OnSettingsChanged;
			_articlesId = null;
			_authors = null;
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
        
		private Article _article;
		private MagazineAction _magazineAction;
		private List<int> _articlesId;
		private List<string> _authors = new List<string>();


	}
}

