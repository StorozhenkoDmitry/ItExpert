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

        public void UpdateContent(SettingsItem item)
        {
            var creator = CreatorFactory(item);

            creator.UpdateContent(this, item);
        }

        public float GetHeightDependingOnContent(SettingsItem item)
        {
            return 44;
        }

        private BaseSettingsItemContentCreator CreatorFactory(SettingsItem item)
        {
            switch (item.Type)
            {
                case SettingsItem.ContentType.Buttons:
                    if (_creatorsPool.ContainsKey(SettingsItem.ContentType.Buttons))
                    {
                        _creatorsPool.Add(SettingsItem.ContentType.Buttons, new SettingsButtonsContentCreator());
                    }

                    return _creatorsPool[SettingsItem.ContentType.Buttons];

                case SettingsItem.ContentType.RadioButton:
                    if (_creatorsPool.ContainsKey(SettingsItem.ContentType.RadioButton))
                    {
                        _creatorsPool.Add(SettingsItem.ContentType.RadioButton, new SettingsRadioButtonContentCreator());
                    }

                    return _creatorsPool[SettingsItem.ContentType.RadioButton];

                case SettingsItem.ContentType.Slider:
                    if (_creatorsPool.ContainsKey(SettingsItem.ContentType.Slider))
                    {
                        _creatorsPool.Add(SettingsItem.ContentType.Slider, new SettingsSliderContentCreator());
                    }

                    return _creatorsPool[SettingsItem.ContentType.Slider];

                case SettingsItem.ContentType.Switch:
                    if (_creatorsPool.ContainsKey(SettingsItem.ContentType.Switch))
                    {
                        _creatorsPool.Add(SettingsItem.ContentType.Switch, new SettingsSwitchContentCreator());
                    }

                    return _creatorsPool[SettingsItem.ContentType.Switch];

                case SettingsItem.ContentType.Tap:
                    if (_creatorsPool.ContainsKey(SettingsItem.ContentType.Tap))
                    {
                        _creatorsPool.Add(SettingsItem.ContentType.Tap, new SettingsTapContentCreator());
                    }

                    return _creatorsPool[SettingsItem.ContentType.Tap];

                default:
                    throw new NotImplementedException("Content creator type for settings view isn't implemented.");
            }
        }

        Dictionary<SettingsItem.ContentType, BaseSettingsItemContentCreator> _creatorsPool;
    }
}

