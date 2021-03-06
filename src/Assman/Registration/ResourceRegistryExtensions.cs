using System;
using System.Collections.Generic;
using System.IO;

using Assman.Configuration;

namespace Assman.Registration
{
	public static class ResourceRegistryExtensions
	{
		public static void RequireEmbeddedResource(this IResourceRegistry registry, Type type, string resourceName)
		{
			registry.RequireEmbeddedResource(type.Assembly.GetName().Name, resourceName);
		}

		public static void RequireEmbeddedResource(this IResourceRegistry registry, string assemblyName, string resourceName)
		{
			var urls = GetEmbeddedResourceUrls(registry, assemblyName, resourceName);
		    foreach (var url in urls)
		    {
		        registry.Require(url);
		    }
		}

		/// <summary>
		/// Registers an inline block that will appear inline on the page directly below the includes of this <see cref="IResourceRegistry"/>.
		/// </summary>
		/// <param name="registry"></param>
		/// <param name="block">The inline css or javascript that will appear on the page.</param>
		/// <param name="key">A unique key used to identify the inline block.  This is optional and can be set to <c>null</c>.</param>
		public static void RegisterInlineBlock(this IResourceRegistry registry, string block, object key)
		{
			registry.RegisterInlineBlock(w => w.Write(block), key);
		}

		public static void RegisterInlineBlock(this IResourceRegistry registry, Action<TextWriter> block)
		{
			registry.RegisterInlineBlock(block, null);
		}

		public static void RegisterInlineBlock(this IResourceRegistry registry, string block)
		{
			registry.RegisterInlineBlock(block, null);
		}

		private static IEnumerable<string> GetEmbeddedResourceUrls(IResourceRegistry registry, string assemblyName, string resourceName)
		{
			string shortAssemblyName = assemblyName.ToShortAssemblyName();
			string virtualPath = EmbeddedResource.GetVirtualPath(shortAssemblyName, resourceName);
			IEnumerable<string> resolvedUrls;
			if (!registry.TryResolvePath(virtualPath, out resolvedUrls))
			{
				throw new InvalidOperationException(
					@"Cannot include embedded resource because it has not been configured in the Assman.config to be consolidated anywhere.
					Please add an include rule that matches the path 'assembly://" +
					assemblyName + "/" + resourceName + "'.");
			}
			if(!AssmanConfiguration.Current.Assemblies.Contains(shortAssemblyName))
			{
				throw new InvalidOperationException(@"Cannot include embedded resource because the assembly has not been configured in the Assman.config.  If you would like to embed a resource from the assembly '"
				                                    + assemblyName + "' then please add it to the <assemblies> list.");
			}

			return resolvedUrls;
		}
	}
}