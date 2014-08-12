using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;

namespace ItExpert
{
    public class WebViewDelegate : UIWebViewDelegate
    {
        public event EventHandler WebViewLoaded;

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			WebViewLoaded = null;
		}

        public override bool ShouldStartLoad(UIWebView webView, NSUrlRequest request, UIWebViewNavigationType navigationType)
        {
            if (navigationType == UIWebViewNavigationType.LinkClicked)
            {
                UIApplication.SharedApplication.OpenUrl(request.Url);

                return false;
            }

            return true;
        }

        public override void LoadingFinished(UIWebView webView)
        {
            webView.ScrollView.ScrollEnabled = false;

            webView.Frame = new RectangleF(webView.Frame.Location, new SizeF(webView.Frame.Width, 1));

            webView.Frame = new RectangleF(webView.Frame.Location, new SizeF(webView.Frame.Width, webView.ScrollView.ContentSize.Height));

            OnWebViewLoaded(webView);
        }

        private void OnWebViewLoaded(UIWebView webView)
        {
            if (WebViewLoaded != null)
            {
                WebViewLoaded(webView, EventArgs.Empty);
            }
        }
    }
}

