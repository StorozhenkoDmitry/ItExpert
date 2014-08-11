using System;
using MonoTouch.UIKit;
using System.Collections.Generic;
using MonoTouch.Foundation;

namespace ItExpert
{
    public class NavigationBarTableSource: UITableViewSource
    {
        public NavigationBarTableSource(List<NavigationBarItem> settingsItems)
        {
            _items = settingsItems;
            _cellIdentifier = "SettingsCell";
        }

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (_items != null)
			{
				foreach (var item in _items)
				{
					item.Dispose();
				}
				_items.Clear();
			}
			_items = null;
		}

        public override int RowsInSection(UITableView tableview, int section)
        {
            return _items.Count;
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
            cell.UpdateContent(_items[indexPath.Row]);

            return cell;
        }

        public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
        {
            return CreateCell(tableView).GetHeightDependingOnContent(_items[indexPath.Row]);
        }

        private NavigationBarViewCell CreateCell(UITableView tableView)
        {
            var cell = new NavigationBarViewCell(UITableViewCellStyle.Default, _cellIdentifier);

            cell.Frame = new System.Drawing.RectangleF(0, 0, tableView.Frame.Width, cell.Frame.Height);
            cell.ContentView.Frame = new System.Drawing.RectangleF(0, 0, tableView.Frame.Width, cell.Frame.Height);

            return cell;
        }

        string _cellIdentifier;

        private List<NavigationBarItem> _items;
    }
}

