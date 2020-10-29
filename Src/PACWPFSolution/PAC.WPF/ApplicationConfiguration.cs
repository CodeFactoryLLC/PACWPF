using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Configuration;

namespace PAC.WPF
{
    /// <summary>
    /// Provides application level access to the configuration and loaded services.
    /// </summary>
    public static class ApplicationConfiguration
    {
        /// <summary>
        /// Holds the application configuration data from the system.
        /// </summary>
        public static IConfiguration GetConfiguration()
        {
            var services = Application.Current as IApplicationServices;

            return services?.Configuration;
        }

        /// <summary>
        /// Holds the application configuration data from the system.
        /// </summary>
        public static IConfiguration Configuration 
        {
            get
            {
                var services = Application.Current as IApplicationServices;
                return services?.Configuration;
            }
        }

        /// <summary>
        /// Holds the service provider loaded for dependency management.
        /// </summary>
        public static IServiceProvider ServiceProvider 
        {
            get
            {
                var services = Application.Current as IApplicationServices;
                return services?.ServiceProvider;
            }

        }

        /// <summary>
        /// Loads a service from the dependency management container.
        /// </summary>
        /// <typeparam name="T">The target type of the object to load from dependency management.</typeparam>
        /// <returns>Instance of the object or null if the object was not found in the dependency container.</returns>
        public static T GetService<T>() where T : class
        {
            var services = Application.Current as IApplicationServices;
            return services?.GetService<T>();
        }
    }
}
