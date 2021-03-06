using System;

namespace Assman.Registration
{
	public interface IResourceRegistryAccessor
	{
		IResourceRegistry ScriptRegistry { get; }

		IResourceRegistry NamedScriptRegistry(string name);

		IResourceRegistry StyleRegistry { get; }

		IResourceRegistry NamedStyleRegistry(string name);

		RegisteredResources GetRegisteredScripts(string registryName);

		RegisteredResources GetRegisteredStyles(string registryName);
	}
}