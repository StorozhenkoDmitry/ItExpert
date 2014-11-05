using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using ItExpert.Enum;
using ItExpert.Model;
using ItExpert.ServiceLayer;

namespace ItExpert
{
	public class SplashViewController:UIViewController
	{
		private ScreenWidth _firstScreenWidth;
		private ScreenWidth _secondtScreenWidth;
		private UIImageView _splashImage;
		private UIActivityIndicatorView _indicator;


		public SplashViewController ()
		{
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();

			// Release any cached data, images, etc that aren't in use.
		}

		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate(fromInterfaceOrientation);
			InitSplash ();
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			NavigationController.NavigationBarHidden = true;
			// Perform any additional setup after loading the view, typically from a nib.
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			InitSplash ();
			Initialize ();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			InvokeOnMainThread(() =>
			{
				if (_indicator != null)
				{
					_indicator.RemoveFromSuperview();
					_indicator.Dispose();
				}

				if (_splashImage != null)
				{
					_splashImage.RemoveFromSuperview();
					if (_splashImage.Image != null)
					{
						_splashImage.Image.Dispose();
						_splashImage.Image = null;
					}
					_splashImage.Dispose();
				}
				
				_indicator = null;
				_splashImage = null;
			});
		}

		private void InitSplash()
		{
			var fileName = "Splash.png";
			if (InterfaceOrientation == UIInterfaceOrientation.LandscapeLeft || InterfaceOrientation == UIInterfaceOrientation.LandscapeRight)
			{
				fileName = "SplashLand.png";
			}
			if (_splashImage == null)
			{
				_splashImage = new UIImageView (new RectangleF (0, 0, View.Bounds.Width, View.Bounds.Height));
				_splashImage.Image = new UIImage (NSData.FromFile (fileName), 1);
				Add (_splashImage);
			}
			else
			{
				_splashImage.Frame = new RectangleF (0, 0, View.Bounds.Width, View.Bounds.Height);
				_splashImage.Image = new UIImage (NSData.FromFile (fileName), 1);
			}
			if (_indicator == null)
			{
				_indicator = new UIActivityIndicatorView (new RectangleF (0, View.Bounds.Height - 80, View.Bounds.Width, 60));
				_indicator.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.WhiteLarge;
				_indicator.Color = UIColor.DarkGray;
				_indicator.BackgroundColor = UIColor.Clear;
				View.Add (_indicator);
				_indicator.StartAnimating ();
			}
			else
			{
				_indicator.Frame = new RectangleF (0, View.Bounds.Height - 80, View.Bounds.Width, 60);
				_indicator.StartAnimating ();
			}
		}

		void Initialize()
		{
			var connectAccept = IsConnectionAccept();
			if (!connectAccept)
			{
				OnStop ();
				NavigationController.PushViewController (new NewsViewController (), false);
				return;
			}
			var screenWidth =
				ApplicationWorker.Settings.GetScreenWidthForScreen(
					(int)View.Bounds.Width);
			ApplicationWorker.Settings.ScreenWidth = screenWidth;
			_firstScreenWidth = screenWidth;
			_secondtScreenWidth = ApplicationWorker.Settings.GetScreenWidthForScreen(
				(int)View.Bounds.Height);
			ApplicationWorker.Settings.SaveSettings();
			ApplicationWorker.RemoteWorker.BannerGetted += OnFirstBannerGetted;
			ApplicationWorker.RemoteWorker.BeginGetBanner(ApplicationWorker.Settings);
		}

		private void OnFirstBannerGetted(object sender, BannerEventArgs e)
		{
			InvokeOnMainThread (() =>
			{
				ApplicationWorker.RemoteWorker.BannerGetted -= OnFirstBannerGetted;
				ApplicationWorker.BannerEventArgs = e;
				if (_firstScreenWidth != _secondtScreenWidth)
				{
					var settings = ApplicationWorker.Settings.Clone ();
					settings.ScreenWidth = _secondtScreenWidth;
					ApplicationWorker.RemoteWorker.BannerGetted += OnSecondBannerGetted;
					ApplicationWorker.RemoteWorker.BeginGetBanner (settings);
				}
				else
				{
					SaveBanners (ApplicationWorker.BannerEventArgs);
					ApplicationWorker.RemoteWorker.CssGetted += OnCssGetted;
					ApplicationWorker.RemoteWorker.BeginGetCss ();   
				}
			});
		}

		private void OnSecondBannerGetted(object sender, BannerEventArgs e)
		{
			ApplicationWorker.RemoteWorker.BannerGetted -= OnSecondBannerGetted;
			if (ApplicationWorker.BannerEventArgs != null && ApplicationWorker.BannerEventArgs.Banners != null)
			{
				var banner = e.Banners.FirstOrDefault();
				ApplicationWorker.BannerEventArgs.Banners.Add(banner);
			}
			else
			{
				ApplicationWorker.BannerEventArgs = e;
			}
			SaveBanners(ApplicationWorker.BannerEventArgs);
			ApplicationWorker.RemoteWorker.CssGetted += OnCssGetted;
			ApplicationWorker.RemoteWorker.BeginGetCss();
		}

		private void NewNewsGetted(object sender, ArticleEventArgs e)
		{
			InvokeOnMainThread (() =>
			{
				ApplicationWorker.StartArticlesEventArgs = e;
				OnStop ();
				NavigationController.PushViewController (new NewsViewController (), false);
			});
		}

		private void OnCssGetted(object sender, CssEventArs e)
		{
			ApplicationWorker.RemoteWorker.CssGetted -= OnCssGetted;
			if (!string.IsNullOrWhiteSpace(e.Css))
			{
				ApplicationWorker.Css = e.Css;
				ApplicationWorker.EnsureCreateAppDataFolder();
				var path = ApplicationWorker.GetAppDataFilePath("css.dt");
				using (var sw = new StreamWriter(path, false, Encoding.UTF8))
				{
					sw.Write(e.Css);
					sw.Flush();
				}
				ApplicationWorker.SetDoNotBackUpAttribute(path);
			}
			SelectStartSection(ApplicationWorker.Settings.Page);
		}

		private void SaveBanners(BannerEventArgs e)
		{
			if (e == null) return;
			var error = e.Error;
			if (!error)
			{
				var banners = ApplicationWorker.Db.LoadBanners();
				if (banners != null && banners.Any())
				{
					foreach (var model in banners)
					{
						ApplicationWorker.Db.DeleteBanner(model.Id);
						var pictures = ApplicationWorker.Db.GetPicturesForParent(model.Id);
						if (pictures != null && pictures.Any())
						{
							foreach (var picture in pictures)
							{
								if (picture.Type == PictypeType.Banner)
								{
									ApplicationWorker.Db.DeletePicture(picture.Id);
								}
							}
						}
					}
				}
				foreach (var banner in e.Banners)
				{
					if (!banner.Url.StartsWith("http"))
					{
						banner.Url = Settings.Domen + banner.Url;
					}
					ApplicationWorker.Db.InsertBanner(banner);
					var picture = banner.Picture;
					if (picture != null)
					{
						ApplicationWorker.Db.InsertPicture(picture);
					}   
				}
			}
		}

		private void SelectStartSection(Page page)
		{
			InvokeOnMainThread (() =>
			{
				var connectAccept = IsConnectionAccept ();
				if (!connectAccept)
				{
					OnStop ();
					NavigationController.PushViewController (new NewsViewController (), false);
					return;
				}
				ApplicationWorker.RemoteWorker.NewsGetted += NewNewsGetted;
				if (page == Page.News)
				{
					ApplicationWorker.RemoteWorker.BeginGetNews (ApplicationWorker.Settings, -1, -1, -1,
						-1, -1, null);
				}
				else if (page == Page.Trends)
				{
					ApplicationWorker.RemoteWorker.BeginGetNews (ApplicationWorker.Settings, -1, -1, 30,
						-1, -1, null);
				}
				else
				{
					OnStop ();
					NavigationController.PushViewController (new NewsViewController (), false);
				}
			});
		}

		private void OnStop()
		{
			ApplicationWorker.RemoteWorker.BannerGetted -= OnFirstBannerGetted;
			ApplicationWorker.RemoteWorker.NewsGetted -= NewNewsGetted;
			ApplicationWorker.RemoteWorker.CssGetted -= OnCssGetted;
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
	}
}

