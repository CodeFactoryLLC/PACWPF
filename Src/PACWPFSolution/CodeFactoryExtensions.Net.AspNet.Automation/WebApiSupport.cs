using System.Linq;
using System.Threading.Tasks;
using CodeFactory.DotNet.CSharp;
using CodeFactory.VisualStudio;
using CodeFactoryExtensions.Common.Automation;

namespace CodeFactoryExtensions.Net.AspNet.Automation
{
    /// <summary>
    /// Helper classes that are used when supporting a web api implementation in a net implementation.
    /// </summary>
    public static class WebApiSupport
    {
        /// <summary>
        /// Determines if the source code has classes that implement controller base.
        /// </summary>
        /// <param name="source">Source code to evaluate.</param>
        /// <returns>True if found, or false if not</returns>
        public static bool IsControllerSourceCode(VsCSharpSource source)
        {
            if (source == null) return false;
            if (!source.IsLoaded) return false;
            var classes = source.SourceCode.Classes;

            if (classes == null) return false;
            return classes.Any(IsController);
        }

        /// <summary>
        /// Determines if the class supports the MVC controller base class. 
        /// </summary>
        /// <param name="sourceClass"></param>
        /// <returns></returns>
        public static bool IsController(CsClass sourceClass)
        {
            var baseClass = sourceClass?.BaseClass;
            if (baseClass == null) return false;

            if (sourceClass.Namespace != AspNetConstants.MvcNamespace) return false;

            if (sourceClass.Name == AspNetConstants.ControllerBaseClassName) return true;

            bool isBaseClass = false;

            if (baseClass.BaseClass != null) isBaseClass = IsController(baseClass);

            return isBaseClass;

        }

        /// <summary>
        /// Determines if a target method is a controller action.
        /// </summary>
        /// <param name="sourceMethod">Method to check if it is a controller action.</param>
        /// <returns>True if the method supports controller action attributes, false if it does not</returns>
        public static bool IsControllerAction(CsMethod sourceMethod)
        {
            if (sourceMethod == null) return false;
            if (!sourceMethod.IsLoaded) return false;

            return !sourceMethod.HasAttributes ? false :sourceMethod.Attributes.Any(IsWebApiAttribute);
        }

        /// <summary>
        /// Checks the attribute to determine if a HTTP verb supported by web api.
        /// </summary>
        /// <param name="sourceAttribute">Target attribute to search.</param>
        /// <returns>True if found or false if not.</returns>
        public static bool IsWebApiAttribute(CsAttribute sourceAttribute)
        {
            if (sourceAttribute == null) return false;
            if (!sourceAttribute.IsLoaded) return false;
            var attributeType = sourceAttribute.Type;

            if (attributeType?.Namespace != AspNetConstants.MvcNamespace) return false;

            bool result = false;
            switch (attributeType.Name)
            {
                

                case AspNetConstants.HttpGetAttributeName:
                    result = true;
                    break;

                case AspNetConstants.HttpPostAttributeName:
                    result = true;
                    break;

                case AspNetConstants.HttpDeleteAttributeName:
                    result = true;
                    break;

                case AspNetConstants.HttpPutAttributeName:
                    result = true;
                    break;

                default:
                    result = false;
                    break;
            }

            return result;
        }
    }
}
