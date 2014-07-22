using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;

namespace ItExpert
{
	public class InstapaperLoginViewController : UIViewController
	{
		public InstapaperLoginViewController ()
		{
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
			AutomaticallyAdjustsScrollViewInsets = false;
			NavigationController.NavigationBarHidden = true;
			View.BackgroundColor = UIColor.White;
			// Perform any additional setup after loading the view, typically from a nib.
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			Initialize ();
		}

		void Initialize()
		{
			var logoWidth = 280;
			var txtWidth = 200;
			_logo = new UIImageView (new RectangleF ((View.Bounds.Width - logoWidth) / 2, 30, logoWidth, 60));
			_logo.Image = new UIImage (NSData.FromFile ("InstapaperLogo.png"), 1f);
			Add (_logo);
			_labelName = new UILabel();
			_labelName.Text = "Имя пользователя или email";
			_labelName.TextColor = UIColor.Black;
			_labelName.Font = UIFont.BoldSystemFontOfSize(18);
			_labelName.SizeToFit();
			_labelName.Frame = new RectangleF(View.Bounds.Width / 2 - _labelName.Frame.Width / 2, _logo.Frame.Bottom + 10, _labelName.Frame.Width, _labelName.Frame.Height);
			Add (_labelName);
			_txtName = new UITextField (new RectangleF ((View.Bounds.Width - txtWidth) / 2, _labelName.Frame.Bottom + 4, txtWidth, 30));
			_txtName.BackgroundColor = UIColor.FromRGB(220, 220, 220);
			_txtName.ReturnKeyType = UIReturnKeyType.Done;
			_txtName.ShouldReturn += (textField) => 
			{ 
				textField.ResignFirstResponder();
				return true; 
			};
			Add (_txtName);
			_labelPassword = new UILabel();
			_labelPassword.Text = "Пароль, если есть";
			_labelPassword.TextColor = UIColor.Black;
			_labelPassword.Font = UIFont.BoldSystemFontOfSize(18);
			_labelPassword.SizeToFit();
			_labelPassword.Frame = new RectangleF(View.Bounds.Width / 2 - _labelPassword.Frame.Width / 2, _txtName.Frame.Bottom + 10, _labelPassword.Frame.Width, _labelPassword.Frame.Height);
			Add (_labelPassword);
			_txtPassword = new UITextField (new RectangleF ((View.Bounds.Width - txtWidth) / 2, _labelPassword.Frame.Bottom + 4, txtWidth, 30));
			_txtPassword.BackgroundColor = UIColor.FromRGB(220, 220, 220);
			_txtPassword.SecureTextEntry = true;
			_txtPassword.ReturnKeyType = UIReturnKeyType.Done;
			_txtPassword.ShouldReturn += (textField) => 
			{ 
				textField.ResignFirstResponder();
				return true; 
			};
			Add (_txtPassword);
			_button = new UIButton (UIButtonType.RoundedRect);
			_button.SetTitle ("Вход", UIControlState.Normal);
			_button.Frame = new RectangleF ((View.Bounds.Width - 100) / 2, _txtPassword.Frame.Bottom + 10, 100, 40);
			_button.TouchUpInside += Login;
			Add (_button);
		}

		void Login(object sender, EventArgs e)
		{
			Toast.MakeText (this, "Login to Instapaper", 1500).Show ();
		}

		void UpdateViews()
		{
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

