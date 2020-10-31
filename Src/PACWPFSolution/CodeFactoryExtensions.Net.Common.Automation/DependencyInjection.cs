using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                    result.AddRange(classes.Select(csClass => IsTransientClass(csClass, rejectedClassNames, rejectedBaseClasses, targetInterfaceTypeForRegistration)).Where(diInfo => diInfo != null));
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
            if (classData.Constructors.Count > 1) return null;

            var constructor = classData.Constructors.FirstOrDefault();

            if (constructor == null) return null;
            if (!constructor.Parameters.Any(p => p.ParameterType.IsWellKnownType)) return null;

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
    }
}
