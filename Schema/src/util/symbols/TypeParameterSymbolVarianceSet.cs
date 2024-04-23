using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;

namespace schema.util.symbols {
  public interface ITypeParameterSymbolVarianceSet {
    VarianceKind AllowedVariance(ITypeParameterSymbol typeParameterSymbol);
  }

  [Flags]
  public enum AllowedVarianceType {
    NONE = 0,
    OUT = 1,
    IN = 2,

    BOTH = OUT | IN
  }

  public static class AllowedVarianceTypeExtensions {
    public static bool AllowsOut(this AllowedVarianceType allowedVariance)
      => (allowedVariance & AllowedVarianceType.OUT) != 0;

    public static bool AllowsIn(this AllowedVarianceType allowedVariance)
      => (allowedVariance & AllowedVarianceType.IN) != 0;

    public static VarianceKind ToVarianceKind(
        this AllowedVarianceType allowedVariance)
      => allowedVariance.AllowsOut() ? VarianceKind.Out :
          allowedVariance.AllowsIn() ? VarianceKind.In : VarianceKind.None;

    public static AllowedVarianceType Intersection(
        this AllowedVarianceType allowedVariance,
        VarianceKind varianceKind)
      => varianceKind == VarianceKind.In && allowedVariance.AllowsIn()
          ? AllowedVarianceType.IN
          : varianceKind == VarianceKind.Out && allowedVariance.AllowsOut()
              ? AllowedVarianceType.OUT
              : AllowedVarianceType.NONE;
  }

  public class TypeParameterSymbolVarianceSet
      : ITypeParameterSymbolVarianceSet {
    private readonly IDictionary<ITypeSymbol, AllowedVarianceType> impl_
        = new Dictionary<ITypeSymbol, AllowedVarianceType>(
            SymbolEqualityComparer.Default);

    public TypeParameterSymbolVarianceSet(
        IEnumerable<ITypeParameterSymbol> containerTypeParameterSymbols,
        IEnumerable<INamedTypeSymbol> parentTypes,
        IReadOnlyList<IMethodSymbol> constMembers) {
      var knownContainerTypeParameterSymbols
          = new HashSet<ITypeSymbol>(containerTypeParameterSymbols,
                                     SymbolEqualityComparer.Default);

      foreach (var containerTypeParameter in
               knownContainerTypeParameterSymbols) {
        this.impl_[containerTypeParameter] = AllowedVarianceType.BOTH;
      }

      {
        var visitedParentTypeSymbols
            = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        foreach (var parentType in parentTypes) {
          this.VisitParentTypeSymbol_(parentType,
                                      visitedParentTypeSymbols,
                                      knownContainerTypeParameterSymbols);
        }
      }

      var visitedReturnTypeSymbols
          = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
      var visitedParameterTypeSymbols
          = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
      foreach (var constMember in constMembers) {
        this.VisitReturnTypeSymbol_(constMember.ReturnType,
                                    visitedReturnTypeSymbols,
                                    knownContainerTypeParameterSymbols);

        foreach (var parameter in constMember.Parameters) {
          this.VisitParameterTypeSymbol_(parameter.Type,
                                         visitedParameterTypeSymbols,
                                         knownContainerTypeParameterSymbols);
        }
      }
    }

    public VarianceKind AllowedVariance(
        ITypeParameterSymbol typeParameterSymbol)
      => this.impl_[typeParameterSymbol].ToVarianceKind();

    private void VisitParentTypeSymbol_(
        ITypeSymbol parentTypeSymbol,
        ISet<ITypeSymbol> visitedParentTypeSymbols,
        ISet<ITypeSymbol> knownContainerTypeParameterSymbols) {
      ForEachNewlyVisited_(
          parentTypeSymbol,
          visitedParentTypeSymbols,
          knownContainerTypeParameterSymbols,
          (typeSymbol, typeParam) => {
            if (typeParam != null) {
              this.impl_[typeSymbol] =
                  this.impl_[typeSymbol].Intersection(typeParam.Variance);
            }
          });
    }

    private void VisitReturnTypeSymbol_(
        ITypeSymbol returnTypeSymbol,
        ISet<ITypeSymbol> visitedReturnTypeSymbols,
        ISet<ITypeSymbol> knownContainerTypeParameterSymbols) {
      ForEachNewlyVisited_(
          returnTypeSymbol,
          visitedReturnTypeSymbols,
          knownContainerTypeParameterSymbols,
          (typeSymbol, typeParam) => {
            this.impl_[typeSymbol] =
                this.impl_[typeSymbol].Intersection(VarianceKind.Out);

            if (typeParam != null) {
              this.impl_[typeSymbol] =
                  this.impl_[typeSymbol].Intersection(typeParam.Variance);
            }
          });
    }

    private void VisitParameterTypeSymbol_(
        ITypeSymbol parameterTypeSymbol,
        ISet<ITypeSymbol> visitedParameterTypeSymbols,
        ISet<ITypeSymbol> knownContainerTypeParameterSymbols) {
      ForEachNewlyVisited_(
          parameterTypeSymbol,
          visitedParameterTypeSymbols,
          knownContainerTypeParameterSymbols,
          (typeSymbol, typeParam) => {
            this.impl_[typeSymbol] =
                this.impl_[typeSymbol].Intersection(VarianceKind.In);

            if (typeParam != null) {
              this.impl_[typeSymbol] =
                  this.impl_[typeSymbol].Intersection(typeParam.Variance);
            }
          });
    }

    private delegate void ForEachSymbolDelegate(
        ITypeSymbol typeSymbol,
        ITypeParameterSymbol? typeParam);

    private static void ForEachNewlyVisited_(
        ITypeSymbol currentTypeSymbol,
        ISet<ITypeSymbol> visitedTypeSymbols,
        ISet<ITypeSymbol> symbolsToMatchAgainst,
        ForEachSymbolDelegate matchHandler)
      => ForEachNewlyVisitedImpl_(currentTypeSymbol,
                                  null,
                                  visitedTypeSymbols,
                                  symbolsToMatchAgainst,
                                  matchHandler);

    private static void ForEachNewlyVisitedImpl_(
        ITypeSymbol currentTypeSymbol,
        ITypeParameterSymbol? currentTypeParameterSymbol,
        ISet<ITypeSymbol> visitedTypeSymbols,
        ISet<ITypeSymbol> symbolsToMatchAgainst,
        ForEachSymbolDelegate handler) {
      if (!visitedTypeSymbols.Add(currentTypeSymbol)) {
        return;
      }

      if (symbolsToMatchAgainst.Contains(currentTypeSymbol)) {
        handler(currentTypeSymbol, currentTypeParameterSymbol);
      }

      if (currentTypeSymbol.IsGenericTypeParameter(
              out var asTypeParameterSymbol)) {
        foreach (var constraintType in asTypeParameterSymbol.ConstraintTypes) {
          ForEachNewlyVisitedImpl_(constraintType,
                                   asTypeParameterSymbol,
                                   visitedTypeSymbols,
                                   symbolsToMatchAgainst,
                                   handler);
        }
      }

      if (currentTypeSymbol.IsGenericZipped(out var typeParamsAndArgs)) {
        foreach (var (typeParam, typeArg) in typeParamsAndArgs) {
          foreach (var constraintType in typeParam.ConstraintTypes) {
            ForEachNewlyVisitedImpl_(constraintType,
                                     typeParam,
                                     visitedTypeSymbols,
                                     symbolsToMatchAgainst,
                                     handler);
          }

          ForEachNewlyVisitedImpl_(typeArg,
                                   typeParam,
                                   visitedTypeSymbols,
                                   symbolsToMatchAgainst,
                                   handler);
        }
      }
    }
  }
}