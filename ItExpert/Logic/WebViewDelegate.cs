﻿using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace ItExpert
{
    public class WebViewDelegate : UIWebViewDelegate
    {
        public event EventHandler WebViewLoaded;

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

