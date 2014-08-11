using System;
using ItExpert.Model;
using MonoTouch.UIKit;
using System.Linq;

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


            _needToCreateContent = true;
        }

        public void UpdateContent(UITableViewCell cell, Article article)
        {
            ItExpertHelper.RemoveSubviews(cell.ContentView);
			cell.GestureRecognizers = new UIGestureRecognizer[0];

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
            ItExpertHelper.RemoveSubviews(cell.ContentView);
			cell.GestureRecognizers = new UIGestureRecognizer[0];

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

        protected bool _needToCreateContent;

        protected Article _article;
        protected UIEdgeInsets _padding;
    }
}

