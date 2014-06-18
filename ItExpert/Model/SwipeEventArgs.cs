using ItExpert.Enum;
using System;

namespace ItExpert.Model
{
    public class SwipeEventArgs : EventArgs
    {
        public SwipeDirection Direction { get; set; }
    }
}
