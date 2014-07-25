using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace ItExpert
{
    public static class NavigationBarButton
    {
		public static UIBarButtonItem GetMenu(MenuView menu)
		{
			UIButton button = GetButton("NavigationBar/Menu.png", 2);

			var menuButton = new UIBarButtonItem(button);

			menu.TapOutsideTableView += (sender, e) =>
			{
				HideWindow();
			};

			button.TouchUpInside += (sender, e) =>
			{
				ShowWindow(menu);
			};

			return menuButton;
		}

        public static UIBarButtonItem Back
        {
            get
            {
                if (_backButton == null)
                {
                    UIButton button = GetButton("NavigationBar/Back.png", 2);

                    button.TouchUpInside += (sender, e) => 
                    {
                        if (UIApplication.SharedApplication.KeyWindow.RootViewController is UINavigationController)
                        {
                            (UIApplication.SharedApplication.KeyWindow.RootViewController as UINavigationController).PopViewControllerAnimated(true);
                        }
                    };

                    _backButton = new UIBarButtonItem(button);
                }

                return _backButton;
            }
        }

		public static UIBarButtonItem GetDumpInCacheButton(Action clickAction)
		{
			UIButton button = GetButton("NavigationBar/DumpInCache.png", 4);
			button.TouchUpInside += (sender, e) => clickAction();
			var dumpInCacheButton = new UIBarButtonItem(button);
			return dumpInCacheButton;
		}

		public static UIBarButtonItem GetRefreshButton(Action clickAction)
		{
			UIButton button = GetButton("NavigationBar/Refresh.png", 4.1f);
			button.TouchUpInside += (sender, e) => clickAction();
			var refreshButton = new UIBarButtonItem(button);
			return refreshButton;
		}

		public static UIBarButtonItem GetSettingsButton(bool forDetail)
		{
			UIButton button = GetButton("NavigationBar/Settings.png", 4.1f);

			var settingsButton = new UIBarButtonItem(button);

			var settingsView = new SettingsView(forDetail);
			settingsView.TapOutsideTableView += (sender, e) =>
			{
				HideWindow();
			};

			button.TouchUpInside += (sender, e) =>
			{
				ShowWindow(settingsView);
			};

			return settingsButton;
		}

        public static UIBarButtonItem Share
        {
            get
            {
                if (_shareButton == null)
                {
                    UIButton button = GetButton("NavigationBar/Share.png", 4.5f);

                    _shareButton = new UIBarButtonItem(button);

                    _shareView = new ShareView();
                    _shareView.TapOutsideTableView += (sender, e) => 
                    {
                        HideWindow();
                    };

                    button.TouchUpInside += (sender, e) => 
                    {
                        ShowWindow(_shareView);
						_shareView.Update();
                    };
                }

                return _shareButton;
            }
        }

        public static UIBarButtonItem Logo
        {
            get
            {
                if (_logoImage == null)
                {
                    _logoImage = new UIBarButtonItem(new UIImageView(new UIImage(NSData.FromFile("NavigationBar/Logo.png"), 2)));
                }

                return _logoImage;
            }
        }

        private static UIButton GetButton(string filePath, float scale)
        {
            UIButton button = new UIButton();

            var image = new UIImage(NSData.FromFile(filePath), scale);

            button.SetImage(image, UIControlState.Normal);
            button.Frame = new System.Drawing.RectangleF(0, 0, image.Size.Width, image.Size.Height);

            return button;
        }

        private static void ShowWindow(UIViewController view)
        {
            _window = new UIWindow(UIScreen.MainScreen.Bounds);

            _oldKeyWindow = UIApplication.SharedApplication.KeyWindow;

            _window.WindowLevel = UIWindowLevel.Alert;
            _window.RootViewController = view;

            _window.MakeKeyAndVisible();
        }

        private static void HideWindow()
        {
            if (_window != null)
            {
                _window.ResignKeyWindow();
                _window.Alpha = 0;

                if (_oldKeyWindow != null)
                {
                    _oldKeyWindow.MakeKeyWindow();
                }

                _window.Dispose();
                _window = null;
            }
        }

        private static UIBarButtonItem _backButton;
        private static UIBarButtonItem _shareButton;
        private static UIBarButtonItem _logoImage;

        private static UIWindow _oldKeyWindow;
        private static UIWindow _window;
        private static ShareView _shareView;
    }
}

