using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeFactory.DotNet.CSharp;
using CodeFactory.VisualStudio;

namespace CommonDeliveryFramework.Automation
{
    /// <summary>
    /// Helper class that contains common functions used with the common delivery framework.
    /// </summary>
    public static class CommonDeliveryFrameworkHelpers
    {
        /// <summary>
        /// Checks to see if the CommonDeliverFramework library is associated with this project.
        /// </summary>
        /// <param name="sourceProject">Project to check.</param>
        /// <returns>True if library is used.</returns>
        public static async Task<bool> HasCommonDeliveryFrameworkAsync(VsProject sourceProject) => sourceProject == null ? false : await sourceProject.HasReferenceLibraryAsync(CommonDeliveryFrameworkConstants.CommonDeliveryFrameworkAssemblyName);


        /// <summary>
        /// Adds a using statement to the C# source code for the CommonDeliveryFramework.
        /// </summary>
        /// <param name="source">The source code to validate.</param>
        /// <returns>Updated instance of the source code once the namespace has been added, will return the original instance if the namespace has already been registered.</returns>
        public static async Task<CsSource> AddCommonDeliveryFrameworkNamespaceAsync(CsSource source) =>await source.AddUsingStatementAsync(CommonDeliveryFrameworkConstants.CommonDeliveryFrameworkNamespace);
    }
}
