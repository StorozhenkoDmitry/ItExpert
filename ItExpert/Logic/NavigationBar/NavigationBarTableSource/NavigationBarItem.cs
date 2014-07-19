using System;

namespace ItExpert
{
    public class NavigationBarItem
    {
        public enum ContentType
        {
            Slider,
            Switch,
            RadioButton,
            Tap,
            Buttons,
            Search,
            MenuItem
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
    }
}

