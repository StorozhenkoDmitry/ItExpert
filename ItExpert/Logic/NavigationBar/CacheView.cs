﻿using System;
using MonoTouch.UIKit;
using System.Drawing;
using System.Collections.Generic;

namespace ItExpert
{
    public class CacheView: UIView
    {
        public CacheView(RectangleF frame)
        {
            Frame = frame;

            BackgroundColor = UIColor.Black;

            _tableView = new UITableView();

            _tableView.Frame = new RectangleF(0, 0, Frame.Width, 172);
            _tableView.BackgroundColor = UIColor.Black;
            _tableView.ScrollEnabled = false; 
            _tableView.UserInteractionEnabled = true;
            _tableView.SeparatorInset = new UIEdgeInsets (0, 0, 0, 0);
            _tableView.Bounces = false;
            _tableView.SeparatorColor = UIColor.FromRGB(100, 100, 100);

            _tableView.Source = new NavigationBarTableSource(GetCahceSettingsItems());

            Add(_tableView);

            float offsetBetweenButtons = 10;

            _clearCacheButton = GetButton("Очистить кэш", new PointF(0, _tableView.Frame.Bottom + offsetBetweenButtons));
            _deletePdfButton = GetButton("Удалить все Pdf", new PointF(0, _clearCacheButton.Frame.Bottom + offsetBetweenButtons));
            _deleteAllFavoritesButton = GetButton("Удалить все избранные", new PointF(0, _deletePdfButton.Frame.Bottom + offsetBetweenButtons));

            _clearCacheButton.TouchUpInside += OnClearCacheButtonPushed;
            _deletePdfButton.TouchUpInside += OnDeletePdfButtonPushed;
            _deleteAllFavoritesButton.TouchUpInside += OnDeleteAllFavoritesButtonPushed;

            Add(_clearCacheButton);
            Add(_deletePdfButton);
            Add(_deleteAllFavoritesButton);
        }

        private List<NavigationBarItem> GetCahceSettingsItems()
        {
            List<NavigationBarItem> cacheSettingsItems = new List<NavigationBarItem>();

            cacheSettingsItems.Add(new NavigationBarItem(NavigationBarItem.ContentType.Tap) 
            { 
                Title = "Объем данных", 
                GetValue = () => { return "1,45 Мб"; }
            });

            cacheSettingsItems.Add(new NavigationBarItem(NavigationBarItem.ContentType.Tap)
            { 
                Title = "Объем всех Pdf",  
                GetValue = () => { return "20,15 Мб"; }
            });

            cacheSettingsItems.Add(new NavigationBarItem(NavigationBarItem.ContentType.CacheSlider) 
            { 
                Title = "Предел размера данных", 
                GetValue = () => { return 2; },
                SetValue = (value) => { Console.WriteLine ("Slider value: {0}", value); }
            });

            return cacheSettingsItems;
        }

        private UIButton GetButton(string title, PointF location)
        {
            UIButton button = new UIButton();

            button.SetTitle(title, UIControlState.Normal);
            button.SetTitleColor(ItExpertHelper.ButtonTextColor, UIControlState.Normal);
            button.SizeToFit();
            button.BackgroundColor = ItExpertHelper.ButtonColor;
            button.TitleEdgeInsets = new UIEdgeInsets(0, 4, 0, 4);

            SizeF buttonSize = new SizeF(button.Frame.Width + button.TitleEdgeInsets.Left + button.TitleEdgeInsets.Right, 
                                   button.Frame.Height + button.TitleEdgeInsets.Top + button.TitleEdgeInsets.Bottom);

            button.Frame = new RectangleF(new PointF(Frame.Width / 2 - buttonSize.Width / 2, location.Y), buttonSize);

            button.TouchDown += (sender, e) => 
            {
                var senderButton = (sender as UIButton);

                senderButton.BackgroundColor = ItExpertHelper.ButtonPushedColor;
            };

            button.TouchUpOutside += (sender, e) => 
            {
                var senderButton = (sender as UIButton);

                senderButton.BackgroundColor = ItExpertHelper.ButtonColor;
            };

            return button;
        }

        private void OnClearCacheButtonPushed(object sender, EventArgs e)
        {
            var senderButton = (sender as UIButton);

            senderButton.BackgroundColor = ItExpertHelper.ButtonColor;
        }

        private void OnDeletePdfButtonPushed(object sender, EventArgs e)
        {
            var senderButton = (sender as UIButton);

            senderButton.BackgroundColor = ItExpertHelper.ButtonColor;
        }

        private void OnDeleteAllFavoritesButtonPushed(object sender, EventArgs e)
        {
            var senderButton = (sender as UIButton);

            senderButton.BackgroundColor = ItExpertHelper.ButtonColor;
        }

        private UITableView _tableView;
        private UIButton _clearCacheButton;
        private UIButton _deletePdfButton;
        private UIButton _deleteAllFavoritesButton;
    }
}

