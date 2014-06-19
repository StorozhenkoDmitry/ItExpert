﻿using System;
using System.Linq;
using MonoTouch.UIKit;
using System.Collections.Generic;
using ItExpert.Model;
using ItExpert.Enum;
using System.Threading;

namespace ItExpert
{
	public class NewsTableSource: UITableViewSource
	{
		public NewsTableSource (List<Article> items, bool fromFavorite, MagazineAction magazineAction)
		{
			_fromFavorite = fromFavorite;
			_magazineAction = magazineAction;
			_articles = items;
			_cellIdentifier = "NewsCell";

			_largestImageWidth = 0;

			foreach (var article in _articles)
			{
				if (article.PreviewPicture.Width > _largestImageWidth)
				{
					_largestImageWidth = article.PreviewPicture.Width;
				}
			}
		}

		public event EventHandler<PushNewsDetailsEventArgs> PushNewsDetails;

		public override int RowsInSection (UITableView tableview, int section)
		{
			return _articles.Count;
		}

		public override UITableViewCell GetCell (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
		{
			NewsTableViewCell cell = tableView.DequeueReusableCell (_cellIdentifier) as NewsTableViewCell;

			if (cell == null)
			{
				cell = new NewsTableViewCell (_cellIdentifier);

				cell.AddCellContent (_articles [indexPath.Row], _largestImageWidth);
			}
			else
			{
				cell.UpdateCell(_articles [indexPath.Row], _largestImageWidth);
			}

			return cell;
		}

		public override float GetHeightForRow (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
		{
			var cell = new NewsTableViewCell (_cellIdentifier);

			return cell.GetHeightDependingOnContent (_articles [indexPath.Row], _largestImageWidth);
		}

		public override void RowSelected (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
		{
			NewsDetailsViewController newsDetailsView = OpenArticle(_articles[indexPath.Row]);
			if (newsDetailsView != null)
			{
				OnPushNewsDetails (newsDetailsView);
			}
		}

		private void OnPushNewsDetails(NewsDetailsViewController newsDetailsView)
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
		private NewsDetailsViewController OpenArticle(Article article)
		{
			NewsDetailsViewController controller = null;
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
				controller = new NewsDetailsViewController (article, articlesId, _fromFavorite, _magazineAction);
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
		private int _largestImageWidth;
		private readonly bool _fromFavorite;
		private MagazineAction _magazineAction;
		private int _selectItemId = -1;
	}
}

