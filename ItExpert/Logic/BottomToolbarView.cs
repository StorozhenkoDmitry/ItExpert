using System;
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace ItExpert
{
    public class BottomToolbarView: UIView
    {
        public BottomToolbarView()
        {
            UserInteractionEnabled = true;
        }

        public UINavigationController NavigationController
        {
            get;
            set;
        }

        private void AddButtons()
        {
            float buttonWidth = Frame.Width / 5;
            float scale = 3.8f;
            var button = 0;

            var newsButton = new BottomToolbarButton(new RectangleF(button * buttonWidth, 0, buttonWidth, Frame.Height), 
                                 new UIImage(NSData.FromFile("News.png"), scale), "Новости", () =>
            {
                if (NavigationController.VisibleViewController != ViewControllersContainer.NewsViewController)
                {
                    NavigationController.PresentViewController(ViewControllersContainer.NewsViewController, true, () =>
                    {
                    });
                }
            });

            button++;

            var trendsButton = new BottomToolbarButton(new RectangleF(button * buttonWidth, 0, buttonWidth, Frame.Height), 
                new UIImage(NSData.FromFile("Trends.png"), scale), "Тренды", () =>
            {
                if (NavigationController.VisibleViewController != ViewControllersContainer.TrendsViewController)
                {
                    NavigationController.PresentViewController(ViewControllersContainer.TrendsViewController, true, () =>
                    {
                    });
                }
            });

            button++;

            var magazineButton = new BottomToolbarButton(new RectangleF(button * buttonWidth, 0, buttonWidth, Frame.Height), 
                new UIImage(NSData.FromFile("Magazine.png"), scale), "Журнал", () =>
            {
                if (NavigationController.VisibleViewController != ViewControllersContainer.MagazineViewController)
                {
                    NavigationController.PresentViewController(ViewControllersContainer.MagazineViewController, true, () =>
                    {
                    });
                }
            });

            button++;

            var archiveButton = new BottomToolbarButton(new RectangleF(button * buttonWidth, 0, buttonWidth, Frame.Height), 
                new UIImage(NSData.FromFile("Archive.png"), scale), "Архив", () =>
            {
                if (NavigationController.VisibleViewController != ViewControllersContainer.ArchiveViewController)
                {
                    NavigationController.PresentViewController(ViewControllersContainer.ArchiveViewController, true, () =>
                    {
                    });
                }
            });

            button++;

            var favoritesButton = new BottomToolbarButton(new RectangleF(button * buttonWidth, 0, buttonWidth, Frame.Height), 
                new UIImage(NSData.FromFile("Favorites.png"), scale), "Избранное", () =>
            {
                if (NavigationController.VisibleViewController != ViewControllersContainer.FavoritesViewController)
                {
                    NavigationController.PresentViewController(ViewControllersContainer.FavoritesViewController, true, () =>
                    {
                    });
                }
            });

            Add(newsButton);
            Add(trendsButton);
            Add(magazineButton);
            Add(archiveButton);
            Add(favoritesButton);
        }

        public override void LayoutIfNeeded()
        {
            foreach (var subView in Subviews)
            {
                subView.RemoveFromSuperview();
            }

            AddButtons();
        }
    }
}

