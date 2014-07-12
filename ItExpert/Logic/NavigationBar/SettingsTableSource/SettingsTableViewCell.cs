using System;
using MonoTouch.UIKit;
using System.Collections.Generic;

namespace ItExpert
{
    public class SettingsTableViewCell: UITableViewCell
    {
        public SettingsTableViewCell(UITableViewCellStyle style, string reuseIdentifier)
            :base (style, reuseIdentifier)
        {
            BackgroundColor = ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetBackgroundColor());

            _creatorsPool = new Dictionary<SettingsItem.ContentType, BaseSettingsItemContentCreator>();
        }

        Dictionary<SettingsItem.ContentType, BaseSettingsItemContentCreator> _creatorsPool;
    }
}

