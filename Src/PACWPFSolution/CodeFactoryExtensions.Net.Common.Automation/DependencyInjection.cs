using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using CodeFactory;
using CodeFactory.DotNet.CSharp;
using CodeFactory.DotNet.CSharp.FormattedSyntax;
using CodeFactory.VisualStudio;
using CodeFactoryExtensions.Common.Automation;
using CodeFactoryExtensions.Formatting.CSharp;

namespace CodeFactoryExtensions.Net.Common.Automation
{
    /// <summary>
    /// Code Automation for generation of dependency injection in your Net projects.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Builds the services registration method. This will contain the transient registrations for each class in the target project.
        /// This will return a signature of [Public/Private] [static] void [methodName](IServiceCollection [collectionParameterName])
        /// With a body that contains the full transient registrations.
        /// </summary>
        /// <param name="registrations">The registrations to be added.</param>
        /// <param name="isPublicMethod">Flag that determines if the method is public or private in scope.</param>
        /// <param name="isStatic">Flag to determine if the method should be defined as a static or instance method.</param>
        /// <param name="methodName">The target name of the method to be created.</param>
        /// <param name="serviceCollectionParameterName">The name of the service collection parameter where transient registrations will take place.</param>
        /// <param name="manager">The namespace manager that will be used to shorten type name registration with dependency injection. This will need to be loaded from the target class.</param>
        /// <returns>The formatted method.</returns>
        public static string BuildTransientInjectionMethod(IEnumerable<DependencyInjectionRegistrationInformation> registrations, bool isPublicMethod, bool isStatic, string methodName, string serviceCollectionParameterName, NamespaceManager manager = null)
        {

            CodeFactory.SourceFormatter registrationFormatter = new CodeFactory.SourceFormatter();

            string methodSecurity = isPublicMethod ? Security.Public : Security.Private;

            string methodSignature = isStatic
                ? $"{methodSecurity} static void {methodName}(IServiceCollection {serviceCollectionParameterName})"
                : $"{methodSecurity} void {methodName}(IServiceCollection {serviceCollectionParameterName})";

            registrationFormatter.AppendCodeLine(0, "/// <summary>");
            registrationFormatter.AppendCodeLine(0, "/// Automated registration of classes using transient registration.");
            registrationFormatter.AppendCodeLine(0, "/// </summary>");
            registrationFormatter.AppendCodeLine(0, $"/// <param name=\"{serviceCollectionParameterName}\">The service collection to register services.</param>");
            registrationFormatter.AppendCodeLine(0, methodSignature);
            registrationFormatter.AppendCodeLine(0, "{");
            registrationFormatter.AppendCodeLine(1, "//This method was auto generated, do not modify by hand!");
            foreach (var registration in registrations)
            {
                var registrationEntry = FormatTransientRegistrationForDependencyInjection(registration.ClassData, serviceCollectionParameterName,registration.InterfaceData ,manager);
                if (registrationEntry != null) registrationFormatter.AppendCodeLine(1, registrationEntry);
            }
            registrationFormatter.AppendCodeLine(0, "}");

            return registrationFormatter.ReturnSource();
        }

        /// <summary>
        /// Defines the transient registration statement that will register a target class in dependency injection.
        /// </summary>
        /// <param name="classData">The class model to get the registration from.</param>
        /// <param name="serviceCollectionParameterName">The name of the service collection parameter that the transient is being made to.</param>
        /// <param name="targetInterface">The target interface model to use for transient registration, this is an optional parameter </param>
        /// <param name="manager">Optional parameter that contains the namespace manager that contains the known using statements and target namespace for the class that will host this registration data.</param>
        /// <returns>The formatted transient registration call or null if the class data is missing.</returns>
        public static string FormatTransientRegistrationForDependencyInjection(CsClass classData, string serviceCollectionParameterName, CsInterface targetInterface = null, NamespaceManager manager = null)
        {
            //Cannot find the class data will return null
            if (classData == null) return null;

            string registrationType = null;
            string classType = null;

            ICsMethod constructorData = classData.Constructors.FirstOrDefault();

            //Confirming we have a constructor 
            if (constructorData == null) return null;

            //Getting the fully qualified type name for the formatters library for the class.
            classType = classData.CSharpFormatBaseTypeName(manager);

            //if we are not able to format the class name correctly return null.
            if (classType == null) return null;

            //If the interface was provided extract the name for setting up the registration.
            if (targetInterface != null) registrationType = targetInterface.CSharpFormatInheritanceTypeName(manager);

            //Creating statement to add the the container.
            string diStatement = registrationType != null
                ? $"{serviceCollectionParameterName}.AddTransient<{registrationType},{classType}>();" :
                  $"{serviceCollectionParameterName}.AddTransient<{classType}>();";

            return diStatement;
        }

        /// <summary>
        /// Gets a list of all classes in the project that qualify for transient dependency injection registration.
        /// </summary>
        /// <param name="project">Target project to register.</param>
        /// <param name="rejectedClassNames">The class names that are not allowed for dependency injection.</param>
        /// <param name="rejectedBaseClasses">The base classes not allowed for dependency injection.</param>
        /// <param name="targetInterfaceTypeForRegistration">The interfaces of a target type to use for registration, this includes inheriting the target type.</param>
        /// <returns></returns>
        public static async Task<IEnumerable<DependencyInjectionRegistrationInformation>> GetTransientClassesAsync(VsProject project,
            IEnumerable<string> rejectedClassNames = null, IEnumerable<ModelLookupData> rejectedBaseClasses = null,
            IEnumerable<ModelLookupData> targetInterfaceTypeForRegistration = null)
        {
            var result = new List<DependencyInjectionRegistrationInformation>();

            if (project == null) return result;
            if (!project.HasChildren) return result;

                var projectChildren = await project.GetChildrenAsync(true, true);

                var csSourceCodeDocuments = projectChildren
                    .Where(m => m.ModelType == VisualStudioModelType.CSharpSource)
                    .Cast<VsCSharpSource>();

                foreach (var csSourceCodeDocument in csSourceCodeDocuments)
                {
                    var sourceCode = csSourceCodeDocument.SourceCode;
                    if (sourceCode == null) continue;
                    if (!sourceCode.Classes.Any()) continue;

                    var classes = sourceCode.Classes;
                    foreach (var csClass in classes)
                    {
                        var registration = IsTransientClass(csClass, rejectedClassNames, rejectedBaseClasses, targetInterfaceTypeForRegistration);

                        if(registration == null) continue;

                        if (!result.Any( r => (r.ClassData?.Namespace == registration.ClassData?.Namespace && r.ClassData?.Name == registration.ClassData?.Name))) result.Add(registration);
                    }
                    //result.AddRange(classes.Select(csClass => IsTransientClass(csClass, rejectedClassNames,
                    //    rejectedBaseClasses, targetInterfaceTypeForRegistration)).Where(diInfo => diInfo != null));
                }

            return result;
        }

        /// <summary>
        /// Determines if the class meets the criteria for dependency injection and returns the registration information.
        /// </summary>
        /// <param name="classData">The class to check.</param>
        /// <param name="rejectedClassNames">Optional parameter that determines if the class name matches the name in the list it cannot be used for registration.</param>
        /// <param name="rejectedBaseClasses">Optional parameter that determines if the class inherits the target base class it cannot be used for registration.</param>
        /// <param name="targetInterfaceTypeForRegistration">Optional parameter that will determine which interface will be used for registration, This will be the interface itself or anything that inherits the interface.</param>
        /// <returns>Null if the class does not qualify for dependency injection or the registration information if it does.</returns>
        public static DependencyInjectionRegistrationInformation IsTransientClass(CsClass classData,IEnumerable<string> rejectedClassNames = null, IEnumerable<ModelLookupData> rejectedBaseClasses = null, IEnumerable<ModelLookupData> targetInterfaceTypeForRegistration = null)
        {
            if (classData == null) return null;
            if (!classData.IsLoaded) return null;

            if (classData.IsStatic) return null;

            if (!classData.Constructors.Any()) return null;
            if (classData.Constructors.Count(c => !c.IsStatic) > 1) return null;
            
            var constructor = classData.Constructors.FirstOrDefault();

            if (constructor == null) return null;
            if (constructor.Parameters.Any(p => p.ParameterType.IsWellKnownType)) return null;

            var className = classData.Name;

            if (rejectedClassNames != null) if (rejectedClassNames.Any(c => c == className)) return null;
            
            if (rejectedBaseClasses != null) if (rejectedBaseClasses.Any(r => classData.InheritsBaseClass(r.Name, r.Namespace))) return null;
            
            CsInterface targetInterface = null;

            if (targetInterfaceTypeForRegistration != null)
            {
                foreach (var modelLookupData in targetInterfaceTypeForRegistration)
                {
                    targetInterface = classData.HasInterface(modelLookupData);
                    if(targetInterface != null) break;
                }
            }

            if (targetInterface != null) return DependencyInjectionRegistrationInformation.Init(classData, targetInterface);

            if (classData.InheritedInterfaces.Count == 1) targetInterface = classData.InheritedInterfaces[0];

            return DependencyInjectionRegistrationInformation.Init(classData,targetInterface);
        }

        /// <summary>
        /// Create a new registration class from scratch
        /// </summary>
        /// <param name="sourceProject">The source project to build the new registration class for.</param>
        /// <param name="className">The name of the class that is used for service registration. This will be set to the constant <see cref="NetConstants.DefaultDependencyInjectionClassName"/> this can be overwritten with a custom class name.</param>
        /// <param name="automatedRegistrationMethodName">The name of the automatic transient class registration method. This will be set to the constant <see cref="NetConstants.DefaultAutomaticTransientClassRegistrationMethodName"/> this can be overwritten with a custom method name. </param>
        /// <returns>Fully formatted source code for registration class.</returns>
        public static string BuildNewRegistrationClass(VsProject sourceProject, 
            string className = NetConstants.DefaultDependencyInjectionClassName, 
            string automatedRegistrationMethodName = NetConstants.DefaultAutomaticTransientClassRegistrationMethodName)
        {

            if (sourceProject == null) return null;

            if (!sourceProject.IsLoaded) return null;
            
            string defaultNamespace = sourceProject.DefaultNamespace;

            if (string.IsNullOrEmpty(defaultNamespace)) return null;


            CodeFactory.SourceFormatter classFormatter = new CodeFactory.SourceFormatter();

            classFormatter.AppendCodeLine(0, "using System;");
            classFormatter.AppendCodeLine(0, "using System.Collections.Generic;");
            classFormatter.AppendCodeLine(0, "using Microsoft.Extensions.Configuration;");
            classFormatter.AppendCodeLine(0, "using Microsoft.Extensions.DependencyInjection;");
            classFormatter.AppendCodeLine(0, $"namespace {defaultNamespace}");
            classFormatter.AppendCodeLine(0, "{");
            classFormatter.AppendCodeLine(1, "/// <summary>");
            classFormatter.AppendCodeLine(1, $"/// Responsible for dependency inject of services for the library <see cref=\"{sourceProject.Name}\"/>");
            classFormatter.AppendCodeLine(1, "/// </summary>");
            classFormatter.AppendCodeLine(1, $"public static class {className}");
            classFormatter.AppendCodeLine(1, "{");
            classFormatter.AppendCodeLine(2, "/// <summary>");
            classFormatter.AppendCodeLine(2, "/// Flag that determines if registration has already been performed on the library");
            classFormatter.AppendCodeLine(2, "/// </summary>");
            classFormatter.AppendCodeLine(2, "public static bool ServicesRegistered { get; private set; }");
            classFormatter.AppendCodeLine(0);
            classFormatter.AppendCodeLine(2, "/// <summary>");
            classFormatter.AppendCodeLine(2, "/// Register the dependency injection service that are supported by this library and triggers registration for other libraries referenced by this library.");
            classFormatter.AppendCodeLine(2, "/// </summary>");
            classFormatter.AppendCodeLine(2, "/// <param name=\"serviceCollection\">The service collection that dependency injection uses.</param>");
            classFormatter.AppendCodeLine(2, "/// <param name=\"configuration\">The hosting systems configuration.</param>");
            classFormatter.AppendCodeLine(2, "public static void RegisterServices(IServiceCollection serviceCollection, IConfiguration configuration)");
            classFormatter.AppendCodeLine(2, "{");
            classFormatter.AppendCodeLine(3, "//If services have already been registered do no process service registration. This protects from duplicate registration.");
            classFormatter.AppendCodeLine(3, "if (ServicesRegistered) return;");
            classFormatter.AppendCodeLine(0);
            classFormatter.AppendCodeLine(3, "//Call down stream libraries and have them complete their registration.");
            classFormatter.AppendCodeLine(3, "RegisterDependentServices(serviceCollection, configuration);");
            classFormatter.AppendCodeLine(0);
            classFormatter.AppendCodeLine(3, "//Process all manually managed registrations for this library.");
            classFormatter.AppendCodeLine(3, "ManualRegistrationServices(serviceCollection, configuration);");
            classFormatter.AppendCodeLine(0);
            classFormatter.AppendCodeLine(3, "//Process all automatically discovered services for registration.");
            classFormatter.AppendCodeLine(3, $"{automatedRegistrationMethodName}(serviceCollection);");
            classFormatter.AppendCodeLine(0);
            classFormatter.AppendCodeLine(3, "//Update the registration status.");
            classFormatter.AppendCodeLine(3, "ServicesRegistered = true;");
            classFormatter.AppendCodeLine(2, "}");
            classFormatter.AppendCodeLine(0);
            classFormatter.AppendCodeLine(2, "/// <summary>");
            classFormatter.AppendCodeLine(2, "/// Register dependency injection services for child libraries referenced by this library.");
            classFormatter.AppendCodeLine(2, "/// </summary>");
            classFormatter.AppendCodeLine(2, "/// <param name=\"serviceCollection\">The service collection that dependency injection uses.</param>");
            classFormatter.AppendCodeLine(2, "/// <param name=\"configuration\">The hosting systems configuration.</param>");
            classFormatter.AppendCodeLine(2, "private static void RegisterDependentServices(IServiceCollection serviceCollection, IConfiguration configuration)");
            classFormatter.AppendCodeLine(2, "{");
            classFormatter.AppendCodeLine(3, "//Todo: Register services from other libraries directly referenced by this library.");
            classFormatter.AppendCodeLine(2, "}");
            classFormatter.AppendCodeLine(0);
            classFormatter.AppendCodeLine(2, "/// <summary>");
            classFormatter.AppendCodeLine(2, "/// Register services with the service collection manually. This is where manual singleton objects and complex service registration is managed.");
            classFormatter.AppendCodeLine(2, "/// </summary>");
            classFormatter.AppendCodeLine(2, "/// <param name=\"serviceCollection\">The service collection that dependency injection uses.</param>");
            classFormatter.AppendCodeLine(2, "/// <param name=\"configuration\">The hosting systems configuration.</param>");
            classFormatter.AppendCodeLine(2, "private static void ManualRegistrationServices(IServiceCollection serviceCollection, IConfiguration configuration)");
            classFormatter.AppendCodeLine(2, "{");
            classFormatter.AppendCodeLine(3, "//TODO: manually add singleton and manually managed registrations here.");
            classFormatter.AppendCodeLine(2, "}");
            classFormatter.AppendCodeLine(0);
            classFormatter.AppendCodeLine(2, "/// <summary>");
            classFormatter.AppendCodeLine(2, "/// Automated registration of classes using transient registration.");
            classFormatter.AppendCodeLine(2, "/// </summary>");
            classFormatter.AppendCodeLine(2, "/// <param name=\"serviceCollection\">The service collection to register services.</param>");
            classFormatter.AppendCodeLine(2, $"private static void {automatedRegistrationMethodName}(IServiceCollection serviceCollection)");
            classFormatter.AppendCodeLine(2, "{");
            classFormatter.AppendCodeLine(3, "//Will be updated through code automation do not change or add to this method by hand.");
            classFormatter.AppendCodeLine(2, "}");
            classFormatter.AppendCodeLine(1, "}");
            classFormatter.AppendCodeLine(0, "}");

            return classFormatter.ReturnSource();
        }

        /// <summary>
        /// Searches the root of a project for the dependency injection registration class. If it is not found will generate a new service registration class.
        /// </summary>
        /// <param name="source">The source project to load the registration class from.</param>
        /// <param name="className">The name of the class that is used for service registration. This will be set to the constant <see cref="NetConstants.DefaultDependencyInjectionClassName"/> this can be overwritten with a custom class name.</param>
        /// <param name="automatedRegistrationMethodName">The name of the automatic transient class registration method. This will be set to the constant <see cref="NetConstants.DefaultAutomaticTransientClassRegistrationMethodName"/> this can be overwritten with a custom method name. </param>
        /// <returns>The source code model for the registration class.</returns>
        public static async Task<CsSource> GetRegistrationClassAsync(VsProject source, string className = NetConstants.DefaultDependencyInjectionClassName,
            string automatedRegistrationMethodName = NetConstants.DefaultAutomaticTransientClassRegistrationMethodName)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (!source.IsLoaded)
            {
                throw new CodeFactoryException("Project model was not loaded cannot load or create the registration class.");
            }
            if (string.IsNullOrEmpty(className))
            {
                throw new ArgumentNullException(nameof(className));
            }

            var projectFiles = await source.GetChildrenAsync(false, true);

            CsSource registrationSource = null;

            try
            {
                //Searching the projects root directory code files for the registration class
                if (projectFiles.Any())
                    registrationSource = projectFiles.Where(f => f.ModelType == VisualStudioModelType.CSharpSource)
                        .Cast<VsCSharpSource>().FirstOrDefault(s => s.SourceCode.Classes.Any(c => c.Name == className))
                        ?.SourceCode;

                //If no registration source code file was found then create a new registration class from scratch.
                if (registrationSource == null)
                {
                    var registrationClassCode = BuildNewRegistrationClass(source, className,automatedRegistrationMethodName);
                    if (registrationClassCode == null) throw new CodeFactoryException("Could not generate the dependency injection registration source code");
                    string registrationClassFileName = $"{className}.cs";

                    var document = await source.AddDocumentAsync(registrationClassFileName, registrationClassCode);

                    registrationSource = await document.GetCSharpSourceModelAsync();

                }

                if (registrationSource == null) throw new CodeFactoryException("Cannot load the source code for the registration class.");
            }
            catch (CodeFactoryException)
            {
                throw;
            }
            catch (Exception unhandledException)
            {
                throw new CodeFactoryException("An error occured while loading the registration class source code, cannot continue.",unhandledException);
            }

            return registrationSource;
        }

        /// <summary>
        /// Handles the generation or update of a project wide dependency injection class and automatic registration of classes.
        /// </summary>
        /// <param name="source">The target project to add or update dependency injection in.</param>
        /// <param name="className">The name of the class that is used for service registration. This will be set to the constant <see cref="NetConstants.DefaultDependencyInjectionClassName"/> this can be overwritten with a custom class name.</param>
        /// <param name="automatedRegistrationMethodName">The name of the automatic transient class registration method. This will be set to the constant <see cref="NetConstants.DefaultAutomaticTransientClassRegistrationMethodName"/> this can be overwritten with a custom method name. </param>
        /// <param name="rejectedClassNames">Optional parameter that determines if the class name matches the name in the list it cannot be used for registration.</param>
        /// <param name="rejectedBaseClasses">Optional parameter that determines if the class inherits the target base class it cannot be used for registration.</param>
        /// <param name="targetInterfaceTypeForRegistration">Optional parameter that will determine which interface will be used for registration, This will be the interface itself or anything that inherits the interface.</param>
        public static async Task ProjectBuildDependencyInjectionAsync(VsProject source,
            string className = NetConstants.DefaultDependencyInjectionClassName,
            string automatedRegistrationMethodName = NetConstants.DefaultAutomaticTransientClassRegistrationMethodName,
            IEnumerable<string> rejectedClassNames = null, IEnumerable<ModelLookupData> rejectedBaseClasses = null,
            IEnumerable<ModelLookupData> targetInterfaceTypeForRegistration = null)
        {
            try
            {
                var registrationSourceCode = await GetRegistrationClassAsync(source, className, automatedRegistrationMethodName);

                if (registrationSourceCode == null) throw new CodeFactoryException("Could load or create the dependency injection code.");

                if (!registrationSourceCode.IsLoaded) throw new CodeFactoryException("Could load or create the dependency injection code.");

                var registrationClasses = await GetTransientClassesAsync(source, rejectedClassNames, rejectedBaseClasses,
                    targetInterfaceTypeForRegistration);


                foreach (var diInfo in registrationClasses)
                {
                    var targetNamespace = diInfo?.ClassData.Namespace;

                    if(string.IsNullOrEmpty(targetNamespace)) continue;

                    registrationSourceCode = await registrationSourceCode.AddUsingStatementAsync(targetNamespace);
                }

                var manager = registrationSourceCode.LoadNamespaceManager(source.DefaultNamespace);

                var methodSource = BuildTransientInjectionMethod(registrationClasses, false, true,
                    automatedRegistrationMethodName, "serviceCollection", manager);

                if (string.IsNullOrEmpty(methodSource)) throw new CodeFactoryException("Failed to create the automatic transient method");

                var registrationClass =
                    registrationSourceCode.Classes.FirstOrDefault(c =>
                        c.Name == className);

                if (registrationClass == null) throw new CodeFactoryException("Could not load the dependency injection class");

                var autoRegistrationMethod = registrationClass.Methods.FirstOrDefault(m =>
                    m.Name == automatedRegistrationMethodName);

                if (autoRegistrationMethod != null) await autoRegistrationMethod.ReplaceAsync(methodSource);
                else await registrationClass.AddToEndAsync(methodSource);

            }
            catch (CodeFactoryException)
            {
                throw;
            }
            catch (Exception unhandledError)
            {
                throw new CodeFactoryException(
                    "Could not generate the registration class, an unhandled error occured, see exception for details.",
                    unhandledError);
            }
        }
    }
}
