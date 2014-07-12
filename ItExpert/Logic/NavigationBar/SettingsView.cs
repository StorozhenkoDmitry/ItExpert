using System;
using MonoTouch.UIKit;

namespace ItExpert
{
    public class SettingsView: UIView
    {
        public SettingsView()
        {
            _settingsTableView = new UITableView();
        }

        private UITableView _settingsTableView;
    }
}

