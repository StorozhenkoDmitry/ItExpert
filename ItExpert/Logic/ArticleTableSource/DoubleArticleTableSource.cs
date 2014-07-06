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
		public DoubleArticleTableSource (List<Article> articles, bool fromFavorite, MagazineAction magazineAction)
		{
            _cellIdentifier = "ArticleCell";
			_fromFavorite = fromFavorite;
			_magazineAction = magazineAction;
            _doubleArticles = new List<DoubleArticle>();
            _articles = articles;

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

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
            DoubleArticleTableViewCell cell = tableView.DequeueReusableCell (_cellIdentifier) as DoubleArticleTableViewCell;

            if (cell == null)
            {
                cell = CreateCell(tableView);

                cell.CellPushed += (sender, e) => 
                {
                    ArticleDetailsViewController articleDetailsView = OpenArticle(e.Article);

                    if (articleDetailsView != null)
                    {
                        OnCellPushed (articleDetailsView);
                    }
                };
            }           

            cell.UpdateContent(_doubleArticles[indexPath.Row]);

            return cell;
		}

		public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
            var cell = CreateCell(tableView);

            return cell.GetHeightDependingOnContent(_doubleArticles[indexPath.Row]);
		}

        private void OnCellPushed(ArticleDetailsViewController articlesDetails)
		{
			if (PushDetailsView != null)
			{
				PushDetailsView (this, new PushDetailsEventArgs (articlesDetails));
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

		//Вызывается при открытии статьи
        private ArticleDetailsViewController OpenArticle(Article article)
        {
            ArticleDetailsViewController controller = null;
            if (article != null)
            {
                if (article.ArticleType == ArticleType.Header || article.ArticleType == ArticleType.ExtendedObject ||
                    article.ArticleType == ArticleType.Placeholder || article.ArticleType == ArticleType.Banner || 
                    article.ArticleType == ArticleType.PreviousArticlesButton || article.ArticleType == ArticleType.MagazinePreview) return null;
                _selectItemId = article.Id;
                article.IsReaded = true;
                ThreadPool.QueueUserWorkItem(state => SaveArticle(article));
                ApplicationWorker.ClearNews();
                ApplicationWorker.AppendToNewsList(
                    _articles.Where(x => x.ArticleType == ArticleType.Magazine || x.ArticleType == ArticleType.Portal));
                var articlesId = _articles.Where(x => x.ArticleType == ArticleType.Magazine || x.ArticleType == ArticleType.Portal)
                    .Select(x => x.Id).ToList();
                controller = new ArticleDetailsViewController (article, articlesId, _fromFavorite, _magazineAction);
            }
            return controller;
        }

		public static void SetIsReadedForArticle(Article article)
		{
			if (article != null)
			{
				if (article.IsReaded || (!article.IsReaded && article.IsFavorite))
				{
					ThreadPool.QueueUserWorkItem(state => SaveArticle(article));
				}
				if (!article.IsReaded && !article.IsFavorite)
				{
					ThreadPool.QueueUserWorkItem(state => DeleteArticle(article));
				}
			}
		}

		private static void SaveArticle(Article article)
		{
			var dbArticle = ApplicationWorker.Db.GetArticle(article.Id);
			if (dbArticle != null)
			{
				ApplicationWorker.Db.UpdateArticle(article);
			}
			else
			{
				ApplicationWorker.Db.DeleteItemSectionsForArticle(article.Id);
				ApplicationWorker.Db.DeletePicturesForParent(article.Id);
				ApplicationWorker.Db.InsertArticle(article);
				if (article.ArticleType == ArticleType.Portal)
				{
					ApplicationWorker.Db.InsertItemSections(article.Sections);
				}
				if (article.PreviewPicture != null)
				{
					ApplicationWorker.Db.InsertPicture(article.PreviewPicture);
				}
				if (article.DetailPicture != null)
				{
					ApplicationWorker.Db.InsertPicture(article.DetailPicture);
				}
				if (article.AwardsPicture != null)
				{
					ApplicationWorker.Db.InsertPicture(article.AwardsPicture);
				}
			}
		}

		private static void DeleteArticle(Article article)
		{
			ApplicationWorker.Db.DeleteItemSectionsForArticle(article.Id);
			ApplicationWorker.Db.DeletePicturesForParent(article.Id);
			ApplicationWorker.Db.DeleteArticle(article.Id);
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

