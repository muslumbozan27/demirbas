using System;

using AlmWitt.Web.ResourceManagement.Configuration;

namespace AlmWitt.Web.ResourceManagement.BuildSupport
{
    /// <summary>
    /// Consolidates javascript and css resources into fewer files according to the
    /// resource management configuration in the web.config.
    /// </summary>
    public class PreConsolidateCommand
    {
        /// <summary>
        /// Gets an instance of a <see cref="PreConsolidateCommand"/>.
        /// </summary>
        /// <param name="webRoot">The full physical path to the the root of the website.</param>
        /// <returns></returns>
        public static PreConsolidateCommand GetInstance(string webRoot)
        {
            return new PreConsolidateCommand(webRoot);
        }

        private readonly string _webRoot;
        private VirtualPathResolver _resolver;
        private System.Configuration.Configuration _config;
        private IResourceFinder _finder;
        private ILogger _logger = NullLogger.Instance;

        private PreConsolidateCommand(string webRoot)
        {
            _webRoot = webRoot;
        }

        /// <summary>
        /// Gets or sets the physical path the the root of the website.
        /// </summary>
        public string WebRoot
        {
            get { return _webRoot; }
        }

        /// <summary>
        /// Gets or sets the build logger used to log the progress of the command.
        /// </summary>
        public ILogger Logger
        {
            get { return _logger; }
            set { _logger = value; }
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns></returns>
        public void Execute()
        {
            LogMessage("Begin consolidating resources...");
            ResourceManagementConfiguration configSection = GetConfigSection(WebRoot);
            _resolver = GetResolver(configSection.RootFilePath);
            _finder = ResourceFinderFactory.GetInstance(configSection.RootFilePath);
            _finder = configSection.AddCustomFinders(_finder);
            configSection.ClientScripts.ProcessEach((groupElement, excludeFilter) => ConsolidateGroup(groupElement, excludeFilter, ".js"));
            configSection.CssFiles.ProcessEach((groupElement, excludeFilter) => ConsolidateGroup(groupElement, excludeFilter, ".css"));
            configSection.PreConsolidated = true;
            SaveConfigChanges(configSection);
            LogMessage("End consolidating resources.");
        }

        private void ConsolidateGroup(ResourceGroupElement groupElement, IResourceFilter excludeFilter, string extension)
        {
            var consolidatedResource = groupElement.GetResource(_finder, extension, excludeFilter);
            string consolidatedPath = _resolver.MapPath(groupElement.ConsolidatedUrl);
            consolidatedResource.WriteToFile(consolidatedPath);
            LogConsolidation(consolidatedResource, groupElement.ConsolidatedUrl);
        }

        private void LogConsolidation(ConsolidatedResource consolidatedResource, string consolidatedUrl)
        {
            LogMessage(String.Format("Consolidating '{0}'...", consolidatedUrl));
            foreach (IResource resource in consolidatedResource.Resources)
            {
                LogMessage(String.Format("\t...from '{0}'", resource.VirtualPath));
            }
        }

        private void LogMessage(string message)
        {
            _logger.LogMessage(message);
        }

        private ResourceManagementConfiguration GetConfigSection(string webRoot)
        {
            ResourceManagementConfiguration.ConfigLoader = new MappedConfigLoader(webRoot);

            ResourceManagementConfiguration configSection = ResourceManagementConfiguration.OpenForEditing(out _config);
            configSection.RootFilePath = webRoot;
            //set this instance of the config section to current so that the components
            //that need to read from configuration can see it.
            ResourceManagementConfiguration.Current = configSection;

            return configSection;
        }

        private void SaveConfigChanges(ResourceManagementConfiguration configSection)
        {
            configSection.SaveChanges(_config);
        }

        private VirtualPathResolver GetResolver(string webRoot)
        {
            return new VirtualPathResolver(webRoot);
        }
    }
}