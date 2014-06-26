using SQLite;

namespace ItExpert.Model
{
    public class Author
    {
        [PrimaryKey]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
