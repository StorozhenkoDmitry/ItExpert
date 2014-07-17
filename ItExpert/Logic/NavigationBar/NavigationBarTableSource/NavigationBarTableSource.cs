using System;
using MonoTouch.UIKit;
using System.Collections.Generic;

namespace ItExpert
{
    public class NavigationBarTableSource: UITableViewSource
    {
        public NavigationBarTableSource(List<NavigationBarItem> settingsItems)
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
            NavigationBarViewCell cell = tableView.DequeueReusableCell (_cellIdentifier) as NavigationBarViewCell;

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

        private NavigationBarViewCell CreateCell(UITableView tableView)
        {
            var cell = new NavigationBarViewCell(UITableViewCellStyle.Default, _cellIdentifier);

            cell.Frame = new System.Drawing.RectangleF(0, 0, tableView.Frame.Width, cell.Frame.Height);
            cell.ContentView.Frame = new System.Drawing.RectangleF(0, 0, tableView.Frame.Width, cell.Frame.Height);

            return cell;
        }

        string _cellIdentifier;

        private List<NavigationBarItem> _settingsItems;
    }
}

