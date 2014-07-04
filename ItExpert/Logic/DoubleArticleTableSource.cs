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
		public DoubleArticleTableSource (List<Article> items, bool fromFavorite, MagazineAction magazineAction)
		{
			_fromFavorite = fromFavorite;
			_magazineAction = magazineAction;
			_doubleItems = new List<DoubleArticle>();
			for (var i = 0; i < items.Count(); i += 2)
			{
				if (i > items.Count() - 1)
				{
					break;
				}
				var model = new DoubleArticle();
				_doubleItems.Add(model);
				for (var j = 0; j < 2; j++)
				{
					var current = i + j;
					if (current > items.Count() - 1)
					{
						break;
					}
					var item = items[current];
					if (j == 0)
					{
						model.Cell1 = item;
					}
					if (j == 1)
					{
						model.Cell2 = item;
					}
				}
			}
		}

		public event EventHandler<PushDetailsEventArgs> PushDetailsView;

		public override int RowsInSection (UITableView tableview, int section)
		{
			return _doubleItems.Count;
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			return null;
		}

		public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			return 0;
		}

		public override void RowSelected (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
		{

		}

		private void OnCellPushed(ArticleDetailsViewController newsDetailsView)
		{
			if (PushDetailsView != null)
			{
				PushDetailsView (this, new PushDetailsEventArgs (newsDetailsView));
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



		private readonly List<DoubleArticle> _doubleItems;
		private string _cellIdentifier;
		private readonly bool _fromFavorite;
		private MagazineAction _magazineAction;
		private int _selectItemId = -1;

		public class DoubleArticle
		{
			public Article Cell1 { get; set; }

			public Article Cell2 { get; set; }
		}
	}
}

