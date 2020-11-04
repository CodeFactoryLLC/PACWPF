using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeFactory;
using CodeFactory.DotNet.CSharp;
using CodeFactory.DotNet.CSharp.FormattedSyntax;
using CodeFactoryExtensions.Formatting.CSharp;
namespace CodeFactoryExtensions.Common.Automation
{
    /// <summary>
    /// Helper class that provides generation of C# source code from csharp model objects.
    /// </summary>
    public static class CSharpSourceGenerationCommon
    {
        /// <summary>
        /// Generates the source code for a standard method that supports a standard type catch block, and bounds checking.
        /// </summary>
        /// <param name="memberData">Method data to be generated.</param>
        /// <param name="manager">The namespace manager to use for namespace management with type declarations.</param>
        /// <param name="includeBoundsCheck">Flag that determines if string and nullable type bounds checking should be included in a method implementation.</param>
        /// <param name="supportAsyncKeyword">Flag that determines if methods should be implemented with the async keyword when supported by the method implementation.</param>
        /// <param name="security">The security level to add to the generated source code. Will default to public if not provided</param>
        /// <param name="includeKeywords">Determines if the models current keywords should be added to the source code, default if false.</param>
        /// <param name="includeAbstraction">If keywords are to be included, determines if the abstract keyword should be used or ignored, default is false.</param>
        /// <param name="requireStaticKeyword">Adds the static keyword to the signature, default is false.</param>
        /// <param name="requireSealedKeyword">Adds the sealed keyword to the signature, default is false.</param>
        /// <param name="requireAbstractKeyword">Adds the abstract keyword to the signature, default is false.</param>
        /// <param name="requireOverrideKeyword">Adds the override keyword to the signature, default is false.</param>
        /// <param name="requireVirtualKeyword">Adds the virtual keyword to the signature, default is false.</param>
        /// <returns>The fully formatted method source code or null if the member could not be implemented.</returns>
        public static string GenerateStandardMethodSourceCode(CsMethod memberData, NamespaceManager manager, bool includeBoundsCheck, bool supportAsyncKeyword , CsSecurity security = CsSecurity.Public, bool includeKeywords = false,
            bool includeAbstraction = false, bool requireStaticKeyword = false, bool requireSealedKeyword = false, bool requireAbstractKeyword = false,
            bool requireOverrideKeyword = false, bool requireVirtualKeyword = false)
        {
            //Bounds checking to make sure all data that is needed is provided. If any required data is missing will return null.
            if (memberData == null) return null;
            if (!memberData.IsLoaded) return null;
            if (manager == null) return null;

            //C# helper used to format output syntax. 
            var formatter = new CodeFactory.SourceFormatter();

            //Using the formatter helper to generate a method signature.
            string methodSyntax = supportAsyncKeyword
                ? memberData.CSharpFormatMethodSignature(manager, true, true, security,includeKeywords,
                    includeAbstraction,requireStaticKeyword,requireSealedKeyword,requireAbstractKeyword,
                    requireOverrideKeyword,requireVirtualKeyword)
                : memberData.CSharpFormatMethodSignature(manager, false, true, security,includeKeywords, 
                    includeAbstraction, requireStaticKeyword, requireSealedKeyword,
                    requireAbstractKeyword, requireOverrideKeyword, requireVirtualKeyword);

            //If the method syntax was not created return.
            if (string.IsNullOrEmpty(methodSyntax)) return null;

            //If the member has document then will build the documentation.
            if (memberData.HasDocumentation)
                //Using a documentation helper that will generate an enumerator that will output all XML documentation for the member.
                foreach (var documentation in memberData.CSharpFormatXmlDocumentationEnumerator())
                {
                    //Appending each xml document line to the being of the member definition.
                    formatter.AppendCodeLine(0, documentation);
                }

            //The member has attributes assigned to it, append the attributes.
            if (memberData.HasAttributes)
                //Using a documentation helper that will generate an enumerator that will output each attribute definition.
                foreach (var attributeSyntax in memberData.Attributes.CSharpFormatAttributeDeclarationEnumerator(manager))
                {
                    //Appending each attribute definition before the member definition.
                    formatter.AppendCodeLine(0, attributeSyntax);
                }

            //Adding the method declaration
            formatter.AppendCodeLine(0, methodSyntax);
            formatter.AppendCodeLine(0, "{");

            //Processing each parameter for bounds checking if bounds checking is enabled.
            if (includeBoundsCheck & memberData.HasParameters)
            {

                foreach (ICsParameter paramData in memberData.Parameters)
                {
                    //If the parameter has a default value then continue will not bounds check parameters with a default value.
                    if (paramData.HasDefaultValue) continue;

                    //If the parameter is a string type add the following bounds check
                    if (paramData.ParameterType.WellKnownType == CsKnownLanguageType.String)
                    {
                        //Adding an if check 
                        formatter.AppendCodeLine(1, $"if(string.IsNullOrEmpty({paramData.Name}))");
                        formatter.AppendCodeLine(1, "{");

                        //Adding a throw of an argument null exception
                        formatter.AppendCodeLine(2, $"throw new ArgumentNullException(nameof({paramData.Name}));");
                        formatter.AppendCodeLine(1, "}");
                        formatter.AppendCodeLine(0);
                    }

                    // Check to is if the parameter is not a value type or a well know type if not then go ahead and perform a null bounds check.
                    if (!paramData.ParameterType.IsValueType & !paramData.ParameterType.IsWellKnownType)
                    {
                        //Adding an if check 
                        formatter.AppendCodeLine(1, $"if({paramData.Name} == null)");
                        formatter.AppendCodeLine(1, "{");

                        //Adding a throw of an argument null exception
                        formatter.AppendCodeLine(2, $"throw new ArgumentNullException(nameof({paramData.Name}));");
                        formatter.AppendCodeLine(1, "}");
                        formatter.AppendCodeLine(0);
                    }
                }
            }

            //Formatting standard try block for method
            formatter.AppendCodeLine(1, "try");
            formatter.AppendCodeLine(1, "{");
            formatter.AppendCodeLine(2, "//TODO: add execution logic here");
            formatter.AppendCodeLine(1, "}");

            //Formatting standard catch block for method
            formatter.AppendCodeLine(1, "catch (Exception unhandledException)");
            formatter.AppendCodeLine(1, "{");

            formatter.AppendCodeLine(2, "//TODO: Add exception handling for unhandledException");
            
            formatter.AppendCodeLine(1, "}");
            formatter.AppendCodeLine(0);

            //Add an exception for not implemented until the developer updates the logic.
            formatter.AppendCodeLine(1, "throw new NotImplementedException();");

            //if the return type is not void then add a to do message for the developer to add return logic.
            if (!memberData.IsVoid)
            {
                formatter.AppendCodeLine(0);
                formatter.AppendCodeLine(1, "//TODO: add return logic here");
            }
            formatter.AppendCodeLine(0, "}");
            formatter.AppendCodeLine(0);

            //Returning the fully formatted method.
            return formatter.ReturnSource();
        }

        /// <summary>
        /// Generates the source code for a standard event definition.
        /// </summary>
        /// <param name="memberData">Event data to be loaded.</param>
        /// <param name="manager">The namespace manager to use for namespace management with type declarations.</param>
        /// <param name="security">The security level to set the source event source code to. Will default to public if not provided.</param>
        /// <param name="useKeywords">Include the keywords currently assigned to the event model. Default value is to not include them.</param>
        /// <param name="includeAbstractKeyword">Optional parameter that determines if it will include the definition for the abstract keyword in the definition if it is defined. default is false.</param>
        /// <param name="requireStaticKeyword">Adds the static keyword to the signature, default is false.</param>
        /// <param name="requireSealedKeyword">Adds the sealed keyword to the signature, default is false.</param>
        /// <param name="requireAbstractKeyword">Adds the abstract keyword to the signature, default is false.</param>
        /// <param name="requireOverrideKeyword">Adds the override keyword to the signature, default is false.</param>
        /// <param name="requireVirtualKeyword">Adds the virtual keyword to the signature, default is false.</param>
        /// <returns>The fully formatted event source code or null if the member could not be implemented.</returns>
        public static string GenerateStandardEventSourceCode(CsEvent memberData, NamespaceManager manager,CsSecurity security = CsSecurity.Public,
            bool useKeywords = false, bool includeAbstractKeyword = false, bool requireStaticKeyword = false, bool requireSealedKeyword = false, 
            bool requireAbstractKeyword = false, bool requireOverrideKeyword = false, bool requireVirtualKeyword = false)
        {
            //Bounds checking to make sure all data that is needed is provided. If any required data is missing will return null.
            if (memberData == null) return null;
            if (!memberData.IsLoaded) return null;
            if (manager == null) return null;

            //C# helper used to format output syntax. 
            var formatter = new CodeFactory.SourceFormatter();

            //Using the formatter helper to generate a default event signature.
            string eventSyntax = memberData.CSharpFormatEventDeclaration(manager,true,security,useKeywords,
                includeAbstractKeyword,requireStaticKeyword,requireSealedKeyword,requireAbstractKeyword,requireOverrideKeyword,
                requireVirtualKeyword);

            //If the event syntax was not created return.
            if (string.IsNullOrEmpty(eventSyntax)) return null;

            //If the member has document then will build the documentation.
            if (memberData.HasDocumentation)
                //Using a documentation helper that will generate an enumerator that will output all XML documentation for the member.
                foreach (var documentation in memberData.CSharpFormatXmlDocumentationEnumerator())
                {
                    //Appending each xml document line to the being of the member definition.
                    formatter.AppendCodeLine(0, documentation);
                }

            //The member has attributes assigned to it, append the attributes.
            if (memberData.HasAttributes)

                //Using a documentation helper that will generate an enumerator that will output each attribute definition.
                foreach (var attributeSyntax in memberData.Attributes.CSharpFormatAttributeDeclarationEnumerator(manager))
                {
                    //Appending each attribute definition before the member definition.
                    formatter.AppendCodeLine(0, attributeSyntax);
                }

            //Adding the event declaration
            formatter.AppendCodeLine(0, eventSyntax);

            //Adding a extra line feed at the end of the declaration.
            formatter.AppendCodeLine(0);

            //The source formatter returning the final results.
            return formatter.ReturnSource();
        }



        /// <summary>
        /// Generates the source code for a standard property definition. That uses a standard getter and setter.
        /// </summary>
        /// <param name="memberData">property data to be loaded.</param>
        /// <param name="manager">The namespace manager to use for namespace management with type declarations.</param>
        /// <param name="security">The security level to set the source event source code to. Will default to public if not provided.</param>
        /// <param name="getSecurity">Set the security level of the get accessor if defined for the property, will default to unknown</param>
        /// <param name="setSecurity">Set the security level of the set accessor if defined for the property, will default to unknown</param>
        /// <param name="useKeywords">Include the keywords currently assigned to the event model. Default value is to not include them.</param>
        /// <param name="includeAbstractKeyword">Optional parameter that determines if it will include the definition for the abstract keyword in the definition if it is defined. default is false.</param>
        /// <param name="requireStaticKeyword">Adds the static keyword to the signature, default is false.</param>
        /// <param name="requireSealedKeyword">Adds the sealed keyword to the signature, default is false.</param>
        /// <param name="requireAbstractKeyword">Adds the abstract keyword to the signature, default is false.</param>
        /// <param name="requireOverrideKeyword">Adds the override keyword to the signature, default is false.</param>
        /// <param name="requireVirtualKeyword">Adds the virtual keyword to the signature, default is false.</param>
        /// <returns>The fully formatted event source code or null if the member could not be implemented.</returns>
        public static string GenerateStandardPropertySourceCode(CsProperty memberData, NamespaceManager manager,
            bool useKeywords = false, bool includeAbstractKeyword = false, bool requireStaticKeyword = false, 
            bool requireSealedKeyword = false, bool requireAbstractKeyword = false, bool requireOverrideKeyword = false,
            bool requireVirtualKeyword = false, CsSecurity security = CsSecurity.Public, 
            CsSecurity getSecurity = CsSecurity.Unknown, CsSecurity setSecurity = CsSecurity.Unknown )
        {
            //Bounds checking to make sure all data that is needed is provided. If any required data is missing will return null.
            if (memberData == null) return null;
            if (!memberData.IsLoaded) return null;
            if (manager == null) return null;

            //C# helper used to format output syntax. 
            var formatter = new CodeFactory.SourceFormatter();

            //Using the formatter helper to generate a default event signature.
            string propertySyntax = memberData.CSharpFormatDefaultPropertySignature(manager, useKeywords,
                includeAbstractKeyword, requireStaticKeyword, requireSealedKeyword, requireAbstractKeyword,
                requireOverrideKeyword, requireVirtualKeyword, security,setSecurity,getSecurity);

            //If the property syntax was not created return.
            if (string.IsNullOrEmpty(propertySyntax)) return null;

            //If the member has document then will build the documentation.
            if (memberData.HasDocumentation)
                //Using a documentation helper that will generate an enumerator that will output all XML documentation for the member.
                foreach (var documentation in memberData.CSharpFormatXmlDocumentationEnumerator())
                {
                    //Appending each xml document line to the being of the member definition.
                    formatter.AppendCodeLine(0, documentation);
                }

            //The member has attributes assigned to it, append the attributes.
            if (memberData.HasAttributes)

                //Using a documentation helper that will generate an enumerator that will output each attribute definition.
                foreach (var attributeSyntax in memberData.Attributes.CSharpFormatAttributeDeclarationEnumerator(manager))
                {
                    //Appending each attribute definition before the member definition.
                    formatter.AppendCodeLine(0, attributeSyntax);
                }

            //Adding the event declaration
            formatter.AppendCodeBlock(0, propertySyntax);

            //Adding a extra line feed at the end of the declaration.
            formatter.AppendCodeLine(0);

            //The source formatter returning the final results.
            return formatter.ReturnSource();
        }

        /// <summary>
        /// Generates the source code for a standard raise event method.
        /// </summary>
        /// <param name="eventData">The target event model to generate the raise method for.</param>
        /// <param name="manager">The namespace manager to manage type naming.</param>
        /// <param name="eventName">the name of implemented event to raise.</param>
        /// <param name="security">The target security level to set the event to.</param>
        /// <param name="eventHandler">Optional field, that provides the name of the direct event handler field to raise.</param>
        /// <param name="methodName">Optional parameter of the custom name of the method. If not set will be On[eventName]</param>
        /// <returns></returns>
        public static string GenerateStandardRaiseEventMethod(CsEvent eventData, NamespaceManager manager,
            string eventName, CsSecurity security, string eventHandler = null, string methodName = null)
        {
            //Bounds checking to make sure all data that is needed is provided. If any required data is missing will return null.
            if (eventData == null) return null;
            if (!eventData.IsLoaded) return null;
            if (manager == null) return null;
            if (string.IsNullOrEmpty(eventName)) return null;
            if (security == CsSecurity.Unknown) return null;

            var raiseMethod = eventData.EventHandlerDelegate.InvokeMethod;

            if (raiseMethod == null) return null;
            if (!raiseMethod.IsLoaded) return null;

            var parameters = raiseMethod.Parameters.CSharpFormatParametersSignature(manager,false);

            string raiseHandler = string.IsNullOrEmpty(eventHandler) ? eventName : eventHandler;

            StringBuilder parametersBuilder = new StringBuilder();
            int parameterCount = 0;
            foreach (var raiseMethodParameter in raiseMethod.Parameters)
            {
                parameterCount++;
                if (parameterCount > 1)
                {
                    parametersBuilder.Append(", ");
                }

                parametersBuilder.Append(raiseMethodParameter.Name);
            }


            //C# helper used to format output syntax. 
            var formatter = new SourceFormatter();


            string raiseMethodName = string.IsNullOrEmpty(methodName) ? $"On{eventName}" : methodName;
            
            formatter.AppendCodeLine(0, "/// <summary>");
            formatter.AppendCodeLine(0, $"/// Raises the event {eventName} when there are subscribers to the event.");
            formatter.AppendCodeLine(0, "/// </summary>");
            formatter.AppendCodeLine(0,$"{security.CSharpFormatKeyword()} {Keywords.Void} {raiseMethodName}{parameters}");
            formatter.AppendCodeLine(0, "{");
            formatter.AppendCodeLine(0);
            formatter.AppendCodeLine(1,$"var raiseEvent = {raiseHandler};");
            formatter.AppendCodeLine(1,$"raiseEvent?.Invoke({parametersBuilder});");
            formatter.AppendCodeLine(0);
            formatter.AppendCodeLine(0, "}");
            formatter.AppendCodeLine(0);

            return formatter.ReturnSource();
        }


        /// <summary>
        /// Generates the source code for a standard field definition.
        /// </summary>
        /// <param name="memberData">Event data to be loaded.</param>
        /// <param name="manager">The namespace manager to use for namespace management with type declarations.</param>
        /// <param name="fieldSecurity">Parameter to set the target security for the field.</param>
        /// <param name="includeKeywords">Optional parameter that will include all keywords assigned to the field from the source model. This is true by default.</param>
        /// <param name="implementConstant">Determines if the filed is implemented as constant is should be returned as a constant, default is true.</param>
        /// <param name="requireStaticKeyword">Adds the static keyword to the signature, default is false.</param>
        /// <param name="requireReadOnlyKeyword">Adds the readonly keyword to the signature, default is false.</param>
        /// <param name="requireConstant">Implements the field as a constant, default is false.</param>
        /// <param name="requireConstantValue">The value to set the constant to if required.</param>
        /// <returns>The fully formatted event source code or null if the member could not be implemented.</returns>
        public static string GenerateStandardFieldSourceCode(CsField memberData, NamespaceManager manager,
            CsSecurity fieldSecurity, bool includeKeywords = true, bool implementConstant = true,
            bool requireStaticKeyword = false, bool requireReadOnlyKeyword = false, bool requireConstant = false,
            string requireConstantValue = null)
        {
            //Bounds checking to make sure all data that is needed is provided. If any required data is missing will return null.
            if (memberData == null) return null;
            if (!memberData.IsLoaded) return null;
            if (manager == null) return null;

            //C# helper used to format output syntax. 
            var formatter = new CodeFactory.SourceFormatter();

            //Using the formatter helper to generate a default event signature.
            string fieldSyntax = memberData.CSharpFormatFieldDeclaration(manager, includeKeywords, fieldSecurity,
                implementConstant, requireStaticKeyword, requireReadOnlyKeyword, requireConstant, requireConstantValue);

            //If the property syntax was not created return.
            if (string.IsNullOrEmpty(fieldSyntax)) return null;

            //If the member has document then will build the documentation.
            if (memberData.HasDocumentation)
                //Using a documentation helper that will generate an enumerator that will output all XML documentation for the member.
                foreach (var documentation in memberData.CSharpFormatXmlDocumentationEnumerator())
                {
                    //Appending each xml document line to the being of the member definition.
                    formatter.AppendCodeLine(0, documentation);
                }

            //The member has attributes assigned to it, append the attributes.
            if (memberData.HasAttributes)

                //Using a documentation helper that will generate an enumerator that will output each attribute definition.
                foreach (var attributeSyntax in memberData.Attributes.CSharpFormatAttributeDeclarationEnumerator(manager))
                {
                    //Appending each attribute definition before the member definition.
                    formatter.AppendCodeLine(0, attributeSyntax);
                }

            //Adding the event declaration
            formatter.AppendCodeBlock(0, fieldSyntax);

            //Adding a extra line feed at the end of the declaration.
            formatter.AppendCodeLine(0);

            //The source formatter returning the final results.
            return formatter.ReturnSource();
        }

        /// <summary>
        /// Generates the source code for an event that supports add remove, does not generate the field definition.
        /// </summary>
        /// <param name="memberData">Event data to be loaded.</param>
        /// <param name="manager">The namespace manager to use for namespace management with type declarations.</param>
        /// <param name="fieldCamelCase">Flag that determines if the field will be set to camel case or left in proper case.</param>
        /// <param name="security">The security level to set the source event source code to. Will default to public if not provided.</param>
        /// <param name="useKeywords">Include the keywords currently assigned to the event model. Default value is to not include them.</param>
        /// <param name="includeAbstractKeyword">Optional parameter that determines if it will include the definition for the abstract keyword in the definition if it is defined. default is false.</param>
        /// <param name="requireStaticKeyword">Adds the static keyword to the signature, default is false.</param>
        /// <param name="requireSealedKeyword">Adds the sealed keyword to the signature, default is false.</param>
        /// <param name="requireAbstractKeyword">Adds the abstract keyword to the signature, default is false.</param>
        /// <param name="requireOverrideKeyword">Adds the override keyword to the signature, default is false.</param>
        /// <param name="requireVirtualKeyword">Adds the virtual keyword to the signature, default is false.</param>
        /// <param name="fieldNamePrefix">Optional parameter that determines the prefix to add to the field name that backs the event.</param>
        /// <param name="fieldSuffix">Parameter that determines what the suffix of the event name field is. Default is handler.</param>
        /// <returns>The fully formatted event source code or null if the member could not be implemented.</returns>
        public static string GenerateAddRemoveEvent(CsEvent memberData, NamespaceManager manager = null, CsSecurity security = CsSecurity.Public, string fieldNamePrefix = null, string fieldSuffix = "Handler", bool fieldCamelCase = true, 
            bool useKeywords = false, bool includeAbstractKeyword = false, bool requireStaticKeyword = false, bool requireSealedKeyword = false,
            bool requireAbstractKeyword = false, bool requireOverrideKeyword = false, bool requireVirtualKeyword = false)
        {
            //Bounds checking to make sure all data that is needed is provided. If any required data is missing will return null.
            if (memberData == null) return null;
            if (!memberData.IsLoaded) return null;
            if (manager == null) return null;

            //C# helper used to format output syntax. 
            var formatter = new CodeFactory.SourceFormatter();

            //If the member has document then will build the documentation.
            if (memberData.HasDocumentation)
                //Using a documentation helper that will generate an enumerator that will output all XML documentation for the member.
                foreach (var documentation in memberData.CSharpFormatXmlDocumentationEnumerator())
                {
                    //Appending each xml document line to the being of the member definition.
                    formatter.AppendCodeLine(0, documentation);
                }

            //The member has attributes assigned to it, append the attributes.
            if (memberData.HasAttributes)

                //Using a documentation helper that will generate an enumerator that will output each attribute definition.
                foreach (var attributeSyntax in memberData.Attributes.CSharpFormatAttributeDeclarationEnumerator(manager))
                {
                    //Appending each attribute definition before the member definition.
                    formatter.AppendCodeLine(0, attributeSyntax);
                }


            var eventSignature = memberData.CSharpFormatEventSignature(manager, true, security, useKeywords,
                includeAbstractKeyword, requireStaticKeyword, requireSealedKeyword, requireAbstractKeyword,
                requireOverrideKeyword, requireVirtualKeyword);

            if (string.IsNullOrEmpty(eventSignature)) return null;

            bool hasFieldPrefix = fieldNamePrefix != null;

            bool hasFieldSuffix = fieldSuffix != null;

            StringBuilder fieldNameBuilder = new StringBuilder();

            if (hasFieldPrefix) fieldNameBuilder.Append(fieldNamePrefix);

            var fieldNameData = hasFieldSuffix ? $"{memberData.Name}{fieldSuffix}": memberData.Name;

            fieldNameBuilder.Append(fieldCamelCase ? fieldNameData.ConvertToCamelCase() : fieldNameData);

            var fieldName = fieldNameBuilder.ToString();

            //Adding the event declaration
            formatter.AppendCodeLine(0, eventSignature);

            formatter.AppendCodeLine(0);
            formatter.AppendCodeLine(0,"{");
            formatter.AppendCodeLine(0);
            formatter.AppendCodeLine(1, "add");
            formatter.AppendCodeLine(1, "{");
            formatter.AppendCodeLine(2, $"{fieldName} += value;");
            formatter.AppendCodeLine(1, "}");
            formatter.AppendCodeLine(0);
            formatter.AppendCodeLine(1, "remove");
            formatter.AppendCodeLine(1, "{");
            formatter.AppendCodeLine(2, $"if({fieldName} !=null) {fieldName} -= value;");
            formatter.AppendCodeLine(1, "}");
            formatter.AppendCodeLine(0);
            formatter.AppendCodeLine(0, "}");
            formatter.AppendCodeLine(0);

            //The source formatter returning the final results.
            return formatter.ReturnSource();
        }

        public static string GeneratePartialClass(CsClass source,NamespaceManager manager = null)
        {
            if (source == null) return null;
            if (!source.IsLoaded) return null;
            SourceFormatter formatter = new SourceFormatter();

            StringBuilder classBuilder = new StringBuilder($"{source.Security.CSharpFormatKeyword()} partial {Keywords.Class} {source.Name}");

            if (source.IsGeneric) classBuilder.Append(source.GenericParameters.CSharpFormatGenericParametersSignature(manager));

            formatter.AppendCodeLine(0);
            formatter.AppendCodeLine(0,$"namespace {source.Namespace}");
            formatter.AppendCodeLine(0, "{");
            formatter.AppendCodeLine(0);
            formatter.AppendCodeLine(1,classBuilder.ToString());
            formatter.AppendCodeLine(1,"{");
            formatter.AppendCodeLine(1);
            formatter.AppendCodeLine(1, "}");
            formatter.AppendCodeLine(0);
            formatter.AppendCodeLine(0, "}");
            return formatter.ReturnSource();
        }

    }
}
