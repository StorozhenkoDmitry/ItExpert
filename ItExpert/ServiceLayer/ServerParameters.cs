using System.Collections.Generic;

namespace ItExpert.ServiceLayer
{
    public class ServerParameters
    {
        private readonly Dictionary<ScreenWidth, int> _dictWidth;

        private readonly Dictionary<DataObject, string> _dictDataObject;

        private readonly Dictionary<ScreenResolution, string> _dictResolution; 

        public string Action { get; set; }

        public DataObject DataObject { get; set; }

        public bool LoadImages { get; set; }

        public bool LoadDetails { get; set; }

        public ScreenResolution ScreenResolution { get; set; }
    
        public ScreenWidth ScreenWidth { get; set; }

        public string RubricCode { get; set; }

        public int IdRubric { get; set; }

        public int IdMagazineBlock { get; set; }

        public string Search { get; set; }

        public int IdIblock { get; set; }

        public int IdSection { get; set; }

        public int IdElement { get; set; }

        public long FirstElementDateTime { get; set; }

        public long LastElementDateTime { get; set; }

        public int IdAuthor { get; set; }

        public string PdfSrc { get; set; }

        public int MagazinesYear { get; set; }

        public ServerParameters()
        {
            _dictWidth = new Dictionary<ScreenWidth, int>();
            _dictWidth.Add(ScreenWidth.Small, 320);
            _dictWidth.Add(ScreenWidth.Medium, 640);
            _dictWidth.Add(ScreenWidth.Large, 768);
            _dictWidth.Add(ScreenWidth.VeryLarge, 1536);
            _dictDataObject = new Dictionary<DataObject, string>();
            _dictDataObject.Add(DataObject.News, "news");
            _dictDataObject.Add(DataObject.NewsDetails, "news_detail");
            _dictDataObject.Add(DataObject.Magazines, "magazines");
            _dictDataObject.Add(DataObject.Pdf, "pdf");
            _dictDataObject.Add(DataObject.Banner, "banner");
            _dictResolution = new Dictionary<ScreenResolution, string>();
            _dictResolution.Add(ScreenResolution.Smartphone, "mdpi");
            _dictResolution.Add(ScreenResolution.Tablet, "hdpi");
            ScreenWidth = ScreenWidth.Small;
            DataObject = DataObject.News;
            Action = "get";
            ScreenResolution = ScreenResolution.Smartphone;
            IdMagazineBlock = -1;
            IdRubric = -1;
            RubricCode = null;
            FirstElementDateTime = -1;
            LastElementDateTime = -1;
            MagazinesYear = -1;
            IdIblock = -1;
            IdSection = -1;
            IdElement = -1;
            IdAuthor = -1;
        }

        public int GetScreenWidthValue()
        {
            var result = _dictWidth[ScreenWidth];
            return result;
        }

        public string GetDataObjectValue()
        {
            var result = _dictDataObject[DataObject];
            return result;
        }

        public string GetScreenResolutionValue()
        {
            var result = _dictResolution[ScreenResolution];
            return result;
        }
    }
}
