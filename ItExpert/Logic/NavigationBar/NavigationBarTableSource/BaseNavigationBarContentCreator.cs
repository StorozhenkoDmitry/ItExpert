using System;
using MonoTouch.UIKit;
using System.Drawing;
using System.Linq;

namespace ItExpert
{
    public abstract class BaseNavigationBarContentCreator : IDisposable
    {
        public BaseNavigationBarContentCreator()
        {
            _needToCreateContent = true;

            _textFont = UIFont.BoldSystemFontOfSize(14);
            _forecolor = UIColor.White;

            _padding = new UIEdgeInsets (7, 4, 7, 4);

            _height = 44;
        }

		public virtual void Dispose()
		{
			if (_textView != null)
			{
				_textView.Dispose();
			}
			_textView = null;
			_item = null;
		}

        public void UpdateContent(UITableViewCell cell, NavigationBarItem item)
        {
			UIView firstSubview = null;
			if (cell.ContentView.Subviews.Any())
			{
				firstSubview = cell.ContentView.Subviews[0];
			}
            ItExpertHelper.RemoveSubviews(cell.ContentView);
			var cleanup = firstSubview as ICleanupObject;
			if (cleanup != null)
			{
				cleanup.CleanUp();
			}
            _item = item;

            cell.UserInteractionEnabled = true;
            cell.BackgroundColor = UIColor.Black;

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

        public abstract float GetContentHeight(UIView cellContentView, NavigationBarItem item);

        protected void CreateTextView(SizeF viewSize, NavigationBarItem item)
        {
            _textView = ItExpertHelper.GetTextView(ItExpertHelper.GetAttributedString(item.Title, _textFont, _forecolor), viewSize.Width,
                new PointF(0,0));

            _textView.Frame = new RectangleF(new PointF(_padding.Left, _height / 2 - _textView.Frame.Height / 2), _textView.Frame.Size);
            _textView.BackgroundColor = UIColor.Clear;
        }
        
        protected bool _needToCreateContent;
        
        protected UIEdgeInsets _padding;
        protected UIFont _textFont;
        protected UIColor _forecolor;
        protected UITextView _textView;
        protected NavigationBarItem _item;
        protected float _height;

    }
}

