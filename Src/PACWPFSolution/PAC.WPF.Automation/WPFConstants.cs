using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xaml.Permissions;

namespace PAC.WPF.Automation
{
    /// <summary>
    /// Data class that holds constants used in PAC WPF automation
    /// </summary>
    public static class WPFConstants
    {
        /// <summary>
        /// The assembly name of the PAC.WPF library
        /// </summary>
        public const string WPFAssemblyName = "PAC.WPF";

        /// <summary>
        /// The assembly name of the PAC library
        /// </summary>
        public const string PACAssemblyName = "PAC";

        /// <summary>
        /// The default namespace for the PAC library.
        /// </summary>
        public const string PACNamespace = "PAC";

        /// <summary>
        /// The default namespace for the PAC.WPF library.
        /// </summary>
        public const string WPFNamespace = "PAC.WPF";

        public const string ContractNameAbstraction = "IAbstraction";

        public const string ContractNameController = "IController";

        public const string ContractNamePresentation = "IPresentation";

        public const string ApplicationBaseClassName = "PACApplication";

        public const string PresentationDefaultSuffix = ".xaml.cs";

        public const string PresentationContractSuffix = ".xaml.contract.cs";

        public const string DefaultContractSuffix = ".contract.cs";

        public const string SubscriptionSuffix = ".subscribe.cs";
    }
}
