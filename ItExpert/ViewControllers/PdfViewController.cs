using System;
using MonoTouch.UIKit;
using System.Drawing;
using ItExpert.Enum;
using System.IO;
using MonoTouch.Foundation;

namespace ItExpert
{
	public class PdfViewController: UIViewController
	{
		#region Fields

		private UIWebView _webView = null;
		private string _path = null;

		#endregion

		#region UIViewController members

		static bool UserInterfaceIdiomIsPhone {
			get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
		}

		public PdfViewController(string path)
		{
			_path = path;
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			Initialize ();
			if (!string.IsNullOrEmpty (_path))
			{
				var localDocUrl = Path.Combine (NSBundle.MainBundle.BundlePath, _path);
				_webView.LoadRequest(new NSUrlRequest(new NSUrl(localDocUrl, false)));
				_webView.ScalesPageToFit = true;
			}
			// Perform any additional setup after loading the view, typically from a nib.
		}

		#endregion

		#region Initialize

		void Initialize()
		{
			var topOffset = NavigationController.NavigationBar.Frame.Height + ItExpertHelper.StatusBarHeight;
			_webView = new UIWebView (new RectangleF (0, topOffset, View.Bounds.Width, 
				View.Bounds.Height - topOffset));
			Add (_webView);
		}

		#endregion

		#region Logic

		public void ShowPdf(string path)
		{
			var localDocUrl = Path.Combine (NSBundle.MainBundle.BundlePath, path);
			_webView.LoadRequest(new NSUrlRequest(new NSUrl(localDocUrl, false)));
			_webView.ScalesPageToFit = true;
		}

		#endregion
	}
}

