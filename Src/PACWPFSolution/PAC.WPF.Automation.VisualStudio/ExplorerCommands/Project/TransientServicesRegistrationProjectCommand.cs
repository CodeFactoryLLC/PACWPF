using CodeFactory.Logging;
using CodeFactory.VisualStudio;
using CodeFactory.VisualStudio.SolutionExplorer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeFactoryExtensions.Net.AspNet.Automation;
using CodeFactoryExtensions.Net.Common.Automation;

namespace PAC.WPF.Automation.VisualStudio.ExplorerCommands.Project
{
    /// <summary>
    /// Code factory command for automation of a project when selected from solution explorer.
    /// </summary>
    public class TransientServicesRegistrationProjectCommand : ProjectCommandBase
    {
        private static readonly string commandTitle = "TransientServicesRegistration";
        private static readonly string commandDescription = "Dynamically adds all classes that qualify for TransientServicesRegistration.";

#pragma warning disable CS1998

        /// <inheritdoc />
        public TransientServicesRegistrationProjectCommand(ILogger logger, IVsActions vsActions) : base(logger, vsActions, commandTitle, commandDescription)
        {
            //Intentionally blank
        }
#pragma warning disable CS1998
        #region Overrides of VsCommandBase<VsProject>

        /// <summary>
        /// Validation logic that will determine if this command should be enabled for execution.
        /// </summary>
        /// <param name="result">The target model data that will be used to determine if this command should be enabled.</param>
        /// <returns>Boolean flag that will tell code factory to enable this command or disable it.</returns>
        public override async Task<bool> EnableCommandAsync(VsProject result)
        {
            //Result that determines if the the command is enabled and visible in the context menu for execution.
            bool isEnabled = false;

            try
            {
                //Show if the extension libraries are loaded.
                isEnabled = await result.HasMicrosoftExtensionDependencyInjectionLibrariesAsync();
            }
            catch (Exception unhandledError)
            {
                _logger.Error($"The following unhandled error occured while checking if the solution explorer project command {commandTitle} is enabled. ",
                    unhandledError);
                isEnabled = false;
            }

            return isEnabled;
        }

        /// <summary>
        /// Code factory framework calls this method when the command has been executed. 
        /// </summary>
        /// <param name="result">The code factory model that has generated and provided to the command to process.</param>
        public override async Task ExecuteCommandAsync(VsProject result)
        {
            try
            {
                var projectReferences = await result.GetProjectReferencesAsync();

                
                bool isAspNetProject = projectReferences.Any(r => r.Name == AspNetConstants.MvcLibraryName);
                bool isPacWpfProject = projectReferences.Any(r => r.Name == WPFConstants.WPFAssemblyName);
                bool isPacProject = projectReferences.Any(r => r.Name == WPFConstants.PACAssemblyName);

                if (isAspNetProject)
                {
                    await RegisterAspNetProjectAsync(result);
                    return;
                }

                if (isPacWpfProject)
                {
                    await RegisterWPFProjectAsync(result);
                    return;
                }

                if (isPacProject)
                {
                    await RegisterPacProjectAsync(result);
                    return;
                }

                await RegisterProjectAsync(result);
            }
            catch (Exception unhandledError)
            {
                _logger.Error($"The following unhandled error occured while executing the solution explorer project command {commandTitle}. ",
                    unhandledError);

            }
        }

        /// <summary>
        ///  Registers transient classes for a project that is a aspnet project.
        /// </summary>
        /// <param name="project">Target Project to register dependency injection classes.</param>
        private async Task RegisterAspNetProjectAsync(VsProject project)
        {
            var excludeClasses = CodeFactoryExtensions.Net.AspNet.Automation.DependencyInjection
                .DependencyInjectionIgnoreClasses();

            var excludeBaseClasses = CodeFactoryExtensions.Net.AspNet.Automation.DependencyInjection
                .DependencyInjectionIgnoreBaseClasses();

            await CodeFactoryExtensions.Net.Common.Automation.DependencyInjection.ProjectBuildDependencyInjectionAsync(
                project, NetConstants.DefaultDependencyInjectionClassName,
                NetConstants.DefaultAutomaticTransientClassRegistrationMethodName, excludeClasses, excludeBaseClasses);
        }

        /// <summary>
        ///  Registers transient classes for a project that implements a pac wpf application
        /// </summary>
        /// <param name="project">Target Project to register dependency injection classes.</param>
        private async Task RegisterWPFProjectAsync(VsProject project)
        {
            var excludeClasses = PAC.WPF.Automation.DependencyInjection.DependencyInjectionWPFClassNameIgnoreList();
       
            var excludeBaseClasses = PAC.WPF.Automation.DependencyInjection.DependencyInjectionWPFBaseClassIgnoreList();


            var defaultInterfaces = PAC.WPF.Automation.DependencyInjection.DependencyInjectionWPFDefaultInterfaceList();

            await CodeFactoryExtensions.Net.Common.Automation.DependencyInjection.ProjectBuildDependencyInjectionAsync(
                project, NetConstants.DefaultDependencyInjectionClassName,
                NetConstants.DefaultAutomaticTransientClassRegistrationMethodName, excludeClasses, excludeBaseClasses,defaultInterfaces);
        }

        /// <summary>
        ///  Registers transient classes for a project that supports pac contracts.
        /// </summary>
        /// <param name="project">Target Project to register dependency injection classes.</param>
        private async Task RegisterPacProjectAsync(VsProject project)
        {

            var defaultInterfaces = PAC.WPF.Automation.DependencyInjection.DependencyInjectionPACDefaultInterfaceList();

            await CodeFactoryExtensions.Net.Common.Automation.DependencyInjection.ProjectBuildDependencyInjectionAsync(
                project, NetConstants.DefaultDependencyInjectionClassName,
                NetConstants.DefaultAutomaticTransientClassRegistrationMethodName, null, null, defaultInterfaces);
        }

        /// <summary>
        ///  Registers transient classes for a standard project
        /// </summary>
        /// <param name="project">Target Project to register dependency injection classes.</param>
        private async Task RegisterProjectAsync(VsProject project)
        {
            await CodeFactoryExtensions.Net.Common.Automation.DependencyInjection.ProjectBuildDependencyInjectionAsync(project);
        }

        #endregion
    }
}
