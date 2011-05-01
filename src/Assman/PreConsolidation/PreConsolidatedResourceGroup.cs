using System;
using System.Collections.Generic;

namespace Assman.PreConsolidation
{
	public class PreConsolidatedResourceGroup
	{
		public string ConsolidatedUrl { get; set; }

		public List<string> Resources { get; set; }

		public PreConsolidatedResourceGroup()
		{
			Resources = new List<string>();
		}

		public override string ToString()
		{
			return ConsolidatedUrl;
		}
	}
}