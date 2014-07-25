using System;
using MonoTouch.UIKit;
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.Foundation;
using System.Threading;
using ItExpert.Model;
using MonoTouch.MessageUI;
using ItExpert.ServiceLayer;
using ItExpert.Enum;
using BigTed;
using System.Web;
using System.Text;
using Xamarin.Auth;
using System.Text.RegularExpressions;
using Thrift.Transport;
using Thrift.Protocol;
using Evernote.EDAM.NoteStore;
using Evernote.EDAM.Type;
using System.Security.Cryptography;
using Evernote.EDAM.Error;
using System.Net;

namespace ItExpert
{
    public class ShareView : UIViewController
    {
		public class ViewWithButtons : UIView
        {
            enum ButtonsContainerType
            {
                Share,
                PutAside
            }

            public ViewWithButtons(RectangleF frame)
                :base(frame)
            {
                ContentMode = UIViewContentMode.Redraw;

                BackgroundColor = UIColor.Black;

                _padding = new UIEdgeInsets(10, 20, 10, 10);

                _shareTextView = ItExpertHelper.GetTextView(ItExpertHelper.GetAttributedString("Поделиться", UIFont.SystemFontOfSize(14), UIColor.White),
                    Frame.Width, new PointF(_padding.Left, _padding.Top));

                _shareTextView.BackgroundColor = UIColor.Clear;

                Add(_shareTextView);

                AddShareButtons();

                _putAsideTextView = ItExpertHelper.GetTextView(ItExpertHelper.GetAttributedString("Отложить для чтения", UIFont.SystemFontOfSize(14), UIColor.White),
                    Frame.Width, new PointF(_padding.Left, _shareButtonsContainer.Frame.Bottom + _padding.Top));

                _putAsideTextView.BackgroundColor = UIColor.Clear;

                Add(_putAsideTextView);

                AddPutAsideButtons();
            }

            public override void Draw(RectangleF rect)
            {
                base.Draw(rect);

                var context = UIGraphics.GetCurrentContext();

                context.SetLineWidth(2);

                if (_shareTextView != null)
                {
                    context.MoveTo(5, _shareTextView.Frame.Bottom + 2);
                    context.AddLineToPoint(Bounds.Width - 5, _shareTextView.Frame.Bottom + 2);
                    context.SetStrokeColor(UIColor.White.CGColor);
                    context.StrokePath();
                }

                if (_putAsideTextView != null)
                {
                    context.MoveTo(5, _putAsideTextView.Frame.Bottom + 2);
                    context.AddLineToPoint(Bounds.Width - 5, _putAsideTextView.Frame.Bottom + 2);
                    context.SetStrokeColor(UIColor.White.CGColor);
                    context.StrokePath();
                }
            }

            public void CorrectSubviewsFrames()
            {
                if (_shareButtonsContainer != null)
                {
                    _shareButtonsContainer.Frame = new RectangleF(new PointF(Frame.Width / 2 - _shareButtonsContainer.Frame.Width / 2, _shareButtonsContainer.Frame.Y), 
                        _shareButtonsContainer.Frame.Size);
                }

                if (_putAsideButtonsContainer != null)
                {
                    _putAsideButtonsContainer.Frame = new RectangleF(new PointF(Frame.Width / 2 - _putAsideButtonsContainer.Frame.Width / 2, _putAsideButtonsContainer.Frame.Y), 
                        _putAsideButtonsContainer.Frame.Size);
                }

                SetNeedsDisplay();
            }

            private void CreateEmailButton()
            {
                _emailButton = new UIView();

                UIImageView imageView = new UIImageView();

                var image = new UIImage(NSData.FromFile("NavigationBar/Mail.png"), 5);

                imageView.Image = image;

                UITextView textView = ItExpertHelper.GetTextView(ItExpertHelper.GetAttributedString("E-mail", UIFont.SystemFontOfSize(10), UIColor.White),
                    Frame.Width, new PointF(0, image.Size.Height));

                textView.BackgroundColor = UIColor.Clear;

                imageView.Frame = new RectangleF(new PointF(textView.Frame.Width / 2 - image.Size.Width / 2, 0), image.Size);

                _emailButton.Add(imageView);
                _emailButton.Add(textView);

                _emailButton.Frame = new RectangleF(0, 0, textView.Frame.Width, image.Size.Height + textView.Frame.Height);
				_emailButton.AddGestureRecognizer(new UITapGestureRecognizer(
					() => SendEmail()));
			}

			void SendEmail()
			{
				var article = ApplicationWorker.SharedArticle;
				var articleUrl = article.Url;
				if (articleUrl.StartsWith("/"))
				{
					articleUrl = Settings.Domen + article.Url;
				}
				var text = article.Name + "\r\n" + article.PreviewText + "\r\n" + articleUrl;
				var mailController = new MFMailComposeViewController ();
				mailController.SetSubject("Статья от It-Expert");
				mailController.SetMessageBody(text, false);
				mailController.Finished += ( object s, MFComposeResultEventArgs args) => {
					args.Controller.DismissViewController (true, null);
				};
				_parent.PresentViewController(mailController, true, null);
			}

            private void AddShareButtons()
            {
                int buttonsCount = 5;

                _shareButtons = new List<UIButton>(buttonsCount);
				var actions = new Action[] { ShareFacebook, ShareVk, ShareTwitter, ShareGooglePlus, ShareLinkedIn };
                _shareButtonsContainer = new UIView();

                SizeF contentSize = new SizeF();

                float buttonsOffset = 15;

                CreateEmailButton();

                _shareButtonsContainer.Add(_emailButton);

                contentSize.Height = _emailButton.Frame.Height;
                contentSize.Width += _emailButton.Frame.Width + buttonsOffset + 13;

                for (int i = 0; i < buttonsCount; i++)
                {
                    _shareButtons.Add(GetButton(i, ButtonsContainerType.Share));

                    _shareButtons[i].Frame = new RectangleF(new PointF(contentSize.Width, _emailButton.Frame.Height / 2 - _shareButtons[i].Frame.Height / 2), 
                        _shareButtons[i].Frame.Size);

                    contentSize.Width += _shareButtons[i].Frame.Width + buttonsOffset;

                    _shareButtonsContainer.Add(_shareButtons[i]);
					var action = actions[i];
					_shareButtons[i].TouchUpInside += (sender, e) => action();
                }

                contentSize.Width -= buttonsOffset;

                _shareButtonsContainer.Frame = new RectangleF(new PointF(Frame.Width / 2 - contentSize.Width / 2, _shareTextView.Frame.Bottom + _padding.Top), 
                    contentSize);
                _shareButtonsContainer.BackgroundColor = UIColor.Clear;

                Add(_shareButtonsContainer);
            }

			void ShareVk()
			{
				var article = ApplicationWorker.SharedArticle;
				var connectAccept = IsConnectionAccept();
				if (!connectAccept)
				{
					BTProgressHUD.ShowToast ("Нет доступных подключений, для указанных в настройках", ProgressHUD.MaskType.None, false);
					return;
				}
				if (_isOperation)
				{
					BTProgressHUD.ShowToast ("Выполняется операция", ProgressHUD.MaskType.None, false);
					return;
				}
				_toCommonWebOperation = true;
				_isOperation = true;
				var articleUrl = article.Url;
				if (articleUrl.StartsWith("/"))
				{
					articleUrl = Settings.Domen + article.Url;
				}
				var pictureUrl = string.Empty;
				if (article.PreviewPicture != null)
				{
					pictureUrl = article.PreviewPicture.Src;
					if (pictureUrl.StartsWith("/"))
					{
						pictureUrl = Settings.Domen + article.PreviewPicture.Src;
					}
				}
				var url = @"http://vk.com/share.php?url=" + HttpUtility.UrlEncode(articleUrl, Encoding.UTF8) + "&title=" +
					HttpUtility.UrlEncode(article.Name, Encoding.UTF8) +
					((!string.IsNullOrEmpty(pictureUrl))
						? "&image=" + HttpUtility.UrlEncode(pictureUrl, Encoding.UTF8)
						: string.Empty) +
					"&description=" + HttpUtility.UrlEncode(article.PreviewText, Encoding.UTF8) + "&noparse=true";
				UIApplication.SharedApplication.OpenUrl (new NSUrl (url));
			}

			void ShareFacebook()
			{
				var article = ApplicationWorker.SharedArticle;
				var connectAccept = IsConnectionAccept();
				if (!connectAccept)
				{
					BTProgressHUD.ShowToast ("Нет доступных подключений, для указанных в настройках", ProgressHUD.MaskType.None, false);
					return;
				}
				if (_isOperation)
				{
					BTProgressHUD.ShowToast ("Выполняется операция", ProgressHUD.MaskType.None, false);
					return;
				}
				_toCommonWebOperation = true;
				_isOperation = true;
				var articleUrl = article.Url;
				if (articleUrl.StartsWith("/"))
				{
					articleUrl = Settings.Domen + article.Url;
				}
				var url = "https://m.facebook.com/sharer.php?u=" + HttpUtility.UrlEncode(articleUrl, Encoding.UTF8);
				UIApplication.SharedApplication.OpenUrl (new NSUrl (url));
			}

			void ShareTwitter()
			{
				var article = ApplicationWorker.SharedArticle;
				var connectAccept = IsConnectionAccept();
				if (!connectAccept)
				{
					BTProgressHUD.ShowToast ("Нет доступных подключений, для указанных в настройках", ProgressHUD.MaskType.None, false);
					return;
				}
				if (_isOperation)
				{
					BTProgressHUD.ShowToast ("Выполняется операция", ProgressHUD.MaskType.None, false);
					return;
				}
				_toCommonWebOperation = true;
				_isOperation = true;
				var articleUrl = article.Url;
				if (articleUrl.StartsWith("/"))
				{
					articleUrl = Settings.Domen + article.Url;
				}
				var url = "http://twitter.com/share?text=" + HttpUtility.UrlEncode(article.Name, Encoding.UTF8) + "&url=" +
					HttpUtility.UrlEncode(articleUrl, Encoding.UTF8);
				UIApplication.SharedApplication.OpenUrl (new NSUrl (url));
			}

			void ShareGooglePlus()
			{
				var article = ApplicationWorker.SharedArticle;
				var connectAccept = IsConnectionAccept();
				if (!connectAccept)
				{
					BTProgressHUD.ShowToast ("Нет доступных подключений, для указанных в настройках", ProgressHUD.MaskType.None, false);
					return;
				}
				if (_isOperation)
				{
					BTProgressHUD.ShowToast ("Выполняется операция", ProgressHUD.MaskType.None, false);
					return;
				}
				_toCommonWebOperation = true;
				_isOperation = true;
				var articleUrl = article.Url;
				if (articleUrl.StartsWith("/"))
				{
					articleUrl = Settings.Domen + article.Url;
				}
				var url = "https://plus.google.com/share?url=" + HttpUtility.UrlEncode(articleUrl, Encoding.UTF8);
				UIApplication.SharedApplication.OpenUrl (new NSUrl (url));
			}

			void ShareLinkedIn()
			{
				var article = ApplicationWorker.SharedArticle;
				var connectAccept = IsConnectionAccept();
				if (!connectAccept)
				{
					BTProgressHUD.ShowToast ("Нет доступных подключений, для указанных в настройках", ProgressHUD.MaskType.None, false);
					return;
				}
				if (_isOperation)
				{
					BTProgressHUD.ShowToast ("Выполняется операция", ProgressHUD.MaskType.None, false);
					return;
				}
				_toCommonWebOperation = true;
				_isOperation = true;
				var articleUrl = article.Url;
				if (articleUrl.StartsWith("/"))
				{
					articleUrl = Settings.Domen + article.Url;
				}
				var url = "http://www.linkedin.com/shareArticle?mini=true&url=" +
					HttpUtility.UrlEncode(articleUrl, Encoding.UTF8) + "&title=" +
					HttpUtility.UrlEncode(article.Name, Encoding.UTF8) + "&summary=" +
					HttpUtility.UrlEncode(article.PreviewText, Encoding.UTF8) + "&source=" + HttpUtility.UrlEncode("It-Expert Magazine", Encoding.UTF8);
				UIApplication.SharedApplication.OpenUrl (new NSUrl (url));
			}
				
            private void CreateFavoriteButton()
            {
                _favoriteButton = new UIView();

				_favImageView = new UIImageView();

                var image = new UIImage(NSData.FromFile("NavigationBar/Favorite.png"), 2.5f);

				_favImageView.Image = image;

                UITextView textView = ItExpertHelper.GetTextView(ItExpertHelper.GetAttributedString("Избранное", UIFont.SystemFontOfSize(10), UIColor.White),
                    Frame.Width, new PointF(0, image.Size.Height));

                textView.BackgroundColor = UIColor.Clear;

				_favImageView.Frame = new RectangleF(new PointF(textView.Frame.Width / 2 - image.Size.Width / 2, 0), image.Size);

				_favoriteButton.Add(_favImageView);
                _favoriteButton.Add(textView);

                _favoriteButton.Frame = new RectangleF(0, 0, textView.Frame.Width, image.Size.Height + textView.Frame.Height);
				_favoriteButton.AddGestureRecognizer(new UITapGestureRecognizer(
					() => FavoriteButtonClick()));
			}

			public void SetFavoriteButtonState(bool state)
			{
				var value = (state) ? 1 : 0;
				UIImage image = null;
				if (state)
				{
					image = new UIImage(NSData.FromFile("NavigationBar/FavoriteOn.png"), 2.5f);
				}
				else
				{
					image = new UIImage(NSData.FromFile("NavigationBar/Favorite.png"), 2.5f);
				}
				if (image != null && _favImageView != null)
				{
					_favImageView.Image = image;
				}
				_favoriteButton.Tag = value;
			}

			private void FavoriteButtonClick()
			{
				var value = _favoriteButton.Tag;
				var state = (value == 1) ? true : false;
				var newState = !state;
				ApplicationWorker.SharedArticle.IsFavorite = newState;
				ThreadPool.QueueUserWorkItem(obj => ApplicationWorker.Db.UpdateArticle(ApplicationWorker.SharedArticle));
				SetFavoriteButtonState(newState);
			}

            public void AddPutAsideButtons()
            {
                int buttonsCount = 4;

                _putAsideButtons = new List<UIButton>(buttonsCount);

                _putAsideButtonsContainer = new UIView();
				var actions = new Action[] { ButEvernoteOnClick, ButReadabilityOnClick, ButPocketOnClick, ButInstapaperOnClick };
                SizeF contentSize = new SizeF();

                float buttonsOffset = 20;

                CreateFavoriteButton();

                _putAsideButtonsContainer.Add(_favoriteButton);

                contentSize.Height = _favoriteButton.Frame.Height;
                contentSize.Width += _favoriteButton.Frame.Width + buttonsOffset + 13;

                for (int i = 0; i < buttonsCount; i++)
                {
                    _putAsideButtons.Add(GetButton(i, ButtonsContainerType.PutAside));

                    _putAsideButtons[i].Frame = new RectangleF(new PointF(contentSize.Width, contentSize.Height / 2 - _putAsideButtons[i].Frame.Height / 2), 
                        _putAsideButtons[i].Frame.Size);

                    contentSize.Width += _putAsideButtons[i].Frame.Width + buttonsOffset;

                    _putAsideButtonsContainer.Add(_putAsideButtons[i]);
					var action = actions[i];
					_putAsideButtons[i].TouchUpInside += (sender, e) => action();
                }

                contentSize.Width -= buttonsOffset;

                _putAsideButtonsContainer.Frame = new RectangleF(new PointF(Frame.Width / 2 - contentSize.Width / 2, _putAsideTextView.Frame.Bottom + _padding.Top + 5), 
                    contentSize);
                _putAsideButtonsContainer.BackgroundColor = UIColor.Clear;

                Add(_putAsideButtonsContainer);
            }

			public void FinishAuth()
			{
				if (_toInstapaperAuth)
				{
					_toInstapaperAuth = false;
					if (OAuthResult != null && OAuthResult.IsAuthenticated)
					{
						var accountStore = AccountStore.Create();
						var accounts = accountStore.FindAccountsForService("Instapaper");
						foreach (var account in accounts)
						{
							accountStore.Delete(account, "Instapaper");
						}
						accountStore.Save(OAuthResult.Account, "Instapaper");
						var userName = OAuthResult.Account.Properties["username"];
						var password = string.Empty;
						if (OAuthResult.Account.Properties.ContainsKey("password"))
						{
							password = OAuthResult.Account.Properties["password"];
						}
						ShareInstapaper(userName, password);
					}
					else
					{
						BTProgressHUD.ShowToast ("Авторизация не удалась", ProgressHUD.MaskType.None, false);
						_isOperation = false;
					}
				}
				if (_toPocketAuth)
				{
					_toPocketAuth = false;
					if (OAuthResult != null && OAuthResult.IsAuthenticated)
					{
						var accountStore = AccountStore.Create();
						var accounts = accountStore.FindAccountsForService("Pocket");
						foreach (var account in accounts)
						{
							accountStore.Delete(account, "Pocket");
						}
						accountStore.Save(OAuthResult.Account, "Pocket");
						var token = OAuthResult.Account.Properties["oauth_token"];
						SharePocket(token);
					}
					else
					{
						BTProgressHUD.ShowToast ("Авторизация не удалась", ProgressHUD.MaskType.None, false);
						_isOperation = false;
					}
				}
				if (_toEvernoteAuth)
				{
					_toEvernoteAuth = false;
					if (OAuthResult != null && OAuthResult.IsAuthenticated)
					{
						var accountStore = AccountStore.Create();
						var accounts = accountStore.FindAccountsForService("Evernote");
						foreach (var account in accounts)
						{
							accountStore.Delete(account, "Evernote");
						}
						accountStore.Save(OAuthResult.Account, "Evernote");
						var token = OAuthResult.Account.Properties["oauth_token"];
						var url = OAuthResult.Account.Properties["edam_noteStoreUrl"];
						ShareEvernote(url, token);
					}
					else
					{
						BTProgressHUD.ShowToast ("Авторизация не удалась", ProgressHUD.MaskType.None, false);
						_isOperation = false;
					}
				}
				if (_toReadabilityAuth)
				{
					_toReadabilityAuth = false;
					if (OAuthResult != null && OAuthResult.IsAuthenticated)
					{
						var accountStore = AccountStore.Create();
						var accounts = accountStore.FindAccountsForService("Readability");
						foreach (var account in accounts)
						{
							accountStore.Delete(account, "Readability");
						}
						accountStore.Save(OAuthResult.Account, "Readability");
						var token = OAuthResult.Account.Properties["oauth_token"];
						var tokenSecret = OAuthResult.Account.Properties["oauth_token_secret"];
						ShareReadability(token, tokenSecret);
					}
					else
					{
						BTProgressHUD.ShowToast ("Авторизация не удалась", ProgressHUD.MaskType.None, false);
						_isOperation = false;
					}
				}
			}

			private void ButReadabilityOnClick()
			{
				var connectAccept = IsConnectionAccept();
				if (!connectAccept)
				{
					BTProgressHUD.ShowToast ("Нет доступных подключений, для указанных в настройках", ProgressHUD.MaskType.None, false);
					return;
				}
				if (_isOperation)
				{
					BTProgressHUD.ShowToast ("Выполняется операция", ProgressHUD.MaskType.None, false);
					return;
				}
				OAuthResult = null;
				_isOperation = true;
				var token = string.Empty;
				var tokenSecret = string.Empty;
				var isFind = false;
				var accounts = AccountStore.Create().FindAccountsForService("Readability");
				if (accounts != null)
				{
					foreach (var account in accounts)
					{
						if (account.Properties.ContainsKey("oauth_token") && account.Properties.ContainsKey("oauth_token_secret"))
						{
							token = account.Properties["oauth_token"];
							tokenSecret = account.Properties["oauth_token_secret"];
							isFind = true;
							break;
						}
					}
				}
				if (isFind && !string.IsNullOrWhiteSpace(token))
				{
					ShareReadability(token, tokenSecret);
				}
				else
				{
					_toReadabilityAuth = true;
					Auth("Readability");
				}
			}

			private void ButEvernoteOnClick()
			{
				var connectAccept = IsConnectionAccept();
				if (!connectAccept)
				{
					BTProgressHUD.ShowToast ("Нет доступных подключений, для указанных в настройках", ProgressHUD.MaskType.None, false);
					return;
				}
				if (_isOperation)
				{
					BTProgressHUD.ShowToast ("Выполняется операция", ProgressHUD.MaskType.None, false);
					return;
				}
				OAuthResult = null;
				_isOperation = true;
				var token = string.Empty;
				var url = string.Empty;
				var isFind = false;
				var accounts = AccountStore.Create().FindAccountsForService("Evernote");
				if (accounts != null)
				{
					foreach (var account in accounts)
					{
						if (account.Properties.ContainsKey("oauth_token") && account.Properties.ContainsKey("edam_noteStoreUrl"))
						{
							token = account.Properties["oauth_token"];
							url = account.Properties["edam_noteStoreUrl"];
							isFind = true;
							break;
						}
					}
				}
				if (isFind && !string.IsNullOrWhiteSpace(url) && !string.IsNullOrWhiteSpace(token))
				{
					ShareEvernote(url, token);
				}
				else
				{
					_toEvernoteAuth = true;
					Auth("Evernote");
				}
			}

			private void ButPocketOnClick()
			{
				var connectAccept = IsConnectionAccept();
				if (!connectAccept)
				{
					BTProgressHUD.ShowToast ("Нет доступных подключений, для указанных в настройках", ProgressHUD.MaskType.None, false);
					return;
				}
				if (_isOperation)
				{
					BTProgressHUD.ShowToast ("Выполняется операция", ProgressHUD.MaskType.None, false);
					return;
				}
				OAuthResult = null;
				_isOperation = true;
				var token = string.Empty;
				var isFind = false;
				var accounts = AccountStore.Create().FindAccountsForService("Pocket");
				if (accounts != null)
				{
					foreach (var account in accounts)
					{
						if (account.Properties.ContainsKey("oauth_token"))
						{
							token = account.Properties["oauth_token"];
							isFind = true;
							break;
						}
					}
				}
				if (isFind && !string.IsNullOrWhiteSpace(token))
				{
					SharePocket(token);
				}
				else
				{
					_toPocketAuth = true;
					Auth("Pocket");
				}
			}

			private void ButInstapaperOnClick()
			{
				var connectAccept = IsConnectionAccept();
				if (!connectAccept)
				{
					BTProgressHUD.ShowToast ("Нет доступных подключений, для указанных в настройках", ProgressHUD.MaskType.None, false);
					return;
				}
				if (_isOperation)
				{
					BTProgressHUD.ShowToast ("Выполняется операция", ProgressHUD.MaskType.None, false);
					return;
				}
				OAuthResult = null;
				_isOperation = true;
				var userName = string.Empty;
				var password = string.Empty;
				var isFind = false;
				var accounts = AccountStore.Create().FindAccountsForService("Instapaper");
				if (accounts != null)
				{
					foreach (var account in accounts)
					{
						if (account.Properties.ContainsKey("password"))
						{
							password = account.Properties["password"];
						}
						if (account.Properties.ContainsKey("username"))
						{
							userName = account.Properties["username"];
							isFind = true;
							break;
						}
					}
				}
				if (isFind && !string.IsNullOrWhiteSpace(userName))
				{
					ShareInstapaper(userName, password);
				}
				else
				{
					_toInstapaperAuth = true;
					var authController = new InstapaperLoginViewController(this);
					_parent.PresentViewController(authController, true, null);
				}
			}

			private void Auth(string provider)
			{
				InvokeOnMainThread(() => BTProgressHUD.ShowToast ("Авторизация", ProgressHUD.MaskType.None, false));
				Action action = () =>
				{
					Thread.Sleep(250);
					InvokeOnMainThread(() =>
					{
						var authController = new AuthViewController(this, provider);
						_parent.PresentViewController(authController, true, null);
					});
				};
				ThreadPool.QueueUserWorkItem(state=>action());
			}

			private void ShareEvernote(string url, string token)
			{
				BTProgressHUD.ShowToast("Добавление записи...", ProgressHUD.MaskType.None, false);
				Action action = () =>
				{
					var result = AddEveronteNote(url, token);
					ProcessResult(result, "Evernote");
					if (!result.IsComplete && result.IsAuthError)
					{
						_toEvernoteAuth = true;
						Auth("Evernote");
					}
				};
				ThreadPool.QueueUserWorkItem(state => action());
			}

			private void ShareReadability(string token, string tokenSecret)
			{
				BTProgressHUD.ShowToast("Добавление записи...", ProgressHUD.MaskType.None, false);
				Action action = () =>
				{
					var result = AddReadabilityNote(token, tokenSecret);
					ProcessResult(result, "Readability");
					if (!result.IsComplete && result.IsAuthError)
					{
						_toReadabilityAuth = true;
						Auth("Readability");
					}
				};
				ThreadPool.QueueUserWorkItem(state => action());
			}

			private void SharePocket(string token)
			{
				BTProgressHUD.ShowToast("Добавление записи...", ProgressHUD.MaskType.None, false);
				Action action = () =>
				{
					var result = AddPocketNote(token);
					ProcessResult(result, "Pocket");
					if (!result.IsComplete && result.IsAuthError)
					{
						_toPocketAuth = true;
						Auth("Pocket");
					}
				};
				ThreadPool.QueueUserWorkItem(state => action());   
			}

			private void ShareInstapaper(string username, string password)
			{
				BTProgressHUD.ShowToast("Добавление записи...", ProgressHUD.MaskType.None, false);
				Action action = () =>
				{
					var result = AddInstapaperNote(username, password);
					ProcessResult(result, "Instapaper");
					if (!result.IsComplete && result.IsAuthError)
					{
						_toInstapaperAuth = true;
						var authController = new InstapaperLoginViewController(this);
						_parent.PresentViewController(authController, true, null);
					}
				};
				ThreadPool.QueueUserWorkItem(state => action());
			}

			private void ProcessResult(AddNoteResult result, string provider)
			{
				var message = string.Empty;
				if (result.IsComplete)
				{
					message = "Запись добавлена в " + provider;
					_isOperation = false;
				}
				if (!result.IsComplete && !result.IsAuthError && !result.IsFormatError)
				{
					message = "Ошибка при добавлении записи в " + provider;
					_isOperation = false;
				}
				if (!result.IsComplete && !result.IsAuthError && result.IsFormatError)
				{
					message = "Ошибка при добавлении записи в " + provider + ", неверный формат";
					_isOperation = false;
				}
				if (!string.IsNullOrEmpty(message))
				{
					InvokeOnMainThread(
						() => BTProgressHUD.ShowToast(message, ProgressHUD.MaskType.None, false));
				}
			}

			private AddNoteResult AddEveronteNote(string url, string token)
			{
				var article = ApplicationWorker.SharedArticle;
				var result = new AddNoteResult() {IsAuthError = false, IsComplete = false, IsFormatError = false};
				var content = article.DetailText;
				try
				{
					if (!string.IsNullOrWhiteSpace(content))
					{

						content = content.Replace("<br>", "<br/>").Replace("</br>", "<br/>")
							.Replace("</ br>", "<br/>")
							.Replace("<br />", "<br/>");
						content = ApplicationWorker.RemoveImgIfNecessary(content);
						content = ApplicationWorker.AddHostForImg(content);
						content = ApplicationWorker.AddHostForLink(content);
						content = AddEndTagForImage(content);
						content = RemoveClasses(content);
						content = ReplaceAmpersand(content);
					}
					else
					{
						content = article.PreviewText;
					}
					var noteStoreTransport = new THttpClient(new Uri(url));
					var noteStoreProtocol = new TBinaryProtocol(noteStoreTransport);
					var noteStore = new NoteStore.Client(noteStoreProtocol);
					var note = new Note();
					note.Title = article.Name.Trim();
					if (note.Title.Length > 255)
					{
						note.Title = note.Title.Substring(0, 255);
					}
					var imageStr = string.Empty;
					if (article.DetailPicture != null)
					{
						var picture = article.DetailPicture;
						var image = Convert.FromBase64String(picture.Data);
						var hash = new MD5CryptoServiceProvider().ComputeHash(image);
						var data = new Data()
						{
							Size = image.Length,
							BodyHash = hash,
							Body = image
						};
						var resource = new Evernote.EDAM.Type.Resource();
						resource.Mime = "image/" + picture.Extension.ToString().ToLower();
						resource.Data = data;
						note.Resources = new List<Evernote.EDAM.Type.Resource>();
						note.Resources.Add(resource);
						var hashHex = BitConverter.ToString(hash).Replace("-", "").ToLower();
						imageStr = "<en-media type=\"" + resource.Mime + "\" hash=\"" + hashHex + "\"/><br/>";
					}
					note.Content = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
						"<!DOCTYPE en-note SYSTEM \"http://xml.evernote.com/pub/enml2.dtd\">" +
						"<en-note><b>" + article.Name + "</b><br/>" + imageStr + content +
						"</en-note>";
					try
					{
						noteStore.createNote(token, note);
						result.IsComplete = true;
					}
					catch (Exception ex)
					{
						result.IsComplete = false;
						var edamSystemException = ex as EDAMSystemException;
						if (edamSystemException != null)
						{
							var code = edamSystemException.ErrorCode;
							if (code == EDAMErrorCode.AUTH_EXPIRED || code == EDAMErrorCode.INVALID_AUTH)
							{
								result.IsAuthError = true;
							}
						}
						var edamUserException = ex as EDAMUserException;
						if (edamUserException != null)
						{
							var code = edamUserException.ErrorCode;
							if (code == EDAMErrorCode.ENML_VALIDATION)
							{
								result.IsFormatError = true;
							}
						}
					}
				}
				catch (Exception ex)
				{
					result.IsComplete = false;
				}
				return result;
			}

			private AddNoteResult AddReadabilityNote(string token, string tokenSecret)
			{
				var article = ApplicationWorker.SharedArticle;
				var result = new AddNoteResult() {IsAuthError = false, IsComplete = false, IsFormatError = false};
				var articleUrl = article.Url;
				if (articleUrl.StartsWith("/"))
				{
					articleUrl = Settings.Domen + article.Url;
				}
				var url = "https://www.readability.com/api/rest/v1/bookmarks";
				var oAuthBase = new OAuthBase();
				var timestamp = oAuthBase.GenerateTimeStamp();
				var nonce = oAuthBase.GenerateNonce();
				string normalizeUrl;
				string normalizeParam;
				var signature = oAuthBase.GenerateSignature(new Uri(url),
					AuthViewController.ReadabilityConsumerKey, AuthViewController.ReadabilityConsumerSecret,
					token, tokenSecret, "POST", timestamp, nonce, OAuthBase.SignatureTypes.PLAINTEXT, out normalizeUrl,
					out normalizeParam);
				var authHeaderValue = "OAuth realm=\"API\",oauth_signature=\"" + signature + "\",oauth_nonce=\"" + nonce +
					"\",oauth_timestamp=\"" + timestamp + "\",oauth_consumer_key=\"" 
					+ AuthViewController.ReadabilityConsumerKey + "\",oauth_token=\"" +
					token + "\",oauth_version=\"1.0\",oauth_signature_method=\"PLAINTEXT\"";
				var req = (HttpWebRequest)WebRequest.Create(url);
				req.Headers.Add("Authorization", authHeaderValue);
				req.Method = "POST";
				var query = "&url=" + articleUrl +
					"&favorite=0&archive=0&allow_duplicates=1";
				var bytes = Encoding.UTF8.GetBytes(query);
				req.ContentType = "application/x-www-form-urlencoded";
				req.ContentLength = bytes.Length;
				using (var reqStream = req.GetRequestStream())
				{
					reqStream.Write(bytes, 0, bytes.Length);
					reqStream.Flush();
				}
				try
				{
					var response = req.GetResponse() as HttpWebResponse;
					if (response != null)
					{
						var code = response.StatusCode;
						if (code == HttpStatusCode.Accepted || code == HttpStatusCode.Created)
						{
							result.IsComplete = true;
						}
					}
				}
				catch (Exception ex)
				{
					var webException = ex as WebException;
					if (webException != null)
					{
						var response = webException.Response as HttpWebResponse;
						if (response != null)
						{
							var code = response.StatusCode;
							if (code == HttpStatusCode.Unauthorized)
							{
								result.IsAuthError = true;
							}
						}
					}
					result.IsComplete = false;
				}
				return result;
			}

			private AddNoteResult AddPocketNote(string token)
			{
				var article = ApplicationWorker.SharedArticle;
				var result = new AddNoteResult() {IsAuthError = false, IsComplete = false, IsFormatError = false};
				var articleUrl = article.Url;
				if (articleUrl.StartsWith("/"))
				{
					articleUrl = Settings.Domen + article.Url;
				}
				var req = (HttpWebRequest)WebRequest.Create("https://getpocket.com/v3/add");
				req.Method = "POST";
				var query = "{\"url\":\""+ articleUrl +"\",\"consumer_key\":\"" +
					AuthViewController.PocketConsumerKey + "\",\"access_token\":\"" + token + "\"}";
				var bytes = Encoding.UTF8.GetBytes(query);
				req.Headers.Add("X-Accept", "application/json");
				req.ContentType = "application/json; charset=UTF8";
				req.ContentLength = bytes.Length;
				using (var reqStream = req.GetRequestStream())
				{
					reqStream.Write(bytes, 0, bytes.Length);
					reqStream.Flush();
				}
				try
				{
					var response = req.GetResponse();
					var httpWebResponse = response as HttpWebResponse;
					if (httpWebResponse != null)
					{
						if (httpWebResponse.StatusCode == HttpStatusCode.OK)
						{
							result.IsComplete = true;
						}
					}
				}
				catch (Exception ex)
				{
					result.IsComplete = false;
					var webEx = ex as WebException;
					if (webEx != null)
					{
						var response = webEx.Response as HttpWebResponse;
						if (response != null && response.StatusCode == HttpStatusCode.Unauthorized)
						{
							result.IsAuthError = true;
						}
					}
				}
				return result;
			}

			private AddNoteResult AddInstapaperNote(string username, string password)
			{
				var article = ApplicationWorker.SharedArticle;
				var result = new AddNoteResult() {IsAuthError = false, IsComplete = false, IsFormatError = false};
				var articleUrl = article.Url;
				if (articleUrl.StartsWith("/"))
				{
					articleUrl = Settings.Domen + article.Url;
				}
				if (string.IsNullOrWhiteSpace(password))
				{
					password = "password";
				}
				var authString = username + ":" + password;
				var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(authString));
				var req = (HttpWebRequest)WebRequest.Create("https://www.instapaper.com/api/add");
				req.Headers.Add("Authorization", " Basic " + base64);
				req.Method = "POST";
				var query = "url=" + articleUrl;
				var bytes = Encoding.UTF8.GetBytes(query);
				req.ContentType = "application/x-www-form-urlencoded";
				req.ContentLength = bytes.Length;
				using (var reqStream = req.GetRequestStream())
				{
					reqStream.Write(bytes, 0, bytes.Length);
					reqStream.Flush();
				}
				try
				{
					var response = req.GetResponse();
					var httpWebResponse = response as HttpWebResponse;
					if (httpWebResponse != null)
					{
						if (httpWebResponse.StatusCode == HttpStatusCode.Created)
						{
							result.IsComplete = true;
						}
					}
				}
				catch (Exception ex)
				{
					result.IsComplete = false;
					var webEx = ex as WebException;
					if (webEx != null)
					{
						var response = webEx.Response as HttpWebResponse;
						if (response != null && (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized))
						{
							result.IsAuthError = true;
						}
					}
				}
				return result;
			}


			private string RemoveClasses(string data)
			{
				var returnData = data;
				var patterns = new[]
				{
					new {Pattern = @"class=""\S*""", Index = 0},
					new {Pattern = @"class\s=""\S*""", Index = 0},
					new {Pattern = @"class=\s""\S*""", Index = 0},
					new {Pattern = @"class='\S*'", Index = 0},
					new {Pattern = @"class\s='\S*'", Index = 0},
					new {Pattern = @"class=\s'\S*'", Index = 0}
				};
				foreach (var pattern in patterns)
				{
					var regex = new Regex(pattern.Pattern);
					returnData = regex.Replace(returnData, string.Empty);
				}
				return returnData;
			}

			private string AddEndTagForImage(string data)
			{
				var result = data;
				var regex = new Regex(@"<img\s+[^>]*>");
				var matches = regex.Matches(result);
				var count = 0;
				foreach (Match match in matches)
				{
					var value = match.Value;
					if (!value.EndsWith("/>"))
					{
						value = value.Substring(0, value.Length - 2) + "/>";
						count++;
						result = result.Remove(match.Index + count, match.Length);
						result = result.Insert(match.Index + count, value);
					}
				}
				return result;
			}

			private string ReplaceAmpersand(string data)
			{
				var returnData = data;
				var patterns = new[]
				{
					new {Pattern = @"href=""\S*""", Index = 0},
					new {Pattern = @"href\s=""\S*""", Index = 0},
					new {Pattern = @"href=\s""\S*""", Index = 0},
					new {Pattern = @"href='\S*'", Index = 0},
					new {Pattern = @"href\s='\S*'", Index = 0},
					new {Pattern = @"href=\s'\S*'", Index = 0},
					new {Pattern = @"src=""\S*""", Index = 0},
					new {Pattern = @"src\s=""\S*""", Index = 0},
					new {Pattern = @"src=\s""\S*""", Index = 0},
					new {Pattern = @"src='\S*'", Index = 0},
					new {Pattern = @"src\s='\S*'", Index = 0},
					new {Pattern = @"src=\s'\S*'", Index = 0}
				};
				foreach (var pattern in patterns)
				{
					var count = 0;
					var regex = new Regex(pattern.Pattern);
					var matches = regex.Matches(returnData);
					foreach (Match match in matches)
					{
						var length = match.Length;
						var value = match.Value;
						value = value.Replace("&", "&amp;");
						var newLength = value.Length;
						if (newLength != length)
						{
							returnData = returnData.Remove(match.Index, match.Length);
							returnData = returnData.Insert(match.Index + count, value);
							count += (newLength - length);
						}
					}
				}
				return returnData;
			}

            private UIButton GetButton(int index, ButtonsContainerType containerType)
            {
                UIButton button = new UIButton();

                var image = new UIImage(GetButtonImageData(index, containerType), 2.5f);

                button.SetImage(image, UIControlState.Normal);
                button.AdjustsImageWhenHighlighted = false;

                button.Frame = new RectangleF(new PointF(), image.Size);

                return button;
            }

            private NSData GetButtonImageData(int index, ButtonsContainerType containerType)
            {
                if (containerType == ButtonsContainerType.Share)
                {
                    switch (index)
                    {
                        case 0:
                            return NSData.FromFile("NavigationBar/Facebook.png");

                        case 1:
                            return NSData.FromFile("NavigationBar/Vk.png");

                        case 2:
                            return NSData.FromFile("NavigationBar/Twitter.png");

                        case 3:
                            return NSData.FromFile("NavigationBar/Google.png");

                        case 4:
                            return NSData.FromFile("NavigationBar/Linkedin.png");

                        default:
                            throw new NotImplementedException("Wrong index for share button.");
                    }
                }
                else if (containerType == ButtonsContainerType.PutAside)
                {
                    switch (index)
                    {
                        case 0:
                            return NSData.FromFile("NavigationBar/Evernote.png");

                        case 1:
                            return NSData.FromFile("NavigationBar/Readability.png");

                        case 2:
                            return NSData.FromFile("NavigationBar/Pocket.png");

                        case 3:
                            return NSData.FromFile("NavigationBar/Instapaper.png");

                        default:
                            throw new NotImplementedException("Wrong index for share button.");
                    }
                }

                return null;
            }

			public void SetNavigationController(UINavigationController navController)
			{
				_navigationController = navController;
			}

			public void SetParent(ShareView parent)
			{
				_parent = parent;
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



            private UIEdgeInsets _padding;

            private UITextView _shareTextView;
            private UITextView _putAsideTextView;

            private List<UIButton> _shareButtons;
            private List<UIButton> _putAsideButtons;

			private UIImageView _favImageView;
            private UIView _shareButtonsContainer;
            private UIView _putAsideButtonsContainer;
            private UIView _emailButton;
            private UIView _favoriteButton;
			private UINavigationController _navigationController;
			private ShareView _parent;
			private bool _isOperation;
			private bool _toCommonWebOperation;
			private bool _toEvernoteAuth = false;
			private bool _toReadabilityAuth = false;
			private bool _toPocketAuth = false;
			private bool _toInstapaperAuth = false;
			public OAuthResult OAuthResult = null;

			private class AddNoteResult
			{
				public bool IsComplete { get; set; }

				public bool IsAuthError { get; set; }

				public bool IsFormatError { get; set; }
			}
        }

        public event EventHandler TapOutsideTableView;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            AutomaticallyAdjustsScrollViewInsets = false;

            _navigationController = (UIApplication.SharedApplication.KeyWindow.RootViewController as UINavigationController);

            AddHeaderView();

            _viewWithButtons = new ViewWithButtons(new RectangleF(0, ItExpertHelper.StatusBarHeight + _navigationController.NavigationBar.Frame.Height,
                View.Frame.Width, 175));
			_viewWithButtons.SetNavigationController(_navigationController);
			_viewWithButtons.SetParent(this);
            View.Add(_viewWithButtons);

            AddTapView();

        }

		public void Update()
		{
			_viewWithButtons.SetFavoriteButtonState(ApplicationWorker.SharedArticle.IsFavorite);
		}

        public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
        {
            base.DidRotate(fromInterfaceOrientation);

            var screenSize = ItExpertHelper.GetRealScreenSize();

            if (_viewWithButtons != null)
            {
                _viewWithButtons.Frame = new RectangleF(0, ItExpertHelper.StatusBarHeight + _navigationController.NavigationBar.Frame.Height,
                    screenSize.Width, 175);

                _viewWithButtons.CorrectSubviewsFrames();

                if (_tapableView != null)
                {
                    _tapableView.Frame = new RectangleF(0, _viewWithButtons.Frame.Bottom, screenSize.Width, screenSize.Height - _viewWithButtons.Frame.Bottom);
                }
            }

            if (_headerView != null)
            {
                _headerView.Frame = _navigationController.NavigationBar.Frame;

                _backButton.Frame = new RectangleF(new PointF(10, _headerView.Frame.Height / 2 - _backButton.Frame.Height / 2), _backButton.Frame.Size);

                _logoImageView.Frame = new RectangleF(new PointF(_backButton.Frame.Right + 20, _headerView.Frame.Height / 2 - _logoImageView.Frame.Height / 2), 
                    _logoImageView.Frame.Size);
            }
        }

        private void AddHeaderView()
        {
            _headerView = new UIView(_navigationController.NavigationBar.Frame);

			_headerView.BackgroundColor = UIColor.Black;

            var image = new UIImage(NSData.FromFile("NavigationBar/Back.png"), 2);

            _backButton = new UIButton(new RectangleF(new PointF(10, _headerView.Frame.Height / 2 - image.Size.Height / 2), image.Size));

            _backButton.SetImage(image, UIControlState.Normal);
            _backButton.TouchUpInside += (sender, e) => OnTapOutsideTableView();

            _headerView.Add(_backButton);

            _logoImageView = new UIImageView(new UIImage(NSData.FromFile("NavigationBar/Logo.png"), 2));

            _logoImageView.Frame = new RectangleF(new PointF(_backButton.Frame.Right + 20, _headerView.Frame.Height / 2 - _logoImageView.Frame.Height / 2), 
                _logoImageView.Frame.Size);

            _headerView.Add(_logoImageView);

            View.Add(_headerView);
        }

        private void AddTapView()
        {
            _tapableView = new UIView(new RectangleF(0, _viewWithButtons.Frame.Bottom, View.Frame.Width, View.Frame.Height - _viewWithButtons.Frame.Bottom));

            _tapableView.BackgroundColor = UIColor.Clear;
            _tapableView.AddGestureRecognizer(new UITapGestureRecognizer(() =>
            {
                OnTapOutsideTableView();
            }));

            View.Add(_tapableView);
        }

		public void OnTapOutsideTableView()
        {

            if (TapOutsideTableView != null)
            {
                TapOutsideTableView(this, EventArgs.Empty);
            }
        }

        private ViewWithButtons _viewWithButtons;
        private UIImageView _logoImageView;
        private UIView _headerView;
        private UIButton _backButton;
        private UINavigationController _navigationController;
        private UIView _tapableView;
    }

	public class OAuthResult
	{
		public bool IsAuthenticated { get; set; }

		public Account Account { get; set; }
	}
}

