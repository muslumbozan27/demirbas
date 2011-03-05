using System;

namespace AlmWitt.Web.ResourceManagement.PreConsolidation
{
	public static class PreConsolidationPersisterFactory
	{
		private static readonly IPreConsolidationInfoPersister _null = new NullPreConsolidationPersister();

		public static IPreConsolidationInfoPersister Null
		{
			get { return _null; }
		}
	}
}