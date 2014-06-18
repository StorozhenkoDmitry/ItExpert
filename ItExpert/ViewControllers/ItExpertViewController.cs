using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using ItExpert.Model;
using ItExpert.ServiceLayer;

namespace ItExpert
{
	public partial class ItExpertViewController : UIViewController
	{
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

		#region View lifecycle

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			Initialize ();
			// Perform any additional setup after loading the view, typically from a nib.
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
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

		#region Business Logic

		void Initialize()
		{
			btnGetNews.TouchUpInside += butGetNewsClick;

			View.AutosizesSubviews = true;

			_newsTableView = new UITableView(new RectangleF(0, 100, View.Bounds.Width, View.Bounds.Height - 100), UITableViewStyle.Plain);
			_newsTableView.ScrollEnabled = true; 
			_newsTableView.UserInteractionEnabled = true;
			_newsTableView.SeparatorInset = new UIEdgeInsets (0, 0, 0, 0);
			_newsTableView.Bounces = true;

			View.Add (_newsTableView);
		}

		void butGetNewsClick (object sender, EventArgs e)
		{
			ApplicationWorker.RemoteWorker.NewsGetted += NewsGetted;
			ApplicationWorker.Settings.LoadDetails = true;
			ThreadPool.QueueUserWorkItem(state=>{ApplicationWorker.RemoteWorker.BeginGetNews(ApplicationWorker.Settings, -1, -1, -1, -1, -1, null);});
		}

		void NewsGetted (object sender, ArticleEventArgs e)
		{
			ApplicationWorker.RemoteWorker.NewsGetted -= NewsGetted;
			if (!e.Error) 
			{
				var articles = e.Articles;
				InvokeOnMainThread (() => 
				{
					new UIAlertView ("Успешно", "Получено объектов: " + articles.Count.ToString (), null, "OK", null).Show ();

					if (_newsTableView.Source != null) 
					{
						(_newsTableView.Source as NewsTableSource).PushNewsDetails -= OnPushNewsDetails;

						_newsTableView.Source.Dispose();
						_newsTableView.Source = null;
					}

					NewsTableSource source = new NewsTableSource(articles);
					source.PushNewsDetails += OnPushNewsDetails;

					_newsTableView.Source = source;
					_newsTableView.ReloadData();
				});
			} 
			else 
			{
				InvokeOnMainThread (() => 
				{
					new UIAlertView("Ошибка" ,"Ошибка" , null, "OK", null).Show();
				});
			}
		}

		private void OnPushNewsDetails(object sender, PushNewsDetailsEventArgs e)
		{
			NavigationController.PushViewController (e.NewsDetailsView, true);
		}

		private UITableView _newsTableView;

		#endregion
	}
}

