using System;
using MonoTouch.UIKit;
using System.Collections.Generic;

namespace ItExpert
{
    public class SettingsView: UIViewController
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = UIColor.Clear;

            _settingsTableView = new UITableView();

            var screenSize = ItExpertHelper.GetRealScreenSize();

            var navigationBarHeight = (UIApplication.SharedApplication.KeyWindow.RootViewController as UINavigationController).NavigationBar.Frame.Height;

            _settingsTableView.Frame = new System.Drawing.RectangleF(0, ItExpertHelper.StatusBarHeight + navigationBarHeight, screenSize.Width, screenSize.Height / 2);
            _settingsTableView.BackgroundColor = UIColor.Black;
            _settingsTableView.ScrollEnabled = true; 
            _settingsTableView.UserInteractionEnabled = true;
            _settingsTableView.SeparatorInset = new UIEdgeInsets (0, 0, 0, 0);
            _settingsTableView.Bounces = true;
            _settingsTableView.SeparatorColor = UIColor.FromRGB(100, 100, 100);

            _settingsTableView.Source = new SettingsTableSource(CreateSettingsItems());

            View.Add(_settingsTableView);
        }

        private List<SettingsItem> CreateSettingsItems()
        {
            List<SettingsItem> settingsItems = new List<SettingsItem>();

            settingsItems.Add(new SettingsItem(SettingsItem.ContentType.Switch, "Оффлайн режим"));
            settingsItems.Add(new SettingsItem(SettingsItem.ContentType.RadioButton, "Скрывать прочитанные статьи"));
            settingsItems.Add(new SettingsItem(SettingsItem.ContentType.Slider, "Размер шрифта"));
            settingsItems.Add(new SettingsItem(SettingsItem.ContentType.Tap, "Тема"));
            settingsItems.Add(new SettingsItem(SettingsItem.ContentType.Tap, "Стартовый раздел"));
            settingsItems.Add(new SettingsItem(SettingsItem.ContentType.Tap, "Подключения"));
            settingsItems.Add(new SettingsItem(SettingsItem.ContentType.RadioButton, "Отключить загрузку изображений"));
            settingsItems.Add(new SettingsItem(SettingsItem.ContentType.RadioButton, "Сразу закачивать полные статьи"));
            settingsItems.Add(new SettingsItem(SettingsItem.ContentType.Buttons, new string[] { "Очистить кэш", "Настройка" }));

            return settingsItems;
        }

        private UITableView _settingsTableView;
    }
}

