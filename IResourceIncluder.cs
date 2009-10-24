using System;
using System.Web.UI;
using System.Reflection;

namespace AlmWitt.Web.ResourceManagement
{
	/// <summary>
	/// Represents an object that manages the inclusion of resources.
	/// </summary>
	public interface IResourceIncluder
	{
		/// <summary>
		/// Resolves a virtual or relative url to one that is usable by the web browser.
		/// </summary>
		/// <param name="virtualPath">A virtual or relative url.</param>
		/// <returns></returns>
		string ResolveUrl(string virtualPath);
		
		/// <summary>
		/// Gets a url that will return the contents of the specified embedded resource.
		/// </summary>
		/// <param name="assemblyName">The name of the assembly that the resource is embedded in.</param>
		/// <param name="resourceName">The name of the embedded resource.</param>
		/// <returns></returns>
		string GetEmbeddedResourceUrl(string assemblyName, string resourceName);
		
		/// <summary>
		/// Includes the resource on the current web page.
		/// </summary>
		/// <param name="urlToInclude">The url of the resource to be included.</param>
		void IncludeUrl(string urlToInclude);
	}
}
