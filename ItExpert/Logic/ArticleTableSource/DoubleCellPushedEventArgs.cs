using System;
using ItExpert.Model;

namespace ItExpert
{
    public class DoubleCellPushedEventArgs : EventArgs
    {
        public DoubleCellPushedEventArgs(Article article)
        {
            Article = article;
        }

        public Article Article
        {
            get;
            set;
        }
    }
}

