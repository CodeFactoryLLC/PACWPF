
using System.Collections.Generic;
using CodeFactoryExtensions.Common.Automation;

namespace PAC.WPF.Automation
{
    /// <summary>
    /// Extended functionality for PAC WPF dependency injection management.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// The class names that should be ignored in a PAC.WPF based project for registration.
        /// </summary>
        /// <returns></returns>
        public static List<string> DependencyInjectionWPFClassNameIgnoreList() => new List<string>{"App"};

        /// <summary>
        /// The base classes that should be ignored in a PAC.WPF based project for registration.
        /// </summary>
        /// <returns>List of base classes to ignore</returns>
        public static List<ModelLookupData> DependencyInjectionWPFBaseClassIgnoreList()
        {
            return new List<ModelLookupData>
                {ModelLookupData.Init(WPFConstants.WPFNamespace, WPFConstants.ApplicationBaseClassName)};
        }

        /// <summary>
        /// The target interfaces to use for registration by default in a PAC.WPF project.
        /// </summary>
        /// <returns>List of inherited interfaces to search for.</returns>
        public static List<ModelLookupData> DependencyInjectionWPFDefaultInterfaceList()
        {
            var interfaceList = DependencyInjectionPACDefaultInterfaceList();
            interfaceList.Add(ModelLookupData.Init(WPFConstants.WPFNamespace, WPFConstants.ContractNamePresentation));
            return interfaceList;

        }

        /// <summary>
        /// The target interfaces to use for registration by default in a PAC project.
        /// </summary>
        /// <returns>List of inherited interfaces to search for.</returns>
        public static List<ModelLookupData> DependencyInjectionPACDefaultInterfaceList()
        {
            return new List<ModelLookupData>
            {
                ModelLookupData.Init(WPFConstants.PACNamespace, WPFConstants.ContractNameAbstraction),
                ModelLookupData.Init(WPFConstants.PACNamespace, WPFConstants.ContractNameController)
            };
        }
    }
}
