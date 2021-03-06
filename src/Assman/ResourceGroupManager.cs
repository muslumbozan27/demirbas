using System;
using System.Collections.Generic;
using System.Linq;

namespace Assman
{
	public class ResourceGroupManager : IResourceGroupManager
	{
	    public static IResourceGroupManager GetInstance(ResourceMode resourceMode, IResourceCache resourceCache)
		{
			return new CachingResourceGroupManager(new ResourceGroupManager(resourceMode), resourceCache);
		}

	    private readonly ResourceMode _resourceMode;
	    private readonly List<IResourceGroupTemplate> _templates = new List<IResourceGroupTemplate>();
	    private readonly List<string> _globalDependencies = new List<string>();

	    public ResourceGroupManager(ResourceMode resourceMode)
		{
		    _resourceMode = resourceMode;
		    Consolidate = true;
		}

	    public void Add(IResourceGroupTemplate template)
		{
			_templates.Add(template);
		}

		public void Clear()
		{
			_templates.Clear();
		}

		public bool Any()
		{
			return _templates.Any();
		}

		public bool Consolidate { get; set; }

		public string ResolveResourceUrl(string resourceUrl)
		{
			if (!Consolidate)
				return resourceUrl;
			
			foreach (var groupTemplate in _templates)
			{
				string consolidatedUrl;
				if (groupTemplate.TryGetConsolidatedUrl(resourceUrl, _resourceMode, out consolidatedUrl))
				{
					return consolidatedUrl;
				}
			}

			return resourceUrl;
		}

		public bool IsGroupUrlWithConsolidationDisabled(string resourceUrl)
		{
			var groupTemplate = GetGroupTemplateOrDefault(resourceUrl);

			return groupTemplate != null && (!Consolidate || groupTemplate.GroupTemplate.Consolidate.IsFalse(_resourceMode));
		}

		public IEnumerable<string> GetResourceUrlsInGroup(string consolidatedUrl, IResourceFinder finder)
		{
			var group = GetGroupOrDefault(consolidatedUrl, finder);
			if (group == null)
				return Enumerable.Empty<string>();
			else
				return group.GetResources().Select(r => r.VirtualPath);
		}

		public bool IsConsolidatedUrl(string virtualPath)
		{
			return _templates.Any(t => t.MatchesConsolidatedUrl(virtualPath));
		}

		public GroupTemplateContext GetGroupTemplateOrDefault(string consolidatedUrl)
		{
			GroupTemplateContext groupTemplateContext = null;
			EachTemplate(c =>
			{
				if (c.GroupTemplate.MatchesConsolidatedUrl(consolidatedUrl))
				{
					groupTemplateContext = c;
					return false;
				}
				return true;
			});
			
			return groupTemplateContext;
		}

		public IResourceGroup GetGroupOrDefault(string consolidatedUrl, IResourceFinder finder)
		{
			var groupTemplateContext = GetGroupTemplateOrDefault(consolidatedUrl);
			if (groupTemplateContext == null)
				return null;

			var group = groupTemplateContext.FindGroupOrDefault(finder, consolidatedUrl, _resourceMode);
			if (group == null)
				return null;

			return group;
		}

		public void EachGroup(IEnumerable<IResource> allResources, Action<IResourceGroup> handler)
		{
			EachTemplate(templateContext =>
			{
				var groups = templateContext.GetGroups(allResources, _resourceMode);
				foreach (var group in groups)
				{
					handler(group);
				}

				return true;
			});
		}

		public bool IsPartOfGroup(string virtualPath)
		{
			return _templates.Any(t => t.IsMatch(virtualPath));
		}

		public IEnumerable<string> GetGlobalDependencies()
		{
			return _globalDependencies;
		}

		public void AddGlobalDependencies(IEnumerable<string> paths)
		{
			_globalDependencies.AddRange(paths);
		}

		private void EachTemplate(Func<GroupTemplateContext,bool> handler)
		{
			var excludeFilter = new CompositeResourceFilter();
			//add a filter to exclude all resources that match the consolidated url of a group
			excludeFilter.AddFilter(new ConsolidatedUrlFilter(_templates));
			foreach (var groupTemplate in _templates)
			{
				var templateContext = groupTemplate.WithContext(excludeFilter);
				if(!handler(templateContext))
					break;

				excludeFilter.AddFilter(groupTemplate);
			}
		}

		private class ConsolidatedUrlFilter : IResourceFilter
		{
			private readonly IEnumerable<IResourceGroupTemplate> _templates;

			public ConsolidatedUrlFilter(IEnumerable<IResourceGroupTemplate> templates)
			{
				_templates = templates;
			}

			public bool IsMatch(IResource resource)
			{
				return _templates.Any(t => t.MatchesConsolidatedUrl(resource.VirtualPath));
			}
		}

		internal class CachingResourceGroupManager : IResourceGroupManager
		{
			private readonly IResourceGroupManager _inner;
			private readonly IResourceCache _resourceCache;
			private readonly ThreadSafeInMemoryCache<string, string> _resolvedResourceUrls = new ThreadSafeInMemoryCache<string, string>(Comparers.VirtualPath);

			public CachingResourceGroupManager(IResourceGroupManager inner, IResourceCache resourceCache)
			{
				_inner = inner;
				_resourceCache = resourceCache;
			}

			public void Add(IResourceGroupTemplate template)
			{
				_inner.Add(template);
			}

			public void Clear()
			{
				_inner.Clear();
			}

			public bool Any()
			{
				return _inner.Any();
			}

			public bool Consolidate
			{
				get { return _inner.Consolidate; }
				set { _inner.Consolidate = value; }
			}

			public string ResolveResourceUrl(string resourceUrl)
			{
				return _resolvedResourceUrls.GetOrAdd(resourceUrl, () => _inner.ResolveResourceUrl(resourceUrl));
			}

			public bool IsGroupUrlWithConsolidationDisabled(string resourceUrl)
			{
				return _inner.IsGroupUrlWithConsolidationDisabled(resourceUrl);
			}

			public IEnumerable<string> GetResourceUrlsInGroup(string consolidatedUrl, IResourceFinder finder)
			{
				//NOTE: This is basically a duplication of the implementation in _inner.GetResourceUrlsInGroup, but we don't delegate
				//to it because then it would bypass the _resourceCache
				var group = GetGroupOrDefault(consolidatedUrl, finder);
				if (group == null)
					return Enumerable.Empty<string>();
				else
					return group.GetResources().Select(r => r.VirtualPath);
			}

			public bool IsConsolidatedUrl(string virtualPath)
			{
				return _inner.IsConsolidatedUrl(virtualPath);
			}

			public GroupTemplateContext GetGroupTemplateOrDefault(string consolidatedUrl)
			{
				return _inner.GetGroupTemplateOrDefault(consolidatedUrl);
			}

			public IResourceGroup GetGroupOrDefault(string consolidatedUrl, IResourceFinder finder)
			{
				return _resourceCache.GetOrAddGroup(consolidatedUrl, () =>
				{
					return _inner.GetGroupOrDefault(consolidatedUrl, finder);
				});
			}

			public void EachGroup(IEnumerable<IResource> allResources, Action<IResourceGroup> handler)
			{
				_inner.EachGroup(allResources, handler);
			}

			public bool IsPartOfGroup(string virtualPath)
			{
				return _inner.IsPartOfGroup(virtualPath);
			}

			public IEnumerable<string> GetGlobalDependencies()
			{
				return _inner.GetGlobalDependencies();
			}

			public void AddGlobalDependencies(IEnumerable<string> paths)
			{
				_inner.AddGlobalDependencies(paths);
			}
		}
	}
}