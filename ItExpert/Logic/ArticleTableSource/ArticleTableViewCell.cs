using System;
using MonoTouch.UIKit;
using ItExpert.Model;
using ItExpert.Enum;
using MonoTouch.Foundation;
using System.Collections.Generic;
using System.Linq;

namespace ItExpert
{
    public sealed class ArticleTableViewCell: UITableViewCell
    {
        public ArticleTableViewCell(UITableViewCellStyle style, string reuseIdentifier)
            :base (style, reuseIdentifier)
        {
            BackgroundColor = ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetBackgroundColor());

            _creatorsPool = new Dictionary<BaseContentCreator.CreatorType, BaseContentCreator>();
        }

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			InvokeOnMainThread(() =>
			{
				if (_creatorsPool != null && _creatorsPool.Any())
				{
					foreach (var contentCreator in _creatorsPool.Values)
					{
						contentCreator.Dispose();
					}
					_creatorsPool.Clear();
					_creatorsPool = null;
				}
			});
		}

        public void UpdateContent(Article article)
        {
            var creator = CreatorFactory(article);

            creator.UpdateContent(this, article);
        }

        public float GetHeightDependingOnContent(Article article)
        {
            var creator = CreatorFactory(article);

            return creator.GetContentHeight(ContentView, article);
        }

        private BaseContentCreator CreatorFactory(Article article)
        {
            var creatorType = GetCreatorType(article);

            switch (creatorType)
            {
                case BaseContentCreator.CreatorType.Banner:
                    if (!_creatorsPool.ContainsKey(BaseContentCreator.CreatorType.Banner))
                    {
                        _creatorsPool.Add(BaseContentCreator.CreatorType.Banner, new BannerContentCreator());
                    }

                    return _creatorsPool[BaseContentCreator.CreatorType.Banner];

                case BaseContentCreator.CreatorType.LoadMore:
                    if (!_creatorsPool.ContainsKey(BaseContentCreator.CreatorType.LoadMore))
                    {
                        _creatorsPool.Add(BaseContentCreator.CreatorType.LoadMore, new LoadMoreContentCreator());
                    }

                    return _creatorsPool[BaseContentCreator.CreatorType.LoadMore];

                case BaseContentCreator.CreatorType.Portal:
                    if (!_creatorsPool.ContainsKey(BaseContentCreator.CreatorType.Portal))
                    {
                        _creatorsPool.Add(BaseContentCreator.CreatorType.Portal, new PortalContentCreator());
                    }

                    return _creatorsPool[BaseContentCreator.CreatorType.Portal];

                case BaseContentCreator.CreatorType.Header:
                    if (!_creatorsPool.ContainsKey(BaseContentCreator.CreatorType.Header))
                    {
                        _creatorsPool.Add(BaseContentCreator.CreatorType.Header, new HeaderContentCreator());
                    }

                    return _creatorsPool[BaseContentCreator.CreatorType.Header];

                case BaseContentCreator.CreatorType.MagazinePreview:
                    if (!_creatorsPool.ContainsKey(BaseContentCreator.CreatorType.MagazinePreview))
                    {
                        _creatorsPool.Add(BaseContentCreator.CreatorType.MagazinePreview, new MagazinePreviewContentCreator());
                    }

                    return _creatorsPool[BaseContentCreator.CreatorType.MagazinePreview];

                default:
                    throw new NotImplementedException("Content creator type isn't implemented.");
            }
        }

        private BaseContentCreator.CreatorType GetCreatorType(Article article)
        {
            switch (article.ArticleType)
            {
                case ArticleType.Banner:
                    return BaseContentCreator.CreatorType.Banner;

                case ArticleType.PreviousArticlesButton:
                    return BaseContentCreator.CreatorType.LoadMore;               

                case ArticleType.Header:
                    return BaseContentCreator.CreatorType.Header;

                case ArticleType.Magazine:
                case ArticleType.Portal:
                    return BaseContentCreator.CreatorType.Portal;

                case ArticleType.MagazinePreview:
                    return BaseContentCreator.CreatorType.MagazinePreview;

                default:
                    throw new NotImplementedException("Article type isn't implemented.");
            }
        }

        private Dictionary<BaseContentCreator.CreatorType, BaseContentCreator> _creatorsPool;
    }
}

