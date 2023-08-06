﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis;

using schema.binary;
using schema.binary.attributes;
using schema.util.diagnostics;
using schema.util.enumerables;
using schema.util.symbols;

namespace schema.util.types {
  public static partial class TypeV2 {
    public static ITypeV2 FromSymbol(ITypeSymbol symbol)
      => new SymbolTypeV2(symbol, null);

    internal static ITypeV2 FromSymbol(
        ITypeSymbol symbol,
        IDiagnosticReporter diagnosticReporter)
      => new SymbolTypeV2(symbol, diagnosticReporter);

    private class SymbolTypeV2 : BSymbolTypeV2 {
      private readonly ITypeSymbol symbol_;
      private IDiagnosticReporter? diagnosticReporter_;

      public SymbolTypeV2(ITypeSymbol symbol,
                          IDiagnosticReporter? diagnosticReporter) {
        this.symbol_ = symbol;
        this.diagnosticReporter_ = diagnosticReporter;
      }

      public override string Name => this.symbol_.Name;

      public override string FullyQualifiedNamespace
        => this.symbol_.GetFullyQualifiedNamespace();

      public override IEnumerable<string> NamespaceParts
        => this.symbol_.GetContainingNamespaces();

      public override IEnumerable<string> DeclaringTypeNamesDownward
        => this.symbol_.GetDeclaringTypesDownward().Select(type => type.Name);

      public override bool Implements(Type type, out ITypeV2 matchingType) {
        var matchingTypeImpl =
            this.symbol_.Yield()
                .Concat(
                    (this.symbol_ as ITypeSymbol)?.AllInterfaces ??
                    Enumerable.Empty<ISymbol>())
                .SingleOrDefault(
                    symbol => symbol.IsExactlyType(type));
        matchingType = matchingTypeImpl != null
            ? TypeV2.FromSymbol(matchingTypeImpl)
            : null;
        return matchingType != null;
      }

      public override int GenericArgCount
        => (this.symbol_ as INamedTypeSymbol)?.TypeArguments.Length ?? 0;

      public override bool IsClass
        => (this.symbol_ as INamedTypeSymbol)?.TypeKind == TypeKind.Class;

      public override bool IsStruct
        => (this.symbol_ as INamedTypeSymbol)?.TypeKind == TypeKind.Struct;

      public override bool IsString
        => (this.symbol_ as ITypeSymbol)?.SpecialType ==
           SpecialType.System_String;

      public override bool IsArray(out ITypeV2 elementType) {
        var arrayTypeSymbol = this.symbol_ as IArrayTypeSymbol;
        elementType = arrayTypeSymbol != null
            ? TypeV2.FromSymbol(arrayTypeSymbol.ElementType)
            : default;
        return arrayTypeSymbol != null;
      }

      public override bool IsPrimitive(out SchemaPrimitiveType primitiveType) {
        primitiveType = GetPrimitiveType_(this.symbol_);
        return primitiveType != SchemaPrimitiveType.UNDEFINED;
      }

      public override bool IsEnum(out SchemaIntegerType underlyingType) {
        var underlyingSymbol =
            (this.symbol_ as INamedTypeSymbol)?.EnumUnderlyingType;
        if (underlyingSymbol == null) {
          underlyingType = default;
          return false;
        }

        underlyingType = GetPrimitiveType_(underlyingSymbol).AsIntegerType();
        return underlyingType != SchemaIntegerType.UNDEFINED;
      }

      private static SchemaPrimitiveType GetPrimitiveType_(ISymbol symbol) {
        var typeSymbol = symbol as ITypeSymbol;

        if (typeSymbol?.TypeKind == TypeKind.Enum) {
          return SchemaPrimitiveType.ENUM;
        }

        return typeSymbol?.SpecialType switch {
            SpecialType.System_Boolean => SchemaPrimitiveType.BOOLEAN,
            SpecialType.System_Char    => SchemaPrimitiveType.CHAR,
            SpecialType.System_SByte   => SchemaPrimitiveType.SBYTE,
            SpecialType.System_Byte    => SchemaPrimitiveType.BYTE,
            SpecialType.System_Int16   => SchemaPrimitiveType.INT16,
            SpecialType.System_UInt16  => SchemaPrimitiveType.UINT16,
            SpecialType.System_Int32   => SchemaPrimitiveType.INT32,
            SpecialType.System_UInt32  => SchemaPrimitiveType.UINT32,
            SpecialType.System_Int64   => SchemaPrimitiveType.INT64,
            SpecialType.System_UInt64  => SchemaPrimitiveType.UINT64,
            SpecialType.System_Single  => SchemaPrimitiveType.SINGLE,
            SpecialType.System_Double  => SchemaPrimitiveType.DOUBLE,
            _                          => SchemaPrimitiveType.UNDEFINED
        };
      }

      public override bool HasGenericArguments(
          out IEnumerable<ITypeV2> genericArguments) {
        if (this.GenericArgCount == 0) {
          genericArguments = default;
          return false;
        }

        genericArguments =
            (this.symbol_ as INamedTypeSymbol)!.TypeArguments.Select(
                TypeV2.FromSymbol);
        return true;
      }

      public override bool HasGenericConstraints(
          out IEnumerable<ITypeV2> genericConstraints) {
        var constraintTypes =
            (this.symbol_ as ITypeParameterSymbol)?.ConstraintTypes;
        if (constraintTypes?.Length == 0) {
          genericConstraints = default;
          return false;
        }

        genericConstraints = constraintTypes.Value.Select(TypeV2.FromSymbol);
        return true;
      }


      public override bool HasAttribute<TAttribute>()
        => this.GetAttributeData_<TAttribute>().Any();

      public override TAttribute GetAttribute<TAttribute>()
        => this.symbol_.GetAttributes<TAttribute>(this.diagnosticReporter_)
               .SingleOrDefault();

      public override IEnumerable<TAttribute> GetAttributes<TAttribute>()
        => this.symbol_.GetAttributeData<TAttribute>()
               .Select(attributeData => {
                 var attribute =
                     attributeData.Instantiate<TAttribute>(this.symbol_);
                 if (attribute is BMemberAttribute memberAttribute) {
                   memberAttribute.Init(this.diagnosticReporter_,
                                        TypeV2.FromSymbol(
                                            this.symbol_.ContainingType),
                                        this.symbol_.Name);
                 }

                 return attribute;
               });

      private IEnumerable<AttributeData> GetAttributeData_<TAttribute>()
          where TAttribute : Attribute {
        var attributeType = typeof(TAttribute);
        return this.symbol_
                   .GetAttributeData()
                   .Where(attributeData
                              => attributeData.AttributeClass?.IsExactlyType(
                                  attributeType) ?? false);
      }
    }
  }
}