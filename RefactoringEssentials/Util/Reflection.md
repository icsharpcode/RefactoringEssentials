Reflection in Utils
=====================

* CaseCorrector
    * `Microsoft.CodeAnalysis.CaseCorrection.CaseCorrector` (*Microsoft.CodeAnalysis.Workspaces*)
        * `Annotation`
        * `CaseCorrectAsync()`
    
* CSharpSyntaxContext
    * `Microsoft.CodeAnalysis.Shared.Extensions.ContextQuery.AbstractSyntaxContext` (*Microsoft.CodeAnalysis.Workspaces*)
        * `LeftToken`
        * `TargetToken`
        * `SyntaxTree`
        * `IsPreProcessorDirectiveContext`
        * `IsAnyExpressionContext`
        * `IsStatementContext`
        * `IsAttributeNameContext`
        * `IsInQuery`
    * `Microsoft.CodeAnalysis.CSharp.Extensions.ContextQuery.CSharpSyntaxContext` (*Microsoft.CodeAnalysis.CSharp.Workspaces*)
        * `CreateContext()`
        * `IsIsOrAsTypeContext`
        * `IsInstanceContext`
        * `IsNonAttributeExpressionContext`
        * `IsPreProcessorKeywordContext`
        * `ContainingTypeDeclaration`
        * `IsParameterTypeContext`
        * `IsMemberDeclarationContext()`
        * `IsTypeDeclarationContext()`
        * `IsInNonUserCode`
        * `IsIsOrAsContext`
        * `IsTypeAttributeContext()`
        * `IsDefiniteCastTypeContext`
        * `IsObjectCreationTypeContext`
        * `IsGenericTypeArgumentContext`
        * `IsLocalVariableDeclarationContext`
        * `IsFixedVariableDeclarationContext`
        * `IsPossibleLambdaOrAnonymousMethodParameterTypeContext`
        * `IsImplicitOrExplicitOperatorTypeContext`
        * `IsPrimaryFunctionExpressionContext`
        * `IsCrefContext`
        * `IsDelegateReturnTypeContext`
        * `IsEnumBaseListContext`
        * `IsConstantExpressionContext`
        * `IsMemberAttributeContext`
        * `PrecedingModifiers`
        * `IsTypeOfExpressionContext`
        * `ContainingTypeOrEnumDeclaration`
    
* ExpressionSyntaxExtensions
    * `Microsoft.CodeAnalysis.CSharp.Extensions.ExpressionSyntaxExtensions` (*Microsoft.CodeAnalysis.CSharp.Workspaces*)
        * `CastIfPossible()`
        * `TryReduceOrSimplifyExplicitName()`
    
* ITypeSymbolExtensions
    * `Microsoft.CodeAnalysis.CSharp.Extensions.ITypeSymbolExtensions` (*Microsoft.CodeAnalysis.CSharp.Workspaces*)
        * `GenerateTypeSyntax()`
        * `ContainingTypesOrSelfHasUnsafeKeyword()`
    * `Microsoft.CodeAnalysis.Shared.Extensions.ITypeSymbolExtensions` (*Microsoft.CodeAnalysis.Workspaces*)
        * `InheritsFromOrEqualsIgnoringConstruction()`
        * `RemoveUnavailableTypeParameters()`
        * `RemoveUnnamedErrorTypes()`
        * `ReplaceTypeParametersBasedOnTypeConstraints()`
        * `SubstituteTypes()` (2x)
    
* SignatureComparer
    * `Microsoft.CodeAnalysis.Shared.Utilities.SignatureComparer` (*Microsoft.CodeAnalysis.Workspaces*)
        * `HaveSameSignature()` (5x)
        * `HaveSameSignatureAndConstraintsAndReturnTypeAndAccessors()`
    
* SpeculationAnalyzer
    * `Microsoft.CodeAnalysis.Shared.Utilities.AbstractSpeculationAnalyzer´8` (*Microsoft.CodeAnalysis.Workspaces*)
        * `SymbolsForOriginalAndReplacedNodesAreCompatible()`
        * `ReplacementChangesSemantics()`
        * `SymbolInfosAreCompatible()`
    * `Microsoft.CodeAnalysis.CSharp.Utilities.SpeculationAnalyzer` (*Microsoft.CodeAnalysis.CSharp.Workspaces*)
        * `CreateSpeculativeSemanticModelForNode()`
    
* SyntaxExtensions
    * `Microsoft.CodeAnalysis.CSharp.Extensions.ParenthesizedExpressionSyntaxExtensions` (*Microsoft.CodeAnalysis.CSharp.Workspaces*)
        * `CanRemoveParentheses()`
    * `Microsoft.CodeAnalysis.CSharp.Extensions.MemberDeclarationSyntaxExtensions` (*Microsoft.CodeAnalysis.CSharp.Workspaces*)
        * `GetLocalDeclarationMap()`
    * `Microsoft.CodeAnalysis.CSharp.Extensions.MemberDeclarationSyntaxExtensions+LocalDeclarationMap` (*Microsoft.CodeAnalysis.CSharp.Workspaces*)
        * `GetLocalDeclarationMap()`
    * `Microsoft.CodeAnalysis.Shared.Extensions.SyntaxTokenExtensions` (*Microsoft.CodeAnalysis.Workspaces*)
        * `GetAncestors()`
        * `CanReplaceWithReducedName()` (2x)
        
* SyntaxNodeExtensions
    * `Microsoft.CodeAnalysis.CSharp.Extensions.SyntaxNodeExtensions`(*Microsoft.CodeAnalysis.CSharp.Workspaces*)
        * `ContainsInterleavedDirective()`
    
* TypeExtensions
    * `Microsoft.CodeAnalysis.CSharp.Extensions.ITypeSymbolExtensions` (*Microsoft.CodeAnalysis.CSharp.Workspaces*)
        * `GenerateTypeSyntax()`
    * `Microsoft.CodeAnalysis.FindSymbols.DependentTypeFinder` (*Microsoft.CodeAnalysis.Workspaces*)
        * `FindDerivedClassesAsync()`
    
* TypeGenerator
    * `Microsoft.CodeAnalysis.CodeGeneration.TypeGenerator` (*Microsoft.CodeAnalysis.Workspaces*)
        * `CreateArrayTypeSymbol()`
        * `CreatePointerTypeSymbol()`
        * `Construct()`
