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

        protected override void Create(UITableViewCell cell, Article article)
        {
            cell.SelectionStyle = UITableViewCellSelectionStyle.None;
            cell.ContentView.Add(article.ExtendedObject);
        }

        protected override void Update(UITableViewCell cell, Article article)
        {
            cell.SelectionStyle = UITableViewCellSelectionStyle.None;
            cell.ContentView.Add(article.ExtendedObject);
        }
    }
}

