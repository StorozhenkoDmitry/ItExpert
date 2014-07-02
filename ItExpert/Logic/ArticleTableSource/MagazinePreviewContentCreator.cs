using System;
using MonoTouch.UIKit;
using ItExpert.Model;

namespace ItExpert
{
    public class MagazinePreviewContentCreator : BaseContentCreator
    {
        public override float GetContentHeight(UIView cellContentView, Article article)
        {
            return 0;
        }

        protected override void Create(UITableViewCell cell, Article article)
        {

        }

        protected override void Update(UITableViewCell cell, Article article)
        {

        }
    }
}

