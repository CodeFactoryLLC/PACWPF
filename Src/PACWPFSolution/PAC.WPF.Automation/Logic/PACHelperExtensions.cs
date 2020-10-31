using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeFactory.DotNet.CSharp;
using CodeFactory.DotNet;

namespace PAC.WPF.Automation.Logic
{
    /// <summary>
    /// Support extensions for PAC functionality
    /// </summary>
    public static class PACHelperExtensions
    {

        /// <summary>
        /// Extension method that checks the interfaces implemented on a class inherit a target interface type.
        /// </summary>
        /// <param name="source">Class model to check interfaces for.</param>
        /// <param name="baseInterfaceType">The fully qualified name of the base interface to search for in the implemented interfaces.</param>
        /// <returns>True if found or false if not.</returns>
        public static bool ClassInterfaceWithBaseInterface(this CsClass source, string baseInterfaceType)
        {
            if (source == null) return false;

            if (string.IsNullOrEmpty(baseInterfaceType)) return false;

            return source.InheritedInterfaces.Any(i => i.InheritsInterface(baseInterfaceType));
        }

        /// <summary>
        /// Checks to see if an interface inherits from a target base interface type.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="baseInterfaceType"></param>
        /// <returns>True if found or false if not</returns>
        public static bool InheritsInterface(this CsInterface source, string baseInterfaceType)
        {
            if (source == null) return false;

            if (string.IsNullOrEmpty(baseInterfaceType)) return false;

            if (!source.InheritedInterfaces.Any()) return false;

            bool result = false;

            foreach (var sourceInheritedInterface in source.InheritedInterfaces)
            {
                result = $"{sourceInheritedInterface.Namespace}.{sourceInheritedInterface.Name}" == baseInterfaceType;
                if(result) break;

                result = sourceInheritedInterface.InheritedInterfaces.Any(i => i.InheritsInterface(baseInterfaceType));
                if(result) break;
            }

            return result;
        }
    }
}
