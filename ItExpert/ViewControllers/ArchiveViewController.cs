using System;
using MonoTouch.UIKit;
using ItExpert.Model;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using ItExpert.ServiceLayer;

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
			View.BackgroundColor = ItExpertHelper.GetUIColorFromColor (ApplicationWorker.Settings.GetBackgroundColor ());
			SetLoadingProgressVisible(false);
			ApplicationWorker.RemoteWorker.MagazinesPriviewGetted += OnMagazinesYearsGetted;
			ApplicationWorker.PdfLoader.PdfGetted += OnPdfGetted;
			ApplicationWorker.SettingsChanged += ApplicationOnSettingsChanged;
			var connectAccept = IsConnectionAccept();
			if (!ApplicationWorker.Settings.OfflineMode)
			{
				if (!connectAccept)
				{
//					Toast.MakeText(this, "Нет доступных подключений, для указанных в настройках", ToastLength.Long)
//						.Show();
					return;
				}
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
					for (var i = 0; i < years.Count(); i++)
					{
						var year = years[i];
						//Создать кнопку и прикрепить обработчик клика -> ButtonYearOnClick
						//у кнопки свойство Tag = year.Value
					}
					_currentYear = years.First().Value;
					foreach (var year in years)
					{
						previewList = ApplicationWorker.Db.GetMagazinesByYear(year.Value, true);
						if (previewList != null && previewList.Any() && !previewList.Any(x => x.PreviewPicture == null))
						{
							//Выделить кнопку у которой Tag = year.Value
							//первый раз выделить и прекратить цикл
						}
					}
					if (previewList != null && previewList.Any() && !previewList.Any(x => x.PreviewPicture == null))
					{
						UpdateMagazinesPdfExists(previewList, _currentYear);
						var maxWidth = previewList.Where(x => x.PreviewPicture != null).Max(x => x.PreviewPicture.Width);
						previewList = previewList.OrderByDescending(x => x.ActiveFrom).ToList();
						_magazines = previewList;
						//Создать представление архива
					}
				}
			}
		}

		#endregion

		#region Event handlers

		private void ApplicationOnSettingsChanged(object sender, EventArgs eventArgs)
		{
			View.BackgroundColor = ItExpertHelper.GetUIColorFromColor (ApplicationWorker.Settings.GetBackgroundColor ());
		}

		private void ButtonYearOnClick(object sender, EventArgs eventArgs)
		{
			//Выделить активную кнопку и выташить год из кнопка.Tag
			var year = -1;
			if (year != _currentYear)
			{
				_currentYear = year;
				var magazines = ApplicationWorker.Db.GetMagazinesByYear(year, true);
				if (magazines != null && magazines.Any() && !magazines.Any(x => x.PreviewPicture == null))
				{
					magazines = magazines.OrderByDescending(x => x.ActiveFrom).ToList();
					UpdateMagazinesPdfExists(magazines, year);
					var maxWidth = magazines.Where(x => x.PreviewPicture != null).Max(x => x.PreviewPicture.Width);
					_magazines = magazines;
					//Создать представление архива


//					if (_currentDownloadId != -1)
//					{
//						adapter.SetDownloadItem(_currentDownloadId);
//						adapter.NotifyDataSetChanged();
//					}
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
						ApplicationWorker.RemoteWorker.MagazinesPriviewGetted += OnMagazinesPriviewGetted;
						ThreadPool.QueueUserWorkItem(
							state =>
							ApplicationWorker.RemoteWorker.BeginGetMagazinesPreview(ApplicationWorker.Settings, year));
					}
					else
					{
						//Создать пустое представление архива
					}
				}
			}
		}

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
					//UIButton firstButton = null;
					if (years != null && years.Any())
					{
						for (var i = 0; i < years.Count(); i++)
						{
							var year = years[i];
							//Создать кнопку и прикрепить обработчик клика -> ButtonYearOnClick
							//у кнопки свойство Tag = year.Value
							//первую кнопку присвоить переменной firstButton
//							if (i == 0) firstButton = button;
						}
						ThreadPool.QueueUserWorkItem(state => UpdateMagazineYears(years));
						_currentYear = years.First().Value;
					}
					var magazines = e.Magazines;
					if (magazines != null && magazines.Any())
					{
						UpdateMagazinesPdfExists(magazines, _currentYear);
//						if (firstButton != null)
//						{
//							firstButton.SetBackgroundColor(Color.DarkGray);
//						}
						var maxWidth = magazines.Where(x => x.PreviewPicture != null).Max(x => x.PreviewPicture.Width);
						_magazines = magazines;
						//Создать представление архива
					}
				}
				else
				{
//					Toast.MakeText(this, "Ошибка при запросе", ToastLength.Short).Show();
				}
			});
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
						var maxWidth = magazines.Where(x => x.PreviewPicture != null).Max(x => x.PreviewPicture.Width);
						_magazines = magazines;
						//Создать представление архива

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

		private void OnPdfGetted(object sender, PdfEventArgs e)
		{
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
			ApplicationWorker.PdfLoader.PdfGetted -= OnPdfGetted;
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

