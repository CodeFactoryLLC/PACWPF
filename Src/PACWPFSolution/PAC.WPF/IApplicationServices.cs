using System;
using Microsoft.Extensions.Configuration;

namespace PAC.WPF
{
    /// <summary>
    /// Definition of the configuration and service provider to be implemented in the application.
    /// </summary>
    public interface IApplicationServices
    {
        /// <summary>
        /// Holds the application configuration data from the system.
        /// </summary>
        IConfiguration Configuration { get; }

        /// <summary>
        /// Holds the service provider loaded for dependency management.
        /// </summary>
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Loads a service from the dependency management container.
        /// </summary>
        /// <typeparam name="T">The target type of the object to load from dependency management.</typeparam>
        /// <returns>Instance of the object or null if the object was not found in the dependency container.</returns>
        T GetService<T>() where T : class;
    }
}
