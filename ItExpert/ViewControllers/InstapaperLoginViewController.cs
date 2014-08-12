using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;
using Xamarin.Auth;
using BigTed;

namespace ItExpert
{
	public class InstapaperLoginViewController : UIViewController
	{
		private UIImageView _logoImageView;
		private UIView _headerView;
		private UIButton _backButton;
		private ItExpert.ShareView.ViewWithButtons _shareView;
		private UIView _root;

		public InstapaperLoginViewController (ItExpert.ShareView.ViewWithButtons shareView)
		{
			_shareView = shareView;
		}

		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate (fromInterfaceOrientation);
			UpdateViews ();
		}

		static bool UserInterfaceIdiomIsPhone {
			get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			// Perform any additional setup after loading the view, typically from a nib.
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			Initialize ();
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

				if (_headerView != null)
				{
					_headerView.RemoveFromSuperview();
					_headerView.Dispose();
				}
				_headerView = null;

				if (_labelName != null)
				{
					_labelName.RemoveFromSuperview();
					_labelName.Dispose();
				}
				_labelName = null;

				if (_labelPassword != null)
				{
					_labelPassword.RemoveFromSuperview();
					_labelPassword.Dispose();
				}
				_labelPassword = null;

				if (_logo != null)
				{
					_logo.RemoveFromSuperview();
					if (_logo.Image != null)
					{
						_logo.Image.Dispose();
						_logo.Image = null;
					}
					_logo.Dispose();
				}
				_logo = null;

				if (_txtName != null)
				{
					_txtName.ShouldReturn -= TextFieldShouldReturn;
					_txtName.RemoveFromSuperview();
					_txtName.Dispose();
				}
				_txtName = null;

				if (_txtPassword != null)
				{
					_txtPassword.ShouldReturn -= TextFieldShouldReturn;
					_txtPassword.RemoveFromSuperview();
					_txtPassword.Dispose();
				}
				_txtPassword = null;

				if (_button != null)
				{
					_button.TouchUpInside -= Login;
					_button.RemoveFromSuperview();
					_button.Dispose();
				}
				_button = null;

				if (_root != null)
				{
					_root.RemoveFromSuperview();
					_root.Dispose();
				}
				_root = null;
			});
		}

		void Finish()
		{
			DismissViewController(true, null);
			_shareView.FinishAuth();
			Dispose();
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

			_root = new UIView(new RectangleF(0, _headerView.Frame.Bottom, View.Bounds.Width, View.Bounds.Height - _headerView.Frame.Bottom));
			_root.BackgroundColor = UIColor.White;
			View.Add(_root);

			var logoWidth = 280;
			var txtWidth = 200;
			_logo = new UIImageView (new RectangleF ((View.Bounds.Width - logoWidth) / 2, 30, logoWidth, 60));
			_logo.Image = new UIImage (NSData.FromFile ("InstapaperLogo.png"), 1f);
			_root.Add (_logo);
			_labelName = new UILabel();
			_labelName.Text = "Имя пользователя или email";
			_labelName.TextColor = UIColor.Black;
			_labelName.Font = UIFont.BoldSystemFontOfSize(18);
			_labelName.SizeToFit();
			_labelName.Frame = new RectangleF(View.Bounds.Width / 2 - _labelName.Frame.Width / 2, _logo.Frame.Bottom + 10, _labelName.Frame.Width, _labelName.Frame.Height);
			_root.Add (_labelName);
			_txtName = new UITextField (new RectangleF ((View.Bounds.Width - txtWidth) / 2, _labelName.Frame.Bottom + 4, txtWidth, 30));
			_txtName.BackgroundColor = UIColor.FromRGB(220, 220, 220);
			_txtName.ReturnKeyType = UIReturnKeyType.Done;
			_txtName.ShouldReturn += TextFieldShouldReturn;
			_root.Add (_txtName);
			_labelPassword = new UILabel();
			_labelPassword.Text = "Пароль, если есть";
			_labelPassword.TextColor = UIColor.Black;
			_labelPassword.Font = UIFont.BoldSystemFontOfSize(18);
			_labelPassword.SizeToFit();
			_labelPassword.Frame = new RectangleF(View.Bounds.Width / 2 - _labelPassword.Frame.Width / 2, _txtName.Frame.Bottom + 10, _labelPassword.Frame.Width, _labelPassword.Frame.Height);
			_root.Add (_labelPassword);
			_txtPassword = new UITextField (new RectangleF ((View.Bounds.Width - txtWidth) / 2, _labelPassword.Frame.Bottom + 4, txtWidth, 30));
			_txtPassword.BackgroundColor = UIColor.FromRGB(220, 220, 220);
			_txtPassword.SecureTextEntry = true;
			_txtPassword.ReturnKeyType = UIReturnKeyType.Done;
			_txtPassword.ShouldReturn += TextFieldShouldReturn;
			_root.Add (_txtPassword);
			_button = new UIButton (UIButtonType.RoundedRect);
			_button.SetTitle ("Вход", UIControlState.Normal);
			_button.Frame = new RectangleF ((View.Bounds.Width - 100) / 2, _txtPassword.Frame.Bottom + 10, 100, 40);
			_button.TouchUpInside += Login;
			_root.Add (_button);
		}

		void BackButtonTouchUp(object sender, EventArgs e)
		{
			_shareView.OAuthResult = null;
			Finish();
		}

		bool TextFieldShouldReturn(UITextField textField)
		{
			textField.ResignFirstResponder();
			return true;
		}

		void Login(object sender, EventArgs e)
		{
			var username = _txtName.Text;
			var password = _txtPassword.Text;
			if (string.IsNullOrWhiteSpace(username))
			{
				BTProgressHUD.ShowToast("Введите имя пользователя", ProgressHUD.MaskType.None, false, 2500);
				return;
			}
			var account = new Account();
			account.Properties.Add("username", username);
			account.Properties.Add("password", password);
			var result = new OAuthResult() { Account = account, IsAuthenticated = true };
			_shareView.OAuthResult = result;
			Finish();
		}

		void UpdateViews()
		{
			_root.Frame = new RectangleF(0, _headerView.Frame.Bottom, View.Bounds.Width, View.Bounds.Height - _headerView.Frame.Bottom);
			if (_headerView != null)
			{
				_headerView.Frame = new RectangleF(0, 20, View.Bounds.Width, 44);

				_backButton.Frame = new RectangleF(new PointF(10, _headerView.Frame.Height / 2 - _backButton.Frame.Height / 2), _backButton.Frame.Size);

				_logoImageView.Frame = new RectangleF(new PointF(_backButton.Frame.Right + 20, _headerView.Frame.Height / 2 - _logoImageView.Frame.Height / 2), 
					_logoImageView.Frame.Size);
			}
			var logoWidth = 280;
			var txtWidth = 200;
			if (_logo != null)
			{
				_logo.Frame = new RectangleF ((View.Bounds.Width - logoWidth) / 2, 30, logoWidth, 60);
			}
			if (_labelName != null)
			{
				_labelName.Frame = new RectangleF(View.Bounds.Width / 2 - _labelName.Frame.Width / 2, _logo.Frame.Bottom + 10, _labelName.Frame.Width, _labelName.Frame.Height);
			}
			if (_txtName != null)
			{
				_txtName.Frame = new RectangleF ((View.Bounds.Width - txtWidth) / 2, _labelName.Frame.Bottom + 4, txtWidth, 30);
			}
			if (_labelPassword != null)
			{
				_labelPassword.Frame = new RectangleF(View.Bounds.Width / 2 - _labelPassword.Frame.Width / 2, _txtName.Frame.Bottom + 10, _labelPassword.Frame.Width, _labelPassword.Frame.Height);
			}
			if (_txtPassword != null)
			{
				_txtPassword.Frame = new RectangleF ((View.Bounds.Width - txtWidth) / 2, _labelPassword.Frame.Bottom + 4, txtWidth, 30);
			}
			if (_button != null)
			{
				_button.Frame = new RectangleF ((View.Bounds.Width - 100) / 2, _txtPassword.Frame.Bottom + 10, 100, 30);
			}
		}

		private UIImageView _logo;
		private UILabel _labelName;
		private UITextField _txtName;
		private UILabel _labelPassword;
		private UITextField _txtPassword;
		private UIButton _button;
	}
}

