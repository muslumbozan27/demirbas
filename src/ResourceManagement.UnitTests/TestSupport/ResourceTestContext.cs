using System.Linq;

using AlmWitt.Web.ResourceManagement.Configuration;

namespace AlmWitt.Web.ResourceManagement.TestSupport
{
	public class ResourceTestContext
	{
		private readonly StubResourceFinder _finder;
		private readonly ContentFilterMap _contentFilterMap;
		private readonly InMemoryDependencyCache _dependencyCache;
		private readonly StubDependencyProvider _dependencyProvider;
		private readonly DependencyManager _dependencyManager;
		private readonly ResourceGroupManager _scriptGroups;
		private readonly ResourceGroupManager _styleGroups;

		public ResourceTestContext()
		{
			_finder = new StubResourceFinder();
			_contentFilterMap = new ContentFilterMap();
			_scriptGroups = new ResourceGroupManager();
			_styleGroups = new ResourceGroupManager();
			_dependencyCache = new InMemoryDependencyCache();
			_dependencyProvider = new StubDependencyProvider();
			_dependencyManager = new DependencyManager(_finder, _dependencyCache, _scriptGroups, _styleGroups);
			_dependencyManager.MapProvider(".js", _dependencyProvider);
			_dependencyManager.MapProvider(".css", _dependencyProvider);
		}

		public ResourceConsolidator GetConsolidator()
		{
			return new ResourceConsolidator(_contentFilterMap, _dependencyManager, _scriptGroups, _styleGroups, _finder);
		}

		public StubResourceBuilder CreateResource(string virtualPath)
		{
			var builder = new StubResourceBuilder(virtualPath, _dependencyProvider);
			_finder.AddResource(builder.Resource);

			return builder;
		}

		public StubResourceGroup CreateGroup(string consolidatedUrl)
		{
			var group = new StubResourceGroup(consolidatedUrl);
			if(group.ResourceType == ResourceType.ClientScript)
				_scriptGroups.Add(new StubResourceGroupTemplate(group));
			else
				_styleGroups.Add(new StubResourceGroupTemplate(group));

			return group;
		}

		public bool DependencyProviderWasCalled
		{
			get { return _dependencyProvider.GetDependenciesWasCalled; }
		}
	}

	public class StubResourceBuilder
	{
		private readonly StubDependencyProvider _dependencyProvider;
		private readonly StubResource _resource;

		public StubResourceBuilder(string virtualPath, StubDependencyProvider dependencyProvider)
		{
			_dependencyProvider = dependencyProvider;
			_resource = StubResource.WithPath(virtualPath);
		}

		public StubResource Resource
		{
			get { return _resource; }
		}

		public StubResourceBuilder WithDependencies(params IResource[] dependencies)
		{
			_dependencyProvider.SetDependencies(_resource, dependencies.Select(d => d.VirtualPath).ToArray());

			return this;
		}

		public StubResourceBuilder InGroup(StubResourceGroup group)
		{
			group.AddResource(_resource);

			return this;
		}
	}
}