using System;

namespace ItExpert
{
	public class PushNewsDetailsEventArgs: EventArgs
	{
		public PushNewsDetailsEventArgs (ArticleDetailsViewController newsDetailsView)
		{
			NewsDetailsView = newsDetailsView;
		}

		public ArticleDetailsViewController NewsDetailsView { get; set; }
	}
}

