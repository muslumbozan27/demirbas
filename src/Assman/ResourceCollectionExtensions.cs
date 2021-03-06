using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Assman
{
	public static class ResourceCollectionExtensions
	{
		public static ResourceCollection ToResourceCollection(this IEnumerable<IResource> resources)
		{
			var resourceCollection = new ResourceCollection();
			resourceCollection.AddRange(resources);

			return resourceCollection;
		}

		/// <summary>
		/// Adds a collection of resources to this collection.
		/// </summary>
		public static void AddRange(this IList<IResource> list, IEnumerable<IResource> resourcesToAdd)
		{
			foreach (IResource resource in resourcesToAdd)
			{
				list.Add(resource);
			}
		}

		public static IEnumerable<IResource> Where(this IEnumerable<IResource> resources, IResourceFilter filter)
		{
			return resources.Where(filter.IsMatch);
		}

		public static IEnumerable<IResource> Exclude(this IEnumerable<IResource> resources, IResourceFilter excludeFilter)
		{
			return resources.Where(ResourceFilters.Not(excludeFilter));
		}

		/// <summary>
		/// Returns the most recent last modified date of the contained resources.
		/// </summary>
		public static DateTime LastModified(this IEnumerable<IResource> resources)
		{
			if (!resources.Any())
				return DateTime.MinValue;

			return resources.Max(r => r.LastModified);
		}

		/// <summary>
		/// Writes the contents of all of the contained resources to the given <see cref="TextWriter"/>.
		/// </summary>
		public static void ConsolidateContentTo(this IEnumerable<IResource> resources, TextWriter writer)
		{
			resources.ConsolidateContentTo(writer, String.Empty);
		}

		/// <summary>
		/// Writes the contents of all of the contained resources to the given <see cref="TextWriter"/>.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="separator">A string that will be between each resource.</param>
		/// <param name="resources"></param>
		public static void ConsolidateContentTo(this IEnumerable<IResource> resources, TextWriter writer, string separator)
		{
			resources.ConsolidateContentTo(writer, null, separator);
		}

	    /// <summary>
	    /// Writes the contents of all of the contained resources to the given <see cref="TextWriter"/>.
	    /// </summary>
	    /// <param name="resources"></param>
	    /// <param name="writer"></param>
	    /// <param name="getResourceContent"></param>
	    public static void ConsolidateContentTo(this IEnumerable<IResource> resources, TextWriter writer, Func<IResource, string> getResourceContent)
		{
			resources.ConsolidateContentTo(writer, getResourceContent, null);
		}

	    /// <summary>
	    /// Writes the contents of all of the contained resources to the given <see cref="TextWriter"/>.
	    /// </summary>
	    /// <param name="resources"></param>
	    /// <param name="writer"></param>
	    /// <param name="getResourceContent"></param>
	    /// <param name="separator">A string that will be between each resource.</param>
	    public static void ConsolidateContentTo(this IEnumerable<IResource> resources, TextWriter writer, Func<IResource, string> getResourceContent, string separator)
		{
			if (writer == null)
				throw new ArgumentNullException("writer");
			if (getResourceContent == null)
				getResourceContent = r => r.GetContent();
			if (separator == null)
				separator = String.Empty;

			bool first = true;
			foreach (IResource resource in resources)
			{
				if (first)
					first = false;
				else
					writer.Write(separator);
				var content = getResourceContent(resource);
				writer.Write(content);
			}
		}

	    /// <summary>
	    /// Consolidated all of the resources in the collection into a <see cref="ConsolidatedResource"/>.
	    /// </summary>
	    /// <param name="resources"></param>
	    /// <param name="group"></param>
	    /// <param name="getResourceContent"></param>
	    /// <param name="separator"></param>
	    public static ICompiledResource Consolidate(this IEnumerable<IResource> resources, IResourceGroup group, Func<IResource, string> getResourceContent, string separator)
		{
			var contentStream = new MemoryStream();
			var writer = new StreamWriter(contentStream);
			resources.ConsolidateContentTo(writer, getResourceContent, separator);
			writer.Flush();

			return new ConsolidatedResource(group, resources.ToResourceCollection(), contentStream);
		}
	}
}