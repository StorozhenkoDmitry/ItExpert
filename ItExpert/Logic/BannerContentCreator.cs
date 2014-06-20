using System;
using ItExpert.Model;
using MonoTouch.UIKit;

namespace ItExpert
{
    public class BannerContentCreator : BaseContentCreator
    {
        public override float GetContentHeight(UIView cellContentView, Article article)
        {
            return (article.ExtendedObject as UIImageView).Frame.Height;
        }

        protected override void Create(UIView cellContentView, Article article)
        {
            (article.ExtendedObject as UIImageView).StartAnimating();
            cellContentView.Add(article.ExtendedObject);
        }

        protected override void Update(UIView cellContentView, Article article)
        {
            (article.ExtendedObject as UIImageView).StartAnimating();
            cellContentView.Add(article.ExtendedObject);
        }
    }
}

