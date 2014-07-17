using System;
using MonoTouch.UIKit;
using System.Collections.Generic;

namespace ItExpert
{
    public class NavigationBarViewCell: UITableViewCell
    {
        public NavigationBarViewCell(UITableViewCellStyle style, string reuseIdentifier)
            :base (style, reuseIdentifier)
        {
            BackgroundColor = ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetBackgroundColor());

            _creatorsPool = new Dictionary<NavigationBarItem.ContentType, BaseNavigationBarContentCreator>();
        }

        public void UpdateContent(NavigationBarItem item)
        {
            var creator = CreatorFactory(item);

            creator.UpdateContent(this, item);
        }

        public float GetHeightDependingOnContent(NavigationBarItem item)
        {
            return 44;
        }

        private BaseNavigationBarContentCreator CreatorFactory(NavigationBarItem item)
        {
            switch (item.Type)
            {
                case NavigationBarItem.ContentType.Buttons:
                    if (!_creatorsPool.ContainsKey(NavigationBarItem.ContentType.Buttons))
                    {
                        _creatorsPool.Add(NavigationBarItem.ContentType.Buttons, new SettingsButtonsContentCreator());
                    }

                    return _creatorsPool[NavigationBarItem.ContentType.Buttons];

                case NavigationBarItem.ContentType.RadioButton:
                    if (!_creatorsPool.ContainsKey(NavigationBarItem.ContentType.RadioButton))
                    {
                        _creatorsPool.Add(NavigationBarItem.ContentType.RadioButton, new SettingsRadioButtonContentCreator());
                    }

                    return _creatorsPool[NavigationBarItem.ContentType.RadioButton];

                case NavigationBarItem.ContentType.Slider:
                    if (!_creatorsPool.ContainsKey(NavigationBarItem.ContentType.Slider))
                    {
                        _creatorsPool.Add(NavigationBarItem.ContentType.Slider, new SettingsSliderContentCreator());
                    }

                    return _creatorsPool[NavigationBarItem.ContentType.Slider];

                case NavigationBarItem.ContentType.Switch:
                    if (!_creatorsPool.ContainsKey(NavigationBarItem.ContentType.Switch))
                    {
                        _creatorsPool.Add(NavigationBarItem.ContentType.Switch, new SettingsSwitchContentCreator());
                    }

                    return _creatorsPool[NavigationBarItem.ContentType.Switch];

                case NavigationBarItem.ContentType.Tap:
                    if (!_creatorsPool.ContainsKey(NavigationBarItem.ContentType.Tap))
                    {
                        _creatorsPool.Add(NavigationBarItem.ContentType.Tap, new SettingsTapContentCreator());
                    }

                    return _creatorsPool[NavigationBarItem.ContentType.Tap];

                default:
                    throw new NotImplementedException("Content creator type for settings view isn't implemented.");
            }
        }

        Dictionary<NavigationBarItem.ContentType, BaseNavigationBarContentCreator> _creatorsPool;
    }
}

