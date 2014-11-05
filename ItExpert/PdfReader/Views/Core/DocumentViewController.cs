//
// mTouch-PDFReader library
// DocumentViewController.cs (Document view controller implementation)
//
//  Author:
//       Alexander Matsibarov (macasun) <amatsibarov@gmail.com>
//
//  Copyright (c) 2012 Alexander Matsibarov
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Drawing;
using System.Linq;
using MonoTouch.CoreGraphics;
using MonoTouch.UIKit;
using mTouchPDFReader.Library.Data.Enums;
using mTouchPDFReader.Library.Data.Objects;
using MonoTouch.Foundation;

namespace mTouchPDFReader.Library.Views.Core
{
	public class DocumentViewController : UIViewController
	{			
		#region Constants		
		private const int MaxPageViewsCount = 3;
		private const int MaxToolbarButtonsCount = 15;
		private const float BarPaddingH = 5.0f;
		private const float BarPaddingV = 5.0f;
		private const float FirstToolButtonLeft = 20.0f;
		private const float FirstToolButtonTop = 7.0f;
		private const float ToolButtonSize = 36.0f;
		private readonly SizeF PageNumberLabelSize = new SizeF(75.0f, 35.0f);		
		#endregion
		
		#region Fields			
		private readonly string _DocumentName;
		private readonly string _DocumentPath;
		private UIPageViewController _BookPageViewController;
		private UIView _toolbar;
		private UIView _bottomBar;
		private UISlider _slider;
		private UILabel _PageNumberLabel;
		private AutoScaleModes _AutoScaleMode;
		private UIButton _zoomInBut;
		private UIButton _zoomOutBut;
		private UIButton _backButton;
		private UIImageView _logoImageView;
		#endregion
			
		#region Initialization
		public DocumentViewController(string docName, string docPath) : base(null, null)
		{
			_DocumentName = docName;
			_DocumentPath = docPath;
		}
		#endregion
		
		#region UIViewController	
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			// Load document

		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			PDFDocument.OpenDocument(_DocumentName, _DocumentPath);

			// Init View	
			Title = _DocumentName;
			View.BackgroundColor = UIColor.LightGray;
			_AutoScaleMode = Options.Instance.AutoScaleMode;

			_toolbar = _CreateToolbar();
			View.AddSubview(_toolbar);

			_bottomBar = _CreateBottomBar();
			View.AddSubview(_bottomBar);
			_UpdateSliderMaxValue();
			// Create the book PageView controller
			_BookPageViewController = new UIPageViewController(
				Options.Instance.PageTransitionStyle,
				Options.Instance.PageNavigationOrientation, 
				UIPageViewControllerSpineLocation.Min);		
			_BookPageViewController.View.Frame = _GetBookViewFrameRect();
			_BookPageViewController.View.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			_BookPageViewController.View.BackgroundColor = UIColor.GroupTableViewBackgroundColor;
			_BookPageViewController.GetNextViewController = GetNextPageViewController;
			_BookPageViewController.GetPreviousViewController = GetPreviousPageViewController;
			_BookPageViewController.GetSpineLocation = GetSpineLocation;
			_BookPageViewController.DidFinishAnimating += PageViewControllerDidFinishAnimating;
			_BookPageViewController.SetViewControllers(
				new UIViewController[] { GetPageViewController(1) }, 
				UIPageViewControllerNavigationDirection.Forward, 
				false,
				s => ExecAfterOpenPageActions());	

			AddChildViewController(_BookPageViewController);
			_BookPageViewController.DidMoveToParentViewController(this);
			View.Add(_BookPageViewController.View);

		}

		public void PageViewControllerDidFinishAnimating(object sender, UIPageViewFinishedAnimationEventArgs e) 
		{
			PageFinishedAnimation(e.Completed, e.Finished, e.PreviousViewControllers);
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();
			if (_BookPageViewController != null && _BookPageViewController.ChildViewControllers != null)
			{
				foreach (var pageVC in _BookPageViewController.ChildViewControllers.Cast<PageViewController>())
				{
					pageVC.PageView.NeedUpdateZoomAndOffset = true;
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			InvokeOnMainThread(() =>
			{
				PDFDocument.CloseDocument();
				if (_backButton != null)
				{
					_backButton.RemoveFromSuperview();
					if (_backButton.ImageView != null && _backButton.ImageView.Image != null)
					{
						_backButton.ImageView.Image.Dispose();
						_backButton.ImageView.Image = null;
					}
					_backButton.TouchUpInside -= OnBackButtonPushed;
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

				if (_slider != null)
				{
					_slider.RemoveFromSuperview();
					_slider.Dispose();
				}
				_slider = null;

				if (_PageNumberLabel != null)
				{
					_PageNumberLabel.RemoveFromSuperview();
					_PageNumberLabel.Dispose();
				}
				_PageNumberLabel = null;

				if (_zoomInBut != null)
				{
					_zoomInBut.RemoveFromSuperview();
					_zoomInBut.TouchUpInside -= OnZoomInClick;
					if (_zoomInBut.ImageView != null && _zoomInBut.ImageView.Image != null)
					{
						_zoomInBut.ImageView.Image.Dispose();
						_zoomInBut.ImageView.Image = null;
					}
					_zoomInBut.Dispose();
				}
				_zoomInBut = null;

				if (_zoomOutBut != null)
				{
					_zoomOutBut.RemoveFromSuperview();
					_zoomOutBut.TouchUpInside -= OnZoomOutClick;
					if (_zoomOutBut.ImageView != null && _zoomOutBut.ImageView.Image != null)
					{
						_zoomOutBut.ImageView.Image.Dispose();
						_zoomOutBut.ImageView.Image = null;
					}
					_zoomOutBut.Dispose();
				}
				_zoomOutBut = null;

				if (_toolbar != null)
				{
					_toolbar.RemoveFromSuperview();
					_toolbar.Dispose();
				}
				_toolbar = null;

				if (_bottomBar != null)
				{
					_bottomBar.RemoveFromSuperview();
					_bottomBar.Dispose();
				}
				_bottomBar = null;

				if (_BookPageViewController != null)
				{
					_BookPageViewController.GetNextViewController = null;
					_BookPageViewController.GetPreviousViewController = null;
					_BookPageViewController.GetSpineLocation = null;
					_BookPageViewController.DidFinishAnimating -= PageViewControllerDidFinishAnimating;
					if (_BookPageViewController.ViewControllers != null && _BookPageViewController.ViewControllers.Any())
					{
						foreach (var controller in _BookPageViewController.ViewControllers)
						{
							controller.Dispose();
						}
					}
					_BookPageViewController.Dispose();
					_BookPageViewController = null;
				}
			});
		}	

		#endregion
		
		#region UIPageViewController	
		private UIViewController GetPreviousPageViewController(UIPageViewController pageController, UIViewController referenceViewController)
		{		
			var curPageCntr = referenceViewController as PageViewController;
			if (curPageCntr.PageNumber == 0) {
				return null;				
			} 
			int pageNumber = curPageCntr.PageNumber - 1;
			return GetPageViewController(pageNumber);
		}

		private UIViewController GetNextPageViewController(UIPageViewController pageController, UIViewController referenceViewController)
		{	
			var curPageCntr = referenceViewController as PageViewController;
			if (curPageCntr.PageNumber == (PDFDocument.PageCount)) {				
				return _BookPageViewController.SpineLocation == UIPageViewControllerSpineLocation.Mid
					? _GetEmptyPageContentVC()
					: null;	
			} else if (curPageCntr.PageNumber == -1) { 
				return null;
			}
			int pageNumber = curPageCntr.PageNumber + 1;
			return GetPageViewController(pageNumber);
		}

		private UIPageViewControllerSpineLocation GetSpineLocation(UIPageViewController pageViewController, UIInterfaceOrientation orientation)
		{
			var currentPageVC = _GetCurrentPageContentVC();
			pageViewController.DoubleSided = false;
			pageViewController.SetViewControllers(new UIViewController[] { currentPageVC }, UIPageViewControllerNavigationDirection.Forward, true, s => { });
			return UIPageViewControllerSpineLocation.Min;
		}

		private void PageFinishedAnimation(bool completed, bool finished, UIViewController[] previousViewControllers)
		{
			if (completed) {
				ExecAfterOpenPageActions();
			}
		}

		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate(fromInterfaceOrientation);
			RedrawBars();
		}

		void RedrawBars()
		{
			var statusBarHeight = 20;
			_toolbar.Frame = new RectangleF (0, statusBarHeight, View.Bounds.Width, 40);

			var x = View.Bounds.Width - 10 - (30 * 2 + 10);
			_zoomOutBut.Frame = new RectangleF(x, 5, 30, 30);

			x = x + 10 + 30;
			_zoomInBut.Frame = new RectangleF(x, 5, 30, 30);

			_bottomBar.Frame = new RectangleF (0, View.Bounds.Height - 46, View.Bounds.Width, 46);
		}

		#endregion
				
		#region UI Logic			
		
		private UIInterfaceOrientation _GetDeviceOrientation()
		{
			switch (UIDevice.CurrentDevice.Orientation) {
				case UIDeviceOrientation.LandscapeLeft:
					return UIInterfaceOrientation.LandscapeLeft;
				case UIDeviceOrientation.LandscapeRight:
					return UIInterfaceOrientation.LandscapeRight;
				case UIDeviceOrientation.Portrait:
					return UIInterfaceOrientation.Portrait;
				case UIDeviceOrientation.PortraitUpsideDown:
					return UIInterfaceOrientation.PortraitUpsideDown;
			}
			return UIInterfaceOrientation.Portrait;
		}
		
		private void _UpdateSliderMaxValue()
		{
			if (_slider != null) {
				_slider.MaxValue = PDFDocument.PageCount;
			}
		}		

		protected virtual UIView _CreateToolbar()
		{
			var statusBarHeight = 20;
			var toolBar = new UIView ();
			toolBar.Frame = new RectangleF (0, statusBarHeight, View.Bounds.Width, 40);
			toolBar.BackgroundColor = UIColor.Black;

			var image = new UIImage(NSData.FromFile("NavigationBar/Back.png"), 2);
			_backButton = new UIButton(new RectangleF(new PointF(10, toolBar.Frame.Height / 2 - image.Size.Height / 2), image.Size));
			_backButton.SetImage(image, UIControlState.Normal);
			_backButton.TouchUpInside += OnBackButtonPushed;
			toolBar.Add(_backButton);

			_logoImageView = new UIImageView(new UIImage(NSData.FromFile("NavigationBar/Logo.png"), 2));
			_logoImageView.Frame = new RectangleF(new PointF(_backButton.Frame.Right + 20, toolBar.Frame.Height / 2 - _logoImageView.Frame.Height / 2), 
							_logoImageView.Frame.Size);
			toolBar.Add(_logoImageView);

			var x = View.Bounds.Width - 10 - (30 * 2 + 10);
			var btn = new UIButton(new RectangleF(x, 5, 30, 30));
			btn.SetImage(new UIImage (NSData.FromFile ("ZoomOut48.png"), 1f), UIControlState.Normal);
			btn.TouchUpInside += OnZoomOutClick;
			toolBar.Add(btn);
			_zoomOutBut = btn;

			x = x + 10 + 30;
			btn = new UIButton(new RectangleF(x, 5, 30, 30));
			btn.SetImage(new UIImage (NSData.FromFile ("ZoomIn48.png"), 1f), UIControlState.Normal);
			btn.TouchUpInside += OnZoomInClick;
			toolBar.Add(btn);
			_zoomInBut = btn;

			return toolBar;
		}

		void OnZoomInClick(object sender, EventArgs e)
		{
			ZoomIn();
		}

		void OnZoomOutClick(object sender, EventArgs e)
		{
			ZoomOut();
		}

		void OnBackButtonPushed(object sender, EventArgs e)
		{
			DismissViewController(true, null);
			Dispose();
		}

		protected virtual UIView _CreateBottomBar()
		{
			var bottomBar = new UIView ();
			bottomBar.Frame = new RectangleF (0, View.Bounds.Height - 46, View.Bounds.Width, 46);
			bottomBar.BackgroundColor = UIColor.Black;

			var sliderWidth = bottomBar.Frame.Width - 15;
			sliderWidth -= PageNumberLabelSize.Width;
			_slider = new UISlider(new RectangleF(5, 18, sliderWidth, 20));
			_slider.MinValue = 1;
			_slider.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
			_slider.ValueChanged += (sender, e) =>
			{
				if (_PageNumberLabel != null)
				{
					_PageNumberLabel.Text = string.Format (@"{0}/{1}", (int)_slider.Value, PDFDocument.PageCount);
				}
			};
			_slider.TouchUpInside += (sender, e) => OpenDocumentPage ((int)_slider.Value);
			bottomBar.Add(_slider);

			var pageNumberViewFrame = new RectangleF(bottomBar.Frame.Width - PageNumberLabelSize.Width - 5, 10, PageNumberLabelSize.Width, PageNumberLabelSize.Height);
			var pageNumberView = new UIView(pageNumberViewFrame);
			pageNumberView.AutosizesSubviews = false;
			pageNumberView.UserInteractionEnabled = false;
			pageNumberView.AutoresizingMask = UIViewAutoresizing.FlexibleLeftMargin;
			pageNumberView.BackgroundColor = UIColor.FromWhiteAlpha(0.4f, 0.5f);
			pageNumberView.Layer.CornerRadius = 5.0f;
			pageNumberView.Layer.ShadowOffset = new SizeF(0.0f, 0.0f);
			pageNumberView.Layer.ShadowPath = UIBezierPath.FromRect(pageNumberView.Bounds).CGPath;
			pageNumberView.Layer.ShadowRadius = 2.0f;
			pageNumberView.Layer.ShadowOpacity = 1.0f;

			var pageNumberLabelFrame = RectangleFExtensions.Inset(pageNumberView.Bounds, 4.0f, 2.0f);
			_PageNumberLabel = new UILabel(pageNumberLabelFrame);
			_PageNumberLabel.AutosizesSubviews = false;
			_PageNumberLabel.AutoresizingMask = UIViewAutoresizing.None;
			_PageNumberLabel.TextAlignment = UITextAlignment.Center;
			_PageNumberLabel.BackgroundColor = UIColor.Clear;
			_PageNumberLabel.TextColor = UIColor.White;
			_PageNumberLabel.Font = UIFont.SystemFontOfSize(16.0f);
			_PageNumberLabel.ShadowOffset = new SizeF(0.0f, 1.0f);
			_PageNumberLabel.ShadowColor = UIColor.Black;
			_PageNumberLabel.AdjustsFontSizeToFitWidth = true;
			pageNumberView.Add(_PageNumberLabel);
			bottomBar.Add(pageNumberView);	
			return bottomBar;
		}


		#endregion

		#region PDFDocument logic

		private RectangleF _GetBookViewFrameRect()
		{
			var rect = View.Bounds;
			if (_toolbar != null) {
				rect.Y = _toolbar.Frame.Bottom;
				rect.Height -= (_toolbar.Frame.Height + 5);
			}
			if (_bottomBar != null) {
				rect.Height -= (_bottomBar.Frame.Height + 5);
			}
			return rect;
		}
		
		private RectangleF _GetPageViewFrameRect()
		{
			return _BookPageViewController.View.Bounds; 
		}

		private PageViewController _GetCurrentPageContentVC()
		{
			return (PageViewController)_BookPageViewController.ViewControllers[0];
		}

		private PageViewController _GetEmptyPageContentVC()
		{
			return new PageViewController(_GetPageViewFrameRect(), _AutoScaleMode, -1);
		}

		private PageViewController GetPageViewController(int pageNumber)
		{
			if (!PDFDocument.DocumentHasLoaded || pageNumber < 1 || pageNumber > PDFDocument.PageCount || pageNumber == PDFDocument.CurrentPageNumber) {
				return null;
			}
			return new PageViewController(_GetPageViewFrameRect(), _AutoScaleMode, pageNumber);
		}

		public virtual void OpenDocumentPage(int pageNumber)
		{
			if (pageNumber < 1 || pageNumber > PDFDocument.PageCount) {
				return;
			}

			// Calc navigation direction
			var navDirection = pageNumber < PDFDocument.CurrentPageNumber  
				? UIPageViewControllerNavigationDirection.Reverse
				: UIPageViewControllerNavigationDirection.Forward;

			// Create single PageView
			var pageVC = GetPageViewController(pageNumber);

			if (pageVC == null) {
				return;
			}

			// Open page
			_BookPageViewController.SetViewControllers(
				new UIViewController[] { pageVC }, 
				navDirection, 
				true, 
				s => { ExecAfterOpenPageActions(); });
		}		

		private void ExecAfterOpenPageActions()
		{
			// Set current page
			PDFDocument.CurrentPageNumber = _GetCurrentPageContentVC().PageNumber;		
			// Update PageNumber label
			if (_PageNumberLabel != null) {
				_PageNumberLabel.Text = string.Format(@"{0}/{1}", PDFDocument.CurrentPageNumber, PDFDocument.PageCount);
			}
			
			// Update slider position
			if (_slider != null) {
				_slider.Value = PDFDocument.CurrentPageNumber;
			}
		}
		#endregion
		
		#region Events	
		
		protected virtual void ZoomOut()
		{
			var pageVC = _GetCurrentPageContentVC();
			if (PDFDocument.DocumentHasLoaded && pageVC.IsNotEmptyPage) {
				pageVC.PageView.ZoomDecrement();
			}
		}
		
		protected virtual void ZoomIn()
		{
			var pageVC = _GetCurrentPageContentVC();
			if (PDFDocument.DocumentHasLoaded && pageVC.IsNotEmptyPage) {
				pageVC.PageView.ZoomIncrement();
			}
		}

		#endregion
	}
}