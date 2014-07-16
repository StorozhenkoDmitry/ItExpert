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
            _cellIdentifier = "SettingsCell";
        }

        public override int RowsInSection(UITableView tableview, int section)
        {
            return _settingsItems.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
        {
            SettingsTableViewCell cell = tableView.DequeueReusableCell (_cellIdentifier) as SettingsTableViewCell;

            if (cell != null)
            {
                cell.Frame = new System.Drawing.RectangleF(0, 0, tableView.Frame.Width, cell.Frame.Height);
                cell.ContentView.Frame = new System.Drawing.RectangleF(0, 0, tableView.Frame.Width, cell.Frame.Height);
            }
            else
            {
                cell = CreateCell(tableView);
            }           

            cell.SelectionStyle = UITableViewCellSelectionStyle.None;
            cell.ContentView.Bounds = cell.Bounds;
            cell.UpdateContent(_settingsItems[indexPath.Row]);

            return cell;
        }

        private SettingsTableViewCell CreateCell(UITableView tableView)
        {
            var cell = new SettingsTableViewCell(UITableViewCellStyle.Default, _cellIdentifier);

            cell.Frame = new System.Drawing.RectangleF(0, 0, tableView.Frame.Width, cell.Frame.Height);
            cell.ContentView.Frame = new System.Drawing.RectangleF(0, 0, tableView.Frame.Width, cell.Frame.Height);

            return cell;
        }

        string _cellIdentifier;

        private List<SettingsItem> _settingsItems;
    }
}

