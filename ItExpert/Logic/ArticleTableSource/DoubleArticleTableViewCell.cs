using System;
using MonoTouch.UIKit;
using System.Collections.Generic;
using ItExpert.Model;
using ItExpert.Enum;

namespace ItExpert
{
    public class DoubleArticleTableViewCell : UITableViewCell
    {
        public DoubleArticleTableViewCell(UITableViewCellStyle style, string reuseIdentifier)
            :base (style, reuseIdentifier)
        {
            BackgroundColor = ItExpertHelper.GetUIColorFromColor(ApplicationWorker.Settings.GetBackgroundColor());

            _creatorsPool = new Dictionary<BaseContentCreator.CreatorType, BaseContentCreator>();

			delegates = new List<EventHandler<DoubleCellPushedEventArgs>>();
        }

        public event EventHandler<DoubleCellPushedEventArgs> CellPushed
		{
			add
			{
				CellPushedReal += value;
				delegates.Add(value);
			}
			remove
			{
				CellPushedReal -= value;
				delegates.Remove(value);
			}
		}

		private event EventHandler<DoubleCellPushedEventArgs> CellPushedReal;

		public void RemoveAllEvents()
		{
			foreach(EventHandler<DoubleCellPushedEventArgs> eh in delegates)
			{
				CellPushedReal -= eh;
			}
			delegates.Clear();
		}

        public void UpdateContent(DoubleArticle article)
        {
            _isDoubleContent = false;

            var creator = CreatorFactory(article);

            if (_isDoubleContent)
            {
                creator.UpdateDoubleContent(this, article);
            }
            else
            {
                creator.UpdateContent(this, article.LeftContent);
            }
        }

        public float GetHeightDependingOnContent(DoubleArticle article)
        {
            _isDoubleContent = false;

            var creator = CreatorFactory(article);

            if (_isDoubleContent)
            {
                return creator.GetDoubleContentHeight(ContentView, article);
            }
            else
            {
                return creator.GetContentHeight(ContentView, article.LeftContent);
            }
        }

        private BaseContentCreator CreatorFactory(DoubleArticle article)
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
                        var contentCreator = new DoublePortalContentCreator();

                        contentCreator.CellPushed += (sender, e) => 
                        {
							CellPushedReal (sender, e);
                        };

                        _creatorsPool.Add(BaseContentCreator.CreatorType.Portal, contentCreator);
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

        private BaseContentCreator.CreatorType GetCreatorType(DoubleArticle article)
        {
            switch (article.LeftContent.ArticleType)
            {
                case ArticleType.Banner:
                    return BaseContentCreator.CreatorType.Banner;

                case ArticleType.PreviousArticlesButton:
                    return BaseContentCreator.CreatorType.LoadMore;               

                case ArticleType.Header:
                    return BaseContentCreator.CreatorType.Header;

                case ArticleType.Magazine:
                case ArticleType.Portal:
                    _isDoubleContent = true;
                    return BaseContentCreator.CreatorType.Portal;

                case ArticleType.MagazinePreview:
                    return BaseContentCreator.CreatorType.MagazinePreview;

                default:
                    throw new NotImplementedException("Article type isn't implemented.");
            }
        }

        private bool _isDoubleContent;

        private Dictionary<BaseContentCreator.CreatorType, BaseContentCreator> _creatorsPool;

		private List<EventHandler<DoubleCellPushedEventArgs>> delegates;
    }
}

