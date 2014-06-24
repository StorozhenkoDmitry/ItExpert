using System;
using MonoTouch.UIKit;
using System.Drawing;
using ItExpert.Enum;
using ItExpert.Model;
using System.Collections.Generic;
using ItExpert.ServiceLayer;

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

		#endregion

		#region UIViewController members

		static bool UserInterfaceIdiomIsPhone {
			get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
		}

		public MagazineViewController(int magazineId)
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

		public void Initialize()
		{
			View.BackgroundColor = ItExpertHelper.GetUIColorFromColor (ApplicationWorker.Settings.GetBackgroundColor ());

			InitBottomToolbar ();
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

		#endregion

		#region Event handlers

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
			ApplicationWorker.PdfLoader.PdfGetted -= OnPdfGetted;
			if (e.Abort)
			{
				InvokeOnMainThread(() =>
				{
					var view = _extendedObject;
					SetLoadingProgressVisible(false);
//					var button = view.FindViewById<Button>(Resource.Id.getMagazine);
//					button.Enabled = true;
				});
				return;
			}
			var error = e.Error;
			InvokeOnMainThread(() =>
			{
				if (!error)
				{
					var folder = string.Empty;
//					var folder = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
//					var dir = new Java.IO.File(folder + Settings.PdfFolder);
//					if (!dir.Exists())
//					{
//						dir.Mkdir();
//					}
					var fileName = _magazine.Id.ToString("G") + ".pdf";
					var path = System.IO.Path.Combine(folder + Settings.PdfFolder, fileName);
					var fs = System.IO.File.Create(path);
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
//					var button = _extendedObject.FindViewById<Button>(Resource.Id.getMagazine);
//					if (_magazine.Exists)
//					{
//						button.Click -= ButDownloadPdfOnClick;
//						button.Click += ButMagazineOpenOnClick;
//						button.Text = "Открыть";
//						var adapter =
//							FindViewById<ListView>(Resource.Id.listMagazineNews).Adapter as
//							ArticleAdapter;
//						if (adapter != null)
//						{
//							adapter.SetMagazineAction(MagazineAction.Open);
//						}
//					}
				}
				else
				{
//					Toast.MakeText(this, "Ошибка при запросе", ToastLength.Short).Show();
				}
//				_extendedObject.FindViewById<Button>(Resource.Id.getMagazine).Enabled = true;
				SetLoadingProgressVisible(false);
			});
		}

		#endregion

		#region Logic

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

		#endregion
    }
}

