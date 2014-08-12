using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace ItExpert
{
    public static class NavigationBarButton
    {
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

        public static UIButton GetButton(string filePath, float scale)
        {
            UIButton button = new UIButton();

            var image = new UIImage(NSData.FromFile(filePath), scale);

            button.SetImage(image, UIControlState.Normal);
            button.Frame = new System.Drawing.RectangleF(0, 0, image.Size.Width, image.Size.Height);

            return button;
        }

        public static void ShowWindow(UIViewController view)
        {
            _window = new UIWindow(UIScreen.MainScreen.Bounds);

            _oldKeyWindow = UIApplication.SharedApplication.KeyWindow;

            _window.WindowLevel = UIWindowLevel.Alert;
            _window.RootViewController = view;

            _window.MakeKeyAndVisible();
        }

        public static void HideWindow()
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
        private static UIBarButtonItem _logoImage;

        private static UIWindow _oldKeyWindow;
        private static UIWindow _window;
    }
}

