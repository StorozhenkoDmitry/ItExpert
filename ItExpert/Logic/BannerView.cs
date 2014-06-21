using System;
using MonoTouch.UIKit;
using ItExpert.Model;
using MonoTouch.Foundation;
using System.Drawing;

namespace ItExpert
{
	public class BannerView : UIView
	{
		private Banner _banner;
		private float _koefScaling;

		public BannerView (Banner banner, float koefScaling)
		{
			_banner = banner;
			_koefScaling = koefScaling;
		}

		public float GetHeight()
		{
			return _banner.Picture.Height * _koefScaling;
		}

		public UIImageView GetImage()
		{
			var bannerView = 
				AnimatedImageView.GetAnimatedImageView (NSData.FromArray(Convert.FromBase64String(_banner.Picture.Data)));
			bannerView.Frame = 
				new RectangleF(0, 0, _koefScaling * (_banner.Picture.Width), _koefScaling * (_banner.Picture.Height));
			return bannerView;
		}
	}
}

