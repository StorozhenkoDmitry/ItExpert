﻿using System;
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

            AutomaticallyAdjustsScrollViewInsets = false;

			//Tim
			if (_fromFavorite)
			{
				//ссылки не интерактивны
			}

			_padding = new UIEdgeInsets (8, 8, 8, 8);

            _maxWidth = View.Frame.Width - _padding.Left - _padding.Right;
            
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
			//В style я передаю строку стиля HTML с цветом и размером текста и цветом фона
			var style = string.Empty;
			//sectionString - раздел , articleAuthors-авторы, если пустые строки то не отображать
			var html = "<html><head>" + css + "</head><body style='" + style + "'>" + text + video + "</body></html>";

			//После полного отображения выставить флаг и убрать сплаш
			//Прокрутить представление до самого верха
			_isLoading = false;
			//ShowSplash (false);
            AddContent(sectionString, articleAuthors, html);

		}

		//Метод скрытия-отображения экрана заставки при загрузке
		private void ShowSplash(bool isVisible)
		{

		}

        private void AddContent(string section, string author, string text)
        {
            _scrollView = new UIScrollView(new RectangleF(0, 0, _maxWidth, View.Frame.Height));
            _scrollView.UserInteractionEnabled = true;
            _scrollView.ScrollEnabled = true;

            View.Add(_scrollView);
            View.UserInteractionEnabled = true;

            float top = AddArticleSection(section, NavigationController.NavigationBar.Frame.Height + ItExpertHelper.StatusBarHeight + _padding.Top);
            top = AddArticlePicture(top);
            top = AddArticleHeader(top);
            top = AddArticleAuthor(author, top);
            AddArticleText(text, top);

            if (_articleTextWebView == null)
            {
                _scrollView.ContentSize = new SizeF(_maxWidth, top);
            }
        }

        private float AddArticleSection (string section, float top)
		{
            if (!String.IsNullOrWhiteSpace(section))
            {
                _articleSectionView = ItExpertHelper.GetTextView(
                    ItExpertHelper.GetAttributedString(section, UIFont.BoldSystemFontOfSize(ApplicationWorker.Settings.DetailHeaderSize), UIColor.Blue, true),
                    _maxWidth, new PointF(_padding.Left, top));

                _scrollView.Add(_articleSectionView);

                return _articleSectionView.Frame.Bottom + _padding.Bottom;
            }

            return top;
		}

        private float AddArticlePicture(float top)
		{
            if (_article.DetailPicture != null)
            {
                float scale = 1;

                var picture = _article.DetailPicture;
                    
                if (picture.Width > View.Frame.Width)
                {
                    scale = View.Frame.Width / picture.Width;
                }

                if (picture.Height > View.Frame.Height * 0.35)
                {
                    float heightScale = picture.Height / (View.Frame.Height * 0.35f);

                    if (heightScale > scale)
                    {
                        scale = heightScale;
                    }
                }

                var image = ItExpertHelper.GetImageFromBase64String(picture.Data, scale);

                _articleImageView = new UIImageView(new RectangleF((View.Frame.Width - _padding.Left - _padding.Right)/2 - image.Size.Width / 2, 
                    top, image.Size.Width, image.Size.Height));

                _articleImageView.Image = image;

                _scrollView.Add(_articleImageView);

                return _articleImageView.Frame.Bottom + _padding.Bottom;
            }

            return top;
		}

        private float AddArticleHeader(float top)
		{
            if (!String.IsNullOrWhiteSpace(_article.Name))
            {
                _articleHeaderView = ItExpertHelper.GetTextView(
                    ItExpertHelper.GetAttributedString(_article.Name, UIFont.BoldSystemFontOfSize(ApplicationWorker.Settings.DetailHeaderSize), UIColor.Black),
                    _maxWidth, new PointF(_padding.Left, top));

                _scrollView.Add(_articleHeaderView);

                return _articleHeaderView.Frame.Bottom + _padding.Bottom;
            }

            return top;
		}

        private float AddArticleAuthor(string author, float top)
        {
            if (!String.IsNullOrWhiteSpace(author))
            {
                _articleAuthorView = ItExpertHelper.GetTextView(
                    ItExpertHelper.GetAttributedString(author, UIFont.BoldSystemFontOfSize(ApplicationWorker.Settings.DetailHeaderSize), UIColor.Blue, true),
                    _maxWidth, new PointF(_padding.Left, top));

                _scrollView.Add(_articleAuthorView);

                return _articleAuthorView.Frame.Bottom + _padding.Bottom;
            }

            return top;
        }

        private void AddArticleText(string text, float top)
        {
            if (!String.IsNullOrWhiteSpace(text))
            {
                _articleTextWebView = new UIWebView(new RectangleF(_padding.Left,
                    top, _maxWidth, 1));                   

                var webViewDelegate = new WebViewDelegate(); 
                webViewDelegate.WebViewLoaded += OnWebViewLoaded;

                _articleTextWebView.Delegate = webViewDelegate;
                _articleTextWebView.ScrollView.ScrollEnabled = false;
                _articleTextWebView.ScrollView.Bounces = false;
                _articleTextWebView.LoadHtmlString(text, null);
                    
                _scrollView.Add(_articleTextWebView);
            }
        }

        private void OnWebViewLoaded(object sender, EventArgs e)
        {
            _scrollView.ContentSize = new SizeF(_maxWidth, _articleTextWebView.Frame.Bottom);
        }

        private float _maxWidth;
		private Article _article;
		private bool _fromFavorite;
		private bool _isLoading;
		private MagazineAction _magazineAction;
		private List<int> _articlesId;
		private List<string> _authors = new List<string>();
		private event EventHandler<SwipeEventArgs> ArticleChange;

		private UIEdgeInsets _padding;

        private UIScrollView _scrollView;
        private UITextView _articleSectionView;
		private UIImageView _articleImageView;
        private UITextView _articleHeaderView;
        private UITextView _articleAuthorView;
		private UIWebView _articleTextWebView;
	}
}

