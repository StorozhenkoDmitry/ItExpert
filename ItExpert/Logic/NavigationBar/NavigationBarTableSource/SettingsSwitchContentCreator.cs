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

		public override void Dispose()
		{
			base.Dispose();
			if (_textView != null)
			{
				_textView.Dispose();
			}
			_textView = null;
			if (_switch != null)
			{
				_switch.ValueChanged -= SwitchValueChanged;
				_switch.Dispose();
			}
			_switch = null;
		}

		void AddContents(UIView contentView)
		{
			var frame = contentView.Frame;
			var settingsSwitchView = new SettingsSwitchView(frame, _switch, _textView, SwitchValueChanged);
			contentView.Add(settingsSwitchView);
		}

        protected override void Create(UITableViewCell cell, NavigationBarItem item)
        {
            CreateSwitch(cell.ContentView.Frame.Size, item);

            _switch.On = (bool)item.GetValue();

			AddContents(cell.ContentView);
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

			AddContents(cell.ContentView);
        }

        private void CreateSwitch(SizeF viewSize, NavigationBarItem item)
        {
            CreateTextView(viewSize, item);

            _switch = new UISwitch();

            _switch.Frame = new RectangleF(new PointF(viewSize.Width - _switch.Frame.Width - _padding.Right, 
                viewSize.Height / 2 - _switch.Frame.Height / 2), _switch.Frame.Size);

            
        }

		void SwitchValueChanged(object sender, EventArgs e)
		{
			_item.SetValue((sender as UISwitch).On);
		}

        private UISwitch _switch;
    }

	public class SettingsSwitchView : UIView, ICleanupObject
	{
		private UISwitch _switch;
		private UITextView _textView;
		private EventHandler _switchValueChanged;

		public SettingsSwitchView(RectangleF frame, UISwitch switchCtrl, UITextView textView, 
			EventHandler switchValueChanged): base(frame)
		{
			_switch = switchCtrl;
			_textView = textView;
			_switchValueChanged = switchValueChanged;
			_switch.ValueChanged += _switchValueChanged;
			Add(_textView);
			Add(_switch);
		}

		public void CleanUp()
		{
			foreach (var view in Subviews)
			{
				view.RemoveFromSuperview();
			}
			if (_textView != null)
			{
				_textView.Dispose();
			}
			_textView = null;
			if (_switch != null)
			{
				_switch.ValueChanged -= _switchValueChanged;
			}
			_switch = null;
			_switchValueChanged = null;
		}
	}
}

