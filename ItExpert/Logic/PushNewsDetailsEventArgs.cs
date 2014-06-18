using System;

namespace ItExpert
{
	public class PushNewsDetailsEventArgs: EventArgs
	{
		public PushNewsDetailsEventArgs (NewsDetailsViewController newsDetailsView)
		{
			NewsDetailsView = newsDetailsView;
		}

		public NewsDetailsViewController NewsDetailsView { get; set; }
	}
}

