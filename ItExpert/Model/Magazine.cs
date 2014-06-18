using System;
using SQLite;

namespace ItExpert.Model
{
    public class Magazine
    {
        public const int BlockId = 11;

        [PrimaryKey]
        public int Id { get; set; }
    
        public int Code { get; set; }

        public string Name { get; set; }

        public DateTime ActiveFrom { get; set; }

        public int Year { get; set; }

        public bool InCache { get; set; }

        [Ignore]
        public Picture PreviewPicture { get; set; }

        public string PdfFileSrc { get; set; }

        public int PdfFileSize { get; set; }

        public bool Exists { get; set; } 
    }
}
