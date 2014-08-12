using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;
using ItExpert.ServiceLayer;
using System.Net;
using System.IO;
using System.Text;
using Xamarin.Auth;
using BigTed;

namespace ItExpert
{
	public class AuthViewController : UIViewController
	{
		#region Fields

		private const string EvernoteConsumerKey = "tim452114";
		private const string EvernoteConsumerSecret = "10191f261dafa5ef";
		private const string EvernoteRequestTokenUrl = "https://www.evernote.com/oauth";
		private const string EvernoteAuthUrl = "https://www.evernote.com/OAuth.action";
		public const string ReadabilityConsumerKey = "Tim45250";
		public const string ReadabilityConsumerSecret = "ss2tMsyg4qdZvmUUqZKURP2zmsDAtxXt";
		private const string ReadabilityRequestTokenUrl = "https://www.readability.com/api/rest/v1/oauth/request_token";
		private const string ReadabilityAuthUrl = "https://www.readability.com/api/rest/v1/oauth/authorize";
		private const string ReadabilityAccessTokenUrl = "https://www.readability.com/api/rest/v1/oauth/access_token";
		private const string EvernoteAuthCallback = "http://www.it-world.ru/evernote";
		private const string ReadabilityAuthCallback = "http://www.it-world.ru/readability";
		public const string PocketConsumerKey = "26671-5e04ef50228099cd2d5dee90";
		private const string PocketAuthCallback = "http://www.it-world.ru/pocket";
		private string _tokenSecret = null;
		private ItExpert.ShareView.ViewWithButtons _shareView;
		private string _provider;
		private UIWebView _webView;
		private UIImageView _logoImageView;
		private UIView _headerView;
		private UIButton _backButton;
		private UILabel _splashLabel;
		private EventHandler _webViewLoadStarted;

		#endregion

		public AuthViewController(ItExpert.ShareView.ViewWithButtons shareView, string provider)
		{
			_shareView = shareView;
			_provider = provider;
		}

		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);
			Initialize();
		}

		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate(fromInterfaceOrientation);
			SetFrame();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			InvokeOnMainThread(() =>
			{
				if (_backButton != null)
				{
					_backButton.TouchUpInside -= BackButtonTouchUp;
					_backButton.RemoveFromSuperview();
					if (_backButton.ImageView != null && _backButton.ImageView.Image != null)
					{
						_backButton.ImageView.Image.Dispose();
						_backButton.ImageView.Image = null;
					}
					_backButton.Dispose();
				}
				_backButton = null;

				if (_logoImageView != null)
				{
					_logoImageView.RemoveFromSuperview();
					if (_logoImageView.Image != null)
					{
						_logoImageView.Image.Dispose();
						_logoImageView.Image = null;
					}
					_logoImageView.Dispose();
				}
				_logoImageView = null;


				if (_splashLabel != null)
				{
					_splashLabel.RemoveFromSuperview();
					_splashLabel.Dispose();
				}
				_splashLabel = null;

				if (_headerView != null)
				{
					_headerView.RemoveFromSuperview();
					_headerView.Dispose();
				}
				_headerView = null;

				if (_webView != null)
				{
					_webView.RemoveFromSuperview();
					if (_webViewLoadStarted != null)
					{
						_webView.LoadStarted -= _webViewLoadStarted;
					}
					_webView.Dispose();
				}
				_webView = null;
			});
		}

		void Finish()
		{
			InvokeOnMainThread(() =>
			{
				DismissViewController(true, null);
				_shareView.FinishAuth();
				Dispose();
			});
		}

		void SetFrame()
		{
			if (_headerView != null)
			{
				_headerView.Frame = new RectangleF(0, 20, View.Bounds.Width, 44);

				_backButton.Frame = new RectangleF(new PointF(10, _headerView.Frame.Height / 2 - _backButton.Frame.Height / 2), _backButton.Frame.Size);

				_logoImageView.Frame = new RectangleF(new PointF(_backButton.Frame.Right + 20, _headerView.Frame.Height / 2 - _logoImageView.Frame.Height / 2), 
					_logoImageView.Frame.Size);
			}
			if (_webView != null)
			{
				_webView.Frame = new RectangleF(0, _headerView.Frame.Bottom, View.Bounds.Width, View.Bounds.Height - _headerView.Frame.Bottom);
			}
			if (_splashLabel != null)
			{
				_splashLabel.SizeToFit();
				_splashLabel.Frame = new RectangleF((View.Bounds.Width - _splashLabel.Frame.Width) / 2, 20, _splashLabel.Frame.Width, _splashLabel.Frame.Height);
			}
		}

		void Initialize()
		{
			AutomaticallyAdjustsScrollViewInsets = false;
			var frame = new RectangleF(0, 20, View.Bounds.Width, 44);
			_headerView = new UIView(frame);
			_headerView.BackgroundColor = UIColor.Black;

			var image = new UIImage(NSData.FromFile("NavigationBar/Back.png"), 2);
			_backButton = new UIButton(new RectangleF(new PointF(10, _headerView.Frame.Height / 2 - image.Size.Height / 2), image.Size));
			_backButton.SetImage(image, UIControlState.Normal);
			_backButton.TouchUpInside += BackButtonTouchUp;
			_headerView.Add(_backButton);

			_logoImageView = new UIImageView(new UIImage(NSData.FromFile("NavigationBar/Logo.png"), 2));
			_logoImageView.Frame = new RectangleF(new PointF(_backButton.Frame.Right + 20, _headerView.Frame.Height / 2 - _logoImageView.Frame.Height / 2), 
				_logoImageView.Frame.Size);
			_headerView.Add(_logoImageView);

			View.Add(_headerView);
			_webView = new UIWebView(new RectangleF(0, _headerView.Frame.Bottom, View.Bounds.Width, View.Bounds.Height - _headerView.Frame.Bottom));
			_webView.ScrollView.Bounces = false;
			_webView.Hidden = false;
			Add(_webView);

			_splashLabel = new UILabel();
			_splashLabel.Text = "Завершение процесса авторизации";
			_splashLabel.Font = UIFont.BoldSystemFontOfSize (14);
			_splashLabel.TextColor = UIColor.Black;
			_splashLabel.SizeToFit();
			_splashLabel.Frame = new RectangleF((View.Bounds.Width - _splashLabel.Frame.Width) / 2, 20, _splashLabel.Frame.Width, _splashLabel.Frame.Height);
			_splashLabel.Hidden = true;
			Add (_splashLabel);

			Auth();
		}

		void BackButtonTouchUp(object sender, EventArgs e)
		{
			_shareView.OAuthResult = null;
			Finish();
		}

		void Auth()
		{
			var providerFind = false;
			var tempProvider = _provider;
			switch (tempProvider)
			{
				case "Evernote":
					providerFind = true;
					AuthEvernote();
					break;
				case "Readability":
					providerFind = true;
					AuthReadability();
					break;
				case "Pocket":
					providerFind = true;
					AuthPocket();
					break;
			}
			if (!providerFind)
			{
				_shareView.OAuthResult = null;
				Finish();
			}
		}

		private void AuthReadability()
		{
			var methodCalled = false;
			_webViewLoadStarted = (sender, e) =>
			{
				if (!methodCalled)
				{
					var loadedUrl = _webView.Request.Url.AbsoluteString;
					if (loadedUrl.StartsWith(ReadabilityAuthCallback))
					{
						methodCalled = true;
						_webView.StopLoading();
						ReadabilityOnRedirectComplited(loadedUrl);
						return;
					}
				}
			};
			_webView.LoadStarted += _webViewLoadStarted;
			BTProgressHUD.ShowToast ("Подготовительные действия...", ProgressHUD.MaskType.None, false, 2500);
			var oAuthBase = new OAuthBase();
			var nonce = oAuthBase.GenerateNonce();
			var timestamp = oAuthBase.GenerateTimeStamp();
			string normalizeUrl;
			string normalizeParam;
			var signature = oAuthBase.GenerateSignature(new Uri(ReadabilityRequestTokenUrl),
				ReadabilityConsumerKey, ReadabilityConsumerSecret,
				string.Empty, string.Empty, "GET", timestamp, nonce, OAuthBase.SignatureTypes.PLAINTEXT, out normalizeUrl,
				out normalizeParam);
			var url = ReadabilityRequestTokenUrl + "?oauth_version=1.0&oauth_consumer_key=" + ReadabilityConsumerKey + "&oauth_signature=" +
				signature + "&oauth_signature_method=PLAINTEXT&oauth_timestamp=" + timestamp +
				"&oauth_nonce=" + nonce + "&oauth_callback=" + Uri.EscapeDataString(ReadabilityAuthCallback);
			var getRequestTokenRequest = (HttpWebRequest)WebRequest.Create(url);
			getRequestTokenRequest.Method = "GET";
			try
			{
				var response = getRequestTokenRequest.GetResponse();
				var requestToken = string.Empty;
				var tokenSecret = string.Empty;
				if (response != null)
				{
					var stream = response.GetResponseStream();
					if (stream != null)
					{
						using (var sr = new StreamReader(stream, Encoding.UTF8))
						{
							var data = sr.ReadToEnd().Split(new[] {"&"}, StringSplitOptions.RemoveEmptyEntries);
							tokenSecret = data[0].Split(
								new[] {"="}, StringSplitOptions.RemoveEmptyEntries)[1];
							requestToken = data[1].Split(
								new[] {"="}, StringSplitOptions.RemoveEmptyEntries)[1];
							_tokenSecret = tokenSecret;
						}
					}
				}
				if (!string.IsNullOrEmpty(requestToken) && !string.IsNullOrEmpty(tokenSecret))
				{
					var authUrl = ReadabilityAuthUrl + "?oauth_token=" + requestToken + "&oauth_token_secret=" + tokenSecret;
					_webView.LoadRequest(new NSUrlRequest(new NSUrl(authUrl)));
				}
				else
				{
					_shareView.OAuthResult = null;
					Finish();
				}
			}
			catch (Exception ex)
			{
				_shareView.OAuthResult = null;
				Finish();
			}
		}

		private void ReadabilityOnRedirectComplited(string redirectUrl)
		{
			if (!string.IsNullOrEmpty(redirectUrl))
			{
				InvokeOnMainThread(() =>
				{
					_webView.Hidden = true;
					_splashLabel.Hidden = false;
					View.BringSubviewToFront(_splashLabel);
				});
				var data = redirectUrl.Split(new[] { "?" }, StringSplitOptions.RemoveEmptyEntries)[1].Split(new[] { "&" },
					StringSplitOptions.RemoveEmptyEntries);
				var oauthVerifier = data[0].Split(new[] {"="}, StringSplitOptions.RemoveEmptyEntries)[1];
				var oauthToken = data[1].Split(new[] {"="}, StringSplitOptions.RemoveEmptyEntries)[1];
				var oAuthBase = new OAuthBase();
				var nonce = oAuthBase.GenerateNonce();
				var timestamp = oAuthBase.GenerateTimeStamp();
				var tokenSecret = _tokenSecret;
				string normalizeUrl;
				string normalizeParam;
				var signature = oAuthBase.GenerateSignature(new Uri(ReadabilityAccessTokenUrl),
					ReadabilityConsumerKey, ReadabilityConsumerSecret,
					oauthToken, tokenSecret ?? string.Empty, "GET", timestamp, nonce, OAuthBase.SignatureTypes.PLAINTEXT,
					out normalizeUrl,
					out normalizeParam);
				var url = ReadabilityAccessTokenUrl + "?oauth_version=1.0&oauth_consumer_key=" + ReadabilityConsumerKey + "&oauth_signature=" +
					signature + "&oauth_signature_method=PLAINTEXT&oauth_timestamp=" + timestamp +
					"&oauth_nonce=" + nonce + "&oauth_token=" + oauthToken + "&oauth_verifier=" + oauthVerifier;
				var getRequestTokenRequest = (HttpWebRequest)WebRequest.Create(url);
				getRequestTokenRequest.Method = "GET";
				try
				{
					var response = getRequestTokenRequest.GetResponse();
					if (response != null)
					{
						var stream = response.GetResponseStream();
						if (stream != null)
						{
							using (var sr = new StreamReader(stream, Encoding.UTF8))
							{
								data = sr.ReadToEnd().Split(new[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
								var oTokenSecret =
									Uri.UnescapeDataString(
										data[0].Split(new[] {"="}, StringSplitOptions.RemoveEmptyEntries)[1]);
								var oToken =
									Uri.UnescapeDataString(
										data[1].Split(new[] {"="}, StringSplitOptions.RemoveEmptyEntries)[1]);

								var account = new Account();
								account.Properties.Add("oauth_token", oToken);
								account.Properties.Add("oauth_token_secret", oTokenSecret);
								var result = new OAuthResult()
								{
									IsAuthenticated = true,
									Account = account
								};
								_shareView.OAuthResult = result;
								Finish();
							}
						}
					}
				}
				catch (Exception ex)
				{
					_shareView.OAuthResult = null;
					Finish();
				}

			}
			else
			{
				_shareView.OAuthResult = null;
				Finish();
			}
		}

		private void AuthEvernote()
		{
			var methodCalled = false;
			_webViewLoadStarted = (sender, e) =>
			{
				if (!methodCalled)
				{
					var loadedUrl = _webView.Request.Url.AbsoluteString;
					if (loadedUrl.StartsWith(EvernoteAuthCallback))
					{
						methodCalled = true;
						_webView.StopLoading();
						EvernoteOnRedirectComplited(loadedUrl);
						return;
					}
				}
			};
			_webView.LoadStarted += _webViewLoadStarted;
			BTProgressHUD.ShowToast ("Подготовительные действия...", ProgressHUD.MaskType.None, false, 2500);
			var oAuthBase = new OAuthBase();
			var nonce = oAuthBase.GenerateNonce();
			var timestamp = oAuthBase.GenerateTimeStamp();
			string normalizeUrl;
			string normalizeParam;
			var signature = oAuthBase.GenerateSignature(new Uri(EvernoteRequestTokenUrl),
				EvernoteConsumerKey, EvernoteConsumerSecret,
				EvernoteConsumerKey, EvernoteConsumerSecret, "GET", timestamp, nonce, OAuthBase.SignatureTypes.PLAINTEXT, out normalizeUrl,
				out normalizeParam);
			var url = EvernoteRequestTokenUrl + "?oauth_consumer_key=" + EvernoteConsumerKey + "&oauth_signature=" +
				signature + "&oauth_signature_method=PLAINTEXT&oauth_timestamp=" + timestamp + 
				"&oauth_nonce=" + nonce + "&oauth_callback=" + Uri.EscapeDataString(EvernoteAuthCallback);
			var getRequestTokenRequest = (HttpWebRequest) WebRequest.Create(url);
			getRequestTokenRequest.Method = "GET";
			try
			{
				var response = getRequestTokenRequest.GetResponse();
				var requestToken = string.Empty;
				if (response != null)
				{
					var stream = response.GetResponseStream();
					if (stream != null)
					{
						using (var sr = new StreamReader(stream, Encoding.UTF8))
						{
							requestToken =
								sr.ReadToEnd().Split(new[] {"&"}, StringSplitOptions.RemoveEmptyEntries)[0].Split(
									new[] {"="}, StringSplitOptions.RemoveEmptyEntries)[1];

						} 
					}
				}
				if (!string.IsNullOrEmpty(requestToken))
				{
					var authUrl = EvernoteAuthUrl + "?oauth_token=" + requestToken;
					_webView.LoadRequest(new NSUrlRequest(new NSUrl(authUrl)));
				}
				else
				{
					_shareView.OAuthResult = null;
					Finish();
				}
			}
			catch (Exception ex)
			{
				_shareView.OAuthResult = null;
				Finish();
			}
		}

		private void EvernoteOnRedirectComplited(string redirectUrl)
		{
			if (!string.IsNullOrEmpty(redirectUrl))
			{
				InvokeOnMainThread(() =>
				{
					_webView.Hidden = true;
					_splashLabel.Hidden = false;
					View.BringSubviewToFront(_splashLabel);
				});
				var data = redirectUrl.Split(new[] { "?" }, StringSplitOptions.RemoveEmptyEntries)[1].Split(new[] { "&" },
					StringSplitOptions.RemoveEmptyEntries);
				var oauthToken = data[0].Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries)[1];
				var oauthVerifier = data[1].Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries)[1];
				var oAuthBase = new OAuthBase();
				var nonce = oAuthBase.GenerateNonce();
				var timestamp = oAuthBase.GenerateTimeStamp();
				string normalizeUrl;
				string normalizeParam;
				var signature = oAuthBase.GenerateSignature(new Uri(EvernoteRequestTokenUrl),
					EvernoteConsumerKey, EvernoteConsumerSecret,
					oauthToken, string.Empty, "GET", timestamp, nonce, OAuthBase.SignatureTypes.PLAINTEXT,
					out normalizeUrl,
					out normalizeParam);
				var url = EvernoteRequestTokenUrl + "?oauth_consumer_key=" + EvernoteConsumerKey + "&oauth_signature=" +
					signature + "&oauth_signature_method=PLAINTEXT&oauth_timestamp=" + timestamp +
					"&oauth_nonce=" + nonce + "&oauth_token=" + oauthToken + "&oauth_verifier=" + oauthVerifier;
				var getRequestTokenRequest = (HttpWebRequest)WebRequest.Create(url);
				getRequestTokenRequest.Method = "GET";
				try
				{
					var response = getRequestTokenRequest.GetResponse();
					if (response != null)
					{
						var stream = response.GetResponseStream();
						if (stream != null)
						{
							using (var sr = new StreamReader(stream, Encoding.UTF8))
							{
								data = sr.ReadToEnd().Split(new[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
								var oToken =
									Uri.UnescapeDataString(
										data[0].Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries)[1]);
								var edamUrl =
									Uri.UnescapeDataString(
										data[5].Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries)[1]);
								var account = new Account();
								account.Properties.Add("oauth_token", oToken);
								account.Properties.Add("edam_noteStoreUrl", edamUrl);
								var result = new OAuthResult()
								{
									IsAuthenticated = true,
									Account = account
								};
								_shareView.OAuthResult = result;
								Finish();
							}
						}
					}
				}
				catch (Exception ex)
				{
					_shareView.OAuthResult = null;
					Finish();
				}

			}
			else
			{
				_shareView.OAuthResult = null;
				Finish();
			}
		}

		private void AuthPocket()
		{
			var methodCalled = false;
			_webViewLoadStarted = (sender, e) =>
			{
				if (!methodCalled)
				{
					var loadedUrl = _webView.Request.Url.AbsoluteString;
					if (loadedUrl.StartsWith(PocketAuthCallback))
					{
						methodCalled = true;
						_webView.StopLoading();
						PocketOnRedirectComplited(loadedUrl);
						return;
					}
				}
			};
			_webView.LoadStarted += _webViewLoadStarted;
			BTProgressHUD.ShowToast ("Подготовительные действия...", ProgressHUD.MaskType.None, false, 2500);
			var req = (HttpWebRequest)WebRequest.Create("https://getpocket.com/v3/oauth/request");
			req.Method = "POST";
			var query = "consumer_key=" + PocketConsumerKey + "&redirect_uri=" + PocketAuthCallback;
			var bytes = Encoding.UTF8.GetBytes(query);
			req.ContentType = "application/x-www-form-urlencoded; charset=UTF8";
			req.ContentLength = bytes.Length;
			using (var reqStream = req.GetRequestStream())
			{
				reqStream.Write(bytes, 0, bytes.Length);
				reqStream.Flush();
			}
			var response = req.GetResponse();
			var stream = response.GetResponseStream();
			if (stream != null)
			{
				var data = string.Empty;
				using (var sr = new StreamReader(stream))
				{
					data = sr.ReadToEnd();
				}
				_tokenSecret = data.Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries)[1];
			}
			var authUrl =
				"https://getpocket.com/auth/authorize?request_token=" + _tokenSecret +
				"&redirect_uri=" + PocketAuthCallback;
			_webView.LoadRequest(new NSUrlRequest(new NSUrl(authUrl)));
		}

		private void PocketOnRedirectComplited(string redirectUrl)
		{
			if (string.IsNullOrEmpty(_tokenSecret))
			{
				InvokeOnMainThread(() =>
				{
					BTProgressHUD.ShowToast("Сбой процесса авторизации", ProgressHUD.MaskType.None, false, 2500);
					_shareView.OAuthResult = null;
					Finish();
				});
				return;
			}
			InvokeOnMainThread(() =>
			{
				_webView.Hidden = true;
				_splashLabel.Hidden = false;
				View.BringSubviewToFront(_splashLabel);
			});
			if (string.IsNullOrEmpty(_tokenSecret)) return;
			var req = (HttpWebRequest) WebRequest.Create("https://getpocket.com/v3/oauth/authorize");
			req.Method = "POST";
			var query = "consumer_key=" + PocketConsumerKey + "&code=" + _tokenSecret;
			var bytes = Encoding.UTF8.GetBytes(query);
			req.ContentType = "application/x-www-form-urlencoded; charset=UTF8";
			req.ContentLength = bytes.Length;
			using (var reqStream = req.GetRequestStream())
			{
				reqStream.Write(bytes, 0, bytes.Length);
				reqStream.Flush();
			}
			var response = req.GetResponse();
			var stream = response.GetResponseStream();
			var accessToken = string.Empty;
			if (stream != null)
			{
				var data = string.Empty;
				using (var sr = new StreamReader(stream))
				{
					data = sr.ReadToEnd();
				}
				accessToken =
					data.Split(new[] {"&"}, StringSplitOptions.RemoveEmptyEntries)[0].Split(new[] {"="},
						StringSplitOptions.RemoveEmptyEntries)[1];
			}
			var account = new Account();
			account.Properties.Add("oauth_token", accessToken);
			var result = new OAuthResult() {Account = account, IsAuthenticated = true};
			_shareView.OAuthResult = result;
			Finish();
		}

	}
}

