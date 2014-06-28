using System;

namespace ItExpert
{
    public class BlackAlertViewButtonEventArgs : EventArgs
    {
        public BlackAlertViewButtonEventArgs(int buttonIndex, int selectedRadiobutton = 0)
        {
            ButtonIndex = buttonIndex;
            SelectedRadioButton = selectedRadiobutton;
        }

        public int ButtonIndex
        {
            get;
            set;
        }

        public int SelectedRadioButton
        {
            get;
            set;
        }
    }
}

