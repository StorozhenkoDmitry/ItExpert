using System;

namespace ItExpert
{
    public class SettingsItem
    {
        public enum ContentType
        {
            Switch,
            RadioButton,
            Tap,
            Buttons
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
    }
}

