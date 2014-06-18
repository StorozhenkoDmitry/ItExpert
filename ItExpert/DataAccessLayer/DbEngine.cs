using ItExpert.Enum;
using ItExpert.Model;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Path = System.IO.Path;
using Picture = ItExpert.Model.Picture;

namespace ItExpert.DataAccessLayer
{
    public class DbEngine : IDisposable
    {
        #region Fields

        private object _lockObj = new object();

        private const string DbName = "data.db";

        private SQLiteConnection _db;

        #endregion

        #region Constructor

        public DbEngine()
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            _db = new SQLiteConnection(Path.Combine(folder, DbName));
            _db.CreateTable<Block>();
            _db.CreateTable<Section>();
            _db.CreateTable<ItemSection>();
            _db.CreateTable<Picture>();
            _db.CreateTable<Article>();
            _db.CreateTable<Banner>();
            _db.CreateTable<MagazineYear>();
            _db.CreateTable<Magazine>();
            _db.CreateTable<Rubric>();
            _db.CreateTable<Author>();
        }

        #endregion

        #region Properties

        public SQLiteConnection Db
        {
            get { return _db; }
        }

        #endregion

        #region Public Methods

        public void Dispose()
        {
            _db = null;
            _lockObj = null;
        }

        #region Block

        public List<Block> GetBlocks()
        {
            var lockTaken = false;
            var lst = new List<Block>();
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                lst = _db.Table<Block>().Select(x => x).ToList();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
            return lst;
        }

        public Block GetBlock(int id)
        {
            var lockTaken = false;
            Block model = null;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                model = _db.Query<Block>("SELECT * FROM Block WHERE Id = ?", id).FirstOrDefault();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
            return model;
        }

        public void InsertNewBlocks(List<Block> oldLst, IEnumerable<Block> newLst)
        {
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                foreach (var block in newLst)
                {
                    var exists = oldLst.Count(x => x.Id == block.Id) > 0;
                    if (!exists)
                    {
                        _db.Insert(block, typeof(Block));
                    }
                }
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
        }

        #endregion

        #region Section

        public List<Section> GetSections()
        {
            var lockTaken = false;
            var lst = new List<Section>();
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                lst = _db.Table<Section>().Select(x => x).ToList();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
            return lst;
        }

        public Section GetSection(int id)
        {
            var lockTaken = false;
            Section model = null;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                model = _db.Query<Section>("SELECT * FROM Section WHERE Id = ?", id).FirstOrDefault();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
            return model;
        }

        public void InsertNewSections(List<Section> oldLst, IEnumerable<Section> newLst)
        {
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                foreach (var section in newLst)
                {
                    var exists = oldLst.Count(x => x.Id == section.Id) > 0;
                    if (!exists)
                    {
                        _db.Insert(section, typeof(Section));
                    }
                }
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
        }

        #endregion

        #region Article

        public Article GetArticle(int id)
        {
            var lockTaken = false;
            Article model = null;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                model = _db.Query<Article>("SELECT * FROM Article WHERE Id = ?", id).FirstOrDefault();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
            return model;
        }

        public void InsertArticle(Article model)
        {
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                _db.InsertOrReplace(model, typeof(Article));
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
        }

        public void UpdateArticle(Article model)
        {
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                _db.Update(model, typeof(Article));
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
        }

        public void DeleteArticle(int id)
        {
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                _db.Execute("DELETE FROM Article WHERE Id = ?", id);
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
        }

        public List<Article> LoadArticles()
        {
            var lockTaken = false;
            var lst = new List<Article>();
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                lst = _db.Table<Article>().ToList();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
            return lst;
        }

        public void SetPropertiesForArticles(IEnumerable<Article> articles)
        {
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                foreach (var article in articles)
                {
                    var dbArticle = _db.Query<Article>("SELECT * FROM Article WHERE Id = ?", article.Id).FirstOrDefault();
                    if (dbArticle != null)
                    {
                        article.IsReaded = dbArticle.IsReaded;
                        article.IsFavorite = dbArticle.IsFavorite;
                        if (string.IsNullOrWhiteSpace(article.DetailText) && !string.IsNullOrWhiteSpace(dbArticle.DetailText) && dbArticle.Timespan == article.Timespan)
                        {
                            article.DetailText = dbArticle.DetailText;
                        }
                        if (article.DetailPicture == null)
                        {
                            var pictures = _db.Query<Picture>("SELECT * FROM Picture WHERE IdParent = ?", article.Id);
                            var detailPicture = pictures.FirstOrDefault(x => x.Type == PictypeType.Detail);
                            if (detailPicture != null)
                            {
                                article.DetailPicture = detailPicture;
                            }
                            var awardsPicture = pictures.FirstOrDefault(x => x.Type == PictypeType.Awards);
                            if (article.AwardsPicture == null && awardsPicture != null)
                            {
                                article.AwardsPicture = awardsPicture;
                            }
                        }
                    }
                }
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
        }

        #endregion

        #region Favorite

        public List<Article> GetFavorites(int skip, int take)
        {
            var lst = new List<Article>();
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                lst = _db.Query<Article>("SELECT * FROM Article WHERE IsFavorite = 1 ORDER BY Timespan DESC");
                if (lst != null && lst.Any())
                {
                    lst = lst.Skip(skip).Take(take).ToList();
                    foreach (var article in lst)
                    {
                        var pictures = GetPicturesForParent(article.Id);
                        article.DetailPicture = pictures.FirstOrDefault(x => x.Type == PictypeType.Detail);
                        article.PreviewPicture = pictures.FirstOrDefault(x => x.Type == PictypeType.Preview);
                        article.AwardsPicture = pictures.FirstOrDefault(x => x.Type == PictypeType.Awards);
                        var itemSections = GetItemSectionsForArticle(article.Id);
                        article.Sections = itemSections ?? new List<ItemSection>();
                        article.Rubrics = (GetRubrics(article.RubricsId)) ?? new List<Rubric>();
                        article.Authors = (GetAuthors(article.AuthorsId)) ?? new List<Author>();
                    }
                }
                if (lst == null) lst = new List<Article>();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
            return lst;
        }

        #endregion

        #region ItemSection

        public List<ItemSection> GetItemSectionsForArticle(int articleId)
        {
            var lst = new List<ItemSection>();
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                lst = _db.Query<ItemSection>("SELECT * FROM ItemSection WHERE IdArticle = ?", articleId);
                if (lst != null && lst.Any())
                {
                    foreach (var itemSection in lst)
                    {
                        var section = _db.Query<Section>("SELECT * FROM Section WHERE Id = ?", itemSection.IdSection).FirstOrDefault();
                        itemSection.Section = section;
                    }
                    lst = lst.Where(x => x.Section != null).OrderBy(x=>x.DepthLevel).ToList();
                }
                if (lst == null)
                {
                    lst = new List<ItemSection>();
                }
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
            return lst;
        }

        public void InsertItemSection(ItemSection model)
        {
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                _db.Insert(model, typeof(ItemSection));
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
        }

        public void InsertItemSections(IEnumerable<ItemSection> models)
        {
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                foreach (var model in models)
                {
                    _db.Insert(model, typeof(ItemSection));   
                }
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
        }

        public void DeleteItemSectionsForArticle(int articleId)
        {
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                _db.Execute("DELETE FROM ItemSection WHERE IdArticle = ?", articleId);
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
        }

        #endregion

        #region Picture

        public List<Picture> GetPicturesForParent(int parentId)
        {
            var lst = new List<Picture>();
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                lst = _db.Query<Picture>("SELECT * FROM Picture WHERE IdParent = ?", parentId);
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
            return lst;
        }

        public void InsertPicture(Picture model)
        {
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                _db.Insert(model, typeof(Picture));
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
        }

        public void DeletePicturesForParent(int parentId)
        {
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                _db.Execute("DELETE FROM Picture WHERE IdParent = ?", parentId);
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
        }

        public void DeletePicture(int id)
        {
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                _db.Execute("DELETE FROM Picture WHERE Id = ?", id);
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
        }

        #endregion

        #region Banner

        public void InsertBanner(Banner model)
        {
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                _db.Insert(model, typeof(Banner));
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
        }

        public void DeleteBanner(int id)
        {
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                _db.Execute("DELETE FROM Banner WHERE Id = ?", id);
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
        }

        public List<Banner> LoadBanners()
        {
            var lockTaken = false;
            var lst = new List<Banner>();
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                lst = _db.Table<Banner>().ToList();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
            return lst;
        }

        #endregion

        #region MagazineYear

        public void InsertMagazineYear(MagazineYear model)
        {
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                _db.InsertOrReplace(model, typeof(MagazineYear));
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
        }

        public void DeleteMagazineYear(int id)
        {
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                _db.Execute("DELETE FROM MagazineYear WHERE Id = ?", id);
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
        }

        public List<MagazineYear> LoadMagazineYears()
        {
            var lst = new List<MagazineYear>();
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                lst = _db.Table<MagazineYear>().ToList();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
            return lst;
        }

        public MagazineYear GetMagazineYearByYear(int year)
        {
            MagazineYear model = null;
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                model = _db.Query<MagazineYear>("SELECT * FROM MagazineYear WHERE Value = ?", year).FirstOrDefault();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
            return model;
        }

        public void UpdateMagazineYear(MagazineYear model)
        {
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                _db.Update(model, typeof(MagazineYear));
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
        }

        #endregion

        #region Magazine

        public void InsertMagazine(Magazine model)
        {
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                _db.InsertOrReplace(model, typeof(Magazine));
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
        }

        public Magazine GetMagazine(int id, bool withPicture)
        {
            Magazine model = null;
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                model = _db.Query<Magazine>("SELECT * FROM Magazine WHERE Id = ?", id).FirstOrDefault();
                if (model != null && withPicture)
                {
                    var picture = _db.Query<Picture>("SELECT * FROM Picture WHERE Type = 4 AND IdParent = ?", model.Id).FirstOrDefault();
                    if (picture != null)
                    {
                        model.PreviewPicture = picture;
                    }
                }
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
            return model;
        }

        public List<Magazine> GetMagazinesByYear(int year, bool withPicture)
        {
            var lst = new List<Magazine>();
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                lst = _db.Query<Magazine>("SELECT * FROM Magazine WHERE Year = ?", year);
                if (lst != null && lst.Any() && withPicture)
                {
                    foreach (var magazine in lst)
                    {
                        var picture = _db.Query<Picture>("SELECT * FROM Picture WHERE Type = 4 AND IdParent = ?", magazine.Id).FirstOrDefault();
                        if (picture != null)
                        {
                            magazine.PreviewPicture = picture;
                        }   
                    }
                }
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
            return lst;
        }

        public Magazine GetNewestSavedMagazine()
        {
            Magazine model = null;
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                model =
                    _db.Table<Magazine>().Where(x => x.InCache).OrderByDescending(x => x.ActiveFrom).FirstOrDefault();
                if (model != null)
                {
                    var picture = _db.Query<Picture>("SELECT * FROM Picture WHERE Type = 4 AND IdParent = ?", model.Id).FirstOrDefault();
                    if (picture != null)
                    {
                        model.PreviewPicture = picture;
                    }
                }
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
            return model;
        }

        public void DeleteMagazine(int id)
        {
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                _db.Execute("DELETE FROM Magazine WHERE Id = ?", id);
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
        }

        public void UpdateMagazine(Magazine model)
        {
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                _db.Update(model, typeof(Magazine));
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
        }

        public void SetAllMagazinePdfNotLoaded()
        {
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                _db.Execute("UPDATE Magazine SET [Exists] = 0 WHERE [Exists] = 1");
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
        }

        #endregion

        #region Rubric

        public List<Rubric> LoadAllRubrics()
        {
            var lockTaken = false;
            var lst = new List<Rubric>();
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                lst = _db.Table<Rubric>().Select(x => x).ToList();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
            return lst;
        }

        public Rubric GetRubric(int id)
        {
            var lockTaken = false;
            Rubric model = null;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                model = _db.Query<Rubric>("SELECT * FROM Rubric WHERE Id = ?", id).FirstOrDefault();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
            return model;
        }

        public List<Rubric> GetRubrics(string ids)
        {
            var lockTaken = false;
            var lst = new List<Rubric>();
            if (string.IsNullOrWhiteSpace(ids)) return lst;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                lst = _db.Query<Rubric>("SELECT * FROM Rubric WHERE Id IN (" + ids + ")");
            }
            catch(Exception){}
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
            return lst;
        }

        public void InsertNewRubrics(List<Rubric> oldLst, IEnumerable<Rubric> newLst)
        {
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                foreach (var rubric in newLst)
                {
                    var exists = oldLst.Count(x => x.Id == rubric.Id) > 0;
                    if (!exists)
                    {
                        _db.Insert(rubric, typeof(Rubric));
                    }
                }
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
        }

        public void InsertRubric(Rubric model)
        {
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                _db.InsertOrReplace(model, typeof(Rubric));
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
        }

        #endregion

        #region Author

        public List<Author> LoadAllAuthors()
        {
            var lockTaken = false;
            var lst = new List<Author>();
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                lst = _db.Table<Author>().Select(x => x).ToList();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
            return lst;
        }

        public Author GetAuthor(int id)
        {
            var lockTaken = false;
            Author model = null;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                model = _db.Query<Author>("SELECT * FROM Author WHERE Id = ?", id).FirstOrDefault();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
            return model;
        }

        public List<Author> GetAuthors(string ids)
        {
            var lockTaken = false;
            var lst = new List<Author>();
            if (string.IsNullOrWhiteSpace(ids)) return lst;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                lst = _db.Query<Author>("SELECT * FROM Author WHERE Id IN (" + ids + ")");
            }
            catch (Exception) { }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
            return lst;
        }

        public void InsertAuthor(Author model)
        {
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                _db.Insert(model, typeof(Author));
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
        }

        public void InsertNewAuthors(List<Author> oldLst, IEnumerable<Author> newLst)
        {
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                foreach (var author in newLst)
                {
                    var exists = oldLst.Count(x => x.Id == author.Id) > 0;
                    if (!exists)
                    {
                        _db.Insert(author, typeof(Author));
                    }
                }
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
        }

        #endregion

        #region Cache

        public ClearCacheResult ClearCache()
        {
            var result = new ClearCacheResult() {IsCacheClear = true};
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                var cacheLimit = ApplicationWorker.Settings.GetDbLimitSizeInMb();
                var favIds = _db.Table<Article>().Where(x => x.IsFavorite).ToList();
                var favIdsString = string.Join(",", favIds.Select(x => x.Id));
                _db.Execute("DELETE FROM ItemSection WHERE IdArticle NOT IN(" + favIdsString + ")");
                _db.Execute("DELETE FROM Picture WHERE Type != 3 AND Type != 4 AND IdParent NOT IN(" +
                            favIdsString + ")");
                _db.Execute("DELETE FROM Article WHERE IsReaded = 0 AND IsFavorite = 0");
                _db.Execute(
                    "UPDATE Article SET Video = NULL, RubricsId = NULL, AuthorsId = NULL, PreviewText = NULL, DetailText = NULL, SectionsId = NULL, Name = NULL, Url = NULL, Timespan = 0 WHERE IsReaded = 1 AND IsFavorite = 0");
                var magazines = _db.Table<Magazine>().Where(x => x.Exists).ToList();
                var magIds = string.Join(",", magazines.Select(x => x.Id));
                _db.Execute("DELETE FROM Magazine WHERE Id NOT IN(" + magIds + ")");
                _db.Execute("DELETE FROM Picture WHERE Type = 4");
                var size = GetDbSize();
                var cacheLimitBytes = cacheLimit*(1024*1024);
                if (size > cacheLimitBytes*0.7)
                {
                    var articles = _db.Query<Article>("SELECT FROM Article WHERE IsReaded = 1 AND IsFavorite = 0");
                    var deleteIds = string.Join(",", articles.Skip(100).Select(x => x.Id));
                    _db.Execute("DELETE FROM Article WHERE Id IN(" + deleteIds + ")");
                    var isStop = false;
                    while (!isStop)
                    {
                        size = GetDbSize();
                        if (size > cacheLimitBytes*0.7)
                        {
                            result.IsFavoriteDelete = true;
                            var favArticles = _db.Query<Article>("SELECT FROM Article WHERE IsFavorite = 1");
                            deleteIds = string.Join(",",
                                favArticles.Skip((int) (favArticles.Count()/2)).Select(x => x.Id));
                            _db.Execute("DELETE FROM Article WHERE Id IN(" + deleteIds + ")");
                        }
                        else
                        {
                            isStop = true;
                        }
                    }
                }
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
            return result;
        }

        public void DeleteFavorite()
        {
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                var favIds = _db.Table<Article>().Where(x => x.IsFavorite).ToList();
                var favIdsString = string.Join(",", favIds.Select(x => x.Id));
                _db.Execute("DELETE FROM ItemSection WHERE IdArticle IN(" + favIdsString + ")");
                _db.Execute("DELETE FROM Picture WHERE Type != 3 AND Type != 4 AND IdParent IN(" +
                            favIdsString + ")");
                _db.Execute(
                    "UPDATE Article SET Video = NULL, RubricsId = NULL, AuthorsId = NULL, PreviewText = NULL, DetailText = NULL, SectionsId = NULL, Name = NULL, Url = NULL, Timespan = 0, IsFavorite = 0 WHERE IsFavorite = 1");
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }   
        }

        public void SaveInCache(List<Article> articles)
        {
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                var articlesIds = string.Join(",", articles.Select(x => x.Id));
                _db.Execute("DELETE FROM ItemSection WHERE IdArticle IN (" + articlesIds + ")");
                _db.Execute("DELETE FROM Picture WHERE IdParent IN (" + articlesIds + ")");
                _db.Execute("DELETE FROM Article WHERE Id IN (" + articlesIds + ")");
                foreach (var article in articles)
                {
                    _db.Insert(article, typeof(Article));
                    if (article.ArticleType == ArticleType.Portal)
                    {
                        foreach (var section in article.Sections)
                        {
                            _db.Insert(section, typeof (ItemSection));
                        }
                    }
                    if (article.PreviewPicture != null)
                    {
                        _db.Insert(article.PreviewPicture, typeof(Picture));
                    }
                    if (article.DetailPicture != null)
                    {
                        _db.Insert(article.DetailPicture, typeof(Picture));
                    }
                    if (article.AwardsPicture != null)
                    {
                        _db.Insert(article.AwardsPicture, typeof(Picture));
                    }
                }
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
        }

        public long GetDbSize()
        {
            var folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var file = new FileInfo(Path.Combine(folder, DbName));
            var size = file.Length;
            return size;
        }

        #endregion

        #region Offline

        public List<Article> GetArticlesFromDb(int skip, int take, bool isTrends)
        {
            var lst = new List<Article>();
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                var blockQuery = "AND IdBlock != 30";
                if (isTrends)
                {
                    blockQuery = "AND IdBlock = 30";
                }
                lst = _db.Query<Article>("SELECT * FROM Article WHERE ArticleType = 2 " + blockQuery + " ORDER BY Timespan DESC");
                if (lst != null && lst.Any())
                {
                    lst = lst.Where(x => x.ArticleType == ArticleType.Portal).ToList();
                    lst = lst.Skip(skip).Take(take).Where(x => !string.IsNullOrWhiteSpace(x.PreviewText)).ToList();
                    foreach (var article in lst)
                    {
                        var pictures = GetPicturesForParent(article.Id);
                        article.DetailPicture = pictures.FirstOrDefault(x => x.Type == PictypeType.Detail);
                        article.PreviewPicture = pictures.FirstOrDefault(x => x.Type == PictypeType.Preview);
                        article.AwardsPicture = pictures.FirstOrDefault(x => x.Type == PictypeType.Awards);
                        var itemSections = GetItemSectionsForArticle(article.Id);
                        article.Sections = itemSections ?? new List<ItemSection>();
                        article.Authors = (GetAuthors(article.AuthorsId)) ?? new List<Author>();
                    }
                }
                if (lst == null) lst = new List<Article>();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
            return lst;
        }

        public List<Article> GetMagazineArticlesFromDb(int idMagazine)
        {
            var lst = new List<Article>();
            var lockTaken = false;
            try
            {
                Monitor.Enter(_lockObj, ref lockTaken);
                lst = _db.Query<Article>("SELECT * FROM Article WHERE ArticleType = 1 AND IdMagazine = ? ORDER BY Timespan DESC", idMagazine);
                if (lst != null && lst.Any())
                {
                    lst = lst.Where(x => x.ArticleType == ArticleType.Magazine).Where(x => !string.IsNullOrWhiteSpace(x.PreviewText)).ToList();
                    foreach (var article in lst)
                    {
                        var pictures = GetPicturesForParent(article.Id);
                        article.DetailPicture = pictures.FirstOrDefault(x => x.Type == PictypeType.Detail);
                        article.PreviewPicture = pictures.FirstOrDefault(x => x.Type == PictypeType.Preview);
                        article.AwardsPicture = pictures.FirstOrDefault(x => x.Type == PictypeType.Awards);
                        article.Rubrics = (GetRubrics(article.RubricsId)) ?? new List<Rubric>();
                        article.Authors = (GetAuthors(article.AuthorsId)) ?? new List<Author>();
                    }
                }
                if (lst == null) lst = new List<Article>();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_lockObj);
            }
            return lst;
        }

        #endregion

        #endregion

        #region Private Methods

        #endregion
    }

    public class ClearCacheResult
    {
        public bool IsCacheClear { get; set; }

        public bool IsFavoriteDelete { get; set; }
    }
}
