using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;

namespace ItExpert
{
	public static class ItExpertHelper
	{
		public static int StatusBarHeight 
		{ 
			get
			{
				return 20;
			}
		}

        public static float LargestImageSizeInArticlesPreview
        {
            get;
            set;
        }

        public static UITextView GetTextView(NSAttributedString attributedString, float textViewWidth, PointF textViewLocation, UIView imageView = null)
        {
			var size = UIScreen.MainScreen.Bounds.Size;

			var storage = new NSTextStorage ();
			var container = new NSTextContainer (new Size ((int)textViewWidth, int.MaxValue)) { LineFragmentPadding = 0 };
			var layoutManger = new NSLayoutManager ();

			storage.SetString (attributedString);
			storage.AddLayoutManager (layoutManger);
			layoutManger.AddTextContainer (container);

			var textView = new UITextView (new RectangleF (textViewLocation, new SizeF(container.Size.Width,0)), container);
			textView.TextContainerInset = new UIEdgeInsets (0, 0, 0, 0);
			textView.AttributedText = attributedString;
			textView.ScrollEnabled = false;
			textView.Editable = false;
			textView.UserInteractionEnabled = false;

			container.Size = new Size ((int)textViewWidth, int.MaxValue);

			if (imageView != null)
			{
				var imageRectangle = ConvertRectangleToSubviewCoordinates (imageView.Frame, textViewLocation);

				container.ExclusionPaths = new UIBezierPath[] { UIBezierPath.FromRect (imageRectangle) };
			}

			var range = layoutManger.GetGlyphRange (container);
			var containerSize = layoutManger.BoundingRectForGlyphRange (range, container);

			textView.Frame = new RectangleF (textViewLocation, new SizeF (containerSize.Width, containerSize.Height));

			return textView;
		}

        public static float GetTextHeight(UIFont font, string text, float textViewWidth)
        {
            var textView = GetTextView(GetAttributedString(text, font, UIColor.Black), textViewWidth, new PointF(0, 0));

            return textView.Frame.Height;
        }

		public static RectangleF ConvertRectangleToSubviewCoordinates(RectangleF rectangle, PointF subviewCoordinates)
		{
			return new RectangleF (rectangle.X - subviewCoordinates.X, rectangle.Y - subviewCoordinates.Y, rectangle.Width, rectangle.Height);
		}

        public static UIImage GetImageFromBase64String(string base64String, float scale = 1)
		{
			byte[] encodedDataAsBytes = Convert.FromBase64String(base64String);
			NSData data = NSData.FromArray(encodedDataAsBytes);                            
            return UIImage.LoadFromData(data, scale);
		}

        public static NSMutableAttributedString GetAttributedString(string text, UIFont font, UIColor foregroundColor, bool isUnderlined = false)
		{
			var stringAttributes = new UIStringAttributes ();

			stringAttributes.ForegroundColor = foregroundColor;
			stringAttributes.Font = font;

			if (isUnderlined)
			{
				stringAttributes.UnderlineStyle = NSUnderlineStyle.Single;
				stringAttributes.UnderlineColor = foregroundColor;
			}

			return new NSMutableAttributedString (text, stringAttributes);
		}

		public static UIColor GetUIColorFromColor(Color color)
		{
			return UIColor.FromRGB (
				color.R,
				color.G,
				color.B);
		}

        public static void RemoveSubviews(UIView view)
        {
            foreach (var subView in view.Subviews)
            {
                subView.RemoveFromSuperview();
            }
        }

        public static RectangleF GetRealScreenSize()
        {
            var appDelegate = UIApplication.SharedApplication.Delegate;

            var window = appDelegate.Window;

            RectangleF bounds = window.Bounds;

            if (UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.LandscapeLeft || 
                UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.LandscapeRight)
            {
                bounds = new RectangleF(0, 0, window.Bounds.Height, window.Bounds.Width);
            }

            return bounds;
        }
	}
}

