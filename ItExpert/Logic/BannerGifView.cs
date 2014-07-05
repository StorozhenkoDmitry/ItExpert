using System;
using MonoTouch.UIKit;
using ItExpert.Model;
using MonoTouch.Foundation;
using System.Drawing;

namespace ItExpert
{
	public class BannerGifView : UIView
	{
		private readonly Banner _banner;
		private float _koefScaling;
		private float _screenWidth;

		public BannerGifView (Banner banner, float koefScaling, float screenWidth)
		{
			_banner = banner;
			_koefScaling = koefScaling;
			_screenWidth = screenWidth;
		}

		public float GetHeight()
		{
			return _banner.Picture.Height * _koefScaling;
		}

		public UIImageView GetImage()
		{
			var bannerView = 
				AnimatedImageView.GetAnimatedImageView (NSData.FromArray(Convert.FromBase64String(_banner.Picture.Data)));
			var x = 0;
			if ((int)(_koefScaling * (_banner.Picture.Width)) < (int)_screenWidth)
			{
				x = (int)(((int)_screenWidth - (int)(_koefScaling * (_banner.Picture.Width))) / 2);
			}
			bannerView.Frame = 
				new RectangleF(x, 0, _koefScaling * (_banner.Picture.Width), _koefScaling * (_banner.Picture.Height));
			return bannerView;
		}

		public Banner Banner
		{
			get{ return _banner; }
		}
	}
}

