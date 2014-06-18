using System;
using MonoTouch.UIKit;
using System.Collections.Generic;
using ItExpert.Model;

namespace ItExpert
{
	public class NewsTableSource: UITableViewSource
	{
		public NewsTableSource (List<Article> items)
		{
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
			NewsDetailsViewController newsDetailsView = new NewsDetailsViewController (_articles [indexPath.Row]);

			OnPushNewsDetails (newsDetailsView);
		}

		private void OnPushNewsDetails(NewsDetailsViewController newsDetailsView)
		{
			if (PushNewsDetails != null)
			{
				PushNewsDetails (this, new PushNewsDetailsEventArgs (newsDetailsView));
			}
		}

		//Вызывается при взаимодействии с кнопкой IsReaded
		private void SetIsReadedForArticle(Article article, bool isReaded)
		{

		}

		//Вызывается при открытии статьи
		private void OpenArticle(Article article)
		{

		}

		private List<Article> _articles;
		private string _cellIdentifier;
		private int _largestImageWidth;
	}
}

