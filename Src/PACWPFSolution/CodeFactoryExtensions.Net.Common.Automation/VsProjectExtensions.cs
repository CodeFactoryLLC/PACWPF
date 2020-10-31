using System.Linq;
using System.Threading.Tasks;
using CodeFactory.VisualStudio;
namespace CodeFactoryExtensions.Net.Common.Automation
{
    
    /// <summary>
    /// Extensions class that supports the <see cref="VsProject"/> model.
    /// </summary>
    public static class VsProjectExtensions
    {
        /// <summary>
        /// Determines if the project supports the Microsoft.Extensions.Logging.Abstractions library.
        /// </summary>
        /// <param name="source">Target project to search.</param>
        /// <returns>True if found or false if not.</returns>
        public static async Task<bool> SupportsMicrosoftExtensionsLoggingAsync(this VsProject source)
        {
            return await source.HasReferenceLibraryAsync(NetConstants.MicrosoftExtensionsLoggingLibraryName);
        }

        /// <summary>
        /// Determines if the project supports the Microsoft.Extensions.DependencyInjection.Abstractions library.
        /// </summary>
        /// <param name="source">Target project to search.</param>
        /// <returns>True if found or false if not.</returns>
        public static async Task<bool> SupportsMicrosoftExtensionsDependencyInjectionAsync(this VsProject source)
        {
            return await source.HasReferenceLibraryAsync(NetConstants.MicrosoftExtensionsDependencyInjectionLibraryName);
        }

        /// <summary>
        /// Determines if the project supports the Microsoft.Extensions.Logging.Abstractions library.
        /// </summary>
        /// <param name="source">Target project to search.</param>
        /// <returns>True if found or false if not.</returns>
        public static async Task<bool> SupportsMicrosoftExtensionsConfigurationAsync(this VsProject source)
        {
            return await source.HasReferenceLibraryAsync(NetConstants.MicrosoftExtensionsConfigurationLibraryName);
        }

        /// <summary>
        /// Helper method that confirms a target project supports the microsoft extensions for dependency injection and Configuration.
        /// </summary>
        /// <param name="sourceProject">Target project to check.</param>
        /// <returns>True if found or false of not.</returns>
        public static async Task<bool> HasMicrosoftExtensionDependencyInjectionLibrariesAsync(VsProject sourceProject)
        {
            if (sourceProject == null) return false;
            if (!sourceProject.IsLoaded) return false;
            var references = await sourceProject.GetProjectReferencesAsync();

            //Checking for dependency injection libraries.
            bool returnResult = references.Any(r => r.Name == NetConstants.MicrosoftExtensionsDependencyInjectionLibraryName);
            if (!returnResult) return false;

            //Checking for the configuration libraries.
            returnResult = references.Any(r => r.Name == NetConstants.MicrosoftExtensionsConfigurationLibraryName);
            return returnResult;
        }
    }
}
