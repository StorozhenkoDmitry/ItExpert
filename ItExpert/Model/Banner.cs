using ItExpert.ServiceLayer;
using SQLite;

namespace ItExpert.Model
{
    public class Banner
    {
        public int Id { get; set; }
    
        public string Url { get; set; }

        public ScreenWidth ScreenWidth { get; set; }

        [Ignore]
        public Picture Picture { get; set; }
    }
}
