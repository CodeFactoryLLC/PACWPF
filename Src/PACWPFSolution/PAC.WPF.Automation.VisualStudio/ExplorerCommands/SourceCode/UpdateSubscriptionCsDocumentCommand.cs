using CodeFactory.Logging;
using CodeFactory.VisualStudio;
using CodeFactory.VisualStudio.SolutionExplorer;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeFactory;
using CodeFactory.DotNet.CSharp;
using CodeFactoryExtensions.Common.Automation;
using CodeFactoryExtensions.Formatting.CSharp;

namespace PAC.WPF.Automation.VisualStudio.ExplorerCommands.SourceCode
{
    /// <summary>
    /// Code factory command for automation of a C# document when selected from a project in solution explorer.
    /// </summary>
    public class UpdateSubscriptionCsDocumentCommand : CSharpSourceCommandBase
    {
        private static readonly string commandTitle = "Replace with command title to be displayed in context menu";
        private static readonly string commandDescription = "Replace with description of what this command does";

#pragma warning disable CS1998

        /// <inheritdoc />
        public UpdateSubscriptionCsDocumentCommand(ILogger logger, IVsActions vsActions) : base(logger, vsActions, commandTitle, commandDescription)
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
                if (!result.SourceCode.Classes.Any()) return false;
                isEnabled = result.SourceCode.Classes.Any(c => ContractHelper.GetSubscriptions(c) != null);
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
                var sourceCode = result.SourceCode;

                var classes = sourceCode.Classes;

                foreach (var csClass in classes)
                {
                    if(!(sourceCode.GetModel(csClass.LookupPath) is CsClass currentClass)) throw new CodeFactoryException("Cannot access class data cannot update subscriptions");

                    var subscriptions = ContractHelper.GetSubscriptions(currentClass);
                    if(subscriptions == null) continue;

                    if (!subscriptions.Any()) continue;
                    foreach (var subscription in subscriptions)
                    {
                        sourceCode = await UpdateSubscriptionAsync(subscription, currentClass, result);
                    }
                }
            }
            catch (Exception unhandledError)
            {
                _logger.Error($"The following unhandled error occured while executing the solution explorer C# document command {commandTitle}. ",
                    unhandledError);
            }
        }

        private async Task<CsSource> UpdateSubscriptionAsync(CsField subscriptionField, CsClass sourceClass, VsCSharpSource source)
        {
            SourceFormatter formatter = new SourceFormatter();
            string injectSourceCode = null;

            var contract = subscriptionField.DataType.GetInterfaceModel();
            if (contract == null) return null;

            CsSource sourceCode = source.SourceCode;

            try
            {
                CsClass currentClass = sourceClass;

                var events = contract.Events;

                var subscribePath = ContractHelper.GetSubscribeFilePath(currentClass);

                
                if (!subscribePath.hasFile)
                {
                    var manager = sourceCode.LoadNamespaceManager(sourceClass.Namespace);

                    var parent = await source.GetParentAsync();

                    if (parent == null) throw new CodeFactoryException("Cannot access the parent of the source code document, cannot update subscription");

                    VsDocument generatedDocument = null;
                    string partialClassSource = CSharpSourceGenerationCommon.GeneratePartialClass(currentClass, manager);
                    if (parent.ModelType == VisualStudioModelType.ProjectFolder)
                    {
                        var parentFolder = parent as VsProjectFolder;

                        if (parentFolder == null) throw new CodeFactoryException("Cannot access the parent of the source code document, cannot update subscription");

                        generatedDocument = await parentFolder.AddDocumentAsync(Path.GetFileName(subscribePath.filePath),
                            partialClassSource);
                    }
                    else
                    {
                        var parentProject = parent as VsProject;

                        if (parentProject == null) throw new CodeFactoryException("Cannot access the parent of the source code document, cannot update subscription");

                        generatedDocument = await parentProject.AddDocumentAsync(subscribePath.filePath,
                            partialClassSource);
                    }
                    sourceCode = await generatedDocument.GetCSharpSourceModelAsync();

                    sourceCode = await sourceCode.AddMissingNamespaces(contract.Events, currentClass.Namespace);

                    currentClass = sourceCode.GetModel(currentClass.LookupPath) as CsClass;

                    if (currentClass == null) throw new CodeFactoryException("Cannot access the parent of the source code document, cannot update subscription");
                }
                else
                {
                    var parent = await source.GetParentAsync();

                    VsCSharpSource sourceDocument = null;
                    if (parent.ModelType == VisualStudioModelType.ProjectFolder)
                    {
                        
                        var parentFolder = parent as VsProjectFolder;

                        if (parentFolder == null) throw new CodeFactoryException("Cannot access the parent of the source code document, cannot update subscription");

                        var children = await parentFolder.GetChildrenAsync(false, true);

                        sourceDocument = children.Where(c => c.ModelType == VisualStudioModelType.CSharpSource)
                            .Cast<VsCSharpSource>()
                            .FirstOrDefault(s => s.SourceCode.SourceDocument == subscribePath.filePath);
                    }
                    else
                    {
                        var parentProject = parent as VsProject;

                        if (parentProject == null) throw new CodeFactoryException("Cannot access the parent of the source code document, cannot update subscription");

                        var children = await parentProject.GetChildrenAsync(false, true);

                        sourceDocument = children.Where(c => c.ModelType == VisualStudioModelType.CSharpSource)
                            .Cast<VsCSharpSource>()
                            .FirstOrDefault(s => s.SourceCode.SourceDocument == subscribePath.filePath); ;
                    }

                    if(sourceDocument == null) throw new CodeFactoryException("Could load the contract document.");

                    sourceCode = sourceDocument.SourceCode;

                    sourceCode = await sourceCode.AddMissingNamespaces(contract.Events, currentClass.Namespace);

                    currentClass = sourceCode.GetModel(currentClass.LookupPath) as CsClass;
                    if (currentClass == null) throw new CodeFactoryException("Cannot access the parent of the source code document, cannot update subscription");
                }

                var namespaceManager = sourceCode.LoadNamespaceManager(currentClass.Namespace);

                foreach (var contractEvent in contract.Events)
                {
                    var eventHandlerName =
                        CSharpSourceGenerationWPF.GenerateContractEventHandlerMethodName(subscriptionField, contractEvent);

                    if (eventHandlerName == null) throw new CodeFactoryException($"Could not create the source code for a contract event handler.");

                    if (currentClass.Methods.Any(m => m.Name == eventHandlerName)) continue;

                    var eventHandlerSource = CSharpSourceGenerationWPF.GenerateContractEventHandlerMethod(subscriptionField, contractEvent,
                        namespaceManager);

                    if (eventHandlerSource == null) throw new CodeFactoryException($"Could not create the source code for the event handler {eventHandlerName}");

                    sourceCode = await currentClass.AddToEndAsync(subscribePath.filePath, InjectSourceCodeAtLevel(2,eventHandlerSource));
                    currentClass = sourceCode.GetModel(currentClass.LookupPath) as CsClass;
                    if (currentClass == null) throw new CodeFactoryException("Cannot access the parent of the source code document, cannot update subscription");

                }

                var subscriptionName = CSharpSourceGenerationWPF.GenerateContractSubscriptionMethodName(subscriptionField);
                var subscriptionMethod = currentClass.Methods.FirstOrDefault(m => m.Name == subscriptionName);

                if (subscriptionMethod != null)
                {
                    sourceCode = await subscriptionMethod.DeleteAsync();

                    currentClass = sourceCode.GetModel(currentClass.LookupPath) as CsClass;
                    if (currentClass == null) throw new CodeFactoryException("Cannot access the parent of the source code document, cannot update subscription");
                }

                var subscriptionSource = CSharpSourceGenerationWPF.GenerateContractSubscriptionMethod(subscriptionField, contract);

                if (subscriptionSource == null) throw new CodeFactoryException("Cannot generate the subscription contract source code.");

                sourceCode = await currentClass.AddToEndAsync(subscribePath.filePath, InjectSourceCodeAtLevel(2,subscriptionSource) );

                currentClass = sourceCode.GetModel(currentClass.LookupPath) as CsClass;
                if (currentClass == null) throw new CodeFactoryException("Cannot access the parent of the source code document, cannot update subscription");

                var releaseName = CSharpSourceGenerationWPF.GenerateContractReleaseMethodName(subscriptionField);
                var releaseMethod = currentClass.Methods.FirstOrDefault(m => m.Name == releaseName);

                if (releaseMethod != null)
                {
                    sourceCode = await releaseMethod.DeleteAsync();

                    currentClass = sourceCode.GetModel(currentClass.LookupPath) as CsClass;
                    if (currentClass == null) throw new CodeFactoryException("Cannot access the parent of the source code document, cannot update subscription");
                }

                var releaseSource = CSharpSourceGenerationWPF.GenerateContractReleaseMethod(subscriptionField, contract);

                if (releaseSource == null) throw new CodeFactoryException("Cannot generate the release contract source code.");
                
                sourceCode = await currentClass.AddToEndAsync(subscribePath.filePath,InjectSourceCodeAtLevel(2,releaseSource));
            }
            catch (CodeFactoryException)
            {
                throw;
            }
            catch (Exception unhandledException)
            {
                throw new CodeFactoryException("The following unhandledException occured",unhandledException);
            }

            return sourceCode;
        }

        private static string InjectSourceCodeAtLevel(int level, string sourceCode)
        {
            SourceFormatter formatter = new SourceFormatter();
            formatter.AppendCodeBlock(level, sourceCode);
            return formatter.ReturnSource();
        }

        #endregion
    }
}
