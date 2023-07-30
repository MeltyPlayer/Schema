using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace schema.util {
  public static class AttributeUtil {
    public static TAttribute Instantiate<TAttribute>(
        this AttributeData attributeData,
        ISymbol attachedSymbol,
        IList<Diagnostic> diagnostics) where TAttribute : Attribute {
      var parameters = attributeData.AttributeConstructor.Parameters;

      // TODO: Does this still work w/ optional arguments?
      var attributeType = typeof(TAttribute);

      var constructor =
          attributeType.GetConstructors()
                       .FirstOrDefault(c => {
                         var cParameters = c.GetParameters();
                         if (cParameters.Length !=
                             parameters.Length) {
                           return false;
                         }

                         for (var i = 0;
                              i < parameters.Length;
                              ++i) {
                           if (parameters[i].Name !=
                               cParameters[i].Name) {
                             return false;
                           }
                         }

                         return true;
                       });
      if (constructor == null) {
        throw new Exception(
            $"Failed to find constructor for {typeof(TAttribute)}");
      }

      var attributeSyntax =
          attributeData.ApplicationSyntaxReference.GetSyntax() as
              AttributeSyntax;
      var argumentSyntaxList = attributeSyntax.ArgumentList?.Arguments;
      var argumentText = argumentSyntaxList?.Select(arg => arg.ToString())
                                           .ToArray();

      ISymbol parentType;
      if (attachedSymbol is IMethodSymbol or IFieldSymbol or IPropertySymbol) {
        parentType = attachedSymbol.ContainingType;
      } else {
        parentType = attachedSymbol;
      }

      var expectedParameterCount = constructor.GetParameters().Length;
      var arguments =
          attributeData
              .ConstructorArguments
              .Select((a, i) => a.Value is string
                          ? NameofUtil
                              .GetChainedAccessFromCallerArgumentExpression(
                                  parentType, argumentText[i])
                          : a.Value)
              .Concat(
                  Enumerable.Repeat(Type.Missing,
                                    Math.Max(0,
                                             expectedParameterCount -
                                             attributeData.ConstructorArguments
                                                 .Length)))
              .ToArray();

      var attribute = (TAttribute) constructor.Invoke(
          BindingFlags.OptionalParamBinding |
          BindingFlags.InvokeMethod |
          BindingFlags.CreateInstance,
          null,
          arguments,
          CultureInfo.InvariantCulture);

      return attribute;
    }
  }
}