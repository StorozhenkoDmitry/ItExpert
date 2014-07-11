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
			if (_articles.Any()) 
			{
				var articlesWithPicture = _articles.Where (x => x.PreviewPicture != null);
				if (articlesWithPicture.Any())
				{
					ItExpertHelper.LargestImageSizeInArticlesPreview = articlesWithPicture.Max (x => x.PreviewPicture.Width);
				}
			}
		}

		public event EventHandler<PushDetailsEventArgs> PushDetailsView;

		public override int RowsInSection (UITableView tableview, int section)
		{
			return _articles.Count;
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
            ArticleTableViewCell cell = tableView.DequeueReusableCell (_cellIdentifier) as ArticleTableViewCell;
			if (cell != null)
			{
				cell.Frame = new System.Drawing.RectangleF(0, 0, tableView.Frame.Width, cell.Frame.Height);
				cell.ContentView.Frame = new System.Drawing.RectangleF(0, 0, tableView.Frame.Width, cell.Frame.Height);
			}
			if (cell == null)
			{
                cell = CreateCell(tableView);
			}			

            cell.UpdateContent(_articles[indexPath.Row]);

			return cell;
		}

		public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
            var cell = CreateCell(tableView);

            return cell.GetHeightDependingOnContent(_articles[indexPath.Row]);
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
            ArticleDetailsViewController articleDetailsView = OpenArticle(_articles[indexPath.Row]);

			if (articleDetailsView != null)
			{
				OnCellPushed (articleDetailsView);
			}

		}

        private void OnCellPushed(ArticleDetailsViewController detailsView)
		{
			if (PushDetailsView != null)
			{
				PushDetailsView (this, new PushDetailsEventArgs (detailsView));
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

		public static void SaveArticle(Article article)
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

		public static void DeleteArticle(Article article)
		{
			ApplicationWorker.Db.DeleteItemSectionsForArticle(article.Id);
			ApplicationWorker.Db.DeletePicturesForParent(article.Id);
			ApplicationWorker.Db.DeleteArticle(article.Id);
		}

        private ArticleTableViewCell CreateCell(UITableView tableView)
        {
            var cell = new ArticleTableViewCell(UITableViewCellStyle.Default, _cellIdentifier);

            cell.Frame = new System.Drawing.RectangleF(0, 0, tableView.Frame.Width, cell.Frame.Height);
            cell.ContentView.Frame = new System.Drawing.RectangleF(0, 0, tableView.Frame.Width, cell.Frame.Height);

            return cell;
        }

		private List<Article> _articles;
		private string _cellIdentifier;
		private readonly bool _fromFavorite;
		private MagazineAction _magazineAction;
		private int _selectItemId = -1;
	}
}

