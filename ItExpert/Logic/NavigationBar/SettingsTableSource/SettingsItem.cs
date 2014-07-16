using System;

namespace ItExpert
{
    public class SettingsItem
    {
        public enum ContentType
        {
            Slider,
            Switch,
            RadioButton,
            Tap,
            Buttons
        }

        public SettingsItem(ContentType type)
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

