using System;
using MonoTouch.UIKit;
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.Foundation;

namespace ItExpert
{
    public class SettingsView: UIViewController
    {
        enum BackButtonState
        {
            Settings,
            Cache
        }

        public event EventHandler TapOutsideTableView;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _navigationController = (UIApplication.SharedApplication.KeyWindow.RootViewController as UINavigationController);

            View.BackgroundColor = UIColor.Clear;

            AddHeaderView();

            AddTableView();

            AddTapView();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            CorrectViewsFrame();
        }

        public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
        {
            base.DidRotate(fromInterfaceOrientation);

            CorrectViewsFrame();

            if (_settingsTableView != null)
            {
                _settingsTableView.ReloadData();
            }
        }

        private void AddHeaderView()
        {
            _headerView = new UIView(_navigationController.NavigationBar.Frame);

			_headerView.BackgroundColor = UIColor.Black;

            var image = new UIImage(NSData.FromFile("NavigationBar/Back.png"), 2);

            _backButton = new UIButton(new RectangleF(new PointF(10, _headerView.Frame.Height / 2 - image.Size.Height / 2), image.Size));

            _backButton.SetImage(image, UIControlState.Normal);
            _backButton.TouchUpInside += OnBackButtonPushed;

            _backButtonState = BackButtonState.Settings;

            _headerView.Add(_backButton);

            _logoImageView = new UIImageView(new UIImage(NSData.FromFile("NavigationBar/Logo.png"), 2));

            _logoImageView.Frame = new RectangleF(new PointF(_backButton.Frame.Right + 20, _headerView.Frame.Height / 2 - _logoImageView.Frame.Height / 2), 
                _logoImageView.Frame.Size);

            _headerView.Add(_logoImageView);

            View.Add(_headerView);
        }

        private void AddTableView()
        {
            var screenSize = ItExpertHelper.GetRealScreenSize();

            _settingsTableView = new UITableView();

            _settingsTableView.Frame = new RectangleF(0, ItExpertHelper.StatusBarHeight + _navigationController.NavigationBar.Frame.Height, screenSize.Width, 
				Math.Min(screenSize.Height / 2, _maxTableViewHeight));
            _settingsTableView.BackgroundColor = UIColor.Black;
            _settingsTableView.ScrollEnabled = true; 
            _settingsTableView.UserInteractionEnabled = true;
            _settingsTableView.SeparatorInset = new UIEdgeInsets (0, 0, 0, 0);
            _settingsTableView.Bounces = true;
            _settingsTableView.SeparatorColor = UIColor.FromRGB(100, 100, 100);

            _settingsTableView.Source = new NavigationBarTableSource(GetSettingsItems());

            View.Add(_settingsTableView);
        }

        private void AddTapView()
        {
            _tapableView = new UIView(new RectangleF(0, _settingsTableView.Frame.Bottom, View.Frame.Width, View.Frame.Height - _settingsTableView.Frame.Bottom));

            _tapableView.BackgroundColor = UIColor.Clear;
            _tapableView.AddGestureRecognizer(new UITapGestureRecognizer(() =>
            {
                OnTapOutsideTableView();
            }));

            View.Add(_tapableView);
        }

        private void CorrectViewsFrame()
        {
            var screenSize = ItExpertHelper.GetRealScreenSize();

            if (_settingsTableView != null)
            {
                _settingsTableView.Frame = new RectangleF(0, ItExpertHelper.StatusBarHeight + _navigationController.NavigationBar.Frame.Height, 
					screenSize.Width, Math.Min(screenSize.Height / 2, _maxTableViewHeight));

                _settingsTableView.ReloadData();
            }

            if (_tapableView != null && _settingsTableView != null)
            {
                _tapableView.Frame = new RectangleF(0, screenSize.Bottom, screenSize.Width, screenSize.Height - _settingsTableView.Frame.Bottom);
            }

            if (_headerView != null)
            {
                _headerView.Frame = _navigationController.NavigationBar.Frame;

                _backButton.Frame = new RectangleF(new PointF(10, _headerView.Frame.Height / 2 - _backButton.Frame.Height / 2), _backButton.Frame.Size);

                _logoImageView.Frame = new RectangleF(new PointF(_backButton.Frame.Right + 20, _headerView.Frame.Height / 2 - _logoImageView.Frame.Height / 2), 
                    _logoImageView.Frame.Size);
            }

			if (_cacheView != null)
			{
				float cacheY = ItExpertHelper.StatusBarHeight + _navigationController.NavigationBar.Frame.Height;

				_cacheView.Frame = new RectangleF(new PointF(0, cacheY), new SizeF(screenSize.Width, screenSize.Height - cacheY));

				_cacheView.CorrectSubviewsFrames();
			}
        }

        private List<NavigationBarItem> GetSettingsItems()
        {
            List<NavigationBarItem> settingsItems = new List<NavigationBarItem>();

            settingsItems.Add(new NavigationBarItem(NavigationBarItem.ContentType.Switch) 
            { 
                Title = "Оффлайн режим", 
                GetValue = () => { return ApplicationWorker.Settings.OfflineMode; },
                SetValue = (value) => { ApplicationWorker.Settings.OfflineMode = (bool)value; }
            });

            settingsItems.Add(new NavigationBarItem(NavigationBarItem.ContentType.RadioButton)
            { 
                Title = "Скрывать прочитанные статьи",  
                GetValue = () => { return ApplicationWorker.Settings.HideReaded; },
                SetValue = (value) => { ApplicationWorker.Settings.HideReaded = (bool)value; }
            });

            settingsItems.Add(new NavigationBarItem(NavigationBarItem.ContentType.Slider) 
            { 
                Title = "Размер шрифта", 
                GetValue = () => { return _testSliderValue; },
                SetValue = (value) => { _testSliderValue = (int)value; }
            });

            settingsItems.Add(new NavigationBarItem(NavigationBarItem.ContentType.Tap) 
            { 
                Title = "Тема", 
                GetValue = () => { return "Светлая"; },
                SetValue = (value) => { Console.WriteLine ("Нажатие на ячейку {0}", (string)value); }
            });

            settingsItems.Add(new NavigationBarItem(NavigationBarItem.ContentType.Tap) 
            { 
                Title = "Стартовый раздел", 
                GetValue = () => { return "Новости"; },
                SetValue = (value) => { Console.WriteLine ("Нажатие на ячейку {0}", (string)value); }
            });

            settingsItems.Add(new NavigationBarItem(NavigationBarItem.ContentType.Tap) 
            { 
                Title = "Подключения", 
                GetValue = () => { return "Любое"; },
                SetValue = (value) => { Console.WriteLine ("Нажатие на ячейку {0}", (string)value); }
            });

            settingsItems.Add(new NavigationBarItem(NavigationBarItem.ContentType.RadioButton) 
            { 
                Title = "Отключить загрузку изображений", 
                GetValue = () => { return ApplicationWorker.Settings.LoadImages; },
                SetValue = (value) => { ApplicationWorker.Settings.LoadImages = (bool)value; }
            });

            settingsItems.Add(new NavigationBarItem(NavigationBarItem.ContentType.RadioButton) 
            { 
                Title = "Сразу закачивать полные статьи", 
                GetValue = () => { return ApplicationWorker.Settings.LoadDetails; },
                SetValue = (value) => { ApplicationWorker.Settings.LoadDetails = (bool)value; }
            });

            settingsItems.Add(new NavigationBarItem(NavigationBarItem.ContentType.Buttons) 
            { 
                Buttons = new string[] { "Очистить кэш", "Настройка" },
                ButtonPushed = (index) =>
                {
                    if (index == 0)
                    {

                    }
                    else if (index == 1)
                    {
                        if (_cacheView != null)
                        {
                            _cacheView.Dispose();
                            _cacheView = null;
                        }

						float cacheY = ItExpertHelper.StatusBarHeight + _navigationController.NavigationBar.Frame.Height;

						SizeF screenSize = ItExpertHelper.GetRealScreenSize().Size;

						_cacheView = new CacheView(new RectangleF(new PointF(0, cacheY), new SizeF(screenSize.Width, screenSize.Height - cacheY)));

                        _backButtonState = BackButtonState.Cache;

                        Add(_cacheView);
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

        private void OnBackButtonPushed(object sender, EventArgs e)
        {
            if (_backButtonState == BackButtonState.Settings)
            {
                OnTapOutsideTableView();
            }
            else if (_backButtonState == BackButtonState.Cache)
            {
                InvokeOnMainThread(() =>
                {
                    _cacheView.RemoveFromSuperview();

                    _cacheView.Dispose();
                    _cacheView = null;

                    _backButtonState = BackButtonState.Settings;
                });
            }
        }

		private float _maxTableViewHeight = 396;
        private UITableView _settingsTableView;
        private UIView _tapableView;
        private int _testSliderValue = 2;
        private UINavigationController _navigationController;
        private UIView _headerView;
        private UIButton _backButton;
        private UIImageView _logoImageView;
        private CacheView _cacheView;
        private BackButtonState _backButtonState;
    }
}

