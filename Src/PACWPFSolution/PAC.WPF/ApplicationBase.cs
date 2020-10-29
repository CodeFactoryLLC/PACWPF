using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PAC.WPF
{
    /// <summary>
    /// Application implementation for a PAC based application.
    /// </summary>
    public abstract class PACApplication:Application,IApplication
    {
        #region Backing fields for properties
        private IConfiguration _configuration;
        private IServiceProvider _serviceProvider;
        #endregion

        #region Implementation of IApplication

        /// <summary>
        /// Holds the application configuration data from the system.
        /// </summary>
        public IConfiguration Configuration => _configuration;

        /// <summary>
        /// Holds the service provider loaded for dependency management.
        /// </summary>
        public IServiceProvider ServiceProvider => _serviceProvider;

        /// <summary>
        /// Loads a service from the dependency management container.
        /// </summary>
        /// <typeparam name="T">The target type of the object to load from dependency management.</typeparam>
        /// <returns>Instance of the object or null if the object was not found in the dependency container.</returns>
        public T GetService<T>() where T : class
        {
            return _serviceProvider.GetRequiredService<T>();
        }

        /// <summary>
        /// Load application configuration information into the configuration store.
        /// </summary>
        /// <param name="config">The configuration store to populate.</param>
        public abstract void LoadApplicationConfiguration(ConfigurationBuilder config);

        /// <summary>
        /// Loads the application configuration and initializes all PAC's to be used in the system.
        /// </summary>
        /// <param name="config">Stores the application configuration from the system itself.</param>
        /// <param name="serviceCollection">The service collection to register all dependency objects</param>
        public abstract void StartupConfiguration(IConfiguration config, IServiceCollection serviceCollection);

        /// <summary>
        /// Called by the hosting application to reference the window that will be the starting
        /// </summary>
        /// <returns>The Hosting window for the application.</returns>
        public abstract Window ApplicationStartup();
        
        #endregion

        /// <summary>
        /// Starts the PAC based application.
        /// </summary>
        /// <param name="sender">Application Host</param>
        /// <param name="e">Parameters from the startup operation</param>
        public void App_Startup(object sender, StartupEventArgs e)
        {
            string[] commandLineArguments = null;

            if (e != null) commandLineArguments = e.Args;

            try
            {
                LoadConfiguration(commandLineArguments);
                var serviceCollection = new ServiceCollection();
                StartupConfiguration(_configuration, serviceCollection);
                _serviceProvider = serviceCollection.BuildServiceProvider(true);
                var hostingWindow = ApplicationStartup();
                this.MainWindow = hostingWindow;
                hostingWindow.Show();
            }
            catch (Exception unhandledException)
            {
                //Add exception handling
            }
        }

        /// <summary>
        /// Loads the application settings from the 
        /// </summary>
        private void LoadConfiguration( string[] commandLineArguments)
        {
            ConfigurationBuilder builder = new ConfigurationBuilder();

             bool useAppSettings = false;
             try
             {
                 var currentDirectory = Directory.GetCurrentDirectory();
                 if (!string.IsNullOrEmpty(currentDirectory))
                 {
                     useAppSettings = true;
                     builder.SetBasePath(currentDirectory);
                     builder.AddJsonFile("appsettings.json", true);
                 }
             }
             catch (NotSupportedException)
             {
                 //Add Exception Logic
             }
             catch (Exception unhandledError)
             {
                 //Intentionally blank
             }

             try
             {
                 if (commandLineArguments != null) builder.AddCommandLine(commandLineArguments);
                 LoadApplicationConfiguration(builder);
             }
             catch (Exception unhandledConfigurationErrors)
             {
                 //Intentionally blank
             }

             try
             {
                 _configuration = builder.Build();
             }
             catch (Exception configurationException)
             {
                 //Add Exception Logic
             }

        }
    }
}
