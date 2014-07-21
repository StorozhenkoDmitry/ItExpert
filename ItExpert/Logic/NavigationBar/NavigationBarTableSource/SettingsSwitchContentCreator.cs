using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace ItExpert
{
    public class SettingsSwitchContentCreator : BaseNavigationBarContentCreator
    {
        public override float GetContentHeight(UIView cellContentView, NavigationBarItem item)
        {
            return _height;
        }

        protected override void Create(UITableViewCell cell, NavigationBarItem item)
        {
            CreateSwitch(cell.ContentView.Frame.Size, item);

            _switch.On = (bool)item.GetValue();

            cell.ContentView.Add(_textView);
            cell.ContentView.Add(_switch);
        }

        protected override void Update(UITableViewCell cell, NavigationBarItem item)
        {
            if (_switch == null)
            {
                CreateSwitch(cell.ContentView.Frame.Size, item);
            }
            else
            {
                _textView.Dispose();
                _textView = null;

                CreateTextView(cell.ContentView.Frame.Size, item);
            }

            _switch.Frame = new RectangleF(new PointF(cell.ContentView.Frame.Width - _switch.Frame.Width - _padding.Right, 
                cell.ContentView.Frame.Height / 2 - _switch.Frame.Height / 2), _switch.Frame.Size);

            _switch.On = (bool)item.GetValue();

            cell.ContentView.Add(_textView);
            cell.ContentView.Add(_switch);
        }

        private void CreateSwitch(SizeF viewSize, NavigationBarItem item)
        {
            CreateTextView(viewSize, item);

            _switch = new UISwitch();

            _switch.Frame = new RectangleF(new PointF(viewSize.Width - _switch.Frame.Width - _padding.Right, 
                viewSize.Height / 2 - _switch.Frame.Height / 2), _switch.Frame.Size);

            _switch.ValueChanged += (sender, e) => 
            {
                _item.SetValue((sender as UISwitch).On);
            };
        }

        private UISwitch _switch;
    }
}

