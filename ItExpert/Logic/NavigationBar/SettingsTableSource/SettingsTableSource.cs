using System;
using MonoTouch.UIKit;
using System.Collections.Generic;

namespace ItExpert
{
    public class SettingsTableSource: UITableViewSource
    {
        public SettingsTableSource(List<SettingsItem> settingsItems)
        {

        }

        public override int RowsInSection(UITableView tableview, int section)
        {
            return _settingsItems.Count;
        }

        private List<SettingsItem> _settingsItems;
    }
}

