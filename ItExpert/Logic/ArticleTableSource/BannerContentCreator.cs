using System;
using ItExpert.Model;
using MonoTouch.UIKit;
using System.Drawing;

namespace ItExpert
{
    public class BannerContentCreator : BaseContentCreator
    {
        public override float GetContentHeight(UIView cellContentView, Article article)
        {
			var imageView = article.ExtendedObject as UIImageView;
			if (imageView != null)
			{
				return imageView.Frame.Height;
			}
			var bannerView = article.ExtendedObject as BannerView;
			if (bannerView != null)
			{
				return bannerView.GetHeight ();
			}
			return 0;
        }

        protected override void Create(UITableViewCell cell, Article article)
        {
            cell.SelectionStyle = UITableViewCellSelectionStyle.None;
			var bannerView = article.ExtendedObject as BannerView;
			if (bannerView != null)
			{
				var image = bannerView.GetImage ();
				cell.ContentView.Add(image);
			}
			var imageView = article.ExtendedObject as UIImageView;
			if (imageView != null)
			{
				cell.ContentView.Add(imageView);
			}
        }

        protected override void Update(UITableViewCell cell, Article article)
        {
            cell.SelectionStyle = UITableViewCellSelectionStyle.None;
			var bannerView = article.ExtendedObject as BannerView;
			if (bannerView != null)
			{
				var image = bannerView.GetImage ();
				cell.ContentView.Add(image);
			}
			var imageView = article.ExtendedObject as UIImageView;
			if (imageView != null)
			{
				cell.ContentView.Add(imageView);
			}
        }
    }
}

