using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeFactory.DotNet.CSharp;
using CodeFactory.DotNet.CSharp.FormattedSyntax;

namespace CodeFactoryExtensions.Formatting.CSharp
{
    /// <summary>
    /// Extensions class that manage extensions that support CodeFactory models that implement the <see cref="CsField"/> model.
    /// </summary>
    public static class CsFieldExtensions
    {
        /// <summary>
        /// Generates the syntax definition of field in c# syntax. The default definition with all options turned off will return the filed signature and constants if defined and the default values.
        /// </summary>
        /// <example>
        /// With Keywords [Security] [Keywords] [FieldType] [Name];
        /// With Keywords and a constant [Security] [Keywords] [FieldType] [Name] = [Constant Value];
        /// Without Keywords [Security] [FieldType] [Name];
        /// Without Keywords and a constant [Security] [FieldType] [Name] = [Constant Value];
        /// </example>
        /// <param name="source">The source <see cref="CsField"/> model to generate.</param>
        /// <param name="manager">Namespace manager used to format type names.This is an optional parameter.</param>
        /// <param name="includeKeywords">Optional parameter that will include all keywords assigned to the field from the source model. This is true by default.</param>
        /// <param name="fieldSecurity">Optional parameter to set the target security for the field.</param>
        /// <param name="implementConstant">Determines if the filed is implemented as constant is should be returned as a constant, default is true.</param>
        /// <param name="requireStaticKeyword">Adds the static keyword to the signature, default is false.</param>
        /// <param name="requireReadOnlyKeyword">Adds the readonly keyword to the signature, default is false.</param>
        /// <param name="requireConstant">Implements the field as a constant, default is false.</param>
        /// <param name="requireConstantValue">The value to set the constant to if required.</param>
        /// <returns>Fully formatted field definition or null if the field data could not be generated.</returns>
        public static string CSharpFormatFieldDeclaration(this CsField source,NamespaceManager manager = null,
            bool includeKeywords = true, CsSecurity fieldSecurity = CsSecurity.Unknown, bool implementConstant = true, 
            bool requireStaticKeyword = false, bool requireReadOnlyKeyword = false, bool requireConstant = false,
            string requireConstantValue = null)
        {
            if (source == null) return null;

            StringBuilder fieldFormatting = new StringBuilder();


            CsSecurity security = fieldSecurity == CsSecurity.Unknown ? source.Security : fieldSecurity;

            fieldFormatting.Append($"{security.CSharpFormatKeyword()} ");

            string constantValue = null;

            bool staticKeyword = false;
            bool readOnlyKeyword = false;
            bool constantKeyword = false;

            if (includeKeywords)
            {
                if (source.IsStatic) staticKeyword = true;
                if (source.IsReadOnly) readOnlyKeyword = true;
            }

            if (source.IsConstant & implementConstant)
            {
                constantKeyword = true;
                constantValue = source.ConstantValue;
            }

            if (!staticKeyword) staticKeyword = requireStaticKeyword;
            if (!readOnlyKeyword) readOnlyKeyword = requireReadOnlyKeyword;
            if (!constantKeyword) constantKeyword = requireConstant;

            if (constantKeyword & string.IsNullOrEmpty(constantValue)) constantValue = requireConstantValue;


            if (staticKeyword) fieldFormatting.Append($"{Keywords.Static} ");
            if (readOnlyKeyword) fieldFormatting.Append($"{Keywords.Readonly} ");
            if (constantKeyword) fieldFormatting.Append($"{Keywords.Constant} ");

            fieldFormatting.Append($"{source.DataType.CSharpFormatTypeName(manager)} ");
            fieldFormatting.Append($"{source.Name}");
            if (constantKeyword) fieldFormatting.Append($" = {source.DataType.CSharpFormatValueSyntax(constantValue)}");
            fieldFormatting.Append(";");

            return fieldFormatting.ToString();
        }
    }
}
