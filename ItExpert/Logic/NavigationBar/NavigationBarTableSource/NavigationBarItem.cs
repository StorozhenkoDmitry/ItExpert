using System;

namespace ItExpert
{
    public class NavigationBarItem : IDisposable
    {
        public enum ContentType
        {
            Slider,
            Switch,
            RadioButton,
            Tap,
            Buttons,
            Search,
            MenuItem,
            CacheSlider
        }

        public NavigationBarItem(ContentType type)
        {
            Type = type;
        }

        public ContentType Type
        {
            get;
            set;
        }

        public string Title
        {
            get;
            set;
        }

        public string[] Buttons
        {
            get;
            set;
        }

        public Func<object> GetValue
        {
            get;
            set;
        }

        public Action<object> SetValue
        {
            get;
            set;
        }

        public Action<int> ButtonPushed
        {
            get;
            set;
        }

		public void Dispose()
		{
			ButtonPushed = null;
			SetValue = null;
			GetValue = null;
		}
    }
}

