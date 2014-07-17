﻿using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace ItExpert
{
    public abstract class BaseNavigationBarContentCreator
    {
        public BaseNavigationBarContentCreator()
        {
            _needToCreateContent = true;

            _textFont = UIFont.BoldSystemFontOfSize(ApplicationWorker.Settings.TextSize);
            _forecolor = UIColor.White;

            _padding = new UIEdgeInsets (7, 4, 7, 4);
        }

        public void UpdateContent(UITableViewCell cell, NavigationBarItem item)
        {
            ItExpertHelper.RemoveSubviews(cell.ContentView);

            _item = item;

            cell.UserInteractionEnabled = true;
            cell.BackgroundColor = UIColor.Black;

            if (item.Type != NavigationBarItem.ContentType.Buttons)
            {
                _textView = ItExpertHelper.GetTextView(ItExpertHelper.GetAttributedString(item.Title, _textFont, _forecolor), cell.ContentView.Frame.Width,
                        new PointF(0,0));

                _textView.Frame = new RectangleF(new PointF(_padding.Left, cell.ContentView.Frame.Height / 2 - _textView.Frame.Height / 2), _textView.Frame.Size);
                _textView.BackgroundColor = UIColor.Clear;

                cell.ContentView.Add(_textView);
            }

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

        protected abstract void Create(UITableViewCell cell, NavigationBarItem item);
        
        protected abstract void Update(UITableViewCell cell, NavigationBarItem item);
        
        protected bool _needToCreateContent;
        
        protected UIEdgeInsets _padding;
        protected UIFont _textFont;
        protected UIColor _forecolor;
        protected UITextView _textView;
        protected NavigationBarItem _item;
    }
}

