using System;
using MonoTouch.UIKit;
using System.Collections.Generic;
using System.Drawing;

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

            _settingsTableView.Frame = new RectangleF(0, ItExpertHelper.StatusBarHeight + navigationBarHeight, screenSize.Width, screenSize.Height / 2);
            _settingsTableView.BackgroundColor = UIColor.Black;
            _settingsTableView.ScrollEnabled = true; 
            _settingsTableView.UserInteractionEnabled = true;
            _settingsTableView.SeparatorInset = new UIEdgeInsets (0, 0, 0, 0);
            _settingsTableView.Bounces = true;
            _settingsTableView.SeparatorColor = UIColor.FromRGB(100, 100, 100);

            _settingsTableView.Source = new SettingsTableSource(CreateSettingsItems());

            _tapableView = new UIView(new RectangleF(0, _settingsTableView.Frame.Bottom, View.Frame.Width, View.Frame.Height - _settingsTableView.Frame.Bottom));

            _tapableView.BackgroundColor = UIColor.Clear;
            _tapableView.AddGestureRecognizer(new UITapGestureRecognizer(() =>
            {
                OnTapOutsideTableView();
            }));

            View.Add(_settingsTableView);
            View.Add(_tapableView);

        }

        public event EventHandler TapOutsideTableView;

        private List<SettingsItem> CreateSettingsItems()
        {
            List<SettingsItem> settingsItems = new List<SettingsItem>();

            settingsItems.Add(new SettingsItem(SettingsItem.ContentType.Switch) 
            { 
                Title = "Оффлайн режим", 
                GetValue = () => { return ApplicationWorker.Settings.OfflineMode; },
                SetValue = (value) => { ApplicationWorker.Settings.OfflineMode = (bool)value; }
            });

            settingsItems.Add(new SettingsItem(SettingsItem.ContentType.RadioButton)
            { 
                Title = "Скрывать прочитанные статьи",  
                GetValue = () => { return ApplicationWorker.Settings.HideReaded; },
                SetValue = (value) => { ApplicationWorker.Settings.HideReaded = (bool)value; }
            });

            settingsItems.Add(new SettingsItem(SettingsItem.ContentType.Slider) 
            { 
                Title = "Размер шрифта", 
                GetValue = () => { return 0; },
                SetValue = (value) => { Console.WriteLine ("Размер шрифта - цифра {0}", (int)value); }
            });

            settingsItems.Add(new SettingsItem(SettingsItem.ContentType.Tap) 
            { 
                Title = "Тема", 
                GetValue = () => { return "Светлая"; },
                SetValue = (value) => { Console.WriteLine ("Нажатие на ячейку {0}", (string)value); }
            });

            settingsItems.Add(new SettingsItem(SettingsItem.ContentType.Tap) 
            { 
                Title = "Стартовый раздел", 
                GetValue = () => { return "Новости"; },
                SetValue = (value) => { Console.WriteLine ("Нажатие на ячейку {0}", (string)value); }
            });

            settingsItems.Add(new SettingsItem(SettingsItem.ContentType.Tap) 
            { 
                Title = "Подключения", 
                GetValue = () => { return "Любое"; },
                SetValue = (value) => { Console.WriteLine ("Нажатие на ячейку {0}", (string)value); }
            });

            settingsItems.Add(new SettingsItem(SettingsItem.ContentType.RadioButton) 
            { 
                Title = "Отключить загрузку изображений", 
                GetValue = () => { return ApplicationWorker.Settings.LoadImages; },
                SetValue = (value) => { ApplicationWorker.Settings.LoadImages = (bool)value; }
            });

            settingsItems.Add(new SettingsItem(SettingsItem.ContentType.RadioButton) 
            { 
                Title = "Сразу закачивать полные статьи", 
                GetValue = () => { return ApplicationWorker.Settings.LoadDetails; },
                SetValue = (value) => { ApplicationWorker.Settings.LoadDetails = (bool)value; }
            });

            settingsItems.Add(new SettingsItem(SettingsItem.ContentType.Buttons) 
            { 
                Buttons = new string[] { "Очистить кэш", "Настройка" },
                ButtonPushed = (index) =>
                {
                    Console.WriteLine ("Нажата кнопка с индексом {0}", index);

                    if (index == 0)
                    {
                        // действия для кнопки "Очистить кэш"
                    }
                    else if (index == 1)
                    {
                        // действия для кнопки "Настройка"
                    }
                    else 
                    {
                        throw new ArgumentException("Wrong settings cell button index!", "index");
                    }
                }
            });

            return settingsItems;
        }

        private void OnTapOutsideTableView()
        {
            if (TapOutsideTableView != null)
            {
                TapOutsideTableView(this, EventArgs.Empty);
            }
        }

        private UITableView _settingsTableView;
        private UIView _tapableView;
    }
}

