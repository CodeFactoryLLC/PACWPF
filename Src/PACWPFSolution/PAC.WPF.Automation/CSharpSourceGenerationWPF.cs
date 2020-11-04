using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using CodeFactory;
using CodeFactory.DotNet.CSharp;
using CodeFactoryExtensions.Formatting.CSharp;

namespace PAC.WPF.Automation
{
    public static class CSharpSourceGenerationWPF
    {

        public static string GenerateContractSubscriptionMethodName(CsField sourceField)
        {
            if (sourceField == null) return null;
            return $"{sourceField.Name.ConvertToProperCase(new[] {'_'})}_ContractSubscribe";
        }

        /// <summary>
        /// Generates a method that subscribes to a target interface contract for the target field.
        /// </summary>
        /// <param name="sourceField">The field for which events to subscribe to.</param>
        /// <param name="contract">The contract to subscribe to.</param>
        /// <returns>Full source code for the subscribe method.</returns>
        public static string GenerateContractSubscriptionMethod(CsField sourceField, CsInterface contract)
        {
            if (sourceField == null) return null;
            if (!sourceField.IsLoaded) return null;
            if (contract == null) return null;
            if (!contract.IsLoaded) return null;

            SourceFormatter formatter = new SourceFormatter();

            formatter.AppendCodeLine(0,"/// <summary>");
            formatter.AppendCodeLine(0, $"/// Subscribes to the events from the interface contract of {contract.Namespace}.{contract.Name} for the field {sourceField.Name}");
            formatter.AppendCodeLine(0, "/// </summary>");
            formatter.AppendCodeLine(0, $"private void {GenerateContractSubscriptionMethodName(sourceField)}()");
            formatter.AppendCodeLine(0, "{");
            formatter.AppendCodeLine(0);
            formatter.AppendCodeLine(1,$"if({sourceField.Name} == null) return;");
            formatter.AppendCodeLine(0);
            foreach (var contractEvent in contract.Events)
            {
                if(!contractEvent.IsLoaded) continue;
                formatter.AppendCodeLine(1,$"{sourceField.Name}.{contractEvent.Name} += {sourceField.Name.ConvertToProperCase(new[] { '_' })}_{contractEvent.Name}_EventHandler;");
            }
            formatter.AppendCodeLine(0);
            formatter.AppendCodeLine(0, "}");
            formatter.AppendCodeLine(0);

            return formatter.ReturnSource();
        }

        /// <summary>
        /// Generates the name of the contract release method name for the target field.
        /// </summary>
        /// <param name="sourceField">field to generate the release name for.</param>
        /// <returns></returns>
        public static string GenerateContractReleaseMethodName(CsField sourceField)
        {
            if (sourceField == null) return null;
            return $"{sourceField.Name.ConvertToProperCase(new[] {'_'})}_ContractRelease";
        }

        /// <summary>
        /// Generates a method that releases to a target interface contract for the target field.
        /// </summary>
        /// <param name="sourceField">The field for which events to subscribe to.</param>
        /// <param name="contract">The contract to subscribe to.</param>
        /// <returns>Full source code for the subscribe method.</returns>
        public static string GenerateContractReleaseMethod(CsField sourceField, CsInterface contract)
        {
            if (sourceField == null) return null;
            if (!sourceField.IsLoaded) return null;
            if (contract == null) return null;
            if (!contract.IsLoaded) return null;

            SourceFormatter formatter = new SourceFormatter();

            formatter.AppendCodeLine(0, "/// <summary>");
            formatter.AppendCodeLine(0, $"/// Releases the events from the interface contract of {contract.Namespace}.{contract.Name} for the field {sourceField.Name}");
            formatter.AppendCodeLine(0, "/// </summary>");
            formatter.AppendCodeLine(0, $"private void {GenerateContractReleaseMethodName(sourceField)}()");
            formatter.AppendCodeLine(0, "{");
            formatter.AppendCodeLine(0);
            formatter.AppendCodeLine(1, $"if({sourceField.Name} == null) return;");
            formatter.AppendCodeLine(0);
            foreach (var contractEvent in contract.Events)
            {
                if (!contractEvent.IsLoaded) continue;
                formatter.AppendCodeLine(1, $"{sourceField.Name}.{contractEvent.Name} -= {sourceField.Name.ConvertToProperCase(new []{'_'})}_{contractEvent.Name}_EventHandler;");
            }
            formatter.AppendCodeLine(0);
            formatter.AppendCodeLine(0, "}");
            formatter.AppendCodeLine(0);

            return formatter.ReturnSource();
        }

        public static string GenerateContractEventHandlerMethodName(CsField sourceField, CsEvent targetEvent)
        {
            if (sourceField == null) return null;
            if (!sourceField.IsLoaded) return null;
            if (targetEvent == null) return null;
            if (!targetEvent.IsLoaded) return null;

            return $"{sourceField.Name.ConvertToProperCase(new[] {'_'})}_{targetEvent.Name}_EventHandler";
        }

        public static string GenerateContractEventHandlerMethod(CsField sourceField, CsEvent targetEvent, NamespaceManager manager = null)
        {
            if (sourceField == null) return null;
            if (!sourceField.IsLoaded) return null;
            if (targetEvent == null) return null;
            if (!targetEvent.IsLoaded) return null;

            SourceFormatter formatter = new SourceFormatter();

            formatter.AppendCodeLine(0, "/// <summary>");
            formatter.AppendCodeLine(0, $"/// Handles the raised event {targetEvent.Name}");
            formatter.AppendCodeLine(0, "/// </summary>");
            formatter.AppendCodeLine(0, $"protected void {GenerateContractEventHandlerMethodName(sourceField,targetEvent)}{targetEvent.EventHandlerDelegate.Parameters.CSharpFormatParametersSignature(manager,false)}");
            formatter.AppendCodeLine(0, "{");
            formatter.AppendCodeLine(0);
            formatter.AppendCodeLine(1,"//TODO: Add Event handler logic");
            formatter.AppendCodeLine(0);
            formatter.AppendCodeLine(0, "}");
            formatter.AppendCodeLine(0);

            return formatter.ReturnSource();
        }

        public static string GenerateRaiseContractEvent(CsEvent targetEvent,NamespaceManager manager = null)
        {
            if (targetEvent == null) return null;
            if (!targetEvent.IsLoaded) return null;

            SourceFormatter formatter = new SourceFormatter();


            string eventParameters = targetEvent.RaiseMethod.Parameters.CSharpFormatParametersSignature(manager);


            int parameterCount = 0;

            StringBuilder parameterBuilder = new StringBuilder();

            foreach (var raiseMethodParameter in targetEvent.RaiseMethod.Parameters)
            {
                parameterCount++;

                if (parameterCount > 1)
                {
                    parameterBuilder.Append($", {raiseMethodParameter.Name}");
                    continue;
                }

                parameterBuilder.Append(raiseMethodParameter.Name);
            }

            formatter.AppendCodeLine(0, "/// <summary>");
            formatter.AppendCodeLine(0, $"/// Raises the event {targetEvent.Name}");
            formatter.AppendCodeLine(0, "/// </summary>");
            formatter.AppendCodeLine(0, $"protected void On{targetEvent.Name}{targetEvent.RaiseMethod.Parameters.CSharpFormatParametersSignature(manager,false)}");
            formatter.AppendCodeLine(0, "{");
            formatter.AppendCodeLine(0);
            formatter.AppendCodeLine(1, $"var raiseHandler = _{targetEvent.Name.ConvertToCamelCase()};");
            formatter.AppendCodeLine(1, $"raiseHandler?.Invoke({parameterBuilder});");
            formatter.AppendCodeLine(0);
            formatter.AppendCodeLine(0, "}");
            formatter.AppendCodeLine(0);

            return formatter.ReturnSource();
        }

        
    }
}
