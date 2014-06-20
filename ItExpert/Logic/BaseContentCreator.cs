using System;
using ItExpert.Model;
using MonoTouch.UIKit;

namespace ItExpert
{
    public abstract class BaseContentCreator
    {
        public enum CreatorType
        {
            Portal,
            Magazine,
            Header,
            Banner,
            LoadMore
        }

        public BaseContentCreator()
        {
            _padding = new UIEdgeInsets (8, 12, 8, 12);
            _previewTextFont = UIFont.SystemFontOfSize(ApplicationWorker.Settings.TextSize);
            _previewHeaderFont = UIFont.BoldSystemFontOfSize(ApplicationWorker.Settings.HeaderSize);
            _forecolor = ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetForeColor());
            _needToCreateContent = true;
        }

        public void UpdateContent(UIView cellContentView, Article article)
        {
            RemoveSubviews(cellContentView);

            if (_needToCreateContent)
            {
                Create(cellContentView, article);

                _needToCreateContent = false;
            }
            else
            {
                Update(cellContentView, article);
            }
        }

        public abstract float GetContentHeight(UIView cellContentView, Article article);

        protected abstract void Create(UIView cellContentView, Article article);

        protected abstract void Update(UIView cellContentView, Article article);

        protected void RemoveSubviews(UIView view)
        {
            foreach (var subView in view.Subviews)
            {
                subView.RemoveFromSuperview();
            }
        }

        protected bool _needToCreateContent;

        protected Article _article;
        protected UIEdgeInsets _padding;
        protected UIFont _previewHeaderFont;
        protected UIFont _previewTextFont;
        protected UIColor _forecolor;
    }
}

