using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using ItExpert.Enum;
using ItExpert.Model;
using ItExpert.ServiceLayer;

namespace ItExpert
{
	public class ArticleDetailsViewController: UIViewController
	{
		public ArticleDetailsViewController (Article article, List<int> articlesId, bool fromFavorite, MagazineAction magazineAction)
		{
			_article = article;
			_fromFavorite = fromFavorite;
			_magazineAction = magazineAction;
			_articlesId = articlesId;
			ArticleChange += OnArticleChange;
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			//Tim
			if (_fromFavorite)
			{
				//ссылки не интерактивны
			}


			_padding = new UIEdgeInsets (8, 8, 8, 8);

			View.BackgroundColor = UIColor.White;

			GetArticleData ();
		}

		#region Worked with server

		private void GetArticleData()
		{
			ShowSplash (true);
			var requestNeeded = false;
			var article = ApplicationWorker.GetArticle(_article.Id);
			if (article != null)
			{
				_authors.Clear();
				_authors.AddRange(article.Authors.Select(x => x.Name));
				if (!ApplicationWorker.Settings.OfflineMode)
				{
					if ((ApplicationWorker.Settings.LoadImages && article.DetailPicture == null) ||
						string.IsNullOrWhiteSpace(article.DetailText))
					{
						requestNeeded = true;
					}
				}
				else
				{
					if (string.IsNullOrWhiteSpace(article.DetailText))
					{
						requestNeeded = true;
					}
				}
			}
			else
			{
				_article = null;
				requestNeeded = true;
			}
			if (!requestNeeded)
			{
				_article = article;
				UpdateScreen();
			}
			else
			{
				if (!ApplicationWorker.Settings.OfflineMode)
				{
					var connectAccept = IsConnectionAccept();
					if (!connectAccept)
					{
//						Toast.MakeText(this, "Нет доступных подключений, для указанных в настройках",
//							ToastLength.Long).Show();
						return;
					}
					_article = article;
					_isLoading = true;
					if (_article.ArticleType == ArticleType.Portal)
					{
						ApplicationWorker.RemoteWorker.ArticleDetailGetted += OnArticleDetailGetted;
						ThreadPool.QueueUserWorkItem(
							state =>
							ApplicationWorker.RemoteWorker.BeginGetArticleDetail(ApplicationWorker.Settings, -1,
								-1,
								_article.Id));
					}
					if (_article.ArticleType == ArticleType.Magazine)
					{
						ApplicationWorker.RemoteWorker.MagazineArticleDetailGetted += OnMagazineArticleDetailGetted;
						ThreadPool.QueueUserWorkItem(
							state =>
							ApplicationWorker.RemoteWorker.BeginGetMagazineArticleDetail(
								ApplicationWorker.Settings, -1, -1, _article.Id));
					}
				}
				else
				{
					if (_article.ArticleType  == ArticleType.Magazine && _magazineAction == MagazineAction.Open)
					{
//						ShowDialog(ShowPdfDialog);
						return;
					}
					if (_article.ArticleType  == ArticleType.Magazine && _magazineAction == MagazineAction.Download)
					{
//						ShowDialog(LoadPdfDialog);
						return;
					}
//					Toast.MakeText(this, "Не найдена детальная статья в кэше", ToastLength.Short).Show();
//					Finish();
				}
			}
		}

		private void OnMagazineArticleDetailGetted(object sender, MagazineArticleDetailEventArgs e)
		{
			ApplicationWorker.RemoteWorker.MagazineArticleDetailGetted -= OnMagazineArticleDetailGetted;
			if (e.Abort)
			{
				return;
			}
			InvokeOnMainThread(() =>
			{
				var error = e.Error;
				if (!error)
				{
					var article = e.Article;
					if (article != null)
					{
						article.IsReaded = true;
						if (_article != null)
						{
							_article.DetailText = article.DetailText;
							_article.IsReaded = article.IsReaded;
							_article.Authors = article.Authors;
							_article.AuthorsId = article.AuthorsId;
							_article.Video = article.Video;
							_authors.Clear();
							_authors.AddRange(article.Authors.Select(x => x.Name));
							if (ApplicationWorker.Settings.LoadImages && article.DetailPicture != null)
							{
								_article.DetailPicture = article.DetailPicture;
							}
							if (article.AwardsPicture != null)
							{
								_article.AwardsPicture = article.AwardsPicture;
							}
							ThreadPool.QueueUserWorkItem(state => SaveArticle(_article));
						}
						else
						{
							_article = article;
						}
						UpdateScreen();
					}
				}
				else
				{
//					Toast.MakeText(this, "Ошибка при загрузке", ToastLength.Short).Show();
					_isLoading = false;
				}
			});
		}

		private void OnArticleDetailGetted(object sender, ArticleDetailEventArgs e)
		{
			ApplicationWorker.RemoteWorker.ArticleDetailGetted -= OnArticleDetailGetted;
			if (e.Abort)
			{
				return;
			}
			InvokeOnMainThread(() =>
			{
				var error = e.Error;
				if (!error)
				{
					var article = e.Article;
					if (article != null)
					{
						article.IsReaded = true;
						if (_article != null)
						{
							_article.DetailText = article.DetailText;
							_article.IsReaded = article.IsReaded;
							_article.Authors = article.Authors;
							_article.AuthorsId = article.AuthorsId;
							_article.Video = article.Video;
							_authors.Clear();
							_authors.AddRange(article.Authors.Select(x => x.Name));
							if (ApplicationWorker.Settings.LoadImages && article.DetailPicture != null)
							{
								_article.DetailPicture = article.DetailPicture;
							}
							if (article.AwardsPicture != null)
							{
								_article.AwardsPicture = article.AwardsPicture;
							}
							ThreadPool.QueueUserWorkItem(state => SaveArticle(_article));
						}
						else
						{
							_article = article;
						}
						UpdateScreen();
					}
				}
				else
				{
//					Toast.MakeText(this, "Ошибка при загрузке", ToastLength.Short).Show();
					_isLoading = false;
				}
			});
		}

		#endregion

		#region Logic

		private void OnArticleChange(object sender, SwipeEventArgs e)
		{
			if (_isLoading) return;
			ShowSplash (true);
			Action actionChange = () =>
			{
				var currentIndex = _articlesId.FindIndex(x => x == _article.Id);
				if (currentIndex != -1)
				{
					var searchElemIndex = (e.Direction == SwipeDirection.Next) ? currentIndex + 1 : currentIndex - 1;
					if (searchElemIndex < 0)
					{
						searchElemIndex = _articlesId.Count() - 1;
					}
					if (searchElemIndex > _articlesId.Count() - 1)
					{
						searchElemIndex = 0;
					}
					var articleId = _articlesId[searchElemIndex];
					var requestNeeded = false;
					var article = ApplicationWorker.GetArticle(articleId);
					if (article != null)
					{
						_authors.Clear();
						_authors.AddRange(article.Authors.Select(x => x.Name));
						if (!ApplicationWorker.Settings.OfflineMode)
						{
							if ((ApplicationWorker.Settings.LoadImages && article.DetailPicture == null) ||
								string.IsNullOrWhiteSpace(article.DetailText))
							{
								requestNeeded = true;
							}
						}
						else
						{
							if (string.IsNullOrWhiteSpace(article.DetailText))
							{
								requestNeeded = true;
							}
						}
						article.IsReaded = true;
						ThreadPool.QueueUserWorkItem(state => SaveArticle(article));
					}
					else
					{
						_article = null;
						requestNeeded = true;
					}
					if (!requestNeeded)
					{
						_article = article;
						InvokeOnMainThread(() => UpdateScreen());
					}
					else
					{
						if (!ApplicationWorker.Settings.OfflineMode)
						{
							var connectAccept = IsConnectionAccept();
							if (!connectAccept)
							{
//								Toast.MakeText(this, "Нет доступных подключений, для указанных в настройках",
//									ToastLength.Long).Show();
								return;
							}
							_article = article;
							ApplicationWorker.RemoteWorker.ArticleDetailGetted -= OnArticleDetailGetted;
							ApplicationWorker.RemoteWorker.MagazineArticleDetailGetted -= OnMagazineArticleDetailGetted;
							ApplicationWorker.RemoteWorker.Abort();
							_isLoading = true;
							if (_article.ArticleType == ArticleType.Portal)
							{
								ApplicationWorker.RemoteWorker.ArticleDetailGetted += OnArticleDetailGetted;
								ApplicationWorker.RemoteWorker.BeginGetArticleDetail(
									ApplicationWorker.Settings, -1, -1, articleId);
							}
							if (_article.ArticleType == ArticleType.Magazine)
							{
								ApplicationWorker.RemoteWorker.MagazineArticleDetailGetted += OnMagazineArticleDetailGetted;
								ApplicationWorker.RemoteWorker.BeginGetMagazineArticleDetail(
									ApplicationWorker.Settings, -1, -1, articleId);
							}
						}
						if (ApplicationWorker.Settings.OfflineMode)
						{
							InvokeOnMainThread(() =>
							{
								if (_article.ArticleType == ArticleType.Magazine && _magazineAction == MagazineAction.Open)
								{
//									ShowDialog(ShowPdfDialog);
//									return;
								}
								if (_article.ArticleType== ArticleType.Magazine && _magazineAction == MagazineAction.Download)
								{
//									ShowDialog(LoadPdfDialog);
//									return;
								}
//								Toast.MakeText(this, "Не найдена детальная статья в кэше", ToastLength.Short).Show();
//								Finish();
							});
						}
					}
				}
			};
			ThreadPool.QueueUserWorkItem(state => actionChange());
		}


		private void SaveArticle(Article article)
		{
			var pictures = ApplicationWorker.Db.GetPicturesForParent(article.Id);
			if (pictures != null && pictures.Any())
			{
				var detailPicture = pictures.FirstOrDefault(x => x.Type == PictypeType.Detail);
				if (detailPicture != null)
				{
					ApplicationWorker.Db.DeletePicture(detailPicture.Id);
				}
				var awardsPicture = pictures.FirstOrDefault(x => x.Type == PictypeType.Awards);
				if (awardsPicture != null)
				{
					ApplicationWorker.Db.DeletePicture(awardsPicture.Id);
				}
				var previewPicture = pictures.FirstOrDefault(x => x.Type == PictypeType.Preview);
				if (previewPicture != null)
				{
					ApplicationWorker.Db.DeletePicture(previewPicture.Id);
				}
			}
			if (article.DetailPicture != null)
			{
				ApplicationWorker.Db.InsertPicture(article.DetailPicture);
			}
			if (article.AwardsPicture != null)
			{
				ApplicationWorker.Db.InsertPicture(article.AwardsPicture);
			}
			if (article.PreviewPicture != null)
			{
				ApplicationWorker.Db.InsertPicture(article.PreviewPicture);
			}
			var authors = article.Authors;
			if (authors != null && authors.Any())
			{
				foreach (var author in authors)
				{
					var dbAuthor = ApplicationWorker.Db.GetAuthor(author.Id);
					if (dbAuthor == null)
					{
						ApplicationWorker.Db.InsertAuthor(author);
					}
				}
			}
			var rubrics = article.Rubrics;
			if (rubrics != null && rubrics.Any())
			{
				foreach (var rubric in rubrics)
				{
					var dbRubric = ApplicationWorker.Db.GetRubric(rubric.Id);
					if (dbRubric == null)
					{
						ApplicationWorker.Db.InsertRubric(rubric);
					}
				}
			}
			ApplicationWorker.Db.DeleteItemSectionsForArticle(article.Id);
			if (article.Sections != null && article.Sections.Any())
			{
				ApplicationWorker.Db.InsertItemSections(article.Sections);
			}
			ApplicationWorker.Db.InsertArticle(article);
		}
			
		private bool IsConnectionAccept()
		{
			var result = true;
			return result;
		}

		#endregion

		private void UpdateScreen()
		{
			var article = _article;
			if (string.IsNullOrWhiteSpace(article.DetailText))
			{
				_isLoading = false;
				if (_article.ArticleType == ArticleType.Magazine && _magazineAction == MagazineAction.Open)
				{
					//					ShowDialog(ShowPdfDialog);
					//					return;
				}
				if (_article.ArticleType  == ArticleType.Magazine && _magazineAction == MagazineAction.Download)
				{
					//					ShowDialog(LoadPdfDialog);
					//					return;
				}
				new UIAlertView ("Информация", "Отсутствует детальная статья", null, "OK", null).Show ();
				//Закрыть экран
				return;
			}
			var sectionString = string.Empty;
			if (article.ArticleType == ArticleType.Portal)
			{
				if (article.Sections != null && article.Sections.Any())
				{
					var section = string.Join(@"/",
						article.Sections.OrderBy(x => x.DepthLevel).Select(x => x.Section.Name));
					if (!string.IsNullOrWhiteSpace(section))
					{
						sectionString = section;
					}
				}
			}
			if (article.ArticleType == ArticleType.Magazine)
			{
				var rubricsId = article.RubricsId;
				if (!string.IsNullOrWhiteSpace(rubricsId))
				{
					var rubrics = article.Rubrics;
					var rubricText = string.Join("/", rubrics.Select(x => x.Name));
					sectionString = rubricText;
				}
			}
			var articleAuthors = string.Empty;
			if (article.Authors != null && article.Authors.Any())
			{
				articleAuthors = string.Join("/", article.Authors.Select(x => x.Name));
			}
			var text = ApplicationWorker.NormalizeDetailText(article, (int)UIScreen.MainScreen.Bounds.Size.Width);
			var video = string.Empty;
			if (!string.IsNullOrWhiteSpace(article.Video))
			{
				video = "<br/><a href='" + article.Video + "'>Видео</a>";
			}
			var css = string.Empty;
			if (!string.IsNullOrWhiteSpace(ApplicationWorker.Css))
			{
				css = "<style>" + ApplicationWorker.Css + "</style>";
			}
			var style = string.Empty;
			var html = "<html><head>" + css + "</head><body style='" + style + "'>" + text + video + "</body></html>";

			//			После полного отображения выставить флаг и убрать сплаш
			_isLoading = false;
			//ShowSplash (false);
		}

		private void ShowSplash(bool isVisible)
		{

		}

		private void AddArticleSection ()
		{
			var section = _article.Sections.LastOrDefault ();

			if (section != null && !String.IsNullOrEmpty(section.Section.Name))
			{
				//				var textView = ItManagerHelper.GetTextView (UIFont.BoldSystemFontOfSize (ApplicationWorker.Settings.DetailHeaderSize), 
				//					               ItManagerHelper.GetUIColorFromColor (ApplicationWorker.Settings.GetForeColor ()), section.Section.Name,
				//					               View.Frame.Width - _padding.Left - _padding.Right, new PointF (0, 0));
				var textView = new UITextView(new RectangleF(20,20, 300, 200));
				textView.Editable = false;
				View.AddSubview (textView);
				textView.AttributedText = new NSAttributedString (section.Section.Name, new UIStringAttributes {
					ForegroundColor = UIColor.Blue,
					Font = UIFont.FromName ("Courier", 18f),
					UnderlineStyle = NSUnderlineStyle.Single
				});
				//				float textHeight = textView.Frame.Height;
				//
				//				textView.Dispose ();
				//				textView = null;

				//				_articleSectionLabel = new UILabel(new RectangleF(_padding.Left, 
				//					NavigationController.NavigationBar.Frame.Height + _padding.Top + ItManagerHelper.StatusBarHeight, 
				//					View.Frame.Width - _padding.Left - _padding.Right, textHeight));
				//
				//				_articleSectionLabel.AttributedText = ItManagerHelper.GetAttributedString(section.Section.Name, 
				//					UIFont.BoldSystemFontOfSize (ApplicationWorker.Settings.DetailHeaderSize), UIColor.Blue, true);
				//
				//				View.AddSubview (_articleSectionLabel);

				//				UIWebView sectionWebView = new UIWebView (new RectangleF (_padding.Left, 
				//					NavigationController.NavigationBar.Frame.Height + _padding.Top + ItManagerHelper.StatusBarHeight, 
				//					View.Frame.Width - _padding.Left - _padding.Right, 
				//					textHeight + 20));
				////				{
				////					BackgroundColor = UIColor.White
				////				};
				//				AutomaticallyAdjustsScrollViewInsets = false;
				//				sectionWebView.ScrollView.ScrollEnabled = false;
				//				sectionWebView.ScrollView.Bounces = false;
				//
				//				var htmlSectionName = String.Format ("<body><a href=\"URL\">olololo!!!</a></body>", section.Section.Name);
				//
				//				sectionWebView.LoadHtmlString (htmlSectionName, null);
				//
				//				View.Add (sectionWebView);
			}
		}

		private void AddArticlePicture ()
		{
		}

		private void AddArticleHeader ()
		{

		}

		private void AddArticleText ()
		{
		}

		private Article _article;
		private bool _fromFavorite;
		private bool _isLoading;
		private MagazineAction _magazineAction;
		private List<int> _articlesId;
		private List<string> _authors = new List<string>();
		private event EventHandler<SwipeEventArgs> ArticleChange;

		private UIEdgeInsets _padding;

		private UILabel _articleSectionLabel;
		private UIImageView _articleImageView;
		private UILabel _articleHeaderLabel;
		private UILabel _articleAuthorLabel;
		private UIWebView _articleTextWebView;
	}
}

