using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace ItExpert
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		
		public override UIWindow Window {
			get;
			set;
		}
		
        public override void FinishedLaunching(UIApplication application)
        {
            Window = new UIWindow(UIScreen.MainScreen.Bounds);

            UINavigationController rootController = new UINavigationController();

            Window.RootViewController = rootController;
            Window.MakeKeyAndVisible();

            rootController.AutomaticallyAdjustsScrollViewInsets = false;
			rootController.PushViewController (new SplashViewController (), false);
        }
	}
}

