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
		private UIButton _backButton;
		private UIBarButtonItem _backButtonBar;
		private SettingsView _settingsView;
		private UIButton _settingsButton;
		private UIBarButtonItem _settingsBarButton;
		private ShareView _shareView;
		private UIButton _shareButton;
		private UIBarButtonItem _shareBarButton;

		public ArticleDetailsViewController (Article article, List<int> articlesId, MagazineAction magazineAction)
		{
			_article = article;
			_magazineAction = magazineAction;
			_articlesId = articlesId;
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
			InvokeOnMainThread(() =>
			{
				if (_backButton != null)
				{
					_backButton.RemoveFromSuperview();
					if (_backButton.ImageView != null && _backButton.ImageView.Image != null)
					{
						_backButton.ImageView.Image.Dispose();
						_backButton.ImageView.Image = null;
					}
					_backButton.TouchUpInside -= BackButtonTouchUp;
					_backButton.Dispose();
				}

				if (_backButtonBar != null)
				{
					_backButtonBar.Dispose();
				}
					
				_backButton = null;
				_backButtonBar = null;

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

				if (_shareButton != null)
				{
					_shareButton.RemoveFromSuperview();
					if (_shareButton.ImageView != null && _shareButton.ImageView.Image != null)
					{
						_shareButton.ImageView.Image.Dispose();
						_shareButton.ImageView.Image = null;
					}
					_shareButton.TouchUpInside -= ShareButtonTouchUp;
					_shareButton.Dispose();
				}
				_shareButton = null;

				if (_shareBarButton != null)
				{
					_shareBarButton.Dispose();
				}
				_shareBarButton = null;

				if (_shareView != null)
				{
					_shareView.TapOutsideTableView -= ViewTapOutsideTableView;
					_shareView.Dispose();
				}
				_shareView = null;

				if (_pageController != null)
				{
					_pageController.GetNextViewController = null;
					_pageController.GetPreviousViewController = null;
					_pageController.Dispose();
				}
				_pageController = null;

				ApplicationWorker.SettingsChanged -= OnSettingsChanged;
			});
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
			NavigationController.PopViewControllerAnimated(true);
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

			_backButton = NavigationBarButton.GetButton("NavigationBar/Back.png", 2);
			_backButton.TouchUpInside += BackButtonTouchUp;
			_backButtonBar = new UIBarButtonItem(_backButton);

			NavigationItem.LeftBarButtonItems = new UIBarButtonItem[] { spaceForBack, _backButtonBar, spaceForLogo, NavigationBarButton.Logo };           

            UIBarButtonItem spaceForSettings = new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace);

            spaceForSettings.Width = -10;

			_settingsButton = NavigationBarButton.GetButton("NavigationBar/Settings.png", 4.1f);
			_settingsBarButton = new UIBarButtonItem(_settingsButton);
			_settingsView = new SettingsView(true);
			_settingsView.TapOutsideTableView += ViewTapOutsideTableView;
			_settingsButton.TouchUpInside += SettingsButtonTouchUp;


			_shareButton = NavigationBarButton.GetButton("NavigationBar/Share.png", 4.5f);
			_shareBarButton = new UIBarButtonItem(_shareButton);
			_shareView = new ShareView();
			_shareView.TapOutsideTableView += ViewTapOutsideTableView;
			_shareButton.TouchUpInside += ShareButtonTouchUp;

			NavigationItem.RightBarButtonItems = new UIBarButtonItem[] { spaceForSettings, _settingsBarButton, _shareBarButton };
        }

		void BackButtonTouchUp(object sender, EventArgs e)
		{
			Exit();
		}

		void ViewTapOutsideTableView(object sender, EventArgs e)
		{
			NavigationBarButton.HideWindow();
		}

		void SettingsButtonTouchUp(object sender, EventArgs e)
		{
			NavigationBarButton.ShowWindow(_settingsView);
		}

		void ShareButtonTouchUp(object sender, EventArgs e)
		{
			NavigationBarButton.ShowWindow(_shareView);
			_shareView.Update();
		}

		#region Logic

		#region Filter articles

		public void SetArticle(Article article)
		{
			_article = article;
		}

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
					NavigationController.PopToViewController (showController, false);
					showController.FilterSection (sectionId, blockId);
					Dispose();
				}
				else
				{
					var filterParams = new FilterParameters() 
					{
						Filter = Filter.Section,
						SectionId = sectionId,
						BlockId = blockId
					};
					NavigationController.DismissViewController(false, null);
					showController = new NewsViewController(filterParams);
					NavigationController.PushViewController(showController, false);
					Dispose();
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
					NavigationController.PopToViewController(showController, false);
					showController.SearchRubric(sectionId);
				}
				else
				{
					var filterParams = new FilterParameters() {
						Filter = Filter.Section,
						SectionId = sectionId,
					};
					NavigationController.DismissViewController(false, null);
					showController = new MagazineViewController(filterParams);
					NavigationController.PushViewController(showController, false);
					Dispose();
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
				NavigationController.PopToViewController(showController, false);
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
				NavigationController.DismissViewController(false, null);
				showController = new NewsViewController(filterParams);
				NavigationController.PushViewController(showController, false);
				Dispose();
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
				if (!ApplicationWorker.Settings.OfflineMode)
				{
					ShowLoadPdfDialog();
					return;
				}
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
						Dispose();
					}
					else
					{
						Exit();
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
						Dispose();
					}
					else
					{
						Exit();
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
		}

		#endregion
        
		private Article _article;
		private MagazineAction _magazineAction;
		private List<int> _articlesId;


	}
}

