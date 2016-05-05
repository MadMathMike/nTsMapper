using System;

namespace nTsMapper
{
	public class TypeMapping
	{
		public Func<Type, bool> MatchesType { get; set; }
		public string DestinationType { get; set; }
		public string DestinationAssignmentTemplate { get; set; }
	}

}