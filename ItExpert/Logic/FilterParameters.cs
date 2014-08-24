using System;
using ItExpert.Enum;
using ItExpert.Model;

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

		public string Search { get; set;}
	
		public Rubric SearchRubric { get; set; }
	}
}

