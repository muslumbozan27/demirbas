using System;
using System.Collections.Generic;
using System.Linq;

namespace AlmWitt.Web.ResourceManagement.PreConsolidation
{
	public class PreConsolidatedDependencyCache : IDependencyCache
	{
		private readonly IDictionary<string,IEnumerable<string>> _dependencyMap = new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase);

		public PreConsolidatedDependencyCache(IEnumerable<PreConsolidatedResourceDependencies> dependencies)
		{
			PopulateDependencyCache(dependencies);
		}

		public bool TryGetDependencies(string virtualPath, out IEnumerable<string> dependencies)
		{
			if (!_dependencyMap.TryGetValue(virtualPath, out dependencies))
				dependencies = Enumerable.Empty<string>();

			return true;
		}

		public bool TryGetDependencies(IResource resource, out IEnumerable<string> dependencies)
		{
			dependencies = Enumerable.Empty<string>();
			return true;
		}

		public void StoreDependencies(IResource resource, IEnumerable<string> dependencies)
		{
			//no-op
		}

		private void PopulateDependencyCache(IEnumerable<PreConsolidatedResourceDependencies> dependencies)
		{
			foreach (var resourceWithDependency in dependencies)
			{
				_dependencyMap[resourceWithDependency.ResourcePath] = resourceWithDependency.Dependencies;
			}
		}
	}
}