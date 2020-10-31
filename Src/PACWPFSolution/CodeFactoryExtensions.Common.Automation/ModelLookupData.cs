using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFactoryExtensions.Common.Automation
{
    /// <summary>
    /// Data class used to find a target model by its namespace and name.
    /// </summary>
    public class ModelLookupData
    {
        private readonly string _namespace;

        private readonly string _name;

        private ModelLookupData(string nameSpace, string name)
        {
            _namespace = nameSpace;
            _name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelLookupData"/>
        /// </summary>
        /// <param name="nameSpace">The target namespace for the lookup.</param>
        /// <param name="name">The name of the model to find.</param>
        /// <returns></returns>
        public static ModelLookupData Init(string nameSpace, string name) => new ModelLookupData(nameSpace,name);

        /// <summary>
        /// The fully qualified name of the lookup object.
        /// </summary>
        public string FullName => $"{_namespace}.{_name}";

        /// <summary>
        /// The namespace of the lookup object
        /// </summary>
        public string Namespace => _namespace;

        /// <summary>
        /// The name of the lookup object
        /// </summary>
        public string Name => _name;
    }
}
