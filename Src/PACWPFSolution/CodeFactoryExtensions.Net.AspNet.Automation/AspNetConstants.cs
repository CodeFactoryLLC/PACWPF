using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFactoryExtensions.Net.AspNet.Automation
{
    /// <summary>
    /// 
    /// </summary>
    public static class AspNetConstants
    {
        /// <summary>
        /// The fully qualified name of the a aspnet core controller.
        /// </summary>
        public const string ControllerBaseName = "Microsoft.AspNetCore.Mvc.ControllerBase";

        /// <summary>
        /// The fully qualified name of the controller base class name.
        /// </summary>
        public const string ControllerBaseClassName = "ControllerBase";
        /// <summary>
        /// The full namespace for the mvc namespace.
        /// </summary>
        public const string MvcNamespace = "Microsoft.AspNetCore.Mvc";

        /// <summary>
        /// The full name of the library that implements MVC/web api controller functions.
        /// </summary>
        public const string MvcLibraryName = "Microsoft.AspNetCore.Mvc.Core";

        /// <summary>
        /// Name for the abstract classes that support action result.
        /// </summary>
        public const string ActionResultClassName = "ActionResult";

        /// <summary>
        /// Name of the interface for action results.
        /// </summary>
        public const string ActionResultInterfaceName = "IActionResult";

        /// <summary>
        /// Name of the attribute that supports HTTP verb Get in MVC namespace controllers.
        /// </summary>
        public const string HttpGetAttributeName = "HttpGetAttribute";

        /// <summary>
        /// Name of the attribute that supports HTTP verb Post in MVC namespace controllers.
        /// </summary>
        public const string HttpPostAttributeName = "HttpPostAttribute";

        /// <summary>
        /// Name of the attribute that supports HTTP verb Put in MVC namespace controllers.
        /// </summary>
        public const string HttpPutAttributeName = "HttpPutAttribute";

        /// <summary>
        /// Name of the attribute that supports HTTP verb Delete in MVC namespace controllers.
        /// </summary>
        public const string HttpDeleteAttributeName = "HttpDeleteAttribute";
    }
}
