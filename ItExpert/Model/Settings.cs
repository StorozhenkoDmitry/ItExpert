using ItExpert.Enum;
using ItExpert.ServiceLayer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing;

namespace ItExpert.Model
{
    [Serializable]
    public class Settings
    {
        private readonly Dictionary<int, ScreenWidth> _screenWidths;
        private readonly Dictionary<int, FontSizeWrapper> _fontValues;
        private readonly Dictionary<Page, string> _startSections;
        private readonly Dictionary<int, int> _dbSizeLimits; 
        private Theme _theme;
        private int _fontSize;
        private int _detailFontSize;
        [NonSerialized]
        private Color _backColor;
        [NonSerialized]
        private Color _foreColor;
        private const string FileName = "settings.dt";
        public const string PdfFolder = "/ItExpertPdf";
        public const string Domen = "http://www.it-world.ru/";

        public Settings()
        {
            _screenWidths = new Dictionary<int, ScreenWidth>()
            {
                {320, ScreenWidth.Small},
                {640, ScreenWidth.Medium},
                {768, ScreenWidth.Large},
                {1536, ScreenWidth.VeryLarge}
            };
            _fontValues = new Dictionary<int, FontSizeWrapper>()
            {
                {1, new FontSizeWrapper() {TextSize = 12, HeaderSize = 15}},
                {2, new FontSizeWrapper() {TextSize = 14, HeaderSize = 18}},
                {3, new FontSizeWrapper() {TextSize = 16, HeaderSize = 20}},
                {4, new FontSizeWrapper() {TextSize = 18, HeaderSize = 22}},
                {5, new FontSizeWrapper() {TextSize = 20, HeaderSize = 26}},
                {6, new FontSizeWrapper() {TextSize = 22, HeaderSize = 30}}
            };
            _startSections = new Dictionary<Page, string>()
            {
                {Page.News, "Новости"},
                {Page.Trends, "Тренды"},
                {Page.Magazine, "Журнал"},
                {Page.Archive, "Архив"},
                {Page.Favorite, "Избранное"}
            };
            _dbSizeLimits = new Dictionary<int, int>()
            {
                {1, 30},
                {2, 50},
                {3, 100},
                {4, 120},
                {5, 150},
                {6, 200},
                {7, 250},
                {8, 500}
            };
        }

        public bool LoadImages { get; set; }

        public bool LoadDetails { get; set; }

        public ScreenResolution ScreenResolution { get; set; }

        public ScreenWidth ScreenWidth { get; set; }

        public int TextSize { get; set; }

        public int HeaderSize { get; set; }

        public int DetailTextSize { get; set; }

        public int DetailHeaderSize { get; set; }

        public int DbSizeLimit { get; protected set; }

        public Color GetBackgroundColor()
        {
            return _backColor;
        }

        public void SetBackgroundColor(Color value)
        {
            _backColor = value;
        }

        public Color GetForeColor()
        {
            return _foreColor;
        }

        public void SetForeColor(Color value)
        {
            _foreColor = value;
        }

        public bool OfflineMode { get; set; }

        public bool HideReaded { get; set; }

        public Page Page { get; set; }

        public NetworkMode NetworkMode { get; set; }

        public void SetDbLimitSize(int value)
        {
            if (_dbSizeLimits.ContainsKey(value))
            {
                DbSizeLimit = value;
            }
        }

        public long GetDbLimitSizeInMb()
        {
            var returnValue = 0;
            if (_dbSizeLimits.ContainsKey(DbSizeLimit))
            {
                returnValue = _dbSizeLimits[DbSizeLimit];
            }
            return returnValue;
        }

        public Theme GetTheme()
        {
            return _theme;
        }

        public void SetTheme(Theme theme)
        {
            _theme = theme;
            if (_theme == Theme.Light)
            {
                SetBackgroundColor(Color.White);
                SetForeColor(Color.Black);
            }
            if (_theme == Theme.Dark)
            {
                SetBackgroundColor(Color.Black);
				SetForeColor(Color.FromArgb (170, 170, 170));
            }
        }

        public int GetFontSize()
        {
            return _fontSize;
        }

        public void SetFontSize(int size)
        {
            var wrapper = _fontValues[size];
            if (wrapper != null)
            {
                _fontSize = size;
                TextSize = wrapper.TextSize;
                HeaderSize = wrapper.HeaderSize;
            }
        }

        public int GetDetailFontSize()
        {
            return _detailFontSize;
        }

        public void SetDetailFontSize(int size)
        {
            var wrapper = _fontValues[size];
            if (wrapper != null)
            {
                _detailFontSize = size;
                DetailTextSize = wrapper.TextSize;
                DetailHeaderSize = wrapper.HeaderSize;
            }
        }

        public static Settings GetSettings()
        {
            Settings settings = null;
            var folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var file = System.IO.Path.Combine(folder, FileName);
            if (File.Exists(file))
            {
                using (var fs = File.OpenRead(file))
                {
                    var formatter = new BinaryFormatter();
                    settings = (Settings)formatter.Deserialize(fs);
                    settings.OfflineMode = false;
                    settings.SetTheme(settings.GetTheme());
                    settings.SetFontSize(settings.GetFontSize());
                    settings.SetDetailFontSize(settings.GetDetailFontSize());
                }
            }
            if (settings == null)
            {
                settings = new Settings()
                {
                    LoadDetails = false,
                    LoadImages = true,
                    ScreenResolution = ScreenResolution.Smartphone,
                    ScreenWidth = ScreenWidth.Small,
                    Page = Page.News,
                    HideReaded = false,
                    OfflineMode = false,
                    NetworkMode = NetworkMode.All
                };
                settings.SetTheme(Theme.Light);
                settings.SetFontSize(2);
                settings.SetDetailFontSize(2);
                settings.SetDbLimitSize(3);
            }
            return settings;
        }

        public void SaveSettings()
        {
            var folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var file = System.IO.Path.Combine(folder, FileName);
            using (var fs = File.Create(file))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(fs, this);
                fs.Flush();
            }
        }

        public string GetStringTheme(Theme theme)
        {
            var result = string.Empty;
            if (theme == Theme.Light)
            {
                result = "Светлая";
            }
            if (theme == Theme.Dark)
            {
                result = "Темная";
            }
            return result;
        }

        public string GetStringStartSection(Page section)
        {
            var result = _startSections[section];
            return result;
        }

        public string GetStringNetworkMode(NetworkMode mode)
        {
            var result = string.Empty;
            if (mode == NetworkMode.WiFi)
            {
                result = "Wi-Fi";
            }
            if (mode == NetworkMode.All)
            {
                result = "Любое";
            }
            return result;
        }

        public ScreenWidth GetScreenWidthForScreen(int screenWidth)
        {
            var returnValue = ScreenWidth.Small;
            var keys = _screenWidths.Keys.ToArray();
            var max = -1;
            var position = -1;
            for (var i = 0; i < keys.Length; i++)
            {
                var key = keys[i];
                if (key > screenWidth)
                {
                    max = key;
                    position = i;
                    break;
                }
            }
            if (max == -1)
            {
                returnValue = ScreenWidth.VeryLarge;
            }
            else
            {
                if (position == 0)
                {
                    returnValue = ScreenWidth.Small;
                }
                else
                {
                    var endRange = keys[position];
                    var startRange = keys[position - 1];
                    var middlePoint = startRange + ((int) (endRange - startRange)/2);
                    if (screenWidth >= middlePoint)
                    {
                        returnValue = _screenWidths[endRange];
                    }
                    else
                    {
                        returnValue = _screenWidths[startRange];
                    }
                }
            }

            return returnValue;
        }

        public Settings Clone()
        {
            var settings = new Settings()
            {
                HeaderSize = HeaderSize,
                TextSize = TextSize,
                ScreenWidth = ScreenWidth,
                ScreenResolution = ScreenResolution,
                LoadImages = LoadImages,
                LoadDetails = LoadDetails,
                OfflineMode = OfflineMode,
                HideReaded = HideReaded,
                Page = Page,
                NetworkMode = NetworkMode,
                DetailHeaderSize = DetailHeaderSize,
                DetailTextSize = DetailTextSize
            };
            settings.SetTheme(GetTheme());
            settings.SetFontSize(GetFontSize());
            settings.SetDetailFontSize(GetDetailFontSize());
            return settings;
        }
    }

    [Serializable]
    public class FontSizeWrapper
    {
        public int TextSize { get; set; }

        public int HeaderSize { get; set; }
    }
}
