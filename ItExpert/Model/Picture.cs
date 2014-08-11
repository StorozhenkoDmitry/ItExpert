using ItExpert.Enum;
using SQLite;

namespace ItExpert.Model
{
    public class Picture
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public PictypeType Type { get; set; }
 
        public int IdParent { get; set; }

        public string Src { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public PictureExtension Extension { get; set; }

        public string Data { get; set; }
    }
}
