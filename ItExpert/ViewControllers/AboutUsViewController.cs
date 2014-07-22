using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;

namespace ItExpert
{
	public class AboutUsViewController : UIViewController
	{
		public AboutUsViewController ()
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
			NavigationController.NavigationBarHidden = true;
			AutomaticallyAdjustsScrollViewInsets = false;

			// Perform any additional setup after loading the view, typically from a nib.
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			Initialize ();
		}

		void Initialize()
		{
			View.BackgroundColor = UIColor.FromRGB(240, 240, 240);
			_padding = new UIEdgeInsets (8, 8, 8, 8);
			_scrollView = new UIScrollView(new RectangleF(0, 0, View.Frame.Width, View.Frame.Height));
			_scrollView.UserInteractionEnabled = true;
			_scrollView.ScrollEnabled = true;

			View.Add(_scrollView);
			View.UserInteractionEnabled = true;
			var logo = new UIImageView (new RectangleF (_padding.Left, 10, 100, 20));
			logo.Image = new UIImage (NSData.FromFile ("AboutUs.png"), 1f);
			_scrollView.Add (logo);

			var label1 = new UILabel();
			label1.Text = "Контакты";
			label1.Font = UIFont.BoldSystemFontOfSize (16);
			label1.TextColor = UIColor.Black;
			label1.SizeToFit();
			label1.Frame = new RectangleF(_padding.Left, logo.Frame.Bottom + 6, label1.Frame.Width, label1.Frame.Height);
			_scrollView.Add (label1);

			var label2 = new UILabel();
			label2.Text = "Санкт-Петербург:";
			label2.Font = UIFont.BoldSystemFontOfSize (14);
			label2.TextColor = UIColor.Black;
			label2.SizeToFit();
			label2.Frame = new RectangleF(_padding.Left, label1.Frame.Bottom + 4, label2.Frame.Width, label2.Frame.Height);
			_scrollView.Add (label2);

			var label3 = new UILabel();
			label3.Text = "+ 7 (812) 438-15-38";
			label3.Font = UIFont.SystemFontOfSize (14);
			label3.TextColor = UIColor.Black;
			label3.SizeToFit();
			label3.Frame = new RectangleF(_padding.Left + label2.Frame.Width + 4, label1.Frame.Bottom + 4, label3.Frame.Width, label3.Frame.Height);
			_scrollView.Add (label3);

			var label4 = new UILabel();
			label4.Text = "Москва:";
			label4.Font = UIFont.BoldSystemFontOfSize (14);
			label4.TextColor = UIColor.Black;
			label4.SizeToFit();
			label4.Frame = new RectangleF(_padding.Left, label2.Frame.Bottom + 4, label4.Frame.Width, label4.Frame.Height);
			_scrollView.Add (label4);

			var label5 = new UILabel();
			label5.Text = "+ 7 (495) 987-37-20";
			label5.Font = UIFont.SystemFontOfSize (14);
			label5.TextColor = UIColor.Black;
			label5.SizeToFit();
			label5.Frame = new RectangleF(_padding.Left + label4.Frame.Width + 4, label2.Frame.Bottom + 4, label5.Frame.Width, label5.Frame.Height);
			_scrollView.Add (label5);

			var stringAttributes = new UIStringAttributes ();
			stringAttributes.ForegroundColor = UIColor.Blue;
			stringAttributes.Font = UIFont.SystemFontOfSize(14);
			stringAttributes.UnderlineStyle = NSUnderlineStyle.Single;
			stringAttributes.UnderlineColor = UIColor.Blue;

			var label6 = new UILabel();
			label6.Text = "Связь с редакцией:";
			label6.Font = UIFont.SystemFontOfSize (14);
			label6.TextColor = UIColor.Black;
			label6.SizeToFit();
			label6.Frame = new RectangleF(_padding.Left, label4.Frame.Bottom + 6, label6.Frame.Width, label6.Frame.Height);
			_scrollView.Add (label6);

			var label7 = new UILabel();
			label7.UserInteractionEnabled = true;
			label7.AttributedText = new NSAttributedString ("it.news@fsmedia.ru", stringAttributes);
			label7.SizeToFit();
			label7.Frame = new RectangleF(_padding.Left + 30, label6.Frame.Bottom + 4, label7.Frame.Width, label7.Frame.Height);
			var tap = new UITapGestureRecognizer (() =>
			{
				OpenMail("it.news@fsmedia.ru");
			});
			label7.AddGestureRecognizer (tap);
			_scrollView.Add (label7);

			var label8 = new UILabel();
			label8.Text = "По вопросам сотрудничества:";
			label8.Font = UIFont.SystemFontOfSize (14);
			label8.TextColor = UIColor.Black;
			label8.SizeToFit();
			label8.Frame = new RectangleF(_padding.Left, label7.Frame.Bottom + 6, label8.Frame.Width, label8.Frame.Height);
			_scrollView.Add (label8);

			var label9 = new UILabel();
			label9.UserInteractionEnabled = true;
			label9.AttributedText = new NSAttributedString ("it.marketing@fsmedia.ru", stringAttributes);
			label9.SizeToFit();
			label9.Frame = new RectangleF(_padding.Left + 30, label8.Frame.Bottom + 4, label9.Frame.Width, label9.Frame.Height);
			tap = new UITapGestureRecognizer (() =>
			{
				OpenMail("it.marketing@fsmedia.ru");
			});
			label9.AddGestureRecognizer (tap);
			_scrollView.Add (label9);

			var label10 = new UILabel();
			label10.Text = "По вопросам рекламы:";
			label10.Font = UIFont.SystemFontOfSize (14);
			label10.TextColor = UIColor.Black;
			label10.SizeToFit();
			label10.Frame = new RectangleF(_padding.Left, label9.Frame.Bottom + 6, label10.Frame.Width, label10.Frame.Height);
			_scrollView.Add (label10);

			var label11 = new UILabel();
			label11.UserInteractionEnabled = true;
			label11.AttributedText = new NSAttributedString ("it.adv@fsmedia.ru", stringAttributes);
			label11.SizeToFit();
			label11.Frame = new RectangleF(_padding.Left + 30, label10.Frame.Bottom + 4, label11.Frame.Width, label11.Frame.Height);
			tap = new UITapGestureRecognizer (() =>
			{
				OpenMail("it.adv@fsmedia.ru");
			});
			label11.AddGestureRecognizer (tap);
			_scrollView.Add (label11);

			var label12 = new UILabel();
			label12.Text = "Сайты:";
			label12.Font = UIFont.BoldSystemFontOfSize (16);
			label12.TextColor = UIColor.Black;
			label12.SizeToFit();
			label12.Frame = new RectangleF(_padding.Left, label11.Frame.Bottom + 8, label12.Frame.Width, label12.Frame.Height);
			_scrollView.Add (label12);

			var label13 = new UILabel();
			label13.UserInteractionEnabled = true;
			label13.AttributedText = new NSAttributedString ("www.it-world.ru", stringAttributes);
			label13.SizeToFit();
			label13.Frame = new RectangleF(_padding.Left + label12.Frame.Width + 4, label11.Frame.Bottom + 8, label13.Frame.Width, label13.Frame.Height);
			tap = new UITapGestureRecognizer (() =>
			{
				OpenUrl("www.it-world.ru");
			});
			label13.AddGestureRecognizer (tap);
			_scrollView.Add (label13);

			var label14 = new UILabel();
			label14.UserInteractionEnabled = true;
			label14.AttributedText = new NSAttributedString ("www.allcio.ru", stringAttributes);
			label14.SizeToFit();
			label14.Frame = new RectangleF(_padding.Left + label12.Frame.Width + 4, label13.Frame.Bottom + 6, label14.Frame.Width, label14.Frame.Height);
			tap = new UITapGestureRecognizer (() =>
			{
				OpenUrl("www.allcio.ru");
			});
			label14.AddGestureRecognizer (tap);
			_scrollView.Add (label14);

			var label15 = new UILabel();
			label15.UserInteractionEnabled = true;
			label15.AttributedText = new NSAttributedString ("www.it-weekly.ru", stringAttributes);
			label15.SizeToFit();
			label15.Frame = new RectangleF(_padding.Left + label12.Frame.Width + 4, label14.Frame.Bottom + 6, label15.Frame.Width, label15.Frame.Height);
			tap = new UITapGestureRecognizer (() =>
			{
				OpenUrl("www.it-weekly.ru");
			});
			label15.AddGestureRecognizer (tap);
			_scrollView.Add (label15);

			var label16 = new UILabel();
			label16.Text = "Издания:";
			label16.Font = UIFont.BoldSystemFontOfSize (14);
			label16.TextColor = UIColor.Black;
			label16.SizeToFit();
			label16.Frame = new RectangleF(_padding.Left, label15.Frame.Bottom + 8, label16.Frame.Width, label16.Frame.Height);
			_scrollView.Add (label16);

			_busyWidth = (_padding.Left + _padding.Right + label16.Frame.Width + 4);
			var maxWidth = View.Bounds.Width - _busyWidth;
			_publishersTxt = ItExpertHelper.GetTextView(
				ItExpertHelper.GetAttributedString("IT Expert, IT Manager, IT News, IT Weekly", UIFont.SystemFontOfSize(14), 
					UIColor.Black), maxWidth, new PointF(_padding.Left + label16.Frame.Width + 4, label15.Frame.Bottom + 8));
			_publishersTxt.BackgroundColor = UIColor.FromRGB(240, 240, 240);
			_scrollView.Add (_publishersTxt);

			_aboutUsLbl = new UILabel();
			_aboutUsLbl.Text = "О нас:";
			_aboutUsLbl.Font = UIFont.BoldSystemFontOfSize (14);
			_aboutUsLbl.TextColor = UIColor.Black;
			_aboutUsLbl.SizeToFit();
			_aboutUsLbl.Frame = new RectangleF(_padding.Left, _publishersTxt.Frame.Bottom + 8, _aboutUsLbl.Frame.Width, _aboutUsLbl.Frame.Height);
			_scrollView.Add (_aboutUsLbl);

			maxWidth = View.Bounds.Width - (_padding.Left + _padding.Right);
			_aboutUsText = ItExpertHelper.GetTextView(
				ItExpertHelper.GetAttributedString(_text, UIFont.SystemFontOfSize(14), UIColor.Black), maxWidth, 
				new PointF(_padding.Left, _aboutUsLbl.Frame.Bottom + 4));
			_aboutUsText.BackgroundColor = UIColor.FromRGB(240, 240, 240);
			_scrollView.Add (_aboutUsText);

			_scrollView.ContentSize = new SizeF(maxWidth, _aboutUsText.Frame.Bottom + _padding.Bottom);
		}

		void UpdateViews()
		{
			_scrollView.Frame = new RectangleF(0, 0, View.Frame.Width, View.Frame.Height);
			var point = new PointF (_publishersTxt.Frame.X, _publishersTxt.Frame.Y);
			_publishersTxt.RemoveFromSuperview ();
			_aboutUsLbl.RemoveFromSuperview ();
			_aboutUsText.RemoveFromSuperview ();

			var maxWidth = View.Bounds.Width - _busyWidth;
			_publishersTxt = ItExpertHelper.GetTextView(
				ItExpertHelper.GetAttributedString("IT Expert, IT Manager, IT News, IT Weekly", UIFont.SystemFontOfSize(14), 
					UIColor.Black), maxWidth, point);
			_publishersTxt.BackgroundColor = UIColor.FromRGB(240, 240, 240);
			_scrollView.Add (_publishersTxt);

			_aboutUsLbl = new UILabel();
			_aboutUsLbl.Text = "О нас:";
			_aboutUsLbl.Font = UIFont.BoldSystemFontOfSize (14);
			_aboutUsLbl.TextColor = UIColor.Black;
			_aboutUsLbl.SizeToFit();
			_aboutUsLbl.Frame = new RectangleF(_padding.Left, _publishersTxt.Frame.Bottom + 8, _aboutUsLbl.Frame.Width, _aboutUsLbl.Frame.Height);
			_scrollView.Add (_aboutUsLbl);

			maxWidth = View.Bounds.Width - (_padding.Left + _padding.Right);
			_aboutUsText = ItExpertHelper.GetTextView(
				ItExpertHelper.GetAttributedString(_text, UIFont.SystemFontOfSize(14), UIColor.Black), maxWidth, 
				new PointF(_padding.Left, _aboutUsLbl.Frame.Bottom + 4));
			_aboutUsText.BackgroundColor = UIColor.FromRGB(240, 240, 240);
			_scrollView.Add (_aboutUsText);

			_scrollView.ContentSize = new SizeF(maxWidth, _aboutUsText.Frame.Bottom + _padding.Bottom);
		}

		void OpenMail(string address)
		{
			Toast.MakeText (this, address, 1500).Show ();

		}

		void OpenUrl(string address)
		{
			Toast.MakeText (this, address, 1500).Show ();
		}

		private UIEdgeInsets _padding;
		private UIScrollView _scrollView;
		private UITextView _publishersTxt;
		private UITextView _aboutUsText;
		private UILabel _aboutUsLbl;
		private float _busyWidth;
		private string _text = @"«ИТ Медиа» – один из монополистов издательского рынка ИТ-тематики. С 1993 года наши издания освещают информационные технологии во всем их многообразии и богатстве сфер применения. Аудитория – от авторитетных представителей ИТ- сообщества до рядовых пользователей, поклонников всего самого инновационного в нашей быстротекущей жизни. Свою миссию мы видим в обеспечении читателей актуальной, беспристрастной и главное - достоверной информацией, которая поможет им и в профессиональной деятельности, и в обычной жизни.";
	}
}

