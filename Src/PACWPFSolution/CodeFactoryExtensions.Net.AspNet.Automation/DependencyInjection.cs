using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeFactoryExtensions.Common.Automation;

namespace CodeFactoryExtensions.Net.AspNet.Automation
{
    /// <summary>
    /// Additional support for dependency injection for AspNet based projects.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Gets a list of standard class names that should not be registered for dependency injection in a AspNet based project.
        /// </summary>
        /// <returns></returns>
        public static List<string> DependencyInjectionIgnoreClasses()
        {
            return new List<string>{"Startup"};
        }


        /// <summary>
        /// Gets a list of standard base classes that should not be registered for dependency injection in a AspNet based project.
        /// </summary>
        /// <returns></returns>
        public static List<ModelLookupData> DependencyInjectionIgnoreBaseClasses()
        {
            return new List<ModelLookupData>{ModelLookupData.Init(AspNetConstants.MvcNamespace,AspNetConstants.ControllerBaseClassName)};
        }

    }
}
