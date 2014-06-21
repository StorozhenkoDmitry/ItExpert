using System;
using System.Linq;
using MonoTouch.UIKit;
using System.Collections.Generic;
using ItExpert.Model;
using ItExpert.Enum;
using System.Threading;
using MonoTouch.Foundation;

namespace ItExpert
{
	public class ArticlesTableSource: UITableViewSource
	{
		public ArticlesTableSource (List<Article> items, bool fromFavorite, MagazineAction magazineAction)
		{
			_fromFavorite = fromFavorite;
			_magazineAction = magazineAction;
			_articles = items;
            _cellIdentifier = "ArticleCell";

            ItExpertHelper.LargestImageSizeInArticlesPreview = 0;

			foreach (var article in _articles)
			{
                if (article.PreviewPicture != null && article.PreviewPicture.Width > ItExpertHelper.LargestImageSizeInArticlesPreview)
				{
                    ItExpertHelper.LargestImageSizeInArticlesPreview = article.PreviewPicture.Width;
				}
			}
		}

		public event EventHandler<PushNewsDetailsEventArgs> PushNewsDetails;

		public override int RowsInSection (UITableView tableview, int section)
		{
			return _articles.Count;
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
            ArticleTableViewCell cell = tableView.DequeueReusableCell (_cellIdentifier) as ArticleTableViewCell;

			if (cell == null)
			{
                cell = new ArticleTableViewCell(UITableViewCellStyle.Default, _cellIdentifier);				
			}			

            cell.UpdateContent(indexPath.Row == 0, _articles[indexPath.Row]);

			return cell;
		}

		public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
            var cell = new ArticleTableViewCell(UITableViewCellStyle.Default, _cellIdentifier);

            return cell.GetHeightDependingOnContent(indexPath.Row == 0, _articles[indexPath.Row]);
		}

		public override void RowSelected (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
		{
            ArticleDetailsViewController articleDetailsView = OpenArticle(_articles[indexPath.Row]);

			if (articleDetailsView != null)
			{
				OnPushNewsDetails (articleDetailsView);
			}
		}

		private void OnPushNewsDetails(ArticleDetailsViewController newsDetailsView)
		{
			if (PushNewsDetails != null)
			{
				PushNewsDetails (this, new PushNewsDetailsEventArgs (newsDetailsView));
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
					article.ArticleType == ArticleType.Placeholder) return null;
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



		private List<Article> _articles;
		private string _cellIdentifier;
		private readonly bool _fromFavorite;
		private MagazineAction _magazineAction;
		private int _selectItemId = -1;
	}
}

