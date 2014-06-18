using SQLite;

namespace ItExpert.Model
{
    public class BlockBase
    {
        [PrimaryKey]
        public int Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }
    }
}
