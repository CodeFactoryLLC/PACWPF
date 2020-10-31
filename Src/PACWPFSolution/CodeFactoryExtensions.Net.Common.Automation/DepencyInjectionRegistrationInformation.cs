using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CodeFactory.DotNet.CSharp;

namespace CodeFactoryExtensions.Net.Common.Automation
{

    /// <summary>
    /// Immutable data class that is used in automation of dependency injection. 
    /// </summary>
    public class DependencyInjectionRegistrationInformation
    {
        private readonly CsClass _classData;

        private readonly CsInterface _interfaceData;

        /// <summary>
        /// Creates a new instance of the of the data model.
        /// </summary>
        /// <param name="classData">The class data to be used for DI</param>
        /// <param name="interfaceData">Optional a target interface to be used for DI</param>
        private DependencyInjectionRegistrationInformation(CsClass classData, CsInterface interfaceData = null)
        {
            _classData = classData;
            _interfaceData = interfaceData;
        }

        /// <summary>
        /// Class information for DI.
        /// </summary>
        public CsClass ClassData => _classData;

        /// <summary>
        /// Interface information for DI.
        /// </summary>
        public CsInterface InterfaceData => _interfaceData;

        /// <summary>
        /// Creates a new instance of the of the data model.
        /// </summary>
        /// <param name="classData">The class data to be used for DI</param>
        /// <param name="interfaceData">Optional a target interface to be used for DI</param>
        public static DependencyInjectionRegistrationInformation Init(CsClass classData, CsInterface interfaceData = null) => new DependencyInjectionRegistrationInformation(classData,interfaceData);
    }

}
