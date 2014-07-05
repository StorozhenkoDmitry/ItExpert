using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using ItExpert.Enum;
using ItExpert.Model;
using Newtonsoft.Json.Linq;

namespace ItExpert.ServiceLayer
{
    public class RemoteDataWorker : IDisposable
    {
        #region Fields

        private HttpConnector _activeConnector = null;

        public event EventHandler<ArticleEventArgs> NewsGetted;

        public event EventHandler<ArticleDetailEventArgs> ArticleDetailGetted;

        public event EventHandler<MagazinesPreviewEventArgs> MagazinesPriviewGetted;

        public event EventHandler<MagazineArticlesEventArgs> MagazineArticlesGetted;

        public event EventHandler<MagazineArticleDetailEventArgs> MagazineArticleDetailGetted;

        public event EventHandler<BannerEventArgs> BannerGetted;

        public event EventHandler<CssEventArs> CssGetted; 

        #endregion

        #region Private Methods

        private void NewsTransmissed(object sender, HttpConnectorEventArgs eventArgs)
        {
            var error = eventArgs.Error;
            _activeConnector.DataReceived -= NewsTransmissed;
            _activeConnector = null;
            if (eventArgs.Abort)
            {
                var handler = Interlocked.CompareExchange(ref NewsGetted, null, null);
                if (handler != null)
                {
                    handler(this, new ArticleEventArgs() { Abort = true});
                }
                return;
            }
            var articleEventArgs = new ArticleEventArgs();
            try
            {
                if (!error)
                {
                    var data = eventArgs.Data;
                    var settings = (Settings) eventArgs.State;
                    var json = JObject.Parse(data);
                    var status = (string) json["meta"]["status"];
                    if (status == "OK")
                    {
                        var blocksJson = json["data"]["IBLOCK"]["LIST"];
                        var blocks = new List<Block>();
                        if (blocksJson != null)
                        {
                            var childs = blocksJson.Children();
                            blocks = GetBlocksFromJson(childs);
                        }
                        else
                        {
                            blocksJson = json["data"]["IBLOCK"];
                            var id = int.Parse((string) blocksJson["ID"]);
                            var code = (string) blocksJson["CODE"];
                            var name = (string) blocksJson["NAME"];
                            var block = new Block()
                            {
                                Id = id,
                                Code = code,
                                Name = name,
                            };
                            blocks.Add(block);
                        }
                        var articles = new List<Article>();
                        if (json["data"]["ITEMS"] != null && json["data"]["ITEMS"].Type != JTokenType.Null)
                        {
                            var articleJson = json["data"]["ITEMS"].Children();
                            articles = GetArticleFromJson(articleJson, settings, blocks);
                        }
                        var authors = articles.SelectMany(x => x.Authors).Distinct(new AuthorComparer()).ToList();
                        var sections =
                            articles.SelectMany(x => x.Sections)
                                .Select(x => x.Section)
                                .Distinct(new SectionComparer())
                                .ToList();
                        articleEventArgs.Blocks = blocks;
                        articleEventArgs.Articles = articles;
                        articleEventArgs.Sections = sections;
                        articleEventArgs.Authors = authors;
                    }
                    else
                    {
                        if (json["error"]["ERROR_DATA_NOT_FOUND"] != null &&
                            json["error"]["ERROR_DATA_NOT_FOUND"].Type != JTokenType.Null)
                        {
                            articleEventArgs.Articles = new List<Article>();
                            articleEventArgs.Blocks = new List<Block>();
                            articleEventArgs.Sections = new List<Section>();
                            articleEventArgs.Authors = new List<Author>();
                        }
                        else
                        {
                            error = true;   
                        }
                    }
                }
            }
            catch (Exception)
            {
                error = true;
            }
            finally
            {
                articleEventArgs.Error = error;
                var handler = Interlocked.CompareExchange(ref NewsGetted, null, null);
                if (handler != null)
                {
                    handler(this, articleEventArgs);
                }
            }
        }

        private void CssTransmissed(object sender, HttpConnectorEventArgs eventArgs)
        {
            var error = eventArgs.Error;
            _activeConnector.DataReceived -= CssTransmissed;
            _activeConnector = null;
            if (eventArgs.Abort)
            {
                var handler = Interlocked.CompareExchange(ref CssGetted, null, null);
                if (handler != null)
                {
                    handler(this, new CssEventArs() { Abort = true });
                }
                return;
            }
            var cssEventArs = new CssEventArs();
            try
            {
                if (!error)
                {
                    var data = eventArgs.Data;
                    cssEventArs.Css = data;
                }
            }
            catch (Exception)
            {
                error = true;
            }
            finally
            {
                cssEventArs.Error = error;
                var handler = Interlocked.CompareExchange(ref CssGetted, null, null);
                if (handler != null)
                {
                    handler(this, cssEventArs);
                }
            }
        }

        private void ArticleDetailTransmissed(object sender, HttpConnectorEventArgs eventArgs)
        {
            var error = eventArgs.Error;
            _activeConnector.DataReceived -= ArticleDetailTransmissed;
            _activeConnector = null;
            if (eventArgs.Abort)
            {
                var handler = Interlocked.CompareExchange(ref ArticleDetailGetted, null, null);
                if (handler != null)
                {
                    handler(this, new ArticleDetailEventArgs() {Abort = true});
                }
                return;
            }
            var articleDetailEventArgs = new ArticleDetailEventArgs();
            try
            {
                if (!error)
                {
                    var data = eventArgs.Data;
                    var settings = (Settings) eventArgs.State;
                    var json = JObject.Parse(data);
                    var status = (string) json["meta"]["status"];
                    if (status == "OK")
                    {
                        var article = new Article() {Authors = new List<Author>()};
                        var id = int.Parse((string) json["data"]["ITEMS"]["ID"]);
                        var detailText = (string) json["data"]["ITEMS"]["DETAIL_TEXT"];
                        if (settings.LoadImages)
                        {
                            var picture = GetPicture(json["data"]["ITEMS"]["DETAIL_PICTURE"]);
                            if (picture != null)
                            {
                                picture.IdParent = id;
                                picture.Type = PictypeType.Detail;
                                article.DetailPicture = picture;
                            }
                        }
                        if (json["data"]["ITEMS"]["AUTHORS"] != null && json["data"]["ITEMS"]["AUTHORS"].Type != JTokenType.Null)
                        {
                            var authorsArr = json["data"]["ITEMS"]["AUTHORS"].Children();
                            foreach (var author in authorsArr)
                            {
                                var authorValue = ((JProperty)author).Value;
                                var authorId = int.Parse((string)authorValue["ID"]);
                                var authorName = (string)authorValue["NAME"];
                                var authorModel = new Author()
                                {
                                    Id = authorId,
                                    Name = authorName
                                };
                                article.Authors.Add(authorModel);
                            }
                            article.AuthorsId = string.Join(",", article.Authors.Select(x => x.Id));
                        }
                        if (json["data"]["ITEMS"]["AWARDS"] != null &&
                            json["data"]["ITEMS"]["AWARDS"].Type != JTokenType.Null)
                        {
                            var awardsPicture = GetPicture(json["data"]["ITEMS"]["AWARDS"]);
                            if (awardsPicture != null)
                            {
                                awardsPicture.IdParent = id;
                                awardsPicture.Type = PictypeType.Awards;
                            }
                            article.AwardsPicture = awardsPicture;
                        }
                        if (json["data"]["ITEMS"]["VIDEO"] != null &&
                            json["data"]["ITEMS"]["VIDEO"].Type != JTokenType.Null)
                        {
                            var video = (string) json["data"]["ITEMS"]["VIDEO"];
                            article.Video = video;
                        }
                        article.Id = id;
                        article.DetailText = detailText;
                        articleDetailEventArgs.Article = article;
                        article.ArticleType = ArticleType.Portal;
                    }
                    else
                    {
                        error = true;
                    }
                }
            }
            catch (Exception)
            {
                error = true;
            }
            finally
            {
                articleDetailEventArgs.Error = error;
                var handler = Interlocked.CompareExchange(ref ArticleDetailGetted, null, null);
                if (handler != null)
                {
                    handler(this, articleDetailEventArgs);
                }
            }
        }

        private void MagazinesPreviewTransmissed(object sender, HttpConnectorEventArgs eventArgs)
        {
            var error = eventArgs.Error;
            _activeConnector.DataReceived -= MagazinesPreviewTransmissed;
            _activeConnector = null;
            if (eventArgs.Abort)
            {
                var handler = Interlocked.CompareExchange(ref MagazinesPriviewGetted, null, null);
                if (handler != null)
                {
                    handler(this, new MagazinesPreviewEventArgs() {Abort = true});
                }
                return;
            }
            var magazinesPreviewEventArgs = new MagazinesPreviewEventArgs();
            try
            {
                if (!error)
                {
                    var data = eventArgs.Data;
                    var settings = (Settings) eventArgs.State;
                    var json = JObject.Parse(data);
                    var status = (string) json["meta"]["status"];
                    if (status == "OK")
                    {
                        var years = new List<MagazineYear>();
                        var yearsJson = (JArray) (json["data"]["YEARS"]);
                        if (yearsJson.HasValues)
                        {
                            years =
                                yearsJson.ToList()
                                    .Select(x => int.Parse((string) (((JValue) x).Value)))
                                    .Select(x => new MagazineYear() {Value = x})
                                    .ToList();
                        }
                        magazinesPreviewEventArgs.Years = years;
                        var magazinesJson = json["data"]["ITEMS"];
                        var magazines = GetMagazinesPreviewFromJson(magazinesJson, settings);
                        magazinesPreviewEventArgs.Magazines = magazines;
                    }
                    else
                    {
                        error = true;
                    }
                }
            }
            catch (Exception)
            {
                error = true;
            }
            finally
            {
                magazinesPreviewEventArgs.Error = error;
                var handler = Interlocked.CompareExchange(ref MagazinesPriviewGetted, null, null);
                if (handler != null)
                {
                    handler(this, magazinesPreviewEventArgs);
                }
            }
        }

        private void MagazineArticlesTransmissed(object sender, HttpConnectorEventArgs eventArgs)
        {
            var error = eventArgs.Error;
            _activeConnector.DataReceived -= MagazineArticlesTransmissed;
            _activeConnector = null;
            if (eventArgs.Abort)
            {
                var handler = Interlocked.CompareExchange(ref MagazineArticlesGetted, null, null);
                if (handler != null)
                {
                    handler(this, new MagazineArticlesEventArgs(){Abort = true});
                }
                return;
            }
            var magazineArticlesEventArgs = new MagazineArticlesEventArgs();
            try
            {
                if (!error)
                {
                    var data = eventArgs.Data;
                    var settings = (Settings) eventArgs.State;
                    var json = JObject.Parse(data);
                    var status = (string) json["meta"]["status"];
                    if (status == "OK")
                    {
                        var articles = new List<Article>();
                        if (json["data"]["ITEMS"] != null && json["data"]["ITEMS"].Type != JTokenType.Null)
                        {
                            var articlesJson = json["data"]["ITEMS"];
                            articles = GetMagazineArticlesFromJson(articlesJson, settings);
                        }
                        magazineArticlesEventArgs.Articles = articles;
                        var rubrics = articles.SelectMany(x => x.Rubrics).Distinct(new RubricComparer()).ToList();
                        magazineArticlesEventArgs.Rubrics = rubrics;
                        var authors = articles.SelectMany(x => x.Authors).Distinct(new AuthorComparer()).ToList();
                        magazineArticlesEventArgs.Authors = authors;
                    }
                    else
                    {
                        if (json["error"]["ERROR_DATA_NOT_FOUND"] != null &&
                            json["error"]["ERROR_DATA_NOT_FOUND"].Type != JTokenType.Null)
                        {
                            magazineArticlesEventArgs.Articles = new List<Article>();
                            magazineArticlesEventArgs.Rubrics = new List<Rubric>();
                            magazineArticlesEventArgs.Authors = new List<Author>();
                        }
                        else
                        {
                            error = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                error = true;
            }
            finally
            {
                magazineArticlesEventArgs.Error = error;
                var handler = Interlocked.CompareExchange(ref MagazineArticlesGetted, null, null);
                if (handler != null)
                {
                    handler(this, magazineArticlesEventArgs);
                }
            }
        }

        private void MagazineArticleDetailTransmissed(object sender, HttpConnectorEventArgs eventArgs)
        {
            var error = eventArgs.Error;
            _activeConnector.DataReceived -= MagazineArticleDetailTransmissed;
            _activeConnector = null;
            if (eventArgs.Abort)
            {
                var handler = Interlocked.CompareExchange(ref MagazineArticleDetailGetted, null, null);
                if (handler != null)
                {
                    handler(this, new MagazineArticleDetailEventArgs() {Abort = true});
                }
                return;
            }
            var magazineArticleDetailEventArgs = new MagazineArticleDetailEventArgs();
            try
            {
                if (!error)
                {
                    var data = eventArgs.Data;
                    var settings = (Settings) eventArgs.State;
                    var json = JObject.Parse(data);
                    var status = (string) json["meta"]["status"];
                    if (status == "OK")
                    {
                        var article = new Article() {Authors = new List<Author>()};
                        var id = int.Parse((string) json["data"]["ITEMS"]["ID"]);
                        var detailText = (string) json["data"]["ITEMS"]["DETAIL_TEXT"];
                        if (settings.LoadImages)
                        {
                            var picture = GetPicture(json["data"]["ITEMS"]["DETAIL_PICTURE"]);
                            if (picture != null)
                            {
                                picture.IdParent = id;
                                picture.Type = PictypeType.Detail;
                                article.DetailPicture = picture;
                            }
                        }
                        if (json["data"]["ITEMS"]["AUTHORS"] != null && json["data"]["ITEMS"]["AUTHORS"].Type != JTokenType.Null)
                        {
                            var authorsArr = json["data"]["ITEMS"]["AUTHORS"].Children();
                            foreach (var author in authorsArr)
                            {
                                var authorValue = ((JProperty)author).Value;
                                var authorId = int.Parse((string)authorValue["ID"]);
                                var authorName = (string)authorValue["NAME"];
                                var authorModel = new Author()
                                {
                                    Id = authorId,
                                    Name = authorName
                                };
                                article.Authors.Add(authorModel);
                            }
                            article.AuthorsId = string.Join(",", article.Authors.Select(x => x.Id));
                        }
                        if (json["data"]["ITEMS"]["AWARDS"] != null &&
                            json["data"]["ITEMS"]["AWARDS"].Type != JTokenType.Null)
                        {
                            var awardsPicture = GetPicture(json["data"]["ITEMS"]["AWARDS"]);
                            if (awardsPicture != null)
                            {
                                awardsPicture.IdParent = id;
                                awardsPicture.Type = PictypeType.Awards;
                            }
                            article.AwardsPicture = awardsPicture;
                        }
                        if (json["data"]["ITEMS"]["VIDEO"] != null &&
                            json["data"]["ITEMS"]["VIDEO"].Type != JTokenType.Null)
                        {
                            var video = (string)json["data"]["ITEMS"]["VIDEO"];
                            article.Video = video;
                        }
                        article.Id = id;
                        article.DetailText = detailText;
                        article.ArticleType = ArticleType.Magazine;
                        magazineArticleDetailEventArgs.Article = article;
                    }
                    else
                    {
                        error = true;
                    }
                }
            }
            catch (Exception)
            {
                error = true;
            }
            finally
            {
                magazineArticleDetailEventArgs.Error = error;
                var handler = Interlocked.CompareExchange(ref MagazineArticleDetailGetted, null, null);
                if (handler != null)
                {
                    handler(this, magazineArticleDetailEventArgs);
                }
            }
        }

        private void BannerTransmissed(object sender, HttpConnectorEventArgs eventArgs)
        {
            var error = eventArgs.Error;
            _activeConnector.DataReceived -= BannerTransmissed;
            _activeConnector = null;
            if (eventArgs.Abort)
            {
                var handler = Interlocked.CompareExchange(ref BannerGetted, null, null);
                if (handler != null)
                {
                    handler(this, new BannerEventArgs() {Abort = true});
                }
                return;
            }
            var bannerEventArgs = new BannerEventArgs();
            try
            {
                if (!error)
                {
                    var data = eventArgs.Data;
                    var json = JObject.Parse(data);
                    var status = (string) json["meta"]["status"];
                    if (status == "OK")
                    {
                        var bannersJArray = (JArray) json["data"];
                        var banners = GetBannersFromJson(bannersJArray);
                        foreach (var banner in banners)
                        {
                            banner.ScreenWidth = ((Settings) eventArgs.State).ScreenWidth;
                        }
                        bannerEventArgs.Banners = banners;
                    }
                    else
                    {
                        error = true;
                    }
                }
            }
            catch (Exception)
            {
                error = true;
            }
            finally
            {
                bannerEventArgs.Error = error;
                var handler = Interlocked.CompareExchange(ref BannerGetted, null, null);
                if (handler != null)
                {
                    handler(this, bannerEventArgs);
                }
            }
        }

        private List<Block> GetBlocksFromJson(IEnumerable<JToken> tokens)
        {
            var lst = new List<Block>();
            foreach (var item in tokens)
            {
                var value = ((JProperty)item).Value;
                var id = int.Parse((string)value["ID"]);
                var code = (string)value["CODE"];
                var name = (string)value["NAME"];
                var block = new Block()
                {
                    Id = id,
                    Code = code,
                    Name = name,
                };
                lst.Add(block);
            }
            return lst;
        }

        private List<Article> GetArticleFromJson(IEnumerable<JToken> tokens, Settings settings, List<Block> lstBlocks)
        {
            var lst = new List<Article>();
            var lstSections = new List<Section>();
            var lstAuthors = new List<Author>();
            foreach (var item in tokens)
            {
                var value = ((JProperty)item).Value;
                var id = int.Parse((string) value["ID"]);
                var idBlock = int.Parse((string) value["IBLOCK_ID"]);
                var name = (string) value["NAME"];
                var previewText = (string) value["PREVIEW_TEXT"];
                var activeFrom = DateTime.Now;
                if (value["ACTIVE_FROM"] != null && value["ACTIVE_FROM"].Type != JTokenType.Null)
                {
                    if (!string.IsNullOrEmpty((string)value["ACTIVE_FROM"]))
                    {
                        activeFrom = DateTime.ParseExact((string)value["ACTIVE_FROM"], "yyyy-MM-dd HH:mm:ss", null);
                    }
                }
                var dateTime = (long) value["DATETIME"];
                var url = (string) value["ORIGINAL_PAGE_URL"];
                var detailText = string.Empty;
                Picture previewPicture = null;
                Picture detailPicture = null;
                Picture awardsPicture = null;
                if (settings.LoadDetails)
                {
                    detailText = (string)value["DETAIL_TEXT"];
                    awardsPicture = GetPicture(value["AWARDS"]);
                    if (awardsPicture != null)
                    {
                        awardsPicture.IdParent = id;
                        awardsPicture.Type = PictypeType.Awards;
                    }
                }
                if (settings.LoadImages)
                {
                    previewPicture = GetPicture(value["PREVIEW_PICTURE"]);
                    if (previewPicture != null)
                    {
                        previewPicture.IdParent = id;
                        previewPicture.Type = PictypeType.Preview;
                    }
                    if (settings.LoadDetails)
                    {
                        detailPicture = GetPicture(value["DETAIL_PICTURE"]);
                        if (detailPicture != null)
                        {
                            detailPicture.IdParent = id;
                            detailPicture.Type = PictypeType.Detail;   
                        }
                    }
                }
                var article = new Article()
                {
                    Sections = new List<ItemSection>(),
                    ActiveFrom = activeFrom,
                    Block = lstBlocks.FirstOrDefault(x=>x.Id == idBlock),
                    IdBlock = idBlock,
                    PreviewPicture = previewPicture,
                    DetailPicture = detailPicture,
                    AwardsPicture = awardsPicture,
                    DetailText = detailText,
                    PreviewText = previewText,
                    Name = name,
                    Id = id,
                    IsReaded = false,
                    Timespan = dateTime,
                    ArticleType = ArticleType.Portal,
                    Url = url,
                    Authors = new List<Author>()
                };
                if (value["AUTHORS"] != null && value["AUTHORS"].Type != JTokenType.Null)
                {
                    var authorsArr = value["AUTHORS"].Children();
                    foreach (var author in authorsArr)
                    {
                        var authorValue = ((JProperty) author).Value;
                        var authorId = int.Parse((string)authorValue["ID"]);
                        if (lstAuthors.FirstOrDefault(x => x.Id == authorId) == null)
                        {
                            var authorName = (string) authorValue["NAME"];
                            var authorModel = new Author()
                            {
                                Id = authorId,
                                Name = authorName
                            };
                            lstAuthors.Add(authorModel);
                        }
                        var exstAuthor = lstAuthors.FirstOrDefault(x => x.Id == authorId);
                        if (exstAuthor != null)
                        {
                            article.Authors.Add(exstAuthor);
                        }
                    }
                    article.AuthorsId = string.Join(",", article.Authors.Select(x=>x.Id));
                }
                if (value["SECTIONS"] != null && value["SECTIONS"].Type != JTokenType.Null)
                {
                    var sectionsArr = value["SECTIONS"].Children();
                    foreach (var sect in sectionsArr)
                    {
                        var sectValue = ((JProperty) sect).Value;
                        var sectId = int.Parse((string) sectValue["ID"]);
                        if (lstSections.FirstOrDefault(x => x.Id == sectId) == null)
                        {
                            var sectCode = (string) sectValue["CODE"];
                            var sectName = (string) sectValue["NAME"];
                            var section = new Section()
                            {
                                Id = sectId,
                                Code = sectCode,
                                Name = sectName,
                            };
                            lstSections.Add(section);
                        }
                        var depthLevel = int.Parse((string) sectValue["DEPTH_LEVEL"]);
                        var extSection = lstSections.FirstOrDefault(x => x.Id == sectId);
                        if (extSection != null)
                        {
                            var itemSection = new ItemSection()
                            {
                                DepthLevel = depthLevel,
                                IdArticle = article.Id,
                                IdSection = sectId,
                                Section = extSection
                            };
                            article.Sections.Add(itemSection);
                        }
                    }
                    article.SectionsId = string.Join(",",
                        article.Sections.Select(x => x.Section.Id).Distinct().ToArray());
                }
                if (value["VIDEO"] != null && value["VIDEO"].Type != JTokenType.Null)
                {
                    var video = (string)value["VIDEO"];
                    article.Video = video;
                }
                lst.Add(article);
            }
            return lst;
        }

        private List<Magazine> GetMagazinesPreviewFromJson(IEnumerable<JToken> tokens, Settings settings)
        {
            var lst = new List<Magazine>();
            foreach (var item in tokens)
            {
                var value = ((JProperty)item).Value;
                var id = int.Parse((string)value["ID"]);
                var code = int.Parse((string)value["CODE"]);
                var name = (string)value["NAME"];
                var activeFrom = DateTime.Now;
                if (value["ACTIVE_FROM"] != null && value["ACTIVE_FROM"].Type != JTokenType.Null)
                {
                    if (!string.IsNullOrEmpty((string)value["ACTIVE_FROM"]))
                    {
                        activeFrom = DateTime.ParseExact((string)value["ACTIVE_FROM"], "yyyy-MM-dd HH:mm:ss", null);
                    }
                }
                var year = int.Parse((string)value["YEAR"]);
                Picture previewPicture = null;
                    previewPicture = GetPicture(value["PREVIEW_PICTURE"]);
                if (previewPicture != null)
                {
                    previewPicture.IdParent = id;
                    previewPicture.Type = PictypeType.Magazine;
                }
                string pdf = null;
                var size = -1;
                if (value["PDF_FILE"] != null && value["PDF_FILE"].Type != JTokenType.Null)
                {
                    pdf = (string)value["PDF_FILE"]["SRC"];
                    size = (int)value["PDF_FILE"]["SIZE"];
                }
                var magazine = new Magazine()
                {
                    ActiveFrom = activeFrom,
                    Code = code,
                    Id = id,
                    Name = name,
                    PdfFileSrc = pdf,
                    PreviewPicture = previewPicture,
                    Year = year,
                    PdfFileSize = size
                };
                lst.Add(magazine);
            }
            return lst;
        }

        private List<Article> GetMagazineArticlesFromJson(IEnumerable<JToken> tokens, Settings settings)
        {
            var lst = new List<Article>();
            var lstAuthors = new List<Author>();
            foreach (var item in tokens)
            {
                JToken value = null;
                if (item.Type == JTokenType.Property)
                {
                    value = ((JProperty)item).Value;       
                }
                if (item.Type == JTokenType.Object)
                {
                    value = item.Value<JToken>();
                }
                var id = int.Parse((string)value["ID"]);
                var idBlock = (!string.IsNullOrEmpty((string) value["IBLOCK_ID"]))
                    ? int.Parse((string) value["IBLOCK_ID"])
                    : -1;
                var idSection = -1;
                if (value["SECTION_ID"] != null && value["SECTION_ID"].Type != JTokenType.Null)
                {
                    if (!string.IsNullOrEmpty((string)value["SECTION_ID"]))
                    {
                        idSection = int.Parse((string)value["SECTION_ID"]);
                    }
                }
                var name = (string)value["NAME"];
                var previewText = (string)value["PREVIEW_TEXT"];
                var activeFrom = DateTime.Now;
                if (value["ACTIVE_FROM"] != null && value["ACTIVE_FROM"].Type != JTokenType.Null)
                {
                    if (!string.IsNullOrEmpty((string)value["ACTIVE_FROM"]))
                    {
                        activeFrom = DateTime.ParseExact((string)value["ACTIVE_FROM"], "yyyy-MM-dd HH:mm:ss", null);
                    }
                }
                var dateTime = (long)value["DATETIME"];
                var detailText = string.Empty;
                var url = (string)value["ORIGINAL_PAGE_URL"];
                var sort = 0;
                if (value["SORT"] != null && value["SORT"].Type != JTokenType.Null)
                {
                    sort = int.Parse((string) value["SORT"]);
                }
                Picture previewPicture = null;
                Picture detailPicture = null;
                Picture awardsPicture = null;
                if (settings.LoadDetails)
                {
                    detailText = (string)value["DETAIL_TEXT"];
                    awardsPicture = GetPicture(value["AWARDS"]);
                    if (awardsPicture != null)
                    {
                        awardsPicture.IdParent = id;
                        awardsPicture.Type = PictypeType.Awards;
                    }
                }
                if (settings.LoadImages)
                {
                    previewPicture = GetPicture(value["PREVIEW_PICTURE"]);
                    if (previewPicture != null)
                    {
                        previewPicture.IdParent = id;
                        previewPicture.Type = PictypeType.Preview;   
                    }
                    if (settings.LoadDetails)
                    {
                        detailPicture = GetPicture(value["DETAIL_PICTURE"]);
                        if (detailPicture != null)
                        {
                            detailPicture.IdParent = id;
                            detailPicture.Type = PictypeType.Detail;
                        }
                    }
                }
                var article = new Article()
                {
                    ActiveFrom = activeFrom,
                    IdBlock = idBlock,
                    IdSection = idSection,
                    PreviewPicture = previewPicture,
                    DetailPicture = detailPicture,
                    AwardsPicture = awardsPicture,
                    DetailText = detailText,
                    PreviewText = previewText,
                    Name = name,
                    Rubrics = new List<Rubric>(),
                    Id = id,
                    IsReaded = false,
                    Timespan = dateTime,
                    ArticleType = ArticleType.Magazine,
                    Sort = sort,
                    Url = url,
                    Authors = new List<Author>()
                };
                if (value["AUTHORS"] != null && value["AUTHORS"].Type != JTokenType.Null)
                {
                    var authorsArr = value["AUTHORS"].Children();
                    foreach (var author in authorsArr)
                    {
                        var authorValue = ((JProperty)author).Value;
                        var authorId = int.Parse((string)authorValue["ID"]);
                        if (lstAuthors.FirstOrDefault(x => x.Id == authorId) == null)
                        {
                            var authorName = (string)authorValue["NAME"];
                            var authorModel = new Author()
                            {
                                Id = authorId,
                                Name = authorName
                            };
                            lstAuthors.Add(authorModel);
                        }
                        var exstAuthor = lstAuthors.FirstOrDefault(x => x.Id == authorId);
                        if (exstAuthor != null)
                        {
                            article.Authors.Add(exstAuthor);
                        }
                    }
                    article.AuthorsId = string.Join(",", article.Authors.Select(x => x.Id));
                }
                if (value["SECTIONS"] != null && value["SECTIONS"].Type != JTokenType.Null)
                {
                    var rubricCode = (string) value["SECTIONS"]["RUBRIC_CODE"];
                    var rubricName = (string) value["SECTIONS"]["NAME"];
                    var rubricId = int.Parse((string) value["SECTIONS"]["RUBRIC_ID"]);
                    article.Rubrics.Add(new Rubric()
                    {
                        Code = rubricCode,
                        Name = rubricName,
                        Id = rubricId
                    });
                    article.RubricsId = string.Join(",", article.Rubrics.Select(x => x.Id));
                }
                if (value["VIDEO"] != null && value["VIDEO"].Type != JTokenType.Null)
                {
                    var video = (string)value["VIDEO"];
                    article.Video = video;
                }
                lst.Add(article);
            }
            return lst;
        }

        private List<Banner> GetBannersFromJson(JArray array)
        {
            var lst = new List<Banner>();
            foreach (var item in array)
            {
                var id = int.Parse((string)item["ID"]);
                var url = (string) item["URL"];
                var picture = GetPicture(item["IMAGE"]);
                if (picture != null)
                {
                    picture.IdParent = id;
                    picture.Type = PictypeType.Banner;
                }
                var banner = new Banner()
                {
                    Id = id,
                    Url = url,
                    Picture = picture
                };
                lst.Add(banner);
            }
            return lst;
        }

        private Picture GetPicture(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null) return null;
            var width = -1;
            var widthToken = token["WIDTH"];
            if (widthToken.Type == JTokenType.String)
            {
                width = int.Parse((string)widthToken);
            }
            else if (widthToken.Type == JTokenType.Integer)
            {
                width = (int)widthToken;
            }
            var height = -1;
            var heightToken = token["HEIGHT"];
            if (heightToken.Type == JTokenType.String)
            {
                height = int.Parse((string)heightToken);
            }
            else if (heightToken.Type == JTokenType.Integer)
            {
                height = (int)heightToken;
            }
            var src = (string)token["SRC"];
            var typeToken = token["TYPE"] ?? token["CONTENT_TYPE"];
            var type = string.Empty;
            if (typeToken != null && typeToken.Type != JTokenType.Null)
            {
                type = ((string) typeToken).Remove(0, 6);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(src))
                {
                    var dotIndex = src.LastIndexOf(".", StringComparison.InvariantCulture);
                    if (dotIndex != -1)
                    {
                        type = src.Substring(dotIndex + 1);
                    }
                    var exts = new[] {"jpeg", "jpg", "png", "gif", "bmp"};
                    if (!exts.Contains(type)) return null;
                }
                else
                {
                    return null;
                }
            }
            var extension = PictureExtension.Jpeg;
            if (type == "jpg")
            {
                extension = PictureExtension.Jpg;
            }
            else if (type == "png")
            {
                extension = PictureExtension.Png;
            }
            else if (type == "gif")
            {
                extension = PictureExtension.Gif;
            }
            else if (type == "bmp")
            {
                extension = PictureExtension.Bmp;
            }
            var encode = (string)token["ENCODED"];
            var picture = new Picture()
            {
                Data = encode,
                Extension = extension,
                Src = src,
                Width = width,
                Height = height
            };
            return picture;
        }

        private string GetQueryString(ServerParameters parameters)
        {
            var sb = new StringBuilder();
            sb.Append("action=" + parameters.Action + "&");
            sb.Append("object=" + parameters.GetDataObjectValue() + "&");
            var loadImages = (parameters.LoadImages) ? "1" : "0";
            sb.Append("load_images=" + loadImages + "&");
            var loadDetail = (parameters.LoadDetails) ? "1" : "0";
            sb.Append("LOAD_DETAIL=" + loadDetail + "&");
            sb.Append("screen_resolution=" + parameters.GetScreenResolutionValue() + "&");
            sb.Append("screen_width=" + parameters.GetScreenWidthValue() + "&");
            if (parameters.IdMagazineBlock != -1)
            {
                sb.Append("MAGAZINES_IBLOCK_ID=" + parameters.IdMagazineBlock + "&");
            }
            if (!string.IsNullOrWhiteSpace(parameters.RubricCode))
            {
                sb.Append("RUBRIC_CODE=" + parameters.RubricCode + "&");
            }
            if (parameters.IdRubric != -1)
            {
                sb.Append("RUBRIC_ID=" + parameters.IdRubric + "&");
            }
            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                sb.Append("search=" + parameters.Search + "&");
            }
            if (parameters.IdIblock != -1)
            {
                sb.Append("IBLOCK_ID=" + parameters.IdIblock + "&");
            }
            if (parameters.IdSection != -1)
            {
                sb.Append("SECTION_ID=" + parameters.IdSection + "&");
            }
            if (parameters.IdElement != -1)
            {
                sb.Append("ELEMENT_ID=" + parameters.IdElement + "&");
            }
            if (parameters.IdAuthor != -1)
            {
                sb.Append("AUTHOR=" + parameters.IdAuthor + "&");
            }
            if (parameters.LastElementDateTime != -1)
            {
                sb.Append("LAST_ELEMENT_DATETIME=" + parameters.LastElementDateTime + "&");
            }
            if (parameters.FirstElementDateTime != -1)
            {
                sb.Append("FIRST_ELEMENT_DATETIME=" + parameters.FirstElementDateTime + "&");
            }
            if (parameters.DataObject == DataObject.Pdf && !string.IsNullOrWhiteSpace(parameters.PdfSrc))
            {
                sb.Append("SRC=" + parameters.PdfSrc + "&");
            }
            if (parameters.DataObject == DataObject.Magazines && parameters.MagazinesYear != -1)
            {
                sb.Append("YEAR=" + parameters.MagazinesYear + "&");
            }
            sb.Length = sb.Length - 1;
            var query = sb.ToString();
            return query;
        }

        #endregion

        #region Public Methods

        public void Dispose()
        {
            NewsGetted = null;
            ArticleDetailGetted = null;
            MagazinesPriviewGetted = null;
            MagazineArticlesGetted = null;
            MagazineArticleDetailGetted = null;
            BannerGetted = null;
            if (_activeConnector != null)
            {
                _activeConnector.Dispose();
                _activeConnector = null;
            }
        }

        public void BeginGetCss()
        {
             Abort();
            var error = false;
            try
            {
                _activeConnector = new HttpConnector();
                _activeConnector.DataReceived += CssTransmissed;
            }
            catch (Exception)
            {
                error = true;
            }
            finally
            {
                if (!error)
                {
                    _activeConnector.GetCss();
                }
                if (error)
                {
                    var handler = Interlocked.CompareExchange(ref CssGetted, null, null);
                    if (handler != null)
                    {
                        handler(this, new CssEventArs() { Error = true });
                    }
                }
            }
        }

        public void BeginGetNews(Settings settings, long lastDateTime, long firstDateTime, int idBlock, int idSection, int idAuthor, string search)
        {
            Abort();
            var error = false;
            var query = string.Empty;
            try
            {
                _activeConnector = new HttpConnector();
                _activeConnector.DataReceived += NewsTransmissed;
                var param = new ServerParameters();
                param.DataObject = DataObject.News;
                param.LoadDetails = settings.LoadDetails;
                param.LoadImages = settings.LoadImages;
                param.ScreenResolution = settings.ScreenResolution;
                param.ScreenWidth = settings.ScreenWidth;
                param.LastElementDateTime = lastDateTime;
                param.FirstElementDateTime = firstDateTime;
                param.IdIblock = idBlock;
                param.IdSection = idSection;
                param.IdAuthor = idAuthor;
                param.Search = search;
                query = GetQueryString(param);
            }
            catch (Exception)
            {
                error = true;
            }
            finally
            {
                if (!error)
                {
                    _activeConnector.GetData(query, settings);
                }
                if (error)
                {
                    var handler = Interlocked.CompareExchange(ref NewsGetted, null, null);
                    if (handler != null)
                    {
                        handler(this, new ArticleEventArgs() { Error = true });
                    }
                }
            }
        }

        public void BeginGetArticleDetail(Settings settings, int idBlock, int idSection, int idElement)
        {
            if (idElement == -1) return;
            Abort();
            var error = false;
            var query = string.Empty;
            try
            {
                _activeConnector = new HttpConnector();
                _activeConnector.DataReceived += ArticleDetailTransmissed;
                var param = new ServerParameters();
                param.DataObject = DataObject.NewsDetails;
                param.LoadDetails = true;
                param.LoadImages = settings.LoadImages;
                param.ScreenResolution = settings.ScreenResolution;
                param.ScreenWidth = settings.ScreenWidth;
                param.IdIblock = idBlock;
                param.IdSection = idSection;
                param.IdElement = idElement;
                query = GetQueryString(param);
            }
            catch (Exception)
            {
                error = true;
            }
            finally
            {
                if (!error)
                {
                    _activeConnector.GetData(query, settings);
                }
                if (error)
                {
                    var handler = Interlocked.CompareExchange(ref ArticleDetailGetted, null, null);
                    if (handler != null)
                    {
                        handler(this, new ArticleDetailEventArgs() { Error = true });
                    }
                }
            }
        }

        public void BeginGetMagazinesPreview(Settings settings, int year)
        {
            Abort();
            var error = false;
            var query = string.Empty;
            try
            {
                _activeConnector = new HttpConnector();
                _activeConnector.DataReceived += MagazinesPreviewTransmissed;
                var param = new ServerParameters();
                param.DataObject = DataObject.Magazines;
                param.LoadDetails = settings.LoadDetails;
                param.LoadImages = true;
                param.ScreenResolution = settings.ScreenResolution;
                param.ScreenWidth = settings.ScreenWidth;
                param.MagazinesYear = year;
                param.IdIblock = Magazine.BlockId;
                query = GetQueryString(param);
            }
            catch (Exception)
            {
                error = true;
            }
            finally
            {
                if (!error)
                {
                    _activeConnector.GetData(query, settings);
                }
                if (error)
                {
                    var handler = Interlocked.CompareExchange(ref MagazinesPriviewGetted, null, null);
                    if (handler != null)
                    {
                        handler(this, new MagazinesPreviewEventArgs() { Error = true });
                    }
                }
            }
        }

        public void BeginGetMagazineArticles(Settings settings, int magazineId)
        {
            Abort();
            var error = false;
            var query = string.Empty;
            try
            {
                _activeConnector = new HttpConnector();
                _activeConnector.DataReceived += MagazineArticlesTransmissed;
                var param = new ServerParameters();
                param.DataObject = DataObject.Magazines;
                param.LoadDetails = settings.LoadDetails;
                param.LoadImages = settings.LoadImages;
                param.ScreenResolution = settings.ScreenResolution;
                param.ScreenWidth = settings.ScreenWidth;
                param.IdIblock = Magazine.BlockId;
                param.IdSection = magazineId;
                param.IdAuthor = -1;
                query = GetQueryString(param);
            }
            catch (Exception)
            {
                error = true;
            }
            finally
            {
                if (!error)
                {
                    _activeConnector.GetData(query, settings);
                }
                if (error)
                {
                    var handler = Interlocked.CompareExchange(ref MagazineArticlesGetted, null, null);
                    if (handler != null)
                    {
                        handler(this, new MagazineArticlesEventArgs() { Error = true });
                    }
                }
            }
        }

        public void BeginGetMagazineArticleDetail(Settings settings, int idBlock, int idSection, int idElement)
        {
            if (idElement == -1) return;
            Abort();
            var error = false;
            var query = string.Empty;
            try
            {
                _activeConnector = new HttpConnector();
                _activeConnector.DataReceived += MagazineArticleDetailTransmissed;
                var param = new ServerParameters();
                param.DataObject = DataObject.News;
                param.LoadDetails = true;
                param.LoadImages = settings.LoadImages;
                param.ScreenResolution = settings.ScreenResolution;
                param.ScreenWidth = settings.ScreenWidth;
                param.IdIblock = idBlock;
                param.IdSection = idSection;
                param.IdElement = idElement;
                query = GetQueryString(param);
            }
            catch (Exception)
            {
                error = true;
            }
            finally
            {
                if (!error)
                {
                    _activeConnector.GetData(query, settings);
                }
                if (error)
                {
                    var handler = Interlocked.CompareExchange(ref MagazineArticleDetailGetted, null, null);
                    if (handler != null)
                    {
                        handler(this, new MagazineArticleDetailEventArgs() { Error = true });
                    }
                }
            }
        }

        public void BeginGetMagazinesArticlesByRubric(Settings settings, Rubric rubric, int idMagazine, long firstDateTime)
        {
            if (idMagazine == -1 || rubric == null) return;
            Abort();
            var error = false;
            var query = string.Empty;
            try
            {
                _activeConnector = new HttpConnector();
                _activeConnector.DataReceived += MagazineArticlesTransmissed;
                var param = new ServerParameters();
                param.DataObject = DataObject.Magazines;
                param.LoadDetails = settings.LoadDetails;
                param.LoadImages = settings.LoadImages;
                param.ScreenResolution = settings.ScreenResolution;
                param.ScreenWidth = settings.ScreenWidth;
                param.IdMagazineBlock = idMagazine;
                param.RubricCode = rubric.Code;
                param.IdRubric = rubric.Id;
                param.FirstElementDateTime = firstDateTime;
                query = GetQueryString(param);
            }
            catch (Exception)
            {
                error = true;
            }
            finally
            {
                if (!error)
                {
                    _activeConnector.GetData(query, settings);
                }
                if (error)
                {
                    var handler = Interlocked.CompareExchange(ref MagazineArticlesGetted, null, null);
                    if (handler != null)
                    {
                        handler(this, new MagazineArticlesEventArgs() { Error = true });
                    }
                }
            }
        }

        public void BeginGetBanner(Settings settings)
        {
            Abort();
            var error = false;
            var query = string.Empty;
            try
            {
                _activeConnector = new HttpConnector();
                _activeConnector.DataReceived += BannerTransmissed;
                var param = new ServerParameters();
                param.DataObject = DataObject.Banner;
                param.LoadDetails = false;
                param.LoadImages = false;
                param.ScreenResolution = settings.ScreenResolution;
                param.ScreenWidth = settings.ScreenWidth;
                query = GetQueryString(param);
            }
            catch (Exception)
            {
                error = true;
            }
            finally
            {
                if (!error)
                {
                    _activeConnector.GetData(query, settings);
                }
                if (error)
                {
                    var handler = Interlocked.CompareExchange(ref BannerGetted, null, null);
                    if (handler != null)
                    {
                        handler(this, new BannerEventArgs() { Error = true });
                    }
                }
            }
        }

        public void Abort()
        {
            try
            {
                if (_activeConnector != null)
                {
                    _activeConnector.RaiseAbortEvent();
                    _activeConnector.DataReceived -= NewsTransmissed;
                    _activeConnector.DataReceived -= MagazinesPreviewTransmissed;
                    _activeConnector.DataReceived -= MagazineArticlesTransmissed;
                    _activeConnector.DataReceived -= MagazineArticleDetailTransmissed;
                    _activeConnector.DataReceived -= ArticleDetailTransmissed;
                    _activeConnector.DataReceived -= BannerTransmissed;
                    _activeConnector.Abort();
                    _activeConnector = null;
                }
            }
            catch (Exception)
            {
            }
        }

        #endregion
    }

    #region Helper Classes 

    public class CssEventArs : ModelEventArgs
    {
        public string Css { get; set; }
    }

    public class ArticleEventArgs : ModelEventArgs
    {
        public List<Article> Articles { get; set; } 

        public List<Block> Blocks { get; set; } 

        public List<Section> Sections { get; set; } 

        public List<Author> Authors { get; set; } 
    }

    public class ArticleDetailEventArgs : ModelEventArgs
    {
        public Article Article { get; set; }
    }

    public class MagazinesPreviewEventArgs : ModelEventArgs
    {
        public List<Magazine> Magazines { get; set; }
 
        public List<MagazineYear> Years { get; set; }
    }

    public class MagazineArticlesEventArgs : ModelEventArgs
    {
        public List<Article> Articles { get; set; } 

        public List<Rubric> Rubrics { get; set; }

        public List<Author> Authors { get; set; } 
    }

    public class MagazineArticleDetailEventArgs : ModelEventArgs
    {
        public Article Article { get; set; }
    }

    public class BannerEventArgs : ModelEventArgs
    {
        public List<Banner> Banners { get; set; }
    }

    public class PdfEventArgs : ModelEventArgs
    {
        public byte[] Pdf { get; set; }
    }

    public class ModelEventArgs : EventArgs
    {
        public bool Error { get; set; }

        public bool Abort { get; set; }
    }

    public class SectionComparer : IEqualityComparer<Section>
    {
        public bool Equals(Section x, Section y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(Section obj)
        {
            return obj.Id.ToString("G").GetHashCode();
        }
    }

    public class RubricComparer : IEqualityComparer<Rubric>
    {
        public bool Equals(Rubric x, Rubric y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(Rubric obj)
        {
            return obj.Id.ToString("G").GetHashCode();
        }
    }

    public class AuthorComparer : IEqualityComparer<Author>
    {
        public bool Equals(Author x, Author y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(Author obj)
        {
            return obj.Id.ToString("G").GetHashCode();
        }
    }

    #endregion
}
