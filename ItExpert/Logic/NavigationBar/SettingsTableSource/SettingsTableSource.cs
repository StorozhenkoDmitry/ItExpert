using System;
using MonoTouch.UIKit;
using System.Collections.Generic;

namespace ItExpert
{
    public class SettingsTableSource: UITableViewSource
    {
        public SettingsTableSource(List<SettingsItem> settingsItems)
        {
            _settingsItems = settingsItems;
        }

        public override int RowsInSection(UITableView tableview, int section)
        {
            return _settingsItems.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
        {
            return new UITableViewCell();
        }

        private List<SettingsItem> _settingsItems;
    }
}

