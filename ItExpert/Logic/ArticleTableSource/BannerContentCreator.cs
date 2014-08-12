using System;
using ItExpert.Model;
using MonoTouch.UIKit;
using System.Drawing;
using System.Linq;
using MonoTouch.Foundation;

namespace ItExpert
{
    public class BannerContentCreator : BaseContentCreator
    {
        public override float GetContentHeight(UIView cellContentView, Article article)
        {
			var imageView = article.ExtendedObject as BannerImageView;
			if (imageView != null)
			{
				return imageView.Frame.Height;
			}
			var bannerView = article.ExtendedObject as BannerGifView;
			if (bannerView != null)
			{
				return bannerView.GetHeight ();
			}
			return 0;
        }

        protected override void Create(UITableViewCell cell, Article article)
        {
            cell.SelectionStyle = UITableViewCellSelectionStyle.None;
			var tap = new UITapGestureRecognizer(() =>
			{
				var obj = article.ExtendedObject;
				Banner banner = null;
				var bannerImg = obj as BannerImageView;
				if (bannerImg != null)
				{
					banner = bannerImg.Banner;
				}
				var bannerGif = obj as BannerGifView;
				if (bannerGif != null)
				{
					banner = bannerGif.Banner;
				}
				if (banner != null)
				{
					UIApplication.SharedApplication.OpenUrl (new NSUrl (banner.Url));
				}
			});
			var bannerView = article.ExtendedObject as BannerGifView;
			var frame = cell.ContentView.Frame;
			if (bannerView != null)
			{
				var image = bannerView.GetImage ();
				image.UserInteractionEnabled = true;
				var bannerViewX = new BannerView(frame, image, tap);
				cell.ContentView.Add(bannerViewX);
			}
			var imageView = article.ExtendedObject as BannerImageView;
			if (imageView != null)
			{
				imageView.UserInteractionEnabled = true;
				var bannerViewX = new BannerView(frame, imageView, tap);
				cell.ContentView.Add(bannerViewX);
			}
		}
			
        protected override void Update(UITableViewCell cell, Article article)
        {
            cell.SelectionStyle = UITableViewCellSelectionStyle.None;
			var tap = new UITapGestureRecognizer(() =>
			{
				var obj = article.ExtendedObject;
				Banner banner = null;
				var bannerImg = obj as BannerImageView;
				if (bannerImg != null)
				{
					banner = bannerImg.Banner;
				}
				var bannerGif = obj as BannerGifView;
				if (bannerGif != null)
				{
					banner = bannerGif.Banner;
				}
				if (banner != null)
				{
					UIApplication.SharedApplication.OpenUrl (new NSUrl (banner.Url));
				}
			});
			var bannerView = article.ExtendedObject as BannerGifView;
			var frame = cell.ContentView.Frame;
			if (bannerView != null)
			{
				var image = bannerView.GetImage ();
				image.UserInteractionEnabled = true;
				var bannerViewX = new BannerView(frame, image, tap);
				cell.ContentView.Add(bannerViewX);
			}
			var imageView = article.ExtendedObject as BannerImageView;
			if (imageView != null)
			{
				imageView.UserInteractionEnabled = true;
				var bannerViewX = new BannerView(frame, imageView, tap);
				cell.ContentView.Add(bannerViewX);
			}
        }
    }

	public class BannerView : UIView, ICleanupObject
	{
		private UIImageView _image;
		private UITapGestureRecognizer _tap;

		public BannerView(RectangleF frame, UIImageView image, UITapGestureRecognizer tap): base(frame)
		{
			_image = image;
			Add(_image);
			_tap = tap;
			AddGestureRecognizer(_tap);
		}

		public void CleanUp()
		{
			foreach (var view in Subviews)
			{
				view.RemoveFromSuperview();
			}
			if (_tap != null)
			{
				_tap.Dispose();
			}
			_tap = null;

			if (_image != null)
			{
				if (_image.Layer != null)
				{
					_image.Layer.Dispose();
				}
				_image.Dispose();
			}
			_image = null;
		}
	}
}

