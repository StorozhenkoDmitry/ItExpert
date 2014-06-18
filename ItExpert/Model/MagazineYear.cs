using SQLite;

namespace ItExpert.Model
{
    public class MagazineYear
    {
        [PrimaryKey]
        public int Value { get; set; }

        public bool DataLoaded { get; set; }
    }
}
