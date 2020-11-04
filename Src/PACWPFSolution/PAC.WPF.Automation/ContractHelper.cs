using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using CodeFactory;
using CodeFactory.DotNet;
using CodeFactory.DotNet.CSharp;
using CodeFactory.VisualStudio;
using CodeFactoryExtensions.Common.Automation;
using CodeFactoryExtensions.Formatting.CSharp;
using VsCSharpSourceExtensions = CodeFactoryExtensions.Common.Automation.VsCSharpSourceExtensions;

namespace PAC.WPF.Automation
{
    public static class ContractHelper
    {
        /// <summary>
        /// Determines if the class is a wpf presenter file or a standard class file.
        /// </summary>
        /// <param name="source">Class to validate.</param>
        /// <returns>True if a presenter file or false if not.</returns>
        public static bool IsPresenterFile(CsClass source)
        {
            return source.SourceFiles.Any(f =>
                f.EndsWith(WPFConstants.PresentationDefaultSuffix, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Checks the target class to see if it supports any subscriptions.
        /// </summary>
        /// <param name="source">Source class to check for subscriptions</param>
        /// <returns>List of the fields that need to be subscribed to.</returns>
        public static IReadOnlyList<CsField> GetSubscriptions(CsClass source)
        {
            if (source == null) return null;
            if (!source.IsLoaded) return null;

            var fields = source.Fields;

            if (!fields.Any()) return null;

            var resultFields = new List<CsField>();
            foreach (var csField in fields)
            {
               if(!csField.DataType.IsInterface) continue;

               var fieldInterface = csField.DataType.GetInterfaceModel();
               if(fieldInterface == null) continue;
               if (!fieldInterface.IsLoaded) continue;

               var contractType = ContractType(fieldInterface);

               if(contractType.controller | contractType.abstraction | contractType.presenter) resultFields.Add(csField);
            }

            return resultFields.Any() ? resultFields.ToImmutableList(): null;
        }

        /// <summary>
        /// Gets the contract source file, if one does not exist it will create the partial class file.
        /// </summary>
        /// <param name="source">The visual studio source file to search.</param>
        /// <param name="sourceClass">The target class that contains the contract.</param>
        /// <returns>The target source code or null if it could not be created.</returns>
        public static async Task<CsSource> GetContractSourceFileAsync(VsCSharpSource source, CsClass sourceClass)
        {
            if (source == null) return null;
            if (sourceClass == null) return null;

            var contractFile = GetContractFilePath(sourceClass);

            CsSource result = null;
            if (contractFile.hasFile)
            {
                result = await source.GetCsSourceDocumentFromParent(contractFile.filePath);
            }
            else
            {
                var manager = source.SourceCode.LoadNamespaceManager(sourceClass.Namespace);
                var partialClassSource = CSharpSourceGenerationCommon.GeneratePartialClass(sourceClass, manager);

                if (string.IsNullOrEmpty(partialClassSource))
                    throw new CodeFactoryException($"Could not generated partial class definition for class {sourceClass.Name}");

                result = await source.AddCSharpCodeFileToParentAsync(partialClassSource,
                    Path.GetFileName(contractFile.filePath));
            }

            return result;
        }

        /// <summary>
        /// Gets the contracts main class file. This will force a model reload.
        /// </summary>
        /// <param name="source">Visual studio source file to start from.</param>
        /// <param name="sourceClass">The target class to check for.</param>
        /// <returns>The source file or null if it could not before loaded. </returns>
        public static async Task<CsSource> GetContractBaseFileAsync(VsCSharpSource source, CsClass sourceClass)
        {
            if (source == null) return null;
            if (sourceClass == null) return null;

            var contractFile = GetContractFilePath(sourceClass);

            var baseFilePath = contractFile.filePath.Replace(WPFConstants.DefaultContractSuffix, ".cs");

            return await source.GetCsSourceDocumentFromParent(baseFilePath);
        }

        /// <summary>
        /// Gets the contract file path.
        /// </summary>
        /// <param name="source">The source class to get the contract from.</param>
        /// <returns>tuple with the information about the file path.</returns>
        /// <exception cref="FileNotFoundException">Will throw if the file information cannot be found.</exception>
        public static (bool hasFile, string filePath) GetContractFilePath(CsClass source)
        {
            bool isPresenter = IsPresenterFile(source);

            bool hasfile = false;

            string filePath = null;
            if (isPresenter)
            {
                filePath = source.SourceFiles.FirstOrDefault(f => f.EndsWith(WPFConstants.PresentationContractSuffix,
                    StringComparison.InvariantCultureIgnoreCase));

                if (!string.IsNullOrEmpty(filePath))
                {
                    hasfile = true;
                }
                else
                {
                    hasfile = false;

                    filePath = source.SourceFiles.FirstOrDefault(f => f.EndsWith(WPFConstants.PresentationDefaultSuffix,
                        StringComparison.InvariantCultureIgnoreCase));

                    if (string.IsNullOrEmpty(filePath)) throw new FileNotFoundException();

                    filePath = filePath.Replace(WPFConstants.PresentationDefaultSuffix,
                        WPFConstants.PresentationContractSuffix);


                }
            }
            else
            {
                filePath = source.SourceFiles.FirstOrDefault(f => f.EndsWith(WPFConstants.DefaultContractSuffix));
                hasfile = true;

                if (string.IsNullOrEmpty(filePath))
                {
                    hasfile = false;
                    filePath = source.SourceFiles.FirstOrDefault(f =>
                        f.EndsWith(WPFConstants.SubscriptionSuffix, StringComparison.InvariantCultureIgnoreCase));

                    if (!string.IsNullOrEmpty(filePath))
                    {
                        filePath = filePath.Replace(WPFConstants.SubscriptionSuffix, WPFConstants.DefaultContractSuffix);
                    }
                    else
                    {
                        filePath = source.SourceFiles.FirstOrDefault(f =>
                            f.EndsWith(".cs", StringComparison.InvariantCultureIgnoreCase));

                        if(string.IsNullOrEmpty(filePath)) throw new FileNotFoundException();

                        filePath = filePath.Replace(".cs", WPFConstants.DefaultContractSuffix);
                    }
                }
            }

            return (hasfile, filePath);
        }


        /// <summary>
        /// Helper method for controllers and abstraction to get the partial file that handles subscribing to contracts that are managed.
        /// </summary>
        /// <param name="source">Target class to get the subscribe partial class file.</param>
        /// <returns>The information about the subscribe file name.</returns>
        /// <exception cref="FileNotFoundException">Raised if the file path data could not be found.</exception>
        public static (bool hasFile, string filePath) GetSubscribeFilePath(CsClass source)
        {
            if(source == null) throw new FileNotFoundException();
            if(!source.IsLoaded) throw new FileNotFoundException();

            bool hasFile = false;
            string filePath = null;

            filePath = source.SourceFiles.FirstOrDefault(f =>
                f.EndsWith(WPFConstants.SubscriptionSuffix, StringComparison.InvariantCultureIgnoreCase));
            hasFile = true;

            if (!string.IsNullOrEmpty(filePath)) return (hasFile, filePath);
            
            hasFile = false;

            filePath = source.SourceFiles.FirstOrDefault(f =>
                f.EndsWith(".cs", StringComparison.InvariantCultureIgnoreCase));

            if (string.IsNullOrEmpty(filePath)) throw new FileNotFoundException();

            filePath = filePath.Replace(".cs", WPFConstants.SubscriptionSuffix);
            

            return (hasFile, filePath);
        }

        /// <summary>
        /// Gets all the members that are part of the contract definition that is missing.
        /// </summary>
        /// <param name="source">target class to implement the contract on.</param>
        /// <param name="missingMembers">All missing members.</param>
        /// <returns>Null or the contract members that are missing.</returns>
        public static IEnumerable<CsMember> MissingContractMembers(CsClass source, IEnumerable<CsMember> missingMembers)
        {
            if (source == null) return null;
            if (missingMembers == null) return null;
            if (!missingMembers.Any()) return null;

            var contract = GetContract(source);
            var contractInterface = contract.contract;
            if (contractInterface == null) return null;

            var contractMembers = contractInterface.Members;

            var contractHashCodes = contractMembers.Select(member => member.FormatCSharpMemberComparisonHashCode(MemberComparisonType.Full)).ToList();

            var result = new List<CsMember>();

            return missingMembers.Where(m => contractHashCodes.Contains(m.FormatCSharpMemberComparisonHashCode(MemberComparisonType.Full))).ToList();
        }

        /// <summary>
        /// Gets all missing members that are not part of contract.
        /// </summary>
        /// <param name="missingMembers">Members to search</param>
        /// <param name="missingContractMembers">Members that are missing from the contract.</param>
        /// <returns>null or the missing members.</returns>
        public static IEnumerable<CsMember> MissingBaseMembers(IEnumerable<CsMember> missingMembers,
            IEnumerable<CsMember> missingContractMembers)
        {
            if (missingMembers == null) return null;
            if (!missingMembers.Any()) return null;
            if (missingContractMembers == null) return null;
            if (missingContractMembers.Any()) return null;

            var contractHashCodes =
                missingContractMembers.Select(m => m.FormatCSharpMemberComparisonHashCode(MemberComparisonType.Full));

            return missingMembers.Where(m => !contractHashCodes.Contains(m.FormatCSharpMemberComparisonHashCode(MemberComparisonType.Full))).ToList();
        }



        /// <summary>
        /// Gets the target contract interface implemented by the class.
        /// </summary>
        /// <param name="source">Source class to get the contract from.</param>
        /// <returns>Type of contract and the target contract interface.</returns>
        public static (bool presenter,bool controller ,bool abstraction, CsInterface contract) GetContract(CsClass source)
        {
            if (source == null) return (false, false, false, null);
            if (!source.IsLoaded | !source.InheritedInterfaces.Any()) return (false, false, false, null);

            bool presenter = false;
            bool controller = false;
            bool abstraction = false;
            CsInterface contract = null;

            contract = source.InheritedInterfaces.FirstOrDefault(i =>
            {
                var contractType = ContractType(i);

                presenter = contractType.presenter;
                controller = contractType.controller;
                abstraction = contractType.abstraction;

                return (presenter | controller | abstraction);
            });

            return (presenter, controller, abstraction, contract);
        }


        /// <summary>
        /// Determines the type of PAC contract the interface inherits.
        /// </summary>
        /// <param name="source">The source interface.</param>
        /// <returns>The target type of contract implemented.</returns>
        public static (bool presenter, bool controller, bool abstraction) ContractType(CsInterface source)
        {
            if (source == null) return (false, false, false);

            if (!source.IsLoaded) return (false, false, false);

            var presenter = ModelLookupData.Init(WPFConstants.WPFAssemblyName, WPFConstants.ContractNamePresentation);
            var contract = source.HasInterface(presenter);

            if (contract != null) return (true, false, false);

            var controller = ModelLookupData.Init(WPFConstants.PACNamespace, WPFConstants.ContractNameController);

            contract = source.HasInterface(controller);

            if (contract != null) return (false, true, false);

            var abstraction = ModelLookupData.Init(WPFConstants.PACNamespace, WPFConstants.ContractNameAbstraction);

            contract = source.HasInterface(abstraction);

            return contract != null ? (false, false, true) : (false, false, false);
        }
    }
}
