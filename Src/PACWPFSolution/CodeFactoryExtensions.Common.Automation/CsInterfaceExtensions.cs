using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using CodeFactory.DotNet.CSharp;
namespace CodeFactoryExtensions.Common.Automation
{
    /// <summary>
    /// Extension methods class for the <see cref="CsInterface"/> model.
    /// </summary>
    public static class CsInterfaceExtensions
    {
        /// <summary>
        /// Extension method that checks if this interface is the target type, or if it inherits it.
        /// </summary>
        /// <param name="source">Interface to check.</param>
        /// <param name="lookupInterface"></param>
        /// <param name="supportGenerics"></param>
        /// <returns></returns>
        public static CsInterface IsTargetInterface(this CsInterface source, ModelLookupData lookupInterface,
            bool supportGenerics = false)
        {
            if (source == null) return null;
            if (!source.IsLoaded) return null;

            if (source.Namespace == lookupInterface.Namespace)
            {
                return source;
            }

            return source.InheritedInterfaces.Any(i => i.InheritsInterface(lookupInterface.FullName, supportGenerics)) ? source : null;
        }

    }
}
