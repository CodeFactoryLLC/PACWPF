using CodeFactory.Logging;
using CodeFactory.VisualStudio;
using CodeFactory.VisualStudio.SolutionExplorer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeFactory;
using CodeFactory.DotNet;
using CodeFactory.DotNet.CSharp;
using CodeFactoryExtensions.Common.Automation;
using CodeFactoryExtensions.Formatting.CSharp;
using CodeFactoryExtensions.Net.AspNet.Automation;
using CodeFactoryExtensions.Net.Common.Automation;
using CommonDeliveryFramework.Automation;

namespace PAC.WPF.Automation.VisualStudio.ExplorerCommands.SourceCode
{
    /// <summary>
    /// Code factory command for automation of a C# document when selected from a project in solution explorer.
    /// </summary>
    public class ImplementMembersCsDocumentCommand : CSharpSourceCommandBase
    {
        private static readonly string commandTitle = "Add Interface Members";
        private static readonly string commandDescription = "Adds missing interface members to the class.";

#pragma warning disable CS1998

        /// <inheritdoc />
        public ImplementMembersCsDocumentCommand(ILogger logger, IVsActions vsActions) : base(logger, vsActions, commandTitle, commandDescription)
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
                 isEnabled = result.SourceCode.SourceMissingInterfaceMembers() != null;
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
                var references = await result.GetHostProjectReferencesAsync();

                    if (!references.Any()) return;

                    bool hasLogging = references.Any(r => r.Name == NetConstants.MicrosoftExtensionsLoggingLibraryName);
                    bool hasCDF = references.Any(r => r.Name == CommonDeliveryFrameworkConstants.CommonDeliveryFrameworkAssemblyName);
                    bool hasCDFAspnet = references.Any(r =>
                        r.Name == CommonDeliveryFrameworkConstants.CommonDeliveryFrameworkNetAspNetAssemblyName);

                    var sourceCode = result.SourceCode;

                
                    foreach (var sourceCodeClass in sourceCode.Classes)
                    {
                        var missingMembers =  sourceCodeClass.MissingInterfaceMembers();

                        if(missingMembers == null) continue;
                        if(!missingMembers.Any()) continue;

                        IEnumerable<CsMember> baseMembers = null;

                        var updatedClass = sourceCodeClass;

                        var contractMembers = ContractHelper.MissingContractMembers(sourceCodeClass, missingMembers);

                        baseMembers = contractMembers != null ? ContractHelper.MissingBaseMembers(missingMembers, contractMembers) : missingMembers;

                        if (contractMembers != null)
                        {
                            sourceCode = await ContractHelper.GetContractSourceFileAsync(result, sourceCodeClass);

                            if (sourceCode == null) throw new CodeFactoryException("Cannot load the source code for the contract to support this class.");

                            updatedClass = sourceCode.GetModel(updatedClass.LookupPath) as CsClass;
                            if(updatedClass == null) throw new CodeFactoryException("Cannot get class data to add members.");

                            sourceCode = await UpdateFileAsync(updatedClass, sourceCode, contractMembers,
                                hasLogging, hasCDF, hasCDFAspnet,true);

                            if (baseMembers != null)
                                sourceCode = await ContractHelper.GetContractBaseFileAsync(result, sourceCodeClass);

                            if(sourceCode == null) throw new CodeFactoryException("Cannot load the source code for the class.");

                            updatedClass = sourceCode.GetModel(updatedClass.LookupPath) as CsClass;
                            if (updatedClass == null) throw new CodeFactoryException("Cannot get class data to add members.");
                        }

                        if(baseMembers != null) sourceCode = await UpdateFileAsync(updatedClass, sourceCode, baseMembers,
                            hasLogging, hasCDF, hasCDFAspnet,false);
                    }


            }
            catch (Exception unhandledError)
            {
                _logger.Error($"The following unhandled error occured while executing the solution explorer C# document command {commandTitle}. ",
                    unhandledError);

            }

        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetClass"></param>
        /// <param name="sourceCode"></param>
        /// <param name="members"></param>
        /// <param name="logging"></param>
        /// <param name="cdf"></param>
        /// <param name="cdfAspnet"></param>
        /// <param name="isContract"></param>
        /// <returns></returns>
        private async Task<CsSource> UpdateFileAsync(CsClass targetClass,  CsSource sourceCode, IEnumerable<CsMember> members, bool logging, bool cdf, bool cdfAspnet, bool isContract)
        {
            if(targetClass == null) throw new CodeFactoryException("Cannot access class data cannot add members");
            if(sourceCode == null) throw new CodeFactoryException("Cannot access the classes source code cannot add members.");
            if (members == null) return sourceCode;
            if (!members.Any()) return sourceCode;

            CsSource updatedSourceCode = await sourceCode.AddMissingNamespaces(members, targetClass.Namespace);
            CsClass updatedClass = targetClass;



            if (logging)
            {
                updatedSourceCode = await updatedSourceCode.AddUsingStatementAsync(NetConstants.MicrosoftLoggerNamespace);

                updatedClass = sourceCode.GetModel(updatedClass.LookupPath) as CsClass;
                if (updatedClass == null) throw new CodeFactoryException("Cannot get class data to add members.");

                if (!updatedClass.Fields.Any(f => f.Name == NetConstants.DefaultClassLoggerName))
                {
                    var formatter = new SourceFormatter();
                    formatter.AppendCodeLine(0,"///<summary>");
                    formatter.AppendCodeLine(0, "///Logger used to manage logging for this class.");
                    formatter.AppendCodeLine(0, "///</summary>");
                    formatter.AppendCodeLine(0, $"private ILogger {NetConstants.DefaultClassLoggerName};");
                    updatedSourceCode =
                        await updatedClass.AddToBeginningAsync(
                            CsSourceFormatter.IndentCodeBlock(2, formatter.ReturnSource()));
                }
            }

            if (cdf) updatedSourceCode = await updatedSourceCode.AddUsingStatementAsync(CommonDeliveryFrameworkConstants
                        .CommonDeliveryFrameworkNamespace);

            if (cdfAspnet)
                updatedSourceCode = await updatedSourceCode.AddUsingStatementAsync(CommonDeliveryFrameworkConstants
                    .CommonDeliveryFrameworkNetAspNetNamespace);
            
            updatedClass = sourceCode.GetModel(updatedClass.LookupPath) as CsClass;
            if (updatedClass == null) throw new CodeFactoryException("Cannot get class data to add members.");

            updatedSourceCode = await UpdateMembersAsync(updatedClass, members, logging, cdf, cdfAspnet,
                updatedSourceCode.SourceDocument,isContract,sourceCode.LoadNamespaceManager(updatedClass.Namespace));

            return updatedSourceCode;
        }

       

        private async Task<CsSource> UpdateMembersAsync(CsClass targetClass, IEnumerable<CsMember> members, bool logging, bool cdf, bool cdfAspnet,string targetFilePath,bool isContract , NamespaceManager manager)
        {
            if (targetClass == null) return null;
            if (members == null) return null;
            if (!members.Any()) return null;

            if (string.IsNullOrEmpty(targetFilePath)) return null;

            CsSource updatedSource = null;
            CsClass updatedClass = targetClass;

            foreach (var csMember in members)
            {
                switch (csMember.MemberType)
                {
                    case CsMemberType.Event:

                        CsEvent eventData = csMember as CsEvent;
                        
                        if(eventData == null) throw new CodeFactoryException("Cannot load event data, cannot process members.");

                        updatedSource = await AddEvent(updatedClass, eventData, isContract, targetFilePath, manager);

                        updatedClass = updatedSource.GetModel(updatedClass.LookupPath) as CsClass;
                        if(updatedClass == null) throw new CodeFactoryException("Could not load class data cannot add members.");

                        break;

                    case CsMemberType.Method:

                        CsMethod methodData = csMember as CsMethod;

                        if (methodData == null) throw new CodeFactoryException("Cannot load method data, cannot process members.");

                        updatedSource = await AddMethodMember(updatedClass, methodData, logging, cdf, cdfAspnet,
                            targetFilePath,manager);

                        updatedClass = updatedSource.GetModel(updatedClass.LookupPath) as CsClass;
                        if (updatedClass == null) throw new CodeFactoryException("Could not load class data cannot add members.");

                        break;

                    case CsMemberType.Property:

                        CsProperty propertyData = csMember as CsProperty;

                        if (propertyData == null) throw new CodeFactoryException("Cannot load property data, cannot process members.");

                        updatedSource = await AddProperty(updatedClass, propertyData, targetFilePath, manager);
                        updatedClass = updatedSource.GetModel(updatedClass.LookupPath) as CsClass;
                        if (updatedClass == null) throw new CodeFactoryException("Could not load class data cannot add members.");

                        break;

                    default:
                        continue;

                }
            }

            return updatedSource;
        }

        private async Task<CsSource> AddMethodMember(CsClass targetClass, CsMethod member, bool logging, bool cdf, bool cdfAspNet, string targetFilePath, NamespaceManager manager)
        {
            CsSource result = null;
            string sourceCode = null;

            if (cdfAspNet)
            {
                if (WebApiSupport.IsControllerAction(member))
                {
                    sourceCode = CSharpSourceGenerationCommonDeliveryFramework.GenerateControllerActionMethodSourceCode(member,
                        manager, true, true, CsSecurity.Public, logging, "_logger");
                }
                else
                {
                    sourceCode = CSharpSourceGenerationCommonDeliveryFramework.GenerateStandardMethodSourceCode(member,
                        manager, true, true, CsSecurity.Public, logging, "_logger");
                }
            }
            else
            {
                if (cdf)
                {
                    sourceCode = CSharpSourceGenerationCommonDeliveryFramework.GenerateStandardMethodSourceCode(member,
                        manager, true, true, CsSecurity.Public, logging, "_logger");
                }
                else
                {
                    if (logging)
                    {
                        sourceCode = CSharpSourceGenerationNetCommon.GenerateStandardMethodSourceCode(member,
                            manager, true, true, CsSecurity.Public, true, "_logger");
                    }
                    else
                    {
                        sourceCode = CSharpSourceGenerationCommon.GenerateStandardMethodSourceCode(member,
                            manager, true, true);
                    }
                }
            }

            if(string.IsNullOrEmpty(sourceCode)) throw new CodeFactoryException("Was not able to generate the source code for the member method.");

            result = await targetClass.AddToEndAsync(targetFilePath, CsSourceFormatter.IndentCodeBlock(2,sourceCode));

            if(result == null) throw new CodeFactoryException("Could not load the source code after adding the member.");

            return result;
        }

        private async Task<CsSource> AddProperty(CsClass targetClass, CsProperty member, string targetFilePath, NamespaceManager manager)
        {
            CsSource result = null;

            string sourceCode = CSharpSourceGenerationCommon.GenerateStandardPropertySourceCode(member, manager);

            if (string.IsNullOrEmpty(sourceCode)) throw new CodeFactoryException("Was not able to generate the source code for the member property.");

            result = await targetClass.AddToEndAsync(targetFilePath, CsSourceFormatter.IndentCodeBlock(2, sourceCode));

            if (result == null) throw new CodeFactoryException("Could not load the source code after adding the member.");

            return result;
        }

        private async Task<CsSource> AddEvent(CsClass targetClass, CsEvent member, bool isContractMember,
            string targetFilePath, NamespaceManager manager)
        {
            CsSource result = null;
            CsClass currentClass = targetClass;
            string eventSourceCode = null;

            string eventFieldName = null;

            if (isContractMember)
            {
                string fieldPrefix = "_";
                string fieldSuffix = "Handler";

                eventFieldName = $"{fieldPrefix}{member.Name.ConvertToCamelCase()}{fieldSuffix}";

                if (!currentClass.Fields.Any(f => f.Name == eventFieldName))
                {
                    string fieldSource = $"private {member.EventType.CSharpFormatTypeName(manager)} {eventFieldName};";

                    result = await currentClass.AddToBeginningAsync(targetFilePath,
                        CsSourceFormatter.IndentCodeBlock(2, fieldSource));

                    currentClass = result.GetModel(currentClass.LookupPath) as CsClass;
                    if(currentClass == null) throw new CodeFactoryException("Could not load class data cannot update members.");
                }

                eventSourceCode = CSharpSourceGenerationCommon.GenerateAddRemoveEvent(member, manager,
                    CsSecurity.Public, fieldPrefix, fieldSuffix);
            }
            else
            {
                eventSourceCode = CSharpSourceGenerationCommon.GenerateStandardEventSourceCode(member, manager);
            }

            if(string.IsNullOrEmpty(eventSourceCode)) throw new CodeFactoryException("Could not generate the event source code.");

            result = await currentClass.AddToEndAsync(targetFilePath, CsSourceFormatter.IndentCodeBlock(2, eventSourceCode));

            currentClass = result.GetModel(currentClass.LookupPath) as CsClass;
            if (currentClass == null) throw new CodeFactoryException("Could not load class data cannot update members.");

            string raiseEventSourceCode = null;

            if (isContractMember)
            {
                if (string.IsNullOrEmpty(eventFieldName)) throw new CodeFactoryException("Event field name was not provided cannot create event handler");

                raiseEventSourceCode = CSharpSourceGenerationCommon.GenerateStandardRaiseEventMethod(member, manager, member.Name,
                        CsSecurity.Protected, eventFieldName);
            }
            else raiseEventSourceCode = CSharpSourceGenerationCommon.GenerateStandardRaiseEventMethod(member, manager, member.Name,
                CsSecurity.Protected);

            result = await currentClass.AddToEndAsync(targetFilePath,
                CsSourceFormatter.IndentCodeBlock(2, raiseEventSourceCode));

            return result;
        }

    }
}
