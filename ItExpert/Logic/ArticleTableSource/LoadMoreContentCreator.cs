using System;
using ItExpert.Model;
using MonoTouch.UIKit;
using System.Drawing;

namespace ItExpert
{
    public class LoadMoreContentCreator : BaseContentCreator
    {
        public override float GetContentHeight(UIView cellContentView, Article article)
        {
			var button = article.ExtendedObject as UIButton;
            if (button != null)
            {
                var textHeight = ItExpertHelper.GetTextHeight(button.TitleLabel.Font, button.TitleLabel.Text, cellContentView.Frame.Width);
                return textHeight + _padding.Top + _padding.Bottom;
            }
            else
            {
                return 0;
            }
        }

        protected override void Create(UITableViewCell cell, Article article)
        {
            if (article.ExtendedObject is UIButton)
            {
                AddButton(cell, article.ExtendedObject as UIButton);
            }
        }

        protected override void Update(UITableViewCell cell, Article article)
        {
            if (article.ExtendedObject is UIButton)
            {
                AddButton(cell, article.ExtendedObject as UIButton);
            }
        }

        private void AddButton(UITableViewCell cell, UIButton button)
        {
            var textHeight = ItExpertHelper.GetTextHeight(button.TitleLabel.Font, button.TitleLabel.Text, cell.ContentView.Frame.Width);

            button.Frame = new RectangleF(0, 0, cell.ContentView.Frame.Width, textHeight + _padding.Top + _padding.Bottom);

            button.SetTitle(button.TitleLabel.Text, UIControlState.Normal);
            button.Font = UIFont.SystemFontOfSize(ApplicationWorker.Settings.HeaderSize);
			button.SetTitleColor(UIColor.FromRGB(140, 140, 140), UIControlState.Normal);

            cell.ContentView.Add(button);

            button.BringSubviewToFront(cell.ContentView);
        }
    }
}

