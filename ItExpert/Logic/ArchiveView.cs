using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MonoTouch.UIKit;
using ItExpert.Model;

namespace ItExpert
{
    public class ArchiveView : UIView
    {
		public event EventHandler MagazinePushed;
		public event EventHandler MagazineOpen;
		public event EventHandler MagazineDelete;
		public event EventHandler MagazineDownload;
		private bool _isNewLineAppeared;
		private UIEdgeInsets _padding;
		private float _verticalSpaceBetweenViews;
		private float _horizontalSpaceBetweenViews;
		private PointF _nextViewPosition;
		private UIScrollView _scrollView;
		private List<Magazine> _items;

        public ArchiveView(RectangleF frame)
            : base(frame)
        {
            _scrollView = new UIScrollView(new RectangleF(0, 0, Frame.Width, Frame.Height));
            _scrollView.UserInteractionEnabled = true;
            _scrollView.ScrollEnabled = true;
            _scrollView.DelaysContentTouches = false;

            Add(_scrollView);

            _verticalSpaceBetweenViews = 10;
            _horizontalSpaceBetweenViews = 35;

            _padding = new UIEdgeInsets(5, 10, 5, 5);
        }

        public override RectangleF Frame
        {
            get
            {
                return base.Frame;
            }
            set
            {
                base.Frame = value;

                if (_scrollView != null)
                {
                    _scrollView.Frame = new RectangleF(0, 0, Frame.Width, Frame.Height);
                }
            }
        }

        public void AddMagazineViews(List<Magazine> magazines)
        {
            ItExpertHelper.RemoveSubviews(_scrollView);
			_items = magazines;
            _nextViewPosition = new PointF(_padding.Left, _padding.Top);

            float magazineViewHeight = 0;
            _isNewLineAppeared = false;

            foreach (var magazine in magazines)
            {
                MagazineView magazineView = new MagazineView(magazine);

                magazineView.MagazineImagePushed += OnMagazineImagePushed;
				magazineView.MagazineDownload += OnMagazineDownload;
				magazineView.MagazineOpen += OnMagazineOpen;
				magazineView.MagazineDelete += OnMagazineDelete;
                SetMagazineViewLocation(magazineView);

                _scrollView.Add(magazineView);

                magazineViewHeight = magazineView.Frame.Height;
            }

            _scrollView.ContentSize = new SizeF(_nextViewPosition.X, _nextViewPosition.Y + _padding.Bottom + (_isNewLineAppeared ? magazineViewHeight : 0));
        }

        void OnMagazineDownload (object sender, EventArgs e)
        {
			if (MagazineDownload != null)
			{
				MagazineDownload (sender, e);
			}
        }

        void OnMagazineDelete (object sender, EventArgs e)
        {
			if (MagazineDelete != null)
			{
				MagazineDelete (sender, e);
			}
        }

        void OnMagazineOpen (object sender, EventArgs e)
        {
			if (MagazineOpen != null)
			{
				MagazineOpen (sender, e);
			}
        }

        private void SetMagazineViewLocation(MagazineView magazineView)
        {
            magazineView.Location = new PointF(_nextViewPosition.X, _nextViewPosition.Y);

            _nextViewPosition.X = magazineView.Frame.Right + _horizontalSpaceBetweenViews + _padding.Left;

            if (_nextViewPosition.X + magazineView.Frame.Width > Frame.Width)
            {
                _isNewLineAppeared = false;

                _nextViewPosition = new PointF(_padding.Left, _nextViewPosition.Y + magazineView.Frame.Height + _verticalSpaceBetweenViews + _padding.Top);
            }
            else if (!_isNewLineAppeared)
            {
                _isNewLineAppeared = true;
            }
        }

        private void OnMagazineImagePushed(object sender, EventArgs e)
        {
            if (MagazinePushed != null)
            {
                MagazinePushed(sender, e);
            }
        }
    }
}

