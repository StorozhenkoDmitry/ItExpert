using System;
using MonoTouch.UIKit;
using ItExpert.Model;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using ItExpert.ServiceLayer;
using System.Drawing;

namespace ItExpert
{
    public class ArchiveViewController: UIViewController
    {
		#region Fields

		private int _currentYear = -1;
		private int _currentDownloadId = -1;
		private Magazine _downloadMagazine;
		private bool _toMenu = false;
		private bool _toSettings = false;
		private List<Magazine> _magazines = null; 
        private YearsView _yearsView;
        private ArchiveView _archiveView;

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

		#endregion

		#region Init

		void Initialize()
		{
            AutomaticallyAdjustsScrollViewInsets = false;

            _yearsView = new YearsView(new RectangleF(0, NavigationController.NavigationBar.Frame.Height + ItExpertHelper.StatusBarHeight,
                View.Frame.Width, 40));

            _archiveView = new ArchiveView(new RectangleF(0, _yearsView.Frame.Bottom, View.Frame.Width, View.Frame.Height - _yearsView.Frame.Bottom));

            View.Add(_yearsView);
            View.Add(_archiveView);

			View.BackgroundColor = ItExpertHelper.GetUIColorFromColor (ApplicationWorker.Settings.GetBackgroundColor ());
			SetLoadingProgressVisible(false);
			ApplicationWorker.RemoteWorker.MagazinesPriviewGetted += OnMagazinesYearsGetted;
			ThreadPool.QueueUserWorkItem(
				state => ApplicationWorker.RemoteWorker.BeginGetMagazinesPreview(ApplicationWorker.Settings, -1));	
			
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
					//Создать представление архива
					_archiveView.AddMagazineViews(_magazines);

//					if (_currentDownloadId != -1)
//					{
//						adapter.SetDownloadItem(_currentDownloadId);
//						adapter.NotifyDataSetChanged();
//					}
				}
				else
				{
					ApplicationWorker.RemoteWorker.MagazinesPriviewGetted += OnMagazinesPriviewGetted;
					ThreadPool.QueueUserWorkItem(
						state =>
						ApplicationWorker.RemoteWorker.BeginGetMagazinesPreview(ApplicationWorker.Settings, year));
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
						//Создать представление архива
						_archiveView.AddMagazineViews(_magazines);
//						if (_currentDownloadId != -1)
//						{
//							adapter.SetDownloadItem(_currentDownloadId);
//							adapter.NotifyDataSetChanged();
//						}
					}
				}
				else
				{
//					Toast.MakeText(this, "Ошибка при запросе", ToastLength.Short).Show();
					_currentYear = -1;
				}
			});
		}

		#endregion

		#region Activity logic

		private void UpdateMagazinesPdfExists(List<Magazine> magazines, int year)
		{
			if (magazines == null || !magazines.Any()) return;
//			var folder = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
//			foreach (var magazine in magazines)
//			{
//				var fileName = magazine.Id.ToString("G") + ".pdf";
//				var path = Path.Combine(folder + Settings.PdfFolder, fileName);
//				var file = new Java.IO.File(path);
//				magazine.Exists = file.Exists();
//			}
			ThreadPool.QueueUserWorkItem(state => UpdateMagazines(magazines, year));
		}

		private void DeleteYesHandler(object sender, EventArgs e)
		{
//			var deleleItem = ((MagazinePreviewAdapter)FindViewById<GridView>(Resource.Id.magazinePreviewContainer).Adapter)
//				.GetActiveItem();
//			if (deleleItem != null)
//			{
//				var folder = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
//				var fileName = deleleItem.Id.ToString("G") + ".pdf";
//				var path = Path.Combine(folder + Settings.PdfFolder, fileName);
//				File.Delete(path);
//				deleleItem.Exists = false;
//				ApplicationWorker.Db.UpdateMagazine(deleleItem);
//				Toast.MakeText(this, "Файл удален", ToastLength.Short).Show();
//				((MagazinePreviewAdapter)FindViewById<GridView>(Resource.Id.magazinePreviewContainer).Adapter).NotifyDataSetChanged();
//			}
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

		private void DownloadMagazinePdf(Magazine magazine)
		{
			if (ApplicationWorker.Settings.OfflineMode)
			{
//				Toast.MakeText(this, "Загрузка Pdf невозможна в оффлайн режиме", ToastLength.Long).Show();
				return;   
			}
			if (string.IsNullOrWhiteSpace(magazine.PdfFileSrc))
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
//				var sdAvailable = ApplicationWorker.IsExternalStorageAvailable();
//				if (!sdAvailable)
//				{
//					Toast.MakeText(this, "Недоступна SD карта, невозможно сохранить файл", ToastLength.Short).Show();
//					return;
//				}
				SetLoadingProgressVisible(true);
				_currentDownloadId = magazine.Id;
				_downloadMagazine = magazine;
//				var adapter = FindViewById<GridView>(Resource.Id.magazinePreviewContainer).Adapter as MagazinePreviewAdapter;
//				if (adapter != null)
//				{
//					adapter.SetDownloadItem(magazine.Id);
//					adapter.NotifyDataSetChanged();
//				}
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
			return result;
		}

		public void DestroyPdfLoader()
		{
//			ApplicationWorker.PdfLoader.PdfGetted -= OnPdfGetted;
			ApplicationWorker.PdfLoader.AbortOperation();
		}

		#endregion

		#region Helper methods

		//Отображение/скрытие прогресса загрузки 
		private void SetLoadingProgressVisible(bool visible)
		{

		}

		#endregion
	}
}

