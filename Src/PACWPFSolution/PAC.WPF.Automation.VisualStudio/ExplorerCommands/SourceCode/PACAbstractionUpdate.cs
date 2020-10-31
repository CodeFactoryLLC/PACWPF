using CodeFactory.Logging;
using CodeFactory.VisualStudio;
using CodeFactory.VisualStudio.SolutionExplorer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeFactory.Formatting.CSharp;
using PAC.WPF.Automation.Logic;

namespace PAC.WPF.Automation.VisualStudio.ExplorerCommands.SourceCode
{
    /// <summary>
    /// Code factory command for automation of a C# document when selected from a project in solution explorer.
    /// </summary>
    public class PACAbstractionUpdate : CSharpSourceCommandBase
    {
        private static readonly string commandTitle = "PAC Abstraction Update";
        private static readonly string commandDescription = "Confirms all members are implemented and rebuilds subscriptions to all contracts that are registered as fields on the abstraction.";

#pragma warning disable CS1998

        /// <inheritdoc />
        public PACAbstractionUpdate(ILogger logger, IVsActions vsActions) : base(logger, vsActions, commandTitle, commandDescription)
        {
            //Intentionally blank
        }

        #region Overrides of VsCommandBase<IVsCSharpDocument>

        /// <summary>
        /// Validation logic that will determine if this command should be enabled for execution.
        /// </summary>
        /// <param name="result">The target model data that will be used to determine if this command should be enabled.</param>
        /// <returns>Boolean flag that will tell code factory to enable this command or disable it.</returns>
        public override async Task<bool> EnableCommandAsync(VsCSharpSource result)
        {
            //Result that determines if the the command is enabled and visible in the context menu for execution.
            bool isEnabled = false;

            try
            {
                if (!result.IsLoaded) return false;
                 
                isEnabled = result.SourceCode.Classes.Any(c => c.ClassInterfaceWithBaseInterface("PAC.IAbstraction"));
            }
            catch (Exception unhandledError)
            {
                _logger.Error($"The following unhandled error occured while checking if the solution explorer C# document command {commandTitle} is enabled. ",
                    unhandledError);
                isEnabled = false;
            }

            return isEnabled;
        }

        /// <summary>
        /// Code factory framework calls this method when the command has been executed. 
        /// </summary>
        /// <param name="result">The code factory model that has generated and provided to the command to process.</param>
        public override async Task ExecuteCommandAsync(VsCSharpSource result)
        {
            try
            {
                var data = result.SourceCode.Classes[0].CSharpFormatBaseTypeName();
            }
            catch (Exception unhandledError)
            {
                _logger.Error($"The following unhandled error occured while executing the solution explorer C# document command {commandTitle}. ",
                    unhandledError);

            }

        }

        #endregion
    }
}
