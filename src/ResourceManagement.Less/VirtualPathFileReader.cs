using System.IO;

using AlmWitt.Web.ResourceManagement.Configuration;

using dotless.Core.Input;

namespace AlmWitt.Web.ResourceManagement.Less
{
	public class VirtualPathFileReader : IFileReader
	{
		private readonly VirtualPathResolver _pathResolver;

		public VirtualPathFileReader()
		{
			var config = ResourceManagementConfiguration.Current;
			_pathResolver = VirtualPathResolver.GetInstance(config.RootFilePath);
		}

		public string GetFileContents(string fileName)
		{
			var resolvedPath = _pathResolver.MapPath(fileName);

			return File.ReadAllText(resolvedPath);
		}
	}
}