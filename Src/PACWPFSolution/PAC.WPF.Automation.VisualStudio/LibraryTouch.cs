using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAC.WPF.Automation.VisualStudio
{
    /// <summary>
    /// HotFix for the packager will be removed once done.
    /// </summary>
    public class LibraryTouch
    {
        /// <summary>
        /// Touches the libraries to make sure the packager calls them
        /// </summary>
        public void Touch()
        {
            var netData = CodeFactoryExtensions.Net.Common.Automation.DependencyInjection.IsTransientClass(null);

            var aspNetData = CodeFactoryExtensions.Net.AspNet.Automation.DependencyInjection
                .DependencyInjectionIgnoreBaseClasses();
            var cSharpFormatter = new CodeFactoryExtensions.Formatting.CSharp.NamespaceManager(null, null);
            var cdfAutomation = CommonDeliveryFramework.Automation.CommonDeliveryFrameworkHelpers
                .HasCommonDeliveryFrameworkAsync(null);
            var commonAutomation =
                CodeFactoryExtensions.Common.Automation.CsContainerExtensions.HasInterface(null, null);
        }
    }
}
