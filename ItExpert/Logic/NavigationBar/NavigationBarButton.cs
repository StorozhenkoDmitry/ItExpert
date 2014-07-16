using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace ItExpert
{
    public static class NavigationBarButton
    {
        public static UIBarButtonItem Menu
        {
            get
            {
                if (_menuButton == null)
                {
                    UIButton button = GetButton("NavigationBar/Menu.png", 2);

                    _menuButton = new UIBarButtonItem(button);
                }

                return _menuButton;
            }
        }

        public static UIBarButtonItem Back
        {
            get
            {
                if (_backButton == null)
                {
                    UIButton button = GetButton("NavigationBar/Back.png", 4);

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

        public static UIBarButtonItem DumpInCache
        {
            get
            {
                if (_dumpInCacheButton == null)
                {
                    UIButton button = GetButton("NavigationBar/DumpInCache.png", 4);

                    _dumpInCacheButton = new UIBarButtonItem(button);
                }

                return _dumpInCacheButton;
            }
        }

        public static UIBarButtonItem Refresh
        {
            get
            {
                if (_refreshButton == null)
                {
                    UIButton button = GetButton("NavigationBar/Refresh.png", 4);

                    button.TouchUpInside += (sender, e) => Console.WriteLine("Refresh touch up inside.");

                    _refreshButton = new UIBarButtonItem(button);
                }

                return _refreshButton;
            }
        }

        public static UIBarButtonItem Settings
        {
            get
            {
                if (_settingsButton == null)
                {
                    UIButton button = GetButton("NavigationBar/Settings.png", 4);

                    _settingsButton = new UIBarButtonItem(button);

                    _settingsView = new SettingsView();
                    _settingsView.TapOutsideTableView += (sender, e) => 
                    {
                        if (_settingsWindow != null)
                        {
                            _settingsWindow.ResignKeyWindow();
                            _settingsWindow.Alpha = 0;

                            if (_oldKeyWindow != null)
                            {
                                _oldKeyWindow.MakeKeyWindow();
                            }

                            _settingsWindow.Dispose();
                            _settingsWindow = null;
                        }
                    };

                    button.TouchUpInside += (sender, e) => 
                    {
                        _settingsWindow = new UIWindow(UIScreen.MainScreen.Bounds);

                        _oldKeyWindow = UIApplication.SharedApplication.KeyWindow;

                        _settingsWindow.WindowLevel = UIWindowLevel.Alert;
                        _settingsWindow.RootViewController = _settingsView;

                        _settingsWindow.MakeKeyAndVisible();
                    };
                }

                return _settingsButton;
            }
        }

        public static UIBarButtonItem Share
        {
            get
            {
                if (_shareButton == null)
                {
                    UIButton button = GetButton("NavigationBar/DumpInCache.png", 4);

                    _shareButton = new UIBarButtonItem(button);
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

        private static UIBarButtonItem _menuButton;
        private static UIBarButtonItem _backButton;
        private static UIBarButtonItem _dumpInCacheButton;
        private static UIBarButtonItem _refreshButton;
        private static UIBarButtonItem _settingsButton;
        private static UIBarButtonItem _shareButton;
        private static UIBarButtonItem _logoImage;

        private static UIWindow _oldKeyWindow;
        private static UIWindow _settingsWindow;
        private static SettingsView _settingsView;
    }
}

