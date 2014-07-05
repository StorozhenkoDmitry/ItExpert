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

namespace ItExpert
{
    public class ArchiveViewController: UIViewController
    {
		#region Fields

		private int _currentYear = -1;
		private int _currentDownloadId = -1;
		private MagazineView _downloadMagazineView;
		private bool _toMenu = false;
		private bool _toSettings = false;
		private List<Magazine> _magazines = null; 
        private YearsView _yearsView;
        private ArchiveView _archiveView;
		private UIActivityIndicatorView _loadingIndicator;
		private UIActivityIndicatorView _loadingPdfIndicator;

		#endregion

		#region UIViewController members

		static bool UserInterfaceIdiomIsPhone {
			get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
		}

        public ArchiveViewController()
        {
        }

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			Initialize ();
			// Perform any additional setup after loading the view, typically from a nib.
		}

		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
			ApplicationWorker.RemoteWorker.MagazinesPriviewGetted -= OnMagazinesYearsGetted;
			ApplicationWorker.RemoteWorker.MagazinesPriviewGetted -= OnMagazinesPriviewGetted;
//			ApplicationWorker.SettingsChanged -= ApplicationOnSettingsChanged;
		}

		#endregion

		#region Init

		void Initialize()
		{
            AutomaticallyAdjustsScrollViewInsets = false;
			InitLoadingProgress ();
			InitLoadingPdfProgress ();
			ApplicationWorker.PdfLoader.PdfGetted += OnPdfGetted;

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
//					Toast.MakeText(this, "Нет доступных подключений, для указанных в настройках", ToastLength.Long)
//						.Show();
					return;
				}
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
					var yearButtons = new List<UIButton>();
					for (var i = 0; i < years.Count(); i++)
					{
						var year = years[i];
						var button = new UIButton(UIButtonType.Custom);
						button.TouchUpInside += ButtonYearOnClick;
						button.SetTitle(year.Value.ToString(), UIControlState.Normal);
						button.Tag = year.Value;
						yearButtons.Add(button);
					}
					_yearsView.AddButtons(yearButtons);
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

		#endregion

		#region Event handlers

		private void OnMagazinesYearsGetted(object sender, MagazinesPreviewEventArgs e)
		{
			ApplicationWorker.RemoteWorker.MagazinesPriviewGetted -= OnMagazinesYearsGetted;
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
                        List<UIButton> yearButtons = new List<UIButton>();

						for (var i = 0; i < years.Count(); i++)
						{
							var year = years[i];

                            UIButton button = new UIButton(UIButtonType.Custom);

                            button.TouchUpInside += ButtonYearOnClick;
                            button.SetTitle(year.Value.ToString(), UIControlState.Normal);
                            button.Tag = year.Value;

                            yearButtons.Add(button);
						}

                        _yearsView.AddButtons(yearButtons);

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
					//					Toast.MakeText(this, "Ошибка при запросе", ToastLength.Short).Show();
				}
				SetLoadingImageVisible (false);
			});
		}

		private void ButtonYearOnClick(object sender, EventArgs eventArgs)
		{
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
					if (!ApplicationWorker.Settings.OfflineMode)
					{
						var connectAccept = IsConnectionAccept();
						if (!connectAccept)
						{
//							Toast.MakeText(this, "Нет доступных подключений, для указанных в настройках",
//								ToastLength.Long).Show();
							return;
						}
						SetLoadingImageVisible (true);
						ApplicationWorker.RemoteWorker.MagazinesPriviewGetted += OnMagazinesPriviewGetted;
						ThreadPool.QueueUserWorkItem(
							state =>
							ApplicationWorker.RemoteWorker.BeginGetMagazinesPreview(ApplicationWorker.Settings, year));
					}
					else
					{
						_archiveView.AddMagazineViews(new List<Magazine>());
					}
				}
			}
		}



		private void OnMagazinesPriviewGetted(object sender, MagazinesPreviewEventArgs e)
		{
			ApplicationWorker.RemoteWorker.MagazinesPriviewGetted -= OnMagazinesPriviewGetted;
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
//					Toast.MakeText(this, "Ошибка при запросе", ToastLength.Short).Show();
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
			if (showController != null)
			{
				NavigationController.PopToViewController (showController, true);
				showController.SetMagazineId (ApplicationWorker.Magazine.Id);
			}
			else
			{
				showController = new MagazineViewController(ApplicationWorker.Magazine.Id);
				NavigationController.PushViewController(showController, true);
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
					_currentDownloadId = -1;
					_downloadMagazineView = null;
				});
				return;
			}
			InvokeOnMainThread(() =>
			{
				if (!error)
				{
					var downloadItem = _downloadMagazineView;
					var folder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
					var dir = new DirectoryInfo(folder + Settings.PdfFolder);
					if (!dir.Exists)
					{
						dir.Create();
					}
					var fileName = downloadItem.Magazine.Id.ToString("G") + ".pdf";
					var path = Path.Combine(folder + Settings.PdfFolder, fileName);
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
//					Toast.MakeText(this, "Ошибка при запросе", ToastLength.Short).Show();
				}
				SetLoadingProgressVisible(false);
				_currentDownloadId = -1;
				_downloadMagazineView = null;
			});
		}


		#endregion

		#region Activity logic

		private void DeleteMagazine(MagazineView magazineView)
		{
			var folder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			var fileName = magazineView.Magazine.Id.ToString("G") + ".pdf";
			var path = Path.Combine(folder + Settings.PdfFolder, fileName);
			File.Delete(path);
			magazineView.Magazine.Exists = false;
			ApplicationWorker.Db.UpdateMagazine(magazineView.Magazine);
			magazineView.UpdateMagazineExists(false);
//			Toast.MakeText(this, "Файл удален", ToastLength.Short).Show();
		}

		private void OpenPdf(Magazine magazine)
		{
			var folder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			var fileName = magazine.Id.ToString("G") + ".pdf";
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

		private void UpdateMagazinesPdfExists(List<Magazine> magazines, int year)
		{
			if (magazines == null || !magazines.Any()) return;
			var folder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			foreach (var magazine in magazines)
			{
				var fileName = magazine.Id.ToString("G") + ".pdf";
				var path = Path.Combine(folder + Settings.PdfFolder, fileName);
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
//				Toast.MakeText(this, "Загрузка Pdf невозможна в оффлайн режиме", ToastLength.Long).Show();
				return;   
			}
			if (string.IsNullOrWhiteSpace(magazineView.Magazine.PdfFileSrc))
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
				_downloadMagazineView = magazineView;
				var magazine = magazineView.Magazine;
				SetLoadingProgressVisible(true);
				_currentDownloadId = magazine.Id;
				ThreadPool.QueueUserWorkItem(state => ApplicationWorker.PdfLoader.BeginGetMagazinePdf(magazine.PdfFileSrc));
			}
			else
			{
//				Toast.MakeText(this, "Идет загрузка... Дождитесь завершения", ToastLength.Short).Show();
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

