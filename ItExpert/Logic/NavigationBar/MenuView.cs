using System;
using MonoTouch.UIKit;
using System.Drawing;
using System.Collections.Generic;

namespace ItExpert
{
    public class MenuView: UIViewController
    {
        public event EventHandler TapOutsideTableView;
		private Action<object,EventArgs> _newsClick;
		private Action<object,EventArgs> _trendsClick;
		private Action<object,EventArgs> _magazineClick;
		private Action<object,EventArgs> _archiveClick;
		private Action<object,EventArgs> _favoriteClick;
		private Action<object,EventArgs> _aboutUsClick;
		private Action<string> _searchClick;

		public MenuView(Action<object,EventArgs> newsClick, Action<object,EventArgs> trendsClick, Action<object,EventArgs> magazineClick,
			Action<object,EventArgs> archiveClick, Action<object,EventArgs> favoriteClick, Action<object,EventArgs> aboutUsClick, Action<string> searchClick)
		{
			_newsClick = newsClick;
			_trendsClick = trendsClick;
			_magazineClick = magazineClick;
			_archiveClick = archiveClick;
			_favoriteClick = favoriteClick;
			_aboutUsClick = aboutUsClick;
			_searchClick = searchClick;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			_newsClick = null;
			_trendsClick = null;
			_magazineClick = null;
			_archiveClick = null;
			_favoriteClick = null;
			_aboutUsClick = null;
			_searchClick = null;
			TapOutsideTableView = null;
			if (_menuTableView != null)
			{
				if (_menuTableView.Source != null)
				{
					_menuTableView.Source.Dispose();
				}
				_menuTableView.Source = null;
				_menuTableView.Dispose();
			}
			_menuTableView = null;
			if (_tapableViewHorizontal != null)
			{
				if (_tapableViewHorizontal.GestureRecognizers != null)
				{
					foreach (var gr in _tapableViewHorizontal.GestureRecognizers)
					{
						gr.Dispose();
					}
				}
				_tapableViewHorizontal.GestureRecognizers = new UIGestureRecognizer[0];
				_tapableViewHorizontal.Dispose();
			}
			_tapableViewHorizontal = null;

			if (_tapableViewVertical != null)
			{
				if (_tapableViewVertical.GestureRecognizers != null)
				{
					foreach (var gr in _tapableViewVertical.GestureRecognizers)
					{
						gr.Dispose();
					}
				}
				_tapableViewVertical.GestureRecognizers = new UIGestureRecognizer[0];
				_tapableViewVertical.Dispose();
			}
			_tapableViewVertical = null;
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _navigationController = (UIApplication.SharedApplication.KeyWindow.RootViewController as UINavigationController);

            View.BackgroundColor = UIColor.Clear;

            _menuTableView = new UITableView();

			_menuTableView.Frame = new RectangleF(4, ItExpertHelper.StatusBarHeight + _navigationController.NavigationBar.Frame.Height / 2, 200, 254);
            _menuTableView.BackgroundColor = UIColor.Black;
            _menuTableView.ScrollEnabled = true; 
            _menuTableView.UserInteractionEnabled = true;
            _menuTableView.SeparatorInset = new UIEdgeInsets (0, 0, 0, 0);
            _menuTableView.Bounces = false;
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
				_menuTableView.Frame = new RectangleF(4, ItExpertHelper.StatusBarHeight + _navigationController.NavigationBar.Frame.Height / 2, 200, 254);
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
					var search = value as string;
					if (search != null)
					{
						search = search.Trim();
						if (!string.IsNullOrWhiteSpace(search))
						{
							_searchClick(search);
						}
					}
					OnTapOutsideTableView();
				}
			});

            menuItems.Add(new NavigationBarItem(NavigationBarItem.ContentType.MenuItem)
            { 
                Title = "Новости", 
                ButtonPushed = (value) =>
                {
					_newsClick(null, new EventArgs());
					OnTapOutsideTableView();
                }
            });

            menuItems.Add(new NavigationBarItem(NavigationBarItem.ContentType.MenuItem) 
            { 
                Title = "Журнал", 
                ButtonPushed = (value) =>
                {
					_magazineClick(null, new EventArgs());
					OnTapOutsideTableView();
                }
            });

            menuItems.Add(new NavigationBarItem(NavigationBarItem.ContentType.MenuItem) 
            { 
                Title = "Архив журнала", 
                ButtonPushed = (value) =>
                {
					_archiveClick(null, new EventArgs());
					OnTapOutsideTableView();
                }
            });

            menuItems.Add(new NavigationBarItem(NavigationBarItem.ContentType.MenuItem) 
            { 
                Title = "Тренды", 
                ButtonPushed = (value) =>
                {
					_trendsClick(null, new EventArgs());
					OnTapOutsideTableView();
                }
            });

            menuItems.Add(new NavigationBarItem(NavigationBarItem.ContentType.MenuItem) 
            { 
                Title = "Избранное", 
                ButtonPushed = (value) =>
                {
					_favoriteClick(null, new EventArgs());
					OnTapOutsideTableView();
                }
            });

            menuItems.Add(new NavigationBarItem(NavigationBarItem.ContentType.MenuItem) 
            { 
                Title = "О нас", 
                ButtonPushed = (value) =>
                {
					_aboutUsClick(null, new EventArgs());
					OnTapOutsideTableView();
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

