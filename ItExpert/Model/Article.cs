using System;
using System.Collections.Generic;
using ItExpert.Enum;
using SQLite;

namespace ItExpert.Model
{
    public class Article
    {
        [PrimaryKey]
        public int Id { get; set; }

        public int IdMagazine { get; set; }

        public int IdBlock { get; set; }

        [Ignore]
        public Block Block { get; set; }

        [Ignore]
        public List<ItemSection> Sections { get; set; }

        public string Url { get; set; }

        public int Sort { get; set; }

        public string Video { get; set; }

        public string SectionsId { get; set; }

        public string Name { get; set; }

        public DateTime ActiveFrom { get; set; }

        public long Timespan { get; set; }

        public string DetailText { get; set; }

        public string PreviewText { get; set; }

        public bool IsReaded { get; set; }

        public int IdSection { get; set; }

        public bool IsFavorite { get; set; }

        public string RubricsId { get; set; }

        public ArticleType ArticleType { get; set; }

        [Ignore]
        public List<Author> Authors { get; set; }
        
        public string AuthorsId { get; set; }

        [Ignore]
        public List<Rubric> Rubrics { get; set; }

        [Ignore]
        public Picture PreviewPicture { get; set; }
    
        [Ignore]
        public Picture DetailPicture { get; set; }

        [Ignore]
        public Picture AwardsPicture { get; set; }

//        [Ignore]
//        public View ExtendedObject { get; set; }
    }
}
