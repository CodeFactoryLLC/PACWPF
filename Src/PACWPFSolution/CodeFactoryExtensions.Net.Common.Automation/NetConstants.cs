namespace CodeFactoryExtensions.Net.Common.Automation
{
    /// <summary>
    /// Constants that support the Net implementation 
    /// </summary>
    public static class NetConstants
    {
        /// <summary>
        /// Logging library name
        /// </summary>
        public const string MicrosoftExtensionsLoggingLibraryName = "Microsoft.Extensions.Logging.Abstractions";

        /// <summary>
        /// Dependency injection library name
        /// </summary>
        public const string MicrosoftExtensionsDependencyInjectionLibraryName = "Microsoft.Extensions.DependencyInjection.Abstractions";

        /// <summary>
        /// Configuration library name
        /// </summary>
        public const string MicrosoftExtensionsConfigurationLibraryName = "Microsoft.Extensions.Configuration.Abstractions";

        /// <summary>
        /// Namespace for tasks 
        /// </summary>
        public const string SystemThreadingTasksNamespace = "System.Threading.Tasks";

        /// <summary>
        /// Class name for the Task class.
        /// </summary>
        public const string TaskClassName = "Task";

        /// <summary>
        /// The default namespace for the Microsoft extensions logger implementation
        /// </summary>
        public const string MicrosoftLoggerNamespace = "Microsoft.Extensions.Logging";

        /// <summary>
        /// The name of the interface for the Microsoft extensions logger.
        /// </summary>
        public const string MicrosoftLoggerInterfaceName = "ILogger";

        /// <summary>
        /// Defined standard for the logger field when one is not provided.
        /// </summary>
        public const string DefaultClassLoggerName = "_logger";

        /// <summary>
        /// Default name to use for library based dependency injection register class.
        /// </summary>
        public const string DefaultDependencyInjectionClassName = "RegisterLibraryServices";

        /// <summary>
        /// Default name to use for the method name that does automatic transient registration for the class.
        /// </summary>
        public const string DefaultAutomaticTransientClassRegistrationMethodName = "AutomaticTransientServicesRegistration";

    }
}
