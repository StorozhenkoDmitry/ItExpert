﻿using System;
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace ItExpert
{
    public class BottomToolbarView: UIView
    {
		#region Fields

		private BottomToolbarButton _newsButton;
		private BottomToolbarButton _trendsButton;
		private BottomToolbarButton _magazineButton;
		private BottomToolbarButton _archiveButton;
		private BottomToolbarButton _favoritesButton;

		#endregion

		#region Public Property

		public BottomToolbarButton NewsButton
		{
			get { return _newsButton; }
		}

		public BottomToolbarButton TrendsButton
		{
			get { return _trendsButton; }
		}

		public BottomToolbarButton MagazineButton
		{
			get { return _magazineButton; }
		}

		public BottomToolbarButton ArchiveButton
		{
			get { return _archiveButton; }
		}

		public BottomToolbarButton FavoritesButton
		{
			get { return _favoritesButton; }
		}

		#endregion

        public BottomToolbarView()
        {
            UserInteractionEnabled = true;
        }

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (_newsButton != null)
			{
				_newsButton.RemoveFromSuperview();
				_newsButton.Dispose();
			}
			_newsButton = null;

			if (_magazineButton != null)
			{
				_magazineButton.RemoveFromSuperview();
				_magazineButton.Dispose();
			}
			_magazineButton = null;

			if (_archiveButton != null)
			{
				_archiveButton.RemoveFromSuperview();
				_archiveButton.Dispose();
			}
			_archiveButton = null;

			if (_trendsButton != null)
			{
				_trendsButton.RemoveFromSuperview();
				_trendsButton.Dispose();
			}
			_trendsButton = null;

			if (_favoritesButton != null)
			{
				_favoritesButton.RemoveFromSuperview();
				_favoritesButton.Dispose();
			}
			_favoritesButton = null;
		}

        private void AddButtons()
        {
            float buttonWidth = Frame.Width / 5;
            float scale = 3.8f;
            var button = 0;

			_newsButton = new BottomToolbarButton(new RectangleF(button * buttonWidth, 0, buttonWidth, Frame.Height), 
                                 new UIImage(NSData.FromFile("News.png"), scale), "Новости");

            button++;

			_trendsButton = new BottomToolbarButton(new RectangleF(button * buttonWidth, 0, buttonWidth, Frame.Height), 
                new UIImage(NSData.FromFile("Trends.png"), scale), "Тренды");

            button++;

			_magazineButton = new BottomToolbarButton(new RectangleF(button * buttonWidth, 0, buttonWidth, Frame.Height), 
                new UIImage(NSData.FromFile("Magazine.png"), scale), "Журнал");

            button++;

			_archiveButton = new BottomToolbarButton(new RectangleF(button * buttonWidth, 0, buttonWidth, Frame.Height), 
                new UIImage(NSData.FromFile("Archive.png"), scale), "Архив");

            button++;

			_favoritesButton = new BottomToolbarButton(new RectangleF(button * buttonWidth, 0, buttonWidth, Frame.Height), 
                new UIImage(NSData.FromFile("Favorites.png"), scale), "Избранное");

			Add(_newsButton);
			Add(_trendsButton);
			Add(_magazineButton);
			Add(_archiveButton);
			Add(_favoritesButton);
        }

        public override void LayoutIfNeeded()
        {
			if (_newsButton != null)
			{
				_newsButton.RemoveFromSuperview();
				_newsButton.Dispose();
			}
			_newsButton = null;

			if (_magazineButton != null)
			{
				_magazineButton.RemoveFromSuperview();
				_magazineButton.Dispose();
			}
			_magazineButton = null;

			if (_archiveButton != null)
			{
				_archiveButton.RemoveFromSuperview();
				_archiveButton.Dispose();
			}
			_archiveButton = null;

			if (_trendsButton != null)
			{
				_trendsButton.RemoveFromSuperview();
				_trendsButton.Dispose();
			}
			_trendsButton = null;

			if (_favoritesButton != null)
			{
				_favoritesButton.RemoveFromSuperview();
				_favoritesButton.Dispose();
			}
			_favoritesButton = null;

            AddButtons();
        }
    }
}

