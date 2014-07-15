using System;
using MonoTouch.UIKit;

namespace ItExpert
{
    public abstract class BaseSettingsItemContentCreator
    {
        public BaseSettingsItemContentCreator()
        {
            _needToCreateContent = true;

            _padding = new UIEdgeInsets (8, 12, 8, 12);
        }

        public void UpdateContent(UITableViewCell cell, SettingsItem item)
        {
            ItExpertHelper.RemoveSubviews(cell.ContentView);

            _textFont = UIFont.SystemFontOfSize(ApplicationWorker.Settings.TextSize);
            _forecolor = ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetForeColor());

            cell.UserInteractionEnabled = true;

            if (_needToCreateContent)
            {
                Create(cell, item);

                _needToCreateContent = false;
            }
            else
            {
                Update(cell, item);
            }
        }

        protected abstract void Create(UITableViewCell cell, SettingsItem item);
        
        protected abstract void Update(UITableViewCell cell, SettingsItem item);
        
        protected bool _needToCreateContent;
        
        protected UIEdgeInsets _padding;
        protected UIFont _textFont;
        protected UIColor _forecolor;
    }
}

