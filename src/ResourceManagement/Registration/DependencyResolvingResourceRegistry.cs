using System;
using System.Collections.Generic;
using System.IO;

using AlmWitt.Web.ResourceManagement.Configuration;

namespace AlmWitt.Web.ResourceManagement.Registration
{
	public class DependencyResolvingResourceRegistry : IReadableResourceRegistry
	{
		private readonly IResourceRegistry _inner;
		private readonly ResourceManagementContext _context;

		public DependencyResolvingResourceRegistry(IResourceRegistry inner, ResourceManagementContext context)
		{
			_inner = inner;
			_context = context;
		}

		public bool TryResolvePath(string path, out IEnumerable<string> resolvedVirtualPaths)
		{
			return _inner.TryResolvePath(path, out resolvedVirtualPaths);
		}

		public void IncludePath(string urlToInclude)
		{
			foreach(var dependency in _context.GetResourceDependencies(urlToInclude))
			{
				_inner.IncludePath(dependency);
			}
			_inner.IncludePath(urlToInclude);
		}

		public void RegisterInlineBlock(Action<TextWriter> block, object key)
		{
			_inner.RegisterInlineBlock(block, key);
		}

		public bool IsInlineBlockRegistered(object key)
		{
			return _inner.IsInlineBlockRegistered(key);
		}

		public IEnumerable<string> GetIncludes()
		{
			return _inner.AsReadable().GetIncludes();
		}

		public IEnumerable<Action<TextWriter>> GetInlineBlocks()
		{
			return _inner.AsReadable().GetInlineBlocks();
		}
	}
}