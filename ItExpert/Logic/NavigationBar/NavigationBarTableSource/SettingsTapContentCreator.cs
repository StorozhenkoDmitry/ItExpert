﻿using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace ItExpert
{
    public class SettingsTapContentCreator : BaseNavigationBarContentCreator
    {
        public override float GetContentHeight(UIView cellContentView, NavigationBarItem item)
        {
            return _height;
        }

        protected override void Create(UITableViewCell cell, NavigationBarItem item)
        {
            CreateRightTextView(cell.ContentView.Frame.Size, item);

            cell.ContentView.Add(_textView);
            cell.ContentView.Add(_rightTextView);
            cell.ContentView.Add(_tapView);
        }

        protected override void Update(UITableViewCell cell, NavigationBarItem item)
        {
            if (_textView == null)
            {
                CreateRightTextView(cell.ContentView.Frame.Size, item);
            }
            else
            {
                UpdateRightTextView(cell.ContentView.Frame.Size, item);
            }

            cell.ContentView.Add(_textView);
            cell.ContentView.Add(_rightTextView);
            cell.ContentView.Add(_tapView);
        }

        private void CreateRightTextView(SizeF viewSize, NavigationBarItem item)
        {
            CreateTextView(viewSize, item);

            _rightTextView = ItExpertHelper.GetTextView(ItExpertHelper.GetAttributedString(item.GetValue().ToString(), UIFont.SystemFontOfSize(12), UIColor.White),
                viewSize.Width, new PointF());

            _rightTextView.Frame = new RectangleF(new PointF(viewSize.Width - _rightTextView.Frame.Width - _padding.Right, viewSize.Height / 2 - _rightTextView.Frame.Height / 2), 
                _rightTextView.Frame.Size);
            _rightTextView.BackgroundColor = UIColor.Clear;

            _tapView = new UIView(new RectangleF(new PointF(0, 0), viewSize));

            _tapView.BackgroundColor = UIColor.Clear;

            _tapView.AddGestureRecognizer(new UITapGestureRecognizer(() =>
            {
                Console.WriteLine ("Нажатие на настройку {0}", _item.Title);

                UpdateRightTextView(viewSize, _item);
            }));
        }

        private void UpdateRightTextView(SizeF viewSize, NavigationBarItem item)
        {
            _rightTextView.AttributedText = ItExpertHelper.GetAttributedString(item.GetValue().ToString(), UIFont.SystemFontOfSize(12), UIColor.White);

            _rightTextView.Frame = new RectangleF(new PointF(viewSize.Width - _rightTextView.Frame.Width - _padding.Right, viewSize.Height / 2 - _rightTextView.Frame.Height / 2), 
                _rightTextView.Frame.Size);
        }

        private UITextView _rightTextView;
        private UIView _tapView;
    }
}

