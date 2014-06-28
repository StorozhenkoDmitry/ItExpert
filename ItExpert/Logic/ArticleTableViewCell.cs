using System;
using MonoTouch.UIKit;
using ItExpert.Model;
using ItExpert.Enum;
using MonoTouch.Foundation;
using System.Collections.Generic;

namespace ItExpert
{
    public sealed class ArticleTableViewCell: UITableViewCell
    {
        public ArticleTableViewCell(UITableViewCellStyle style, string reuseIdentifier)
            :base (style, reuseIdentifier)
        {
            BackgroundColor = ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetBackgroundColor());

            _creatorsPull = new Dictionary<BaseContentCreator.CreatorType, BaseContentCreator>();
        }

        public void UpdateContent(bool isBannerRow, Article article)
        {
            var creator = CreatorFactory(isBannerRow, article);

            creator.UpdateContent(this, article);
        }

        public float GetHeightDependingOnContent(bool isBannerRow, Article article)
        {
            var creator = CreatorFactory(isBannerRow, article);

            return creator.GetContentHeight(ContentView, article);
        }

        private BaseContentCreator CreatorFactory(bool isBannerRow, Article article)
        {
            var creatorType = GetCreatorType(isBannerRow, article);

            switch (creatorType)
            {
                case BaseContentCreator.CreatorType.Banner:
                    if (!_creatorsPull.ContainsKey(BaseContentCreator.CreatorType.Banner))
                    {
                        _creatorsPull.Add(BaseContentCreator.CreatorType.Banner, new BannerContentCreator());
                    }

                    return _creatorsPull[BaseContentCreator.CreatorType.Banner];

                case BaseContentCreator.CreatorType.LoadMore:
                    if (!_creatorsPull.ContainsKey(BaseContentCreator.CreatorType.LoadMore))
                    {
                        _creatorsPull.Add(BaseContentCreator.CreatorType.LoadMore, new LoadMoreContentCreator());
                    }

                    return _creatorsPull[BaseContentCreator.CreatorType.LoadMore];

                case BaseContentCreator.CreatorType.Portal:
                    if (!_creatorsPull.ContainsKey(BaseContentCreator.CreatorType.Portal))
                    {
                        _creatorsPull.Add(BaseContentCreator.CreatorType.Portal, new PortalContentCreator());
                    }

                    return _creatorsPull[BaseContentCreator.CreatorType.Portal];

                case BaseContentCreator.CreatorType.Header:
                    if (!_creatorsPull.ContainsKey(BaseContentCreator.CreatorType.Header))
                    {
                        _creatorsPull.Add(BaseContentCreator.CreatorType.Header, new HeaderContentCreator());
                    }

                    return _creatorsPull[BaseContentCreator.CreatorType.Header];

                default:
                    throw new NotImplementedException("Content creator type isn't implemented.");
            }
        }

        private BaseContentCreator.CreatorType GetCreatorType(bool isBannerRow, Article article)
        {
            switch (article.ArticleType)
            {
                case ArticleType.ExtendedObject:
                    if (isBannerRow)
                    {
                        return BaseContentCreator.CreatorType.Banner;
                    }
                    else
                    {
                        return BaseContentCreator.CreatorType.LoadMore;
                    }

                case ArticleType.Header:
                    return BaseContentCreator.CreatorType.Header;

                case ArticleType.Magazine:
                    return BaseContentCreator.CreatorType.Magazine;

                case ArticleType.Portal:
                    return BaseContentCreator.CreatorType.Portal;

                default:
                    throw new NotImplementedException("Article type isn't implemented.");
            }
        }

        private Dictionary<BaseContentCreator.CreatorType, BaseContentCreator> _creatorsPull;
    }
}

