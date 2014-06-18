using SQLite;

namespace ItExpert.Model
{
    public class ItemSection
    {
        [Ignore]
        public Section Section { get; set; }

        public int IdArticle { get; set; }

        public int IdSection { get; set; }

        public int DepthLevel { get; set; }
    }
}
