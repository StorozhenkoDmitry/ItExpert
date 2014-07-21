using System;
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
                UpdateTextViews(cell.ContentView.Frame.Size, item);
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

            _tapGestureRecognizer = new UITapGestureRecognizer(() =>
            {
                Console.WriteLine("Нажатие на настройку {0}", _item.Title);

                UpdateRightTextView(viewSize, _item);
            });

            _tapView.AddGestureRecognizer(_tapGestureRecognizer);
        }

        private void UpdateRightTextView(SizeF viewSize, NavigationBarItem item)
        {
            var attributedString = ItExpertHelper.GetAttributedString(item.GetValue().ToString(), UIFont.SystemFontOfSize(12), UIColor.White);

            _rightTextView.TextStorage.SetString (attributedString);
            _rightTextView.AttributedText = attributedString;

            _rightTextView.TextContainer.Size = new Size ((int)viewSize.Width, int.MaxValue);

            var range = _rightTextView.LayoutManager.GetGlyphRange (_rightTextView.TextContainer);
            var containerSize = _rightTextView.LayoutManager.BoundingRectForGlyphRange (range, _rightTextView.TextContainer);

            _rightTextView.Frame = new RectangleF (
                new PointF(viewSize.Width - _rightTextView.Frame.Width - _padding.Right, viewSize.Height / 2 - _rightTextView.Frame.Height / 2), 
                new SizeF (containerSize.Width, containerSize.Height));
        }

        private void UpdateTextViews(SizeF viewSize, NavigationBarItem item)
        {
            _textView.Dispose();
            _textView = null;

            _rightTextView.Dispose();
            _rightTextView = null;

            _tapGestureRecognizer.Dispose();
            _tapGestureRecognizer = null;

            _tapView.Dispose();
            _tapView = null;

            CreateRightTextView(viewSize, item);
        }

        private UITextView _rightTextView;
        private UIView _tapView;
        private UITapGestureRecognizer _tapGestureRecognizer;
    }
}

