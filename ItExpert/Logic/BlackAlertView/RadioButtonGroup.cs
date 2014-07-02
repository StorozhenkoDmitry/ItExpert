using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Collections.Generic;

namespace ItExpert
{
    public class RadioButtonGroup: UIView
    {
        public RadioButtonGroup(float width, string[] radioButtons)
        {
            _buttons = new List<RadioButton>(radioButtons.Length);

            float buttonHeight = 40;

            for (int i = 0; i < radioButtons.Length; i++)
            {
                RadioButton button = new RadioButton(new RectangleF(0, buttonHeight * i, width, buttonHeight), radioButtons[i], false, i, (index) => OnButtonChecked(index));

                _buttons.Add(button);

                Add(button);
            }

            if (_buttons.Count > 0)
            {
                CheckButtonAtIndex(0);
            }

            float height = 0;

            foreach (var button in _buttons)
            {
                height += button.Frame.Height;
            }

            Frame = new RectangleF(Frame.X, Frame.Y, width, height);
        }

        public PointF Location
        {
            get
            {
                return Frame.Location;
            }
            set
            {
                Frame = new RectangleF(value.X, value.Y, Frame.Width, Frame.Height);
            }
        }

        public int SelectedIndex
        {
            get
            { 
                if (_checkedButton != null)
                {
                    return _checkedButton.Index;
                }

                return 0;
            }
        }

        private void OnButtonChecked(int index)
        {
            if (_checkedButton != null)
            {
                _checkedButton.ChangeState(false);
            }

            CheckButtonAtIndex(index);
        }

        private void CheckButtonAtIndex(int index)
        {
            _checkedButton = _buttons[index];

            _checkedButton.ChangeState(true);
        }

        private RadioButton _checkedButton;

        private List<RadioButton> _buttons;
    }
}

