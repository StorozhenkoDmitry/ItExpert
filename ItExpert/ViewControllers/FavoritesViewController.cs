﻿using System;
using MonoTouch.UIKit;
using System.Drawing;
using ItExpert.Enum;
using ItExpert.ServiceLayer;

namespace ItExpert
{
    public class FavoritesViewController: UIViewController
    {
		#region Fields

		private BottomToolbarView _bottomBar = null;

		#endregion

		#region UIViewController members

		static bool UserInterfaceIdiomIsPhone {
			get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
		}

		public FavoritesViewController()
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
			_bottomBar.FavoritesButton.SetActiveState (true);	
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
				NavigationController.PopToViewController (showController, true);
				showController.ShowFromAnotherScreen (Page.Trends);
			}
			else
			{
				showController = new NewsViewController (Page.Trends);
				NavigationController.PushViewController (showController, true);
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
				NavigationController.PopToViewController (showController, true);
				showController.ShowFromAnotherScreen (Page.News);
			}
			else
			{
				showController = new NewsViewController (Page.News);
				NavigationController.PushViewController (showController, true);
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
				showController.SetMagazineId (-1);
			}
			else
			{
				showController = new MagazineViewController (-1);
				NavigationController.PushViewController (showController, true);
			}
		}

		private void ButFavoriteOnClick(object sender, EventArgs eventArgs)
		{

		}

		#endregion

		#region Helper methods

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
    }
}

