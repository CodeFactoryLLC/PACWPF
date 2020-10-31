using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeFactory.DotNet.CSharp;
using CodeFactory.VisualStudio;

namespace CodeFactoryExtensions.Common.Automation
{
    /// <summary>
    /// Extensions class that supports the <see cref="CsContainer"/> hosted models.
    /// </summary>
    public static class CsContainerExtensions
    {
        /// <summary>
        ///  Determines if the container inherits the target interface or not.
        /// </summary>
        /// <param name="source">Source container to check</param>
        /// <param name="fullInterfaceName">The fully qualified name of the interface to check for.</param>
        /// <param name="supportGenerics">Optional parameter that determines if a generic name for the interface will be checked.</param>
        /// <returns>True if found or false if not</returns>
        public static bool InheritsInterface(this CsContainer source, string fullInterfaceName, bool supportGenerics = false)
        {
            if (source == null) return false;

            if (string.IsNullOrEmpty(fullInterfaceName)) return false;

            if (!source.InheritedInterfaces.Any()) return false;

            var result = false;

            foreach (var sourceInheritedInterface in source.InheritedInterfaces)
            {
                string interfaceFullName = null;

                if (supportGenerics & sourceInheritedInterface.HasStrongTypesInGenerics)
                {
                    interfaceFullName = $"{sourceInheritedInterface.Namespace}.{sourceInheritedInterface.Name}<{sourceInheritedInterface.GenericParameters.FormatCSharpGenericSignatureSyntax()}>";
                }
                else
                {
                    interfaceFullName = $"{sourceInheritedInterface.Namespace}.{sourceInheritedInterface.Name}";
                }

                result = interfaceFullName == fullInterfaceName;
                if (result) break;

                result = sourceInheritedInterface.InheritedInterfaces.Any(i => i.InheritsInterface(fullInterfaceName,supportGenerics));
                if (result) break;
            }

            return result;
        }

        /// <summary>
        ///  Determines if the container inherits the target interface or not.
        /// </summary>
        /// <param name="source">Source container to check</param>
        /// <param name="lookupInterface">lookup model data for the interface.</param>
        /// <param name="supportGenerics">Optional parameter that determines if a generic name for the interface will be checked.</param>
        /// <returns>Will return the interface that is either the lookup interface type, or inherits it.</returns>
        public static CsInterface HasInterface(this CsContainer source, ModelLookupData lookupInterface, bool supportGenerics = false)
        {
            if (source == null) return null;
            if (lookupInterface == null) return null;

            CsInterface result = null;
            if (source.ModelType == CsModelType.Interface)
            {
                CsInterface target = source as CsInterface;

                if (target == null) return null;

                result = source.HasInterface(lookupInterface, supportGenerics);
            }
            else
            {
                result = source.InheritedInterfaces.FirstOrDefault(i =>
                    i.HasInterface(lookupInterface, supportGenerics) != null);
            }

            return result;
        }

    }
}
