using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using ItExpert.DataAccessLayer;
using ItExpert.Model;
using ItExpert.ServiceLayer;

namespace ItExpert
{
    public static class ApplicationWorker
    {
        #region Fields

        private static object _lockObj = new object();
        private static RemoteDataWorker _remoteDataWorker;
        private static DbEngine _dbEngine;
        private static Settings _applicationSettings;
        private static PdfLoader _pdfLoaderWorker;
        private static List<Article> _workedArticles = new List<Article>(); 
        public static Magazine Magazine = null;
        public static event EventHandler<EventArgs> SettingsChanged;
        public const int WidthForDoubleRow = 1000;

        #endregion

        static ApplicationWorker()
        {
            _remoteDataWorker = new RemoteDataWorker();
            _pdfLoaderWorker = new PdfLoader();
            _dbEngine = new DbEngine();
            _applicationSettings = Settings.GetSettings();
        }

        #region Property

        public static RemoteDataWorker RemoteWorker
        {
            get { return _remoteDataWorker; }
        }

        public static PdfLoader PdfLoader
        {
            get { return _pdfLoaderWorker; }
        }

        public static DbEngine Db
        {
            get { return _dbEngine; }
        }

        public static Settings Settings
        {
            get { return _applicationSettings; }
        }

        public static string Css { get; set; } 

        public static string Search { get; set; }

        public static Article SharedArticle { get; set; }

        public static ArticleEventArgs StartArticlesEventArgs { get; set; }

        public static BannerEventArgs BannerEventArgs { get; set; }

        #endregion

        public static void Clear()
        {
            _remoteDataWorker.Abort();
            _remoteDataWorker.Dispose();
            _pdfLoaderWorker.Dispose();
            _dbEngine.Dispose();
            _remoteDataWorker = null;
            _pdfLoaderWorker = null;
            _dbEngine = null;
            _applicationSettings = null;
            _lockObj = null;
            _workedArticles = null;
            SettingsChanged = null;
            Magazine = null;
            LastMagazine = null;
            LastMagazineArticles = null;
            SharedArticle = null;
        }

        #region News

        public static void ClearNews()
        {
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                _workedArticles.Clear();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
        }

        public static void AppendToNewsList(IEnumerable<Article> addedArticles)
        {
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                _workedArticles.AddRange(addedArticles);
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
        }

        public static Article GetArticle(int id)
        {
            Article article = null;
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                article = _workedArticles.FirstOrDefault(x => x.Id == id);
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
            return article;
        }

        #endregion

        #region Magazine

        public static Magazine LastMagazine { get; set; }

        public static List<Article> LastMagazineArticles { get; set; } 

        #endregion

        #region Helper methods

        public static void OnSettingsChanged()
        {
            var handler = Interlocked.CompareExchange(ref SettingsChanged, null, null);
            if (handler != null)
            {
                handler(null, new EventArgs());
            }
        }

        public static void NormalizePreviewText(IEnumerable<Article> lst)
        {

            foreach (var article in lst)
            {
				article.PreviewText = System.Web.HttpUtility.HtmlDecode(article.PreviewText).ToString().Trim();
                article.PreviewText = article.PreviewText.Replace("<br/>", " ")
                    .Replace("<br />", " ")
                    .Replace("</br>", " ")
                    .Replace("</ br>", " ")
                    .Replace("<br>", " ");
				article.Name = System.Web.HttpUtility.HtmlDecode(article.Name).ToString().Trim();
            }
        }

		public static string NormalizeDetailText(Article article, int width)
        {
            if (string.IsNullOrWhiteSpace(article.DetailText)) return string.Empty;
            var result = RemoveImgIfNecessary(article.DetailText);
			result = SetStyleForImage(result, width);
			result = SetStyleForTable(result, width);
            result = AddHostForLink(result);
            result = AddHostForImg(result);
            result = RemoveHeight(result);
            return result;
        }

		private static string SetStyleForImage(string data, int width)
        {
			var widthPixels = (int)(width * 0.95);
            var style = "style='max-width: " + widthPixels + "px;' ";
            var replacePattern = "<img " + style;
            var returnData = data.Replace("<img", replacePattern);
            return returnData;
        }

		private static string SetStyleForTable(string data, int width)
        {
			var widthPixels = (int)(width * 0.95);
            var etalonTableTag =
                "<table cellspacing='0' cellpadding='0' border='0' style='max-width: " + widthPixels + "px; margin-left: 2px; border-collapse: collapse;'>";
            var regex = new Regex(@"<table\s+[^>]*>");
            var returnData = regex.Replace(data, etalonTableTag);
            regex = new Regex(@"<td\s+[^>]*>");
            returnData = regex.Replace(returnData, "<td>");
            return returnData;
        }

        public static string AddHostForLink(string data)
        {
            var returnData = data;
            var patterns = new[]
            {
                new {Pattern = @"href=""/\S*""", Index = 6},
                new {Pattern = @"href\s=""/\S*""", Index = 7},
                new {Pattern = @"href=\s""/\S*""", Index = 7},
                new {Pattern = @"href='/\S*'", Index = 6},
                new {Pattern = @"href\s='/\S*'", Index = 7},
                new {Pattern = @"href=\s'/\S*'", Index = 7}
            };
            foreach (var pattern in patterns)
            {
                var isMatch = false;
                var regex = new Regex(pattern.Pattern);
                do
                {
                    isMatch = regex.IsMatch(returnData);
                    if (isMatch)
                    {
                        var match = regex.Match(returnData);
                        var index = match.Index + pattern.Index;
                        returnData = returnData.Insert(index, Settings.Domen);
                    }
                } while (isMatch);
            }
            return returnData;
        }

        public static string AddHostForImg(string data)
        {
            var returnData = data;
            var patterns = new[]
            {
                new {Pattern = @"src=""/\S*""", Index = 5},
                new {Pattern = @"src\s=""/\S*""", Index = 6},
                new {Pattern = @"src=\s""/\S*""", Index = 6},
                new {Pattern = @"src='/\S*'", Index = 5},
                new {Pattern = @"src\s='/\S*'", Index = 6},
                new {Pattern = @"src=\s'/\S*'", Index = 6}
            };
            foreach (var pattern in patterns)
            {
                var isMatch = false;
                var regex = new Regex(pattern.Pattern);
                do
                {
                    isMatch = regex.IsMatch(returnData);
                    if (isMatch)
                    {
                        var match = regex.Match(returnData);
                        var index = match.Index + pattern.Index;
                        returnData = returnData.Insert(index, Settings.Domen);
                    }
                } while (isMatch);
            }
            return returnData;
        }

        public static string RemoveImgIfNecessary(string data)
        {
            var regex = new Regex(@"<img\s+[^>]*>");
            var matches = regex.Matches(data);
            if (matches.Count > 12)
            {
                var length = 0;
                for (var i = 12; i < matches.Count; i++)
                {
                    var match = matches[i];
                    data = data.Remove(match.Index - length, match.Length);
                    length += match.Length;
                }
            }
            return data;
        }

        private static string RemoveHeight(string data)
        {
            var regex = new Regex(@"height\s?=\s?""((\w|\W[^""]))*""");
            var returnData = regex.Replace(data, string.Empty);
            return returnData;
        }

        #endregion
    }
}
