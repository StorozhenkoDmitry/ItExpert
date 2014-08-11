using System;
using ItExpert.Enum;

namespace ItExpert
{
	public class FilterParameters
	{
		public FilterParameters()
		{
		}

		public Filter Filter { get; set;}

		public int BlockId {get;set;}

		public int SectionId { get; set;}

		public int AuthorId { get; set;}
	}
}

