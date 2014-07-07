using System;
using MonoTouch.UIKit;
using ItExpert.Model;

namespace ItExpert
{
	public class BannerImageView : UIImageView
	{
		private readonly Banner _banner;

		public BannerImageView (UIImage image, Banner banner):base(image)
		{
			_banner = banner;
		}

		public Banner Banner
		{
			get{ return _banner; }
		}
	}
}

