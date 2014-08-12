using System;
using MonoTouch.UIKit;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using ItExpert.Model;
using BigTed;
using System.Threading;

namespace ItExpert
{
    public class CacheView: UIView
    {
        public CacheView (RectangleF frame)
			: base (frame)
        {
			UserInteractionEnabled = true;

            BackgroundColor = UIColor.Black;

			_scrollView = new UIScrollView (Bounds);
			_scrollView.UserInteractionEnabled = true;
			_scrollView.ScrollEnabled = true;
			_scrollView.Bounces = false;

            _tableView = new UITableView();

            _tableView.Frame = new RectangleF(0, 0, Frame.Width, 172);
            _tableView.BackgroundColor = UIColor.Black;
            _tableView.ScrollEnabled = false; 
            _tableView.UserInteractionEnabled = true;
            _tableView.SeparatorInset = new UIEdgeInsets (0, 0, 0, 0);
            _tableView.Bounces = false;
            _tableView.SeparatorColor = UIColor.FromRGB(100, 100, 100);

            _tableView.Source = new NavigationBarTableSource(GetCacheSettingsItems());

            float offsetBetweenButtons = 10;

            _clearCacheButton = GetButton("Очистить кэш", new PointF(0, _tableView.Frame.Bottom + offsetBetweenButtons));
            _deletePdfButton = GetButton("Удалить все Pdf", new PointF(0, _clearCacheButton.Frame.Bottom + offsetBetweenButtons));
            _deleteAllFavoritesButton = GetButton("Удалить все избранные", new PointF(0, _deletePdfButton.Frame.Bottom + offsetBetweenButtons));

            _clearCacheButton.TouchUpInside += OnClearCacheButtonPushed;
            _deletePdfButton.TouchUpInside += OnDeletePdfButtonPushed;
            _deleteAllFavoritesButton.TouchUpInside += OnDeleteAllFavoritesButtonPushed;

			_scrollView.Add(_tableView);
			_scrollView.Add(_clearCacheButton);
			_scrollView.Add(_deletePdfButton);
			_scrollView.Add(_deleteAllFavoritesButton);

			Add(_scrollView);

			_scrollView.ContentSize = new SizeF (Frame.Width, _deleteAllFavoritesButton.Frame.Bottom + offsetBetweenButtons);
			Action reloadData = () =>
			{
				Thread.Sleep(250);
				InvokeOnMainThread(() =>
				{
					_tableView.ReloadData();
				});
			};
			ThreadPool.QueueUserWorkItem(state => reloadData());
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (_clearCacheButton != null)
			{
				_clearCacheButton.RemoveFromSuperview();
				_clearCacheButton.TouchDown -= ButtonTouchDown;
				_clearCacheButton.TouchUpOutside -= ButtonTouchUpOutside;
				_clearCacheButton.TouchUpInside -= OnClearCacheButtonPushed;
				_clearCacheButton.Dispose();
			}
			if (_deletePdfButton != null)
			{
				_deletePdfButton.RemoveFromSuperview();
				_deletePdfButton.TouchUpInside -= OnDeletePdfButtonPushed;
				_deletePdfButton.TouchDown -= ButtonTouchDown;
				_deletePdfButton.TouchUpOutside -= ButtonTouchUpOutside;
				_deletePdfButton.Dispose();
			}
			if (_deleteAllFavoritesButton != null)
			{
				_deleteAllFavoritesButton.RemoveFromSuperview();
				_deleteAllFavoritesButton.TouchUpInside -= OnDeleteAllFavoritesButtonPushed;
				_deleteAllFavoritesButton.TouchDown -= ButtonTouchDown;
				_deleteAllFavoritesButton.TouchUpOutside -= ButtonTouchUpOutside;
				_deleteAllFavoritesButton.Dispose();
			}
			_clearCacheButton = null;
			_deletePdfButton = null;
			_deleteAllFavoritesButton = null;
			if (_tableView != null)
			{
				_tableView.RemoveFromSuperview();
				if (_tableView.Source != null)
				{
					_tableView.Source.Dispose();
				}
				_tableView.Source = null;
				_tableView.Dispose();
			}
			_tableView = null;
			if (_scrollView != null)
			{
				_scrollView.RemoveFromSuperview();
				_scrollView.Dispose();
			}
			_scrollView = null;
		}

		public void CorrectSubviewsFrames()
		{
			var screenSize = ItExpertHelper.GetRealScreenSize ();

			if (_tableView != null)
			{
				_tableView.Frame = new RectangleF(0, 0, screenSize.Width, 172);

				_tableView.ReloadData ();

				if (_clearCacheButton != null)
				{
					_clearCacheButton.Frame = new RectangleF(
						new PointF(screenSize.Width / 2 - _clearCacheButton.Frame.Width / 2, _clearCacheButton.Frame.Y), 
						_clearCacheButton.Frame.Size);
				}

				if (_deletePdfButton != null)
				{
					_deletePdfButton.Frame = new RectangleF(
						new PointF(screenSize.Width / 2 - _deletePdfButton.Frame.Width / 2, _deletePdfButton.Frame.Y), 
						_deletePdfButton.Frame.Size);
				}

				if (_deleteAllFavoritesButton != null)
				{
					_deleteAllFavoritesButton.Frame = new RectangleF(
						new PointF(screenSize.Width / 2 - _deleteAllFavoritesButton.Frame.Width / 2, _deleteAllFavoritesButton.Frame.Y), 
						_deleteAllFavoritesButton.Frame.Size);
				}

				_scrollView.Frame = Bounds;
				_scrollView.ContentSize = new SizeF (screenSize.Width, _deleteAllFavoritesButton.Frame.Bottom + 10);
			}
		}

        private List<NavigationBarItem> GetCacheSettingsItems()
        {
            List<NavigationBarItem> cacheSettingsItems = new List<NavigationBarItem>();

            cacheSettingsItems.Add(new NavigationBarItem(NavigationBarItem.ContentType.Tap) 
            { 
                Title = "Объем данных", 
                GetValue = () => { 
					var dbSize = ApplicationWorker.Db.GetDbSize();
					var dbSizeMb = Math.Round((double)dbSize/(1024*1024), 2);
					return dbSizeMb + " Мб";
				},
				SetValue = (value) => {  }
            });

            cacheSettingsItems.Add(new NavigationBarItem(NavigationBarItem.ContentType.Tap)
            { 
                Title = "Объем всех Pdf",  
                GetValue = () => { 
					var value = string.Empty;
					var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
					long sizePdfs = 0;
					var fileNames = Directory.GetFiles(folder, "*.pdf");
					foreach (var name in fileNames)
					{
						var file = new FileInfo(name);
						sizePdfs += file.Length;
					}
					var sizePdfsMb = Math.Round((double)sizePdfs / (1024 * 1024), 2);
					value = sizePdfsMb + " Мб";
					return value; 
				},
				SetValue = (value) => {  }
            });

            cacheSettingsItems.Add(new NavigationBarItem(NavigationBarItem.ContentType.CacheSlider) 
            { 
                Title = "Предел размера данных", 
				GetValue = () => { return ApplicationWorker.Settings.DbSizeLimit; },
                SetValue = (value) => {  }
            });

            return cacheSettingsItems;
        }

        private UIButton GetButton(string title, PointF location)
        {
            UIButton button = new UIButton();

            button.SetTitle(title, UIControlState.Normal);
            button.SetTitleColor(ItExpertHelper.ButtonTextColor, UIControlState.Normal);
            button.SizeToFit();
            button.BackgroundColor = ItExpertHelper.ButtonColor;
            button.TitleEdgeInsets = new UIEdgeInsets(0, 4, 0, 4);

            SizeF buttonSize = new SizeF(button.Frame.Width + button.TitleEdgeInsets.Left + button.TitleEdgeInsets.Right, 
                                   button.Frame.Height + button.TitleEdgeInsets.Top + button.TitleEdgeInsets.Bottom);

            button.Frame = new RectangleF(new PointF(Frame.Width / 2 - buttonSize.Width / 2, location.Y), buttonSize);

			button.TouchDown += ButtonTouchDown;

			button.TouchUpOutside += ButtonTouchUpOutside;

            return button;
        }

		void ButtonTouchDown(object sender, EventArgs e)
		{
			var senderButton = (sender as UIButton);

			senderButton.BackgroundColor = ItExpertHelper.ButtonPushedColor;
		}

		void ButtonTouchUpOutside(object sender, EventArgs e)
		{
			var senderButton = (sender as UIButton);

			senderButton.BackgroundColor = ItExpertHelper.ButtonPushedColor;
		}

        private void OnClearCacheButtonPushed(object sender, EventArgs e)
        {
            var senderButton = (sender as UIButton);
            senderButton.BackgroundColor = ItExpertHelper.ButtonColor;

			Action clearCache = () =>
			{
				ApplicationWorker.Db.ClearCache();
				InvokeOnMainThread(() =>
				{
					_tableView.ReloadData();
				});
			};
			ThreadPool.QueueUserWorkItem(state => clearCache());
        }

        private void OnDeletePdfButtonPushed(object sender, EventArgs e)
        {
            var senderButton = (sender as UIButton);
            senderButton.BackgroundColor = ItExpertHelper.ButtonColor;

			var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			var pdfFiles = Directory.GetFiles(folder, "*.pdf");
			foreach (var file in pdfFiles)
			{
				var fileInfo = new FileInfo(file);
				fileInfo.Delete();
			}
			_tableView.ReloadData();
			ApplicationWorker.Db.SetAllMagazinePdfNotLoaded();
			ApplicationWorker.OnAllPdfFilesDeleted();
        }

        private void OnDeleteAllFavoritesButtonPushed(object sender, EventArgs e)
        {
            var senderButton = (sender as UIButton);
            senderButton.BackgroundColor = ItExpertHelper.ButtonColor;

			Action clearCache = () =>
			{
				ApplicationWorker.Db.DeleteFavorite();
				InvokeOnMainThread(() =>
				{
					_tableView.ReloadData();
				});
			};
			ThreadPool.QueueUserWorkItem(state => clearCache());
        }

        private UITableView _tableView;
        private UIButton _clearCacheButton;
        private UIButton _deletePdfButton;
        private UIButton _deleteAllFavoritesButton;
		private UIScrollView _scrollView;
    }
}

