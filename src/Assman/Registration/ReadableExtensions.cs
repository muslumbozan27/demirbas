using System;
using System.Collections.Generic;
using System.IO;

namespace Assman.Registration
{
	public static class ReadableExtensions
	{
		public static IReadableResourceRegistry AsReadable(this IResourceRegistry resourceRegistry)
		{
			if (resourceRegistry is IReadableResourceRegistry)
				return (IReadableResourceRegistry)resourceRegistry;

			return new MakeReadableResourceRegistry(resourceRegistry);
		}

		private class MakeReadableResourceRegistry : IReadableResourceRegistry
		{
			private readonly IResourceRegistry _inner;
			public MakeReadableResourceRegistry(IResourceRegistry inner)
			{
				_inner = inner;
			}

			public bool TryResolvePath(string resourcePath, out IEnumerable<string> resolvedResourcePaths)
			{
				return _inner.TryResolvePath(resourcePath, out resolvedResourcePaths);
			}

			public IEnumerable<string> GetIncludes()
			{
				return ReadableOrDefault().GetIncludes();
			}

			public IEnumerable<Action<TextWriter>> GetInlineBlocks()
			{
				return ReadableOrDefault().GetInlineBlocks();
			}

			public void Require(string resourcePath)
			{
				_inner.Require(resourcePath);
			}

			public void RegisterInlineBlock(Action<TextWriter> block, object key)
			{
				_inner.RegisterInlineBlock(block, key);
			}

			public bool IsInlineBlockRegistered(object key)
			{
				return _inner.IsInlineBlockRegistered(key);
			}

			private IReadableResourceRegistry ReadableOrDefault()
			{
				return _inner is IReadableResourceRegistry ? (IReadableResourceRegistry)_inner : NullResourceRegistry.Instance;
			}
		}
	}
}