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
            LoadMore,
            Placeholder,
            MagazinePreview
        }

        public BaseContentCreator()
        {
            _padding = new UIEdgeInsets (8, 12, 8, 12);
            _previewTextFont = UIFont.SystemFontOfSize(ApplicationWorker.Settings.TextSize);
            _previewHeaderFont = UIFont.BoldSystemFontOfSize(ApplicationWorker.Settings.HeaderSize);
            _forecolor = ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetForeColor());
            _needToCreateContent = true;
        }

        public void UpdateContent(UITableViewCell cell, Article article)
        {
            RemoveSubviews(cell.ContentView);

            cell.UserInteractionEnabled = true;

            if (_needToCreateContent)
            {
                Create(cell, article);

                _needToCreateContent = false;
            }
            else
            {
                Update(cell, article);
            }
        }

        public void UpdateDoubleContent(UITableViewCell cell, DoubleArticle article)
        {
            RemoveSubviews(cell.ContentView);

            cell.UserInteractionEnabled = true;

            if (_needToCreateContent)
            {
                CreateDouble(cell, article);

                _needToCreateContent = false;
            }
            else
            {
                UpdateDouble(cell, article);
            }
        }

        public abstract float GetContentHeight(UIView cellContentView, Article article);

        public virtual float GetDoubleContentHeight(UIView cellContentView, DoubleArticle article) 
        {
            return 0;
        }

        protected abstract void Create(UITableViewCell cell, Article article);

        protected virtual void CreateDouble(UITableViewCell cell, DoubleArticle article) {}

        protected abstract void Update(UITableViewCell cell, Article article);

        protected virtual void UpdateDouble(UITableViewCell cell, DoubleArticle article) {}

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

