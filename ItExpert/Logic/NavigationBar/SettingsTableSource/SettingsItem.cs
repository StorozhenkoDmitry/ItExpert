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

        public SettingsItem(ContentType type, string title)
        {
            Type = type;
            Title = title;
        }

        public SettingsItem(ContentType type, string[] buttons)
        {
            Type = type;
            Buttons = buttons;
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
    }
}

