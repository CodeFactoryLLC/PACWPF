using System;
using System.ComponentModel;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PAC.WPF
{
    /// <summary>
    /// Contract implementation that provides Presentation Abstraction Control information in the hosting application
    /// </summary>
    public interface IApplication:IApplicationServices
    {
        /// <summary>
        /// Load application configuration information into the configuration store.
        /// </summary>
        /// <param name="config">The configuration store to populate.</param>
        void LoadApplicationConfiguration(ConfigurationBuilder config);


        /// <summary>
        /// Loads the application configuration and initializes all PAC's to be used in the system.
        /// </summary>
        /// <param name="config">Stores the application configuration from the system itself.</param>
        /// <param name="serviceCollection">The service collection to register all dependency objects</param>
        void StartupConfiguration(IConfiguration config, IServiceCollection serviceCollection);


        /// <summary>
        /// Called by the hosting application to reference the window that will be the starting
        /// </summary>
        /// <returns>The Hosting window for the application.</returns>
        Window ApplicationStartup();

    }
}