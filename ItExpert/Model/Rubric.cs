using SQLite;

namespace ItExpert.Model
{
    public class Rubric
    {
        [PrimaryKey]
        public int Id { get; set; }
    
        public string Code { get; set; }

        public string Name { get; set; }
    }
}
