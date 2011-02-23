using System;
using System.Collections.Generic;
using System.Web;

namespace AlmWitt.Web.ResourceManagement
{
	public class HttpDependencyCache : IDependencyCache
	{
		private static readonly HttpDependencyCache _instance = new HttpDependencyCache(() => new HttpContextWrapper(HttpContext.Current));
		public static HttpDependencyCache GetInstance()
		{
			return _instance;
		}
		
		private readonly Func<HttpContextBase> _httpContextAccessor;
		private readonly IDictionary<string, CacheItem> _longRunningCache = new Dictionary<string, CacheItem>(StringComparer.OrdinalIgnoreCase);
		private static readonly Type _httpItemsKey = typeof (HttpDependencyCache);

		public HttpDependencyCache(Func<HttpContextBase> httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
		}

		public bool TryGetDependencies(string virtualPath, out IEnumerable<string> dependencies)
		{
			var cacheByVirtualPath = GetCacheKeyedByVirtualPath();
			return cacheByVirtualPath.TryGetValue(virtualPath, out dependencies);
		}

		public bool TryGetDependencies(IResource resource, out IEnumerable<string> dependencies)
		{
			CacheItem cacheItem;
			if (_longRunningCache.TryGetValue(resource.VirtualPath, out cacheItem))
			{
				if(resource.LastModified <= cacheItem.LastModified)
				{
					dependencies = cacheItem.Dependencies;
					return true;
				}
			}

			dependencies = null;
			return false;
		}

		public void StoreDependencies(IResource resource, IEnumerable<string> dependencies)
		{
			var cacheByVirtualPath = GetCacheKeyedByVirtualPath();
			cacheByVirtualPath[resource.VirtualPath] = dependencies;

			_longRunningCache[resource.VirtualPath] = new CacheItem
			{
				Dependencies = dependencies,
				LastModified = resource.LastModified
			};
		}

		private IDictionary<string,IEnumerable<string>> GetCacheKeyedByVirtualPath()
		{
			var httpContext = _httpContextAccessor();
			var cache = httpContext.Items[_httpItemsKey] as IDictionary<string, IEnumerable<string>>;
			if(cache == null)
			{
				cache = new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase);
				httpContext.Items[_httpItemsKey] = cache;
			}

			return cache;
		} 

		private class CacheItem
		{
			public IEnumerable<string > Dependencies { get; set; }
			public DateTime LastModified { get; set; }
		}
	}
}