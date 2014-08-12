using System;
using MonoTouch.UIKit;
using ItExpert.Model;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using ItExpert.ServiceLayer;
using System.Drawing;
using System.IO;
using MonoTouch.QuickLook;
using ItExpert.Enum;
using MonoTouch.Foundation;
using mTouchPDFReader.Library.Views.Core;
using BigTed;

namespace ItExpert
{
    public class ArchiveViewController: UIViewController
    {
		#region Fields

		private int _currentYear = -1;
		private MagazineView _downloadMagazineView;
		private List<Magazine> _magazines = null; 
        private YearsView _yearsView;
        private ArchiveView _archiveView;
		private UIActivityIndicatorView _loadingIndicator;
		private UIActivityIndicatorView _loadingPdfIndicator;
		private bool _dataLoaded = false;
		private bool _isLoading = false;
		private MenuView _menu;
		private UIButton _menuButton;
		private UIBarButtonItem _menuBarButton;
		private SettingsView _settingsView;
		private UIButton _settingsButton;
		private UIBarButtonItem _settingsBarButton;
		private List<UIButton> _yearButtons;

		#endregion

		#region UIViewController members

		static bool UserInterfaceIdiomIsPhone {
			get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
		}

        public ArchiveViewController()
        {
        }

		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate (fromInterfaceOrientation);

            if (fromInterfaceOrientation == UIInterfaceOrientation.Portrait || fromInterfaceOrientation == UIInterfaceOrientation.PortraitUpsideDown)
            {
                _yearsView.Frame = new RectangleF(Math.Max(0, View.Bounds.Width / 2 - _yearsView.GetButtonsWidth() / 2), NavigationController.NavigationBar.Frame.Height + ItExpertHelper.StatusBarHeight,
                    View.Frame.Width, 40);
            }
            else
            {
                _yearsView.Frame = new RectangleF(0, NavigationController.NavigationBar.Frame.Height + ItExpertHelper.StatusBarHeight,
                    View.Frame.Width, 40);
            }

			var height = 50;
			if (_loadingPdfIndicator != null)
			{
				_loadingPdfIndicator.Frame = new RectangleF (0, 100, View.Bounds.Width, height);
			}

			if (_loadingIndicator != null)
			{
				_loadingIndicator.Frame = new RectangleF (0, View.Bounds.Height - height, View.Bounds.Width, height);
			}

            _archiveView.Frame = new RectangleF(0, _yearsView.Frame.Bottom, View.Frame.Width, View.Frame.Height - _yearsView.Frame.Bottom);

			if (_magazines != null && !_isLoading)
			{
            	_archiveView.AddMagazineViews(_magazines);
			}
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
		}

		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);
			if (!_dataLoaded)
			{
				_dataLoaded = true;
				Action action = () =>
				{
					Thread.Sleep(150);
					InvokeOnMainThread(() => Initialize());
				};
				ThreadPool.QueueUserWorkItem(state => action());
			}
		}

		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
			ApplicationWorker.RemoteWorker.MagazinesPriviewGetted -= OnMagazinesYearsGetted;
			ApplicationWorker.RemoteWorker.MagazinesPriviewGetted -= OnMagazinesPriviewGetted;
			_isLoading = false;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			InvokeOnMainThread(() =>
			{
				DestroyPdfLoader();
				ApplicationWorker.RemoteWorker.MagazinesPriviewGetted -= OnMagazinesYearsGetted;
				ApplicationWorker.RemoteWorker.MagazinesPriviewGetted -= OnMagazinesPriviewGetted;
				ApplicationWorker.SettingsChanged -= OnSettingsChanged;
				ApplicationWorker.AllPdfFilesDeleted -= OnAllPdfFilesDeleted;
				_isLoading = false;

				if (_yearButtons != null)
				{
					foreach (var button in _yearButtons)
					{
						button.TouchUpInside -= ButtonYearOnClick;
					}
					_yearButtons.Clear();
				}
				_yearButtons = null;

				if (_yearsView != null)
				{
					_yearsView.RemoveFromSuperview();
					_yearsView.Dispose();
				}
				_yearsView = null;

				if (_archiveView != null)
				{
					_archiveView.RemoveFromSuperview();
					_archiveView.MagazinePushed -= OnMagazinePushed;
					_archiveView.MagazineDelete -= OnMagazineDelete;
					_archiveView.MagazineOpen -= OnMagazineOpen;
					_archiveView.MagazineDownload -= OnMagazineDownload;
					_archiveView.Dispose();
				}
				_archiveView = null;

				if (_loadingIndicator != null)
				{
					_loadingIndicator.RemoveFromSuperview();
					_loadingIndicator.Dispose();
				}
				_loadingIndicator = null;

				if (_loadingPdfIndicator != null)
				{
					_loadingPdfIndicator.RemoveFromSuperview();
					_loadingPdfIndicator.Dispose();
				}
				_loadingPdfIndicator = null;

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
			});
		}

		#endregion

		#region Init

		void Initialize()
		{
            AutomaticallyAdjustsScrollViewInsets = false;
			InitLoadingProgress ();
			InitLoadingPdfProgress ();
			ApplicationWorker.PdfLoader.PdfGetted += OnPdfGetted;
			ApplicationWorker.SettingsChanged += OnSettingsChanged;
			ApplicationWorker.AllPdfFilesDeleted += OnAllPdfFilesDeleted;

            InitNavigationBar();

			if (_yearsView != null)
			{
				_yearsView.RemoveFromSuperview ();
			}
				
			if (_archiveView != null)
			{
				_archiveView.RemoveFromSuperview ();
			}

            _yearsView = new YearsView(new RectangleF(0, NavigationController.NavigationBar.Frame.Height + ItExpertHelper.StatusBarHeight,
                View.Frame.Width, 40));

            _archiveView = new ArchiveView(new RectangleF(0, _yearsView.Frame.Bottom, View.Frame.Width, View.Frame.Height - _yearsView.Frame.Bottom));

            _archiveView.MagazinePushed += OnMagazinePushed;
			_archiveView.MagazineDelete += OnMagazineDelete;
			_archiveView.MagazineOpen += OnMagazineOpen;
			_archiveView.MagazineDownload += OnMagazineDownload;
            View.Add(_yearsView);
            View.Add(_archiveView);

			View.BackgroundColor = ItExpertHelper.GetUIColorFromColor (ApplicationWorker.Settings.GetBackgroundColor ());
			SetLoadingProgressVisible(false);
			InitData ();
		}

		void InitData()
		{
			if (!ApplicationWorker.Settings.OfflineMode)
			{
				var connectAccept = IsConnectionAccept();
				if (!connectAccept)
				{
					BTProgressHUD.ShowToast ("Нет доступных подключений, для указанных в настройках", ProgressHUD.MaskType.None, false, 2500);
					return;
				}
				_isLoading = true;
				SetLoadingImageVisible (true);
				ApplicationWorker.RemoteWorker.MagazinesPriviewGetted += OnMagazinesYearsGetted;
				ThreadPool.QueueUserWorkItem(
					state => ApplicationWorker.RemoteWorker.BeginGetMagazinesPreview(ApplicationWorker.Settings, -1));
			}
			else
			{
				var years = ApplicationWorker.Db.LoadMagazineYears();
				if (years != null && years.Any())
				{
					List<Magazine> previewList = null;
					years = years.OrderByDescending(x => x.Value).ToList();
					_yearButtons = new List<UIButton>();
					for (var i = 0; i < years.Count(); i++)
					{
						var year = years[i];
						var button = new UIButton(UIButtonType.Custom);
						button.TouchUpInside += ButtonYearOnClick;
						button.SetTitle(year.Value.ToString(), UIControlState.Normal);
						button.Tag = year.Value;
						_yearButtons.Add(button);
					}
					_yearsView.AddButtons(_yearButtons);
					_currentYear = years.First().Value;
					previewList = ApplicationWorker.Db.GetMagazinesByYear(_currentYear, true);
					if (previewList != null && previewList.Any() && !previewList.Any(x => x.PreviewPicture == null))
					{
						UpdateMagazinesPdfExists(previewList, _currentYear);
						previewList = previewList.OrderByDescending(x => x.ActiveFrom).ToList();
						_magazines = previewList;
						_archiveView.AddMagazineViews(_magazines);
					}
				}
			}
		}

		void InitLoadingProgress()
		{
			var height = 50;
			_loadingIndicator = new UIActivityIndicatorView (
				new RectangleF (0, View.Bounds.Height - height, View.Bounds.Width, height));
			_loadingIndicator.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.WhiteLarge;
			_loadingIndicator.Color = UIColor.Blue;
			_loadingIndicator.BackgroundColor = ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetBackgroundColor());
			View.Add (_loadingIndicator);
			_loadingIndicator.Hidden = true;
		}

		void InitLoadingPdfProgress()
		{
			var height = 50;
			_loadingPdfIndicator = new UIActivityIndicatorView (
				new RectangleF (0, 100, View.Bounds.Width, height));
			_loadingPdfIndicator.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.WhiteLarge;
			_loadingPdfIndicator.Color = UIColor.Red;
			_loadingPdfIndicator.BackgroundColor = ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetBackgroundColor());
			View.Add (_loadingPdfIndicator);
			_loadingPdfIndicator.Hidden = true;
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
			DestroyPdfLoader();
			_isLoading = false;
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

		void OnAllPdfFilesDeleted(object sender, EventArgs e)
		{
			if (_magazines != null)
			{
				UpdateMagazinesPdfExists(_magazines, _currentYear);
				_archiveView.AddMagazineViews(_magazines);
			}
		}

		void OnSettingsChanged (object sender, EventArgs e)
		{
			View.BackgroundColor = ItExpertHelper.GetUIColorFromColor (ApplicationWorker.Settings.GetBackgroundColor ());
			_loadingIndicator.BackgroundColor = ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetBackgroundColor());
			_loadingPdfIndicator.BackgroundColor = ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetBackgroundColor());
			_archiveView.AddMagazineViews(_magazines);
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
			DestroyPdfLoader();
			_isLoading = false;
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
			_isLoading = false;
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
			DestroyPdfLoader ();
			_isLoading = false;
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
			DestroyPdfLoader ();
			_isLoading = false;
			if (showController != null)
			{
				NavigationController.PopToViewController (showController, false);
				showController.SetMagazineId (-1);
				Dispose();
			}
			else
			{
				showController = new MagazineViewController(-1);
				NavigationController.PushViewController(showController, false);
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
			_isLoading = false;
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

		private void OnMagazinesYearsGetted(object sender, MagazinesPreviewEventArgs e)
		{
			ApplicationWorker.RemoteWorker.MagazinesPriviewGetted -= OnMagazinesYearsGetted;
			_isLoading = false;
			if (e.Abort)
			{
				return;
			}
			var error = e.Error;
			InvokeOnMainThread(() =>
			{
				if (!error)
				{
					var years = e.Years;

					if (years != null && years.Any())
					{
						years = years.OrderByDescending(x => x.Value).ToList();
                        _yearButtons = new List<UIButton>();
						for (var i = 0; i < years.Count(); i++)
						{
							var year = years[i];

                            UIButton button = new UIButton(UIButtonType.Custom);

                            button.TouchUpInside += ButtonYearOnClick;
                            button.SetTitle(year.Value.ToString(), UIControlState.Normal);
                            button.Tag = year.Value;

                            _yearButtons.Add(button);
						}

                        _yearsView.AddButtons(_yearButtons);

						ThreadPool.QueueUserWorkItem(state => UpdateMagazineYears(years));
						_currentYear = years.First().Value;
					}
					var magazines = e.Magazines;
					if (magazines != null && magazines.Any())
					{
						UpdateMagazinesPdfExists(magazines, _currentYear);
						_magazines = magazines;
                        _archiveView.AddMagazineViews(_magazines);
					}
				}
				else
				{
					BTProgressHUD.ShowToast ("Ошибка при загрузке", ProgressHUD.MaskType.None, false, 2500);
				}
				SetLoadingImageVisible (false);
			});
		}

		private void ButtonYearOnClick(object sender, EventArgs eventArgs)
		{
			if (_isLoading)
				return;
			//Выделить активную кнопку и выташить год из кнопка.Tag
			var year = ((UIButton)sender).Tag;
			if (year != _currentYear)
			{
				_currentYear = year;
				var magazines = ApplicationWorker.Db.GetMagazinesByYear(year, true);
				if (magazines != null && magazines.Any() && !magazines.Any(x => x.PreviewPicture == null))
				{
					magazines = magazines.OrderByDescending(x => x.ActiveFrom).ToList();
					UpdateMagazinesPdfExists(magazines, year);
					_magazines = magazines;
					_archiveView.AddMagazineViews(_magazines);
				}
				else
				{
					_archiveView.AddMagazineViews(new List<Magazine>());
					if (!ApplicationWorker.Settings.OfflineMode)
					{
						var connectAccept = IsConnectionAccept();
						if (!connectAccept)
						{
							BTProgressHUD.ShowToast ("Нет доступных подключений, для указанных в настройках", ProgressHUD.MaskType.None, false, 2500);
							return;
						}
						_isLoading = true;
						SetLoadingImageVisible (true);
						ApplicationWorker.RemoteWorker.MagazinesPriviewGetted += OnMagazinesPriviewGetted;
						ThreadPool.QueueUserWorkItem(
							state =>
							ApplicationWorker.RemoteWorker.BeginGetMagazinesPreview(ApplicationWorker.Settings, year));
					}
				}
			}
		}



		private void OnMagazinesPriviewGetted(object sender, MagazinesPreviewEventArgs e)
		{
			ApplicationWorker.RemoteWorker.MagazinesPriviewGetted -= OnMagazinesPriviewGetted;
			_isLoading = false;
			if (e.Abort)
			{
				return;
			}
			var error = e.Error;
			InvokeOnMainThread(() =>
			{
				if (!error)
				{
					var magazines = e.Magazines;
					if (magazines != null && magazines.Any())
					{
						UpdateMagazinesPdfExists(magazines, _currentYear);
						_magazines = magazines;
						_archiveView.AddMagazineViews(_magazines);
					}
				}
				else
				{
					BTProgressHUD.ShowToast ("Ошибка при запросе", ProgressHUD.MaskType.None, false, 2500);
					_currentYear = -1;
				}
				SetLoadingImageVisible (false);
			});
		}

        private void OnMagazinePushed(object sender, EventArgs e)
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
			DestroyPdfLoader ();
			_isLoading = false;
			if (showController != null)
			{
				NavigationController.PopToViewController (showController, false);
				showController.SetMagazineId (ApplicationWorker.Magazine.Id);
				Dispose();
			}
			else
			{
				showController = new MagazineViewController(ApplicationWorker.Magazine.Id);
				NavigationController.PushViewController(showController, false);
        	}
		}

		void OnMagazineDownload (object sender, EventArgs e)
		{
			var magazineView = (MagazineView)sender;
			DownloadMagazinePdf (magazineView);
		}

		void OnMagazineOpen (object sender, EventArgs e)
		{
			var magazineView = (MagazineView)sender;
			var magazine = magazineView.Magazine;
			OpenPdf(magazine);
		}

		void OnMagazineDelete (object sender, EventArgs e)
		{
			var magazineView = (MagazineView)sender;
			var magazine = magazineView.Magazine;
			var alertView = new BlackAlertView("Удаление", String.Format("Удалить журнал: {0}?", magazine.Name), "Нет", "Да");

			alertView.ButtonPushed += (s, ev) => 
			{
				if (ev.ButtonIndex == 1)
				{
					DeleteMagazine(magazineView);
				}
				alertView.Dispose();
			};

			alertView.Show();
		}

		private void OnPdfGetted(object sender, PdfEventArgs e)
		{
			var error = e.Error;
			if (e.Abort)
			{
				InvokeOnMainThread(() =>
				{
					SetLoadingProgressVisible(false);
					_downloadMagazineView = null;
				});
				return;
			}
			InvokeOnMainThread(() =>
			{
				if (!error)
				{
					var downloadItem = _downloadMagazineView;
					var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
					var fileName = downloadItem.Magazine.Id.ToString("G") + ".pdf";
					var path = Path.Combine(folder, fileName);
					var fs = File.Create(path);
					fs.Write(e.Pdf, 0, e.Pdf.Length);
					fs.Flush();
					fs.Close();
					downloadItem.Magazine.Exists = true;
					ApplicationWorker.Db.UpdateMagazine(downloadItem.Magazine);
					downloadItem.UpdateMagazineExists(true);
				}
				else
				{
					BTProgressHUD.ShowToast ("Ошибка при запросе", ProgressHUD.MaskType.None, false, 2500);
				}
				SetLoadingProgressVisible(false);
				_downloadMagazineView = null;
			});
		}


		#endregion

		#region Activity logic

		private void DeleteMagazine(MagazineView magazineView)
		{
			var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			var fileName = magazineView.Magazine.Id.ToString("G") + ".pdf";
			var path = Path.Combine(folder, fileName);
			File.Delete(path);
			magazineView.Magazine.Exists = false;
			ApplicationWorker.Db.UpdateMagazine(magazineView.Magazine);
			magazineView.UpdateMagazineExists(false);
			BTProgressHUD.ShowToast ("Файл удален", ProgressHUD.MaskType.None, false, 2500);
		}

		private void OpenPdf(Magazine magazine)
		{
			var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			var fileName = magazine.Id.ToString("G") + ".pdf";
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

		private void UpdateMagazinesPdfExists(List<Magazine> magazines, int year)
		{
			if (magazines == null || !magazines.Any()) return;
			var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			foreach (var magazine in magazines)
			{
				var fileName = magazine.Id.ToString("G") + ".pdf";
				var path = Path.Combine(folder, fileName);
				var file = new FileInfo(path);
				magazine.Exists = file.Exists;
			}
			ThreadPool.QueueUserWorkItem(state => UpdateMagazines(magazines, year));
		}

		private void UpdateMagazineYears(IEnumerable<MagazineYear> years)
		{
			var dbYears = ApplicationWorker.Db.LoadMagazineYears();
			foreach (var year in years)
			{
				var dbModel = dbYears.FirstOrDefault(x => x.Value == year.Value);
				if (dbModel == null)
				{
					year.DataLoaded = false;
					ApplicationWorker.Db.InsertMagazineYear(year);
				}
			}
		}

		private void UpdateMagazines(IEnumerable<Magazine> magazines, int year)
		{
			var dbModels = ApplicationWorker.Db.GetMagazinesByYear(year, false);
			foreach (var magazine in magazines)
			{
				var dbModel = dbModels.FirstOrDefault(x => x.Id == magazine.Id);
				if (dbModel != null)
				{
					if (!dbModel.Exists && !string.IsNullOrWhiteSpace(dbModel.PdfFileSrc) &&
						!string.IsNullOrWhiteSpace(magazine.PdfFileSrc) &&
						dbModel.PdfFileSrc.Trim() != magazine.PdfFileSrc.Trim())
					{
						ApplicationWorker.Db.UpdateMagazine(magazine);
					}
				}
				else
				{
					ApplicationWorker.Db.InsertMagazine(magazine);
					ApplicationWorker.Db.InsertPicture(magazine.PreviewPicture);
				}
			}
		}

		private void DownloadMagazinePdf(MagazineView magazineView)
		{
			if (ApplicationWorker.Settings.OfflineMode)
			{
				BTProgressHUD.ShowToast ("Загрузка Pdf невозможна в оффлайн режиме", ProgressHUD.MaskType.None, false, 2500);
				return;   
			}
			if (string.IsNullOrWhiteSpace(magazineView.Magazine.PdfFileSrc))
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
				_downloadMagazineView = magazineView;
				var magazine = magazineView.Magazine;
				SetLoadingProgressVisible(true);
				ThreadPool.QueueUserWorkItem(state => ApplicationWorker.PdfLoader.BeginGetMagazinePdf(magazine.PdfFileSrc));
			}
			else
			{
				BTProgressHUD.ShowToast ("Идет загрузка Pdf... Дождитесь завершения", ProgressHUD.MaskType.None, false, 2500);
			}
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

		public void DestroyPdfLoader()
		{
			ApplicationWorker.PdfLoader.PdfGetted -= OnPdfGetted;
			ApplicationWorker.PdfLoader.AbortOperation();
		}

		#endregion

		#region Helper methods

		//Отображение/скрытие прогресса загрузки 
		private void SetLoadingProgressVisible(bool visible)
		{
			if (visible)
			{
				_loadingPdfIndicator.Hidden = false;
				_loadingPdfIndicator.StartAnimating ();
				View.BringSubviewToFront (_loadingPdfIndicator);
			}
			else
			{
				_loadingPdfIndicator.Hidden = true;
				_loadingPdfIndicator.StopAnimating ();
			}
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

