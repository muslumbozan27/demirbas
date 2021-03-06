using System;
using System.Collections.Generic;
using System.Linq;

namespace Assman.DependencyManagement
{
    public class DependencyManager
    {
        private readonly IResourceFinder _resourceFinder;
        private IDependencyCache _dependencyCache;
        private readonly IResourceGroupManager _scriptGroups;
        private readonly IResourceGroupManager _styleGroups;
        private readonly IDictionary<string, IDependencyProvider> _providers = new Dictionary<string, IDependencyProvider>(Comparers.VirtualPath);

        internal DependencyManager(IResourceFinder resourceFinder, IDependencyCache dependencyCache, IResourceGroupManager scriptGroups, IResourceGroupManager styleGroups)
        {
            _resourceFinder = resourceFinder;
            _dependencyCache = dependencyCache;
            _scriptGroups = scriptGroups;
            _styleGroups = styleGroups;
        }

        public void MapProvider(string extension, IDependencyProvider dependencyProvider)
        {
            _providers[extension] = dependencyProvider;
        }

        public IEnumerable<string> GetDependencies(string virtualPath)
        {
            virtualPath = virtualPath.WithoutQuery();
            IEnumerable<string> cachedDependencies;
            if (_dependencyCache.TryGetDependencies(virtualPath, out cachedDependencies))
                return cachedDependencies;

            var dependencyList = new List<IEnumerable<string>>();
            IEnumerable<IResource> resourcesInGroup;
            if(IsConsolidatedUrl(virtualPath, out resourcesInGroup))
            {
                foreach (var resource in resourcesInGroup)
                {
                    AccumulateDependencies(dependencyList, resource);
                }

                //filter out dependencies within the group
                return CollapseDependencies(dependencyList)
                    .Where(d => !resourcesInGroup.Any(r => r.VirtualPath.EqualsVirtualPath(d)));
            }
            else
            {
                AccumulateDependencies(dependencyList, virtualPath);
                return CollapseDependencies(dependencyList);
            }	
        }

        public IEnumerable<string> GetDependencies(IResource resource)
        {
            IEnumerable<string> cachedDependencies;
            if (_dependencyCache.TryGetDependencies(resource.VirtualPath, out cachedDependencies))
                return cachedDependencies;

            var dependencyList = new List<IEnumerable<string>>();
            AccumulateDependencies(dependencyList, resource);

            return CollapseDependencies(dependencyList);
        }

        public void SetCache(IDependencyCache cache)
        {
            _dependencyCache = cache;
        }

        internal int Comparer(IResource x, IResource y)
        {
            if (x.VirtualPath.EqualsVirtualPath(y.VirtualPath))
                return 0;
            
            var xDepends = GetDependencies(x);
            var yDepends = GetDependencies(y);

            if (xDepends.Contains(y.VirtualPath, Comparers.VirtualPath))
                return 1;
            if (yDepends.Contains(x.VirtualPath, Comparers.VirtualPath))
                return -1;

            return 0;
        }

        internal int Comparer(string virtualPath1, string virtualPath2)
        {
            if (virtualPath1.EqualsVirtualPath(virtualPath2))
                return 0;
            
            var xDepends = GetDependencies(virtualPath1);
            var yDepends = GetDependencies(virtualPath2);

            if (xDepends.Contains(virtualPath2, Comparers.VirtualPath))
                return 1;
            if (yDepends.Contains(virtualPath1, Comparers.VirtualPath))
                return -1;

            return 0;
        }

        private bool IsConsolidatedUrl(string virtualPath, out IEnumerable<IResource> resourcesInGroup)
        {
            if (IsConsolidatedUrl(virtualPath, _scriptGroups, out resourcesInGroup))
                return true;
            if (IsConsolidatedUrl(virtualPath, _styleGroups, out resourcesInGroup))
                return true;

            return false;
        }

        private bool IsConsolidatedUrl(string virtualPath, IResourceGroupManager groupTemplates, out IEnumerable<IResource> resourcesInGroup)
        {
            var group = groupTemplates.GetGroupOrDefault(virtualPath, _resourceFinder);
            
            if (group == null)
            {
                resourcesInGroup = null;
                return false;
            }
            
            resourcesInGroup = group.GetResources().SortByDependencies(this);
            return true;
        }

        private void AccumulateDependencies(List<IEnumerable<string>> dependencyList, string virtualPath)
        {
            IEnumerable<string> cachedDependencies;
            if (_dependencyCache.TryGetDependencies(virtualPath, out cachedDependencies))
            {
                dependencyList.Insert(0, cachedDependencies);
                return;
            }
            
            var resource = _resourceFinder.FindResource(virtualPath);
            if(resource == null)
            {
                var globalDependencies = GlobalDependenciesFor(virtualPath);
                dependencyList.Insert(0, globalDependencies);
                return;
            }

            AccumulateDependencies(dependencyList, resource);
        }

        private void AccumulateDependencies(List<IEnumerable<string>> dependencyList, IResource resource)
        {
            IEnumerable<string> cachedDependencies;
            if (_dependencyCache.TryGetDependencies(resource, out cachedDependencies))
            {
                dependencyList.Insert(0, cachedDependencies);
                //store in cache so that it will also be indexed by virtual path
                _dependencyCache.StoreDependencies(resource, cachedDependencies);
                return;
            }

            var dependencyListEntrySize = dependencyList.Count;
            
            IDependencyProvider provider;
            if (_providers.TryGetValue(resource.FileExtension, out provider))
            {

                var dependencies = provider.GetDependencies(resource).ToList();
                if (dependencies.Any())
                {
                    dependencyList.Insert(0, dependencies);
                    foreach (var dependency in dependencies)
                    {
                        AccumulateDependencies(dependencyList, dependency);
                    }
                }
            }

            var globalDependencies = GlobalDependenciesFor(resource.VirtualPath);
            if(globalDependencies.Any())
            {
                dependencyList.Insert(0, globalDependencies);
            }
            
            var dependenciesForCurrentResource = CollapseDependencies(dependencyList.Take(dependencyList.Count - dependencyListEntrySize));
            _dependencyCache.StoreDependencies(resource, dependenciesForCurrentResource);
        }

        private IEnumerable<string> CollapseDependencies(IEnumerable<IEnumerable<string>> dependencyList)
        {
            return dependencyList.SelectMany(d => d)
                .Distinct(Comparers.VirtualPath)
                .ToList();
        }

        private IEnumerable<string> GlobalDependenciesFor(string path)
        {
            var resourceType = ResourceType.FromPath(path);
            IResourceGroupManager groupManager = null;
            if(resourceType == ResourceType.Script)
            {
                groupManager = _scriptGroups;
            }
            else if(resourceType == ResourceType.Stylesheet)
            {
                groupManager = _styleGroups;
            }
            
            if(groupManager != null)
            {
                var allGlobalDependencies = groupManager.GetGlobalDependencies();
                return allGlobalDependencies.TakeWhile(p => !p.Equals(path, Comparisons.VirtualPath));
            }

            return Enumerable.Empty<string>();
        }
    }

    public static class DependencyManagerExtensions
    {
        //we use a partial order by here, as we want to do a topological sort based on our dependency graph
        //the resources are already sorted within their groups (by the order in which they are included in the config),
        //so we should try to preserve that order unless the dependencies instruct otherwise
        
        public static IEnumerable<IResource> SortByDependencies(this IEnumerable<IResource> resources, DependencyManager dependencyManager)
        {
            return resources.PartialOrderBy(dependencyManager.Comparer);
        }

        public static IEnumerable<string> SortByDependencies(this IEnumerable<string> resourcePaths, DependencyManager dependencyManager)
        {
            return resourcePaths.PartialOrderBy(dependencyManager.Comparer);
        }
    }
}