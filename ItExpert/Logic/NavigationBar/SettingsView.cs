using System;
using MonoTouch.UIKit;
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.Foundation;
using ItExpert.Enum;
using System.Linq;
using BigTed;
using System.Threading;

namespace ItExpert
{
    public class SettingsView: UIViewController
    {
		private bool _forDetail = false;
		private bool _settingsChanged = false;

        enum BackButtonState
        {
            Settings,
            Cache
        }

        public event EventHandler TapOutsideTableView;

		public SettingsView(bool forDetail)
		{
			_forDetail = forDetail;
			if (_forDetail)
			{
				_testSliderValue = ApplicationWorker.Settings.GetDetailFontSize();
			}
			else
			{
				_testSliderValue = ApplicationWorker.Settings.GetFontSize();
			}
		}

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
            _settingsTableView.Bounces = false;
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
				_tapableView.Frame = new RectangleF(0, _settingsTableView.Frame.Bottom, screenSize.Width, screenSize.Height - _settingsTableView.Frame.Bottom);
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
                SetValue = (value) => { ApplicationWorker.Settings.HideReaded = (bool)value; 
					_settingsChanged = true;
				}
            });

            settingsItems.Add(new NavigationBarItem(NavigationBarItem.ContentType.Slider) 
            { 
                Title = "Размер шрифта", 
                GetValue = () => { return _testSliderValue; },
                SetValue = (value) => {
					var size = (int)value;
					_testSliderValue = size;
					if (!_forDetail)
					{
						ApplicationWorker.Settings.SetFontSize(size);
					}
					else
					{
						ApplicationWorker.Settings.SetDetailFontSize(size);
					}
					ApplicationWorker.OnSettingsChanged();
				}
            });

            settingsItems.Add(new NavigationBarItem(NavigationBarItem.ContentType.Tap) 
            { 
                Title = "Тема", 
                GetValue = () => { 
					return ApplicationWorker.Settings.GetStringTheme(ApplicationWorker.Settings.GetTheme()); 
				},
                SetValue = (value) => { 
					var themes = new []{ "Светлая", "Темная" };
					var themeAlert = new BlackAlertView ("Выбор темы", "Отмена", themes, "Ok");

					themeAlert.ButtonPushed += (sender, e) =>
					{
						if (e.ButtonIndex == 1)
						{
							var theme = new [] {Theme.Light, Theme.Dark } [e.SelectedRadioButton];
							ApplicationWorker.Settings.SetTheme(theme);
							ApplicationWorker.OnSettingsChanged();
							_settingsTableView.ReloadData();
						}
						themeAlert.Dispose();
					};
					var currentTheme = ApplicationWorker.Settings.GetTheme();
					if (currentTheme == ItExpert.Enum.Theme.Light)
					{
						themeAlert.SetRadionButtonActive(0);
					}
					if (currentTheme == ItExpert.Enum.Theme.Dark)
					{
						themeAlert.SetRadionButtonActive(1);
					}
					themeAlert.Show();
				}
            });

            settingsItems.Add(new NavigationBarItem(NavigationBarItem.ContentType.Tap) 
            { 
                Title = "Стартовый раздел", 
				GetValue = () => { return ApplicationWorker.Settings.GetStringStartSection(ApplicationWorker.Settings.Page); },
                SetValue = (value) => { 
					var pagesStr = new []{ "Новости", "Тренды", "Журнал", "Архив", "Избранное" };
					var pageAlert = new BlackAlertView ("Выбор стартового раздела", "Отмена", pagesStr, "Ok");

					pageAlert.ButtonPushed += (sender, e) =>
					{
						if (e.ButtonIndex == 1)
						{
							var page = new [] {Page.News, Page.Trends, Page.Magazine, Page.Archive, Page.Favorite } [e.SelectedRadioButton];
							ApplicationWorker.Settings.Page = page;
							_settingsTableView.ReloadData();
						}
						pageAlert.Dispose();
					};
					var currentPage = ApplicationWorker.Settings.Page;
					var pages = new [] {Page.News, Page.Trends, Page.Magazine, Page.Archive, Page.Favorite } ;
					var index = pages.ToList().BinarySearch(currentPage);
					pageAlert.SetRadionButtonActive(index);
					pageAlert.Show(); 
				}
            });

            settingsItems.Add(new NavigationBarItem(NavigationBarItem.ContentType.Tap) 
            { 
                Title = "Подключения", 
				GetValue = () => { return ApplicationWorker.Settings.GetStringNetworkMode(ApplicationWorker.Settings.NetworkMode); },
                SetValue = (value) => { 
					var networkModesStr = new []{ "Wi-Fi", "Любое" };
					var networkModeAlert = new BlackAlertView ("Выбор типа подключения", "Отмена", networkModesStr, "Ok");

					networkModeAlert.ButtonPushed += (sender, e) =>
					{
						if (e.ButtonIndex == 1)
						{
							var networkMode = new [] {NetworkMode.WiFi, NetworkMode.All } [e.SelectedRadioButton];
							ApplicationWorker.Settings.NetworkMode = networkMode;
							_settingsTableView.ReloadData();
						}
						networkModeAlert.Dispose();
					};
					var currentNetworkMode = ApplicationWorker.Settings.NetworkMode;
					var networkModes = new []{ NetworkMode.WiFi, NetworkMode.All };
					var index = networkModes.ToList().BinarySearch(currentNetworkMode);
					networkModeAlert.SetRadionButtonActive(index);
					networkModeAlert.Show(); 
				}
            });

            settingsItems.Add(new NavigationBarItem(NavigationBarItem.ContentType.RadioButton) 
            { 
                Title = "Отключить загрузку изображений", 
				GetValue = () => { return !ApplicationWorker.Settings.LoadImages; },
                SetValue = (value) => {
					ApplicationWorker.Settings.LoadImages = !((bool)value); 
					_settingsChanged = true;
				}
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
						ClearCache();
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

		void ClearCache()
		{
			BTProgressHUD.ShowToast("Очистка кэша...", ProgressHUD.MaskType.None, false, 2500);
			Action clearCache = () =>
			{
				var result = ApplicationWorker.Db.ClearCache();
				var message = string.Empty;
				if (result.IsCacheClear)
				{
					message = "Кэш очищен";
				}
				if (result.IsCacheClear && result.IsFavoriteDelete)
				{
					message += ", были также удалены некоторые избранные статьи";
				}
				InvokeOnMainThread(() =>
				{
					BTProgressHUD.ShowToast(message, ProgressHUD.MaskType.None, false, 2500);
				});
			};
			ThreadPool.QueueUserWorkItem(state => clearCache());
		}

        private void OnTapOutsideTableView()
        {
			if (_settingsChanged)
			{
				ApplicationWorker.OnSettingsChanged();
				_settingsChanged = false;
			}
			ApplicationWorker.Settings.SaveSettings();
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
		private int _testSliderValue = 0;
        private UINavigationController _navigationController;
        private UIView _headerView;
        private UIButton _backButton;
        private UIImageView _logoImageView;
        private CacheView _cacheView;
        private BackButtonState _backButtonState;
    }
}

