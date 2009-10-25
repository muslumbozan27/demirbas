using System;

namespace AlmWitt.Web.ResourceManagement
{
	/// <summary>
	/// Represents a resource used on the web.
	/// </summary>
	public interface IResource
	{
		/// <summary>
		/// Gets the name of the resource.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets the Virtual Path of the resource.
		/// </summary>
		string VirtualPath { get; }

		/// <summary>
		/// Gets the date that the resource was last modified.
		/// </summary>
		DateTime LastModified { get; }

		/// <summary>
		/// Gets the content of the resource.
		/// </summary>
		/// <returns></returns>
		string GetContent();
	}
}