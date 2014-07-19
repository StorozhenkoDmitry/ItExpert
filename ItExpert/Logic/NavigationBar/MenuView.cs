using System;
using MonoTouch.UIKit;
using System.Drawing;
using System.Collections.Generic;

namespace ItExpert
{
    public class MenuView: UIViewController
    {
        public event EventHandler TapOutsideTableView;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _navigationController = (UIApplication.SharedApplication.KeyWindow.RootViewController as UINavigationController);

            View.BackgroundColor = UIColor.Clear;

            _menuTableView = new UITableView();

            _menuTableView.Frame = new RectangleF(4, ItExpertHelper.StatusBarHeight + _navigationController.NavigationBar.Frame.Height / 2, 160, 254);
            _menuTableView.BackgroundColor = UIColor.Black;
            _menuTableView.ScrollEnabled = true; 
            _menuTableView.UserInteractionEnabled = true;
            _menuTableView.SeparatorInset = new UIEdgeInsets (0, 0, 0, 0);
            _menuTableView.Bounces = true;
            _menuTableView.SeparatorColor = UIColor.FromRGB(100, 100, 100);
            _menuTableView.Source = new NavigationBarTableSource(GetMenuItems());
            _menuTableView.Bounces = false;

            _tapableViewHorizontal = new UIView(new RectangleF(0, _menuTableView.Frame.Bottom, View.Frame.Width, View.Frame.Height - _menuTableView.Frame.Bottom));

            _tapableViewHorizontal.BackgroundColor = UIColor.Clear;
            _tapableViewHorizontal.AddGestureRecognizer(new UITapGestureRecognizer(() =>
            {
                OnTapOutsideTableView();
            }));

            _tapableViewVertical = new UIView(new RectangleF(_menuTableView.Frame.Right, 0, View.Frame.Width - _menuTableView.Frame.Left, View.Frame.Height - _tapableViewHorizontal.Frame.Top));

            _tapableViewVertical.BackgroundColor = UIColor.Clear;
            _tapableViewVertical.AddGestureRecognizer(new UITapGestureRecognizer(() =>
            {
                OnTapOutsideTableView();
            }));

            View.Add(_menuTableView);
            View.Add(_tapableViewHorizontal);
            View.Add(_tapableViewVertical);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            CorrectViewsFrame();
        }

        public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
        {
            base.DidRotate(fromInterfaceOrientation);

            CorrectViewsFrame();
        }

        private void CorrectViewsFrame()
        {

            if (_menuTableView != null)
            {
                _menuTableView.Frame = new RectangleF(4, ItExpertHelper.StatusBarHeight + _navigationController.NavigationBar.Frame.Height / 2, 160, 254);
            }

            if (_tapableViewHorizontal != null && _menuTableView != null)
            {
                _tapableViewHorizontal.Frame = new RectangleF(0, _menuTableView.Frame.Bottom, View.Frame.Width, View.Frame.Height - _menuTableView.Frame.Bottom);
            }

            if (_tapableViewVertical != null && _menuTableView != null && _tapableViewHorizontal != null)
            {
                _tapableViewVertical.Frame = new RectangleF(_menuTableView.Frame.Right, 0, View.Frame.Width - _menuTableView.Frame.Left, View.Frame.Height - _tapableViewHorizontal.Frame.Top);
            }
        }

        private List<NavigationBarItem> GetMenuItems()
        {
            List<NavigationBarItem> menuItems = new List<NavigationBarItem>();

            menuItems.Add(new NavigationBarItem(NavigationBarItem.ContentType.Search) 
            { 
                SetValue = (value) =>
                {
                    Console.WriteLine ("Поиск со значением {0}", value.ToString());
                }
            });

            menuItems.Add(new NavigationBarItem(NavigationBarItem.ContentType.MenuItem)
            { 
                Title = "Новости", 
                ButtonPushed = (value) =>
                {
                    Console.WriteLine ("Новости");
                }
            });

            menuItems.Add(new NavigationBarItem(NavigationBarItem.ContentType.MenuItem) 
            { 
                Title = "Журнал", 
                ButtonPushed = (value) =>
                {
                    Console.WriteLine ("Журнал");
                }
            });

            menuItems.Add(new NavigationBarItem(NavigationBarItem.ContentType.MenuItem) 
            { 
                Title = "Архив журнала", 
                ButtonPushed = (value) =>
                {
                    Console.WriteLine ("Архив журнала");
                }
            });

            menuItems.Add(new NavigationBarItem(NavigationBarItem.ContentType.MenuItem) 
            { 
                Title = "Тренды", 
                ButtonPushed = (value) =>
                {
                    Console.WriteLine ("Тренды");
                }
            });

            menuItems.Add(new NavigationBarItem(NavigationBarItem.ContentType.MenuItem) 
            { 
                Title = "Избранное", 
                ButtonPushed = (value) =>
                {
                    Console.WriteLine ("Избранное");
                }
            });

            menuItems.Add(new NavigationBarItem(NavigationBarItem.ContentType.MenuItem) 
            { 
                Title = "О нас", 
                ButtonPushed = (value) =>
                {
                    Console.WriteLine ("О нас");
                }
            });

            return menuItems;
        }

        private void OnTapOutsideTableView()
        {
            if (TapOutsideTableView != null)
            {
                TapOutsideTableView(this, EventArgs.Empty);
            }
        }

        private UITableView _menuTableView;
        private UIView _tapableViewHorizontal;
        private UIView _tapableViewVertical;
        private UINavigationController _navigationController;
    }
}

