using System;
using MonoTouch.UIKit;
using System.Collections.Generic;
using ItExpert.Model;
using ItExpert.Enum;
using System.Linq;
using MonoTouch.Foundation;
using System.Threading;

namespace ItExpert
{
	public class DoubleArticleTableSource: UITableViewSource
	{
		private static DoubleArticleTableSource Instance;

		public DoubleArticleTableSource (List<Article> articles, bool fromFavorite, MagazineAction magazineAction)
		{
            _cellIdentifier = "ArticleCell";
			_fromFavorite = fromFavorite;
			_magazineAction = magazineAction;
            _doubleArticles = new List<DoubleArticle>();
            _articles = articles;
			Instance = this;

            ItExpertHelper.LargestImageSizeInArticlesPreview = 0;
            if (articles.Any()) 
            {
                var articlesWithPicture = articles.Where (x => x.PreviewPicture != null);
                if (articlesWithPicture.Any())
                {
                    ItExpertHelper.LargestImageSizeInArticlesPreview = articlesWithPicture.Max (x => x.PreviewPicture.Width);
                }
            }
			
            bool addNextToRight = false;

            foreach (var article in articles)
            {
                if (addNextToRight && (article.ArticleType == ArticleType.Portal || article.ArticleType == ArticleType.Magazine))
                {
                    _doubleArticles.Last().RightContent = article;

                    addNextToRight = false;

                    continue;
                }

                addNextToRight = false;

                if (article.ArticleType == ArticleType.Portal || article.ArticleType == ArticleType.Magazine)
                {
                    addNextToRight = true;
                }

                _doubleArticles.Add(new DoubleArticle() { LeftContent = article });
            }
		}

		public event EventHandler<PushDetailsEventArgs> PushDetailsView;

		public override int RowsInSection (UITableView tableview, int section)
		{
			return _doubleArticles.Count;
		}

		public override void RowSelected (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
		{
			var article = _articles [indexPath.Row];
			tableView.DeselectRow (indexPath, false);
			if (article.ArticleType == ArticleType.Banner)
			{
				var obj = article.ExtendedObject;
				Banner banner = null;
				var bannerImg = obj as BannerImageView;
				if (bannerImg != null)
				{
					banner = bannerImg.Banner;
				}
				var bannerGif = obj as BannerGifView;
				if (bannerGif != null)
				{
					banner = bannerGif.Banner;
				}
				if (banner != null)
				{
					UIApplication.SharedApplication.OpenUrl (new NSUrl (banner.Url));
				}
				return;
			}
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
            DoubleArticleTableViewCell cell = tableView.DequeueReusableCell (_cellIdentifier) as DoubleArticleTableViewCell;

			if (cell != null)
			{
				cell.RemoveAllEvents();

				cell.Frame = new System.Drawing.RectangleF(0, 0, tableView.Frame.Width, cell.Frame.Height);
				cell.ContentView.Frame = new System.Drawing.RectangleF(0, 0, tableView.Frame.Width, cell.Frame.Height);

				cell.CellPushed += OnCellPushed;
			}
            else
            {
                cell = CreateCell(tableView);
				cell.CellPushed += OnCellPushed;
            }

            cell.ContentView.Bounds = cell.Bounds;
            cell.UpdateContent(_doubleArticles[indexPath.Row]);

            return cell;
		}

		public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
            var cell = CreateCell(tableView);

            return cell.GetHeightDependingOnContent(_doubleArticles[indexPath.Row]);
		}

		private void OnCellPushed(object sender, DoubleCellPushedEventArgs e)
		{
			var articleDetailsView = OpenArticle(e.Article);
			if (articleDetailsView != null)
			{
				if (Instance != null)
				{
					var handler = Interlocked.CompareExchange(ref Instance.PushDetailsView, null, null);
					if (handler != null)
					{
						handler(this, new PushDetailsEventArgs (articleDetailsView));
					}
				}
			}

		}

		public void SetMagazineAction(MagazineAction magazineAction)
		{
			_magazineAction = magazineAction;
		}

		public int GetSelectItemId()
		{
			return _selectItemId;
		}

		public void ResetSelectItem()
		{
			_selectItemId = -1;
		}

		public void UpdateSource()
		{
			_doubleArticles.Clear();
			bool addNextToRight = false;
			var articles = _articles;
			foreach (var article in articles)
			{
				if (addNextToRight && (article.ArticleType == ArticleType.Portal || article.ArticleType == ArticleType.Magazine))
				{
					_doubleArticles.Last().RightContent = article;

					addNextToRight = false;

					continue;
				}

				addNextToRight = false;

				if (article.ArticleType == ArticleType.Portal || article.ArticleType == ArticleType.Magazine)
				{
					addNextToRight = true;
				}

				_doubleArticles.Add(new DoubleArticle() { LeftContent = article });
			}
		}

		//Вызывается при открытии статьи
        private ArticleDetailsViewController OpenArticle(Article article)
        {
            ArticleDetailsViewController controller = null;
            if (article != null)
            {
				if (article.ArticleType == ArticleType.Magazine || article.ArticleType == ArticleType.Portal)
				{
					_selectItemId = article.Id;
					article.IsReaded = true;
					ThreadPool.QueueUserWorkItem (state => ArticlesTableSource.SaveArticle (article));
					ApplicationWorker.ClearNews ();
					ApplicationWorker.AppendToNewsList (
						_articles.Where (x => x.ArticleType == ArticleType.Magazine || x.ArticleType == ArticleType.Portal));
					var articlesId = _articles.Where (x => x.ArticleType == ArticleType.Magazine || x.ArticleType == ArticleType.Portal)
                    .Select (x => x.Id).ToList ();
					controller = new ArticleDetailsViewController (article, articlesId, _fromFavorite, _magazineAction);
				}
            }
            return controller;
        }

		public static void SetIsReadedForArticle(Article article)
		{
			if (article != null)
			{
				if (article.IsReaded || (!article.IsReaded && article.IsFavorite))
				{
					ThreadPool.QueueUserWorkItem(state => ArticlesTableSource.SaveArticle(article));
				}
				if (!article.IsReaded && !article.IsFavorite)
				{
					ThreadPool.QueueUserWorkItem(state => ArticlesTableSource.DeleteArticle(article));
				}
			}
		}

        private DoubleArticleTableViewCell CreateCell(UITableView tableView)
        {
            var cell = new DoubleArticleTableViewCell(UITableViewCellStyle.Default, _cellIdentifier);

            cell.Frame = new System.Drawing.RectangleF(0, 0, tableView.Frame.Width, cell.Frame.Height);
            cell.ContentView.Frame = new System.Drawing.RectangleF(0, 0, tableView.Frame.Width, cell.Frame.Height);

            return cell;
        }

        private List<Article> _articles;
		private readonly List<DoubleArticle> _doubleArticles;
		private string _cellIdentifier;
		private readonly bool _fromFavorite;
		private MagazineAction _magazineAction;
		private int _selectItemId = -1;
	}
}

