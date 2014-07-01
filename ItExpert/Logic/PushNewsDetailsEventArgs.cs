using System;

namespace ItExpert
{
	public class PushDetailsEventArgs: EventArgs
	{
		public PushDetailsEventArgs (ArticleDetailsViewController newsDetailsView)
		{
			NewsDetailsView = newsDetailsView;
		}

		public ArticleDetailsViewController NewsDetailsView { get; set; }
	}
}

