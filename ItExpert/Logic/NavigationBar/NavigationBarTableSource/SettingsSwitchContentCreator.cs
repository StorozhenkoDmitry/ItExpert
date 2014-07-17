using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace ItExpert
{
    public class SettingsSwitchContentCreator : BaseNavigationBarContentCreator
    {
        protected override void Create(UITableViewCell cell, NavigationBarItem item)
        {
            CreateSwitch(cell.ContentView.Frame.Size, item);

            _switch.On = (bool)item.GetValue();

            cell.ContentView.Add(_switch);
        }

        protected override void Update(UITableViewCell cell, NavigationBarItem item)
        {
            if (_switch == null)
            {
                CreateSwitch(cell.ContentView.Frame.Size, item);
            }

            _switch.On = (bool)item.GetValue();

            cell.ContentView.Add(_switch);
        }

        private void CreateSwitch(SizeF contentSize, NavigationBarItem item)
        {
            _switch = new UISwitch();

            _switch.Frame = new RectangleF(new PointF(contentSize.Width - _switch.Frame.Width - _padding.Right, 
                contentSize.Height / 2 - _switch.Frame.Height / 2), _switch.Frame.Size);

            _switch.ValueChanged += (sender, e) => 
            {
                _item.SetValue((sender as UISwitch).On);
            };
        }

        private UISwitch _switch;
    }
}

