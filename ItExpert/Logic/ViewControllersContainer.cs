using System;
using MonoTouch.UIKit;

namespace ItExpert
{
    public static class ViewControllersContainer
    {
        public static UIViewController NewsViewController
        {
            get
            {
                if (_newsViewController == null)
                {
                    _newsViewController = new ItExpertViewController();
                }

                return _newsViewController;
            }
        }

        public static UIViewController TrendsViewController
        {
            get
            {
                if (_trendsViewController == null)
                {
                    _trendsViewController = new TrendsViewController();
                }

                return _trendsViewController;
            }
        }

        public static UIViewController MagazineViewController
        {
            get
            {
                if (_magazineViewController == null)
                {
                    _magazineViewController = new MagazineViewController();
                }

                return _magazineViewController;
            }
        }

        public static UIViewController ArchiveViewController
        {
            get
            {
                if (_archiveViewController == null)
                {
                    _archiveViewController = new ArchiveViewController();
                }

                return _archiveViewController;
            }
        }

        public static UIViewController FavoritesViewController
        {
            get
            {
                if (_favoritesViewController == null)
                {
                    _favoritesViewController = new FavoritesViewController();
                }

                return _favoritesViewController;
            }
        }

        public static BottomToolbarView BottomToolbarView
        {
            get
            {
                if (_bottomToolBar == null)
                {
                    _bottomToolBar = new BottomToolbarView();
                }

                return _bottomToolBar;
            }
        }

        private static UIViewController _newsViewController;
        private static UIViewController _trendsViewController;
        private static UIViewController _magazineViewController;
        private static UIViewController _archiveViewController;
        private static UIViewController _favoritesViewController;
        private static BottomToolbarView _bottomToolBar;

    }
}

