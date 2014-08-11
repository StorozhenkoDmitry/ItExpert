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
			var bannerView = article.ExtendedObject as BannerGifView;
			if (bannerView != null)
			{
				var image = bannerView.GetImage ();
				image.UserInteractionEnabled = true;
				cell.ContentView.Add(image);
			}
			var imageView = article.ExtendedObject as BannerImageView;
			if (imageView != null)
			{
				imageView.UserInteractionEnabled = true;
				cell.ContentView.Add(imageView);
			}

			if (cell.GestureRecognizers == null || !cell.GestureRecognizers.Any())
			{
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
				cell.AddGestureRecognizer(tap);
			}
		}
			
        protected override void Update(UITableViewCell cell, Article article)
        {
            cell.SelectionStyle = UITableViewCellSelectionStyle.None;
			var bannerView = article.ExtendedObject as BannerGifView;
			if (bannerView != null)
			{
				var image = bannerView.GetImage ();
				cell.ContentView.Add(image);
			}
			var imageView = article.ExtendedObject as BannerImageView;
			if (imageView != null)
			{
				cell.ContentView.Add(imageView);
			}
			if (cell.GestureRecognizers == null || !cell.GestureRecognizers.Any())
			{
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
				cell.AddGestureRecognizer(tap);
			}
        }
    }
}

