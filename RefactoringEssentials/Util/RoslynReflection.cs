using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactoringEssentials
{
	/// <summary>
	/// Builds a lazy-loaded cache of Roslyn's internal types and members used through reflection.
	/// </summary>
	static class RoslynReflection
    {
        // CaseCorrector
        public static CaseCorrectorWrapper CaseCorrector => caseCorrectorWrapper.Value;
        static readonly Lazy<CaseCorrectorWrapper> caseCorrectorWrapper = new Lazy<CaseCorrectorWrapper>(() => new CaseCorrectorWrapper());
        public class CaseCorrectorWrapper
        {
            public readonly FieldInfo AnnotationField;
            public readonly MethodInfo CaseCorrectAsyncMethod;

            public CaseCorrectorWrapper()
            {
                var typeInfo = Type.GetType("Microsoft.CodeAnalysis.CaseCorrection.CaseCorrector" + ReflectionNamespaces.WorkspacesAsmName, true);

                AnnotationField = typeInfo.GetField("Annotation", BindingFlags.Public | BindingFlags.Static);
                CaseCorrectAsyncMethod = typeInfo.GetMethod("CaseCorrectAsync", new[] {
                    typeof(Document),
                    typeof(SyntaxAnnotation),
                    typeof(CancellationToken)
                });
            }
        }

        // CSharpSyntaxContext
        public static AbstractSyntaxContextWrapper AbstractSyntaxContext => abstractSyntaxContextWrapper.Value;
        static readonly Lazy<AbstractSyntaxContextWrapper> abstractSyntaxContextWrapper =
            new Lazy<AbstractSyntaxContextWrapper>(() => new AbstractSyntaxContextWrapper());
        public class AbstractSyntaxContextWrapper
        {
            public readonly Type TypeInfo;

            public readonly PropertyInfo LeftTokenProperty;
            public readonly PropertyInfo TargetTokenProperty;
            public readonly PropertyInfo SyntaxTreeProperty;
            public readonly PropertyInfo IsPreProcessorDirectiveContextProperty;
            public readonly PropertyInfo IsAnyExpressionContextProperty;
            public readonly PropertyInfo IsStatementContextProperty;
            public readonly PropertyInfo IsAttributeNameContextProperty;
            public readonly PropertyInfo IsInQueryProperty;

            public AbstractSyntaxContextWrapper()
            {
                TypeInfo = Type.GetType("Microsoft.CodeAnalysis.Shared.Extensions.ContextQuery.AbstractSyntaxContext" + ReflectionNamespaces.WorkspacesAsmName, true);

                LeftTokenProperty = TypeInfo.GetProperty("LeftToken");
                TargetTokenProperty = TypeInfo.GetProperty("TargetToken");
                SyntaxTreeProperty = TypeInfo.GetProperty("SyntaxTree");
                IsPreProcessorDirectiveContextProperty = TypeInfo.GetProperty("IsPreProcessorDirectiveContext");
                IsAnyExpressionContextProperty = TypeInfo.GetProperty("IsAnyExpressionContext");
                IsStatementContextProperty = TypeInfo.GetProperty("IsStatementContext");
                IsAttributeNameContextProperty = TypeInfo.GetProperty("IsAttributeNameContext");
                IsInQueryProperty = TypeInfo.GetProperty("IsInQuery");
            }
        }

        public static CSharpSyntaxContextWrapper CSharpSyntaxContext => cSharpSyntaxContextWrapper.Value;
        static readonly Lazy<CSharpSyntaxContextWrapper> cSharpSyntaxContextWrapper =
            new Lazy<CSharpSyntaxContextWrapper>(() => new CSharpSyntaxContextWrapper());
        public class CSharpSyntaxContextWrapper
        {
            public readonly Type TypeInfo;

            public readonly MethodInfo CreateContextMethod;
            public readonly FieldInfo IsIsOrAsTypeContextField;
            public readonly FieldInfo IsInstanceContextField;
            public readonly FieldInfo IsNonAttributeExpressionContextField;
            public readonly FieldInfo IsPreProcessorKeywordContextField;
            public readonly FieldInfo IsPreProcessorExpressionContextField;
            public readonly FieldInfo ContainingTypeDeclarationField;
            public readonly FieldInfo IsGlobalStatementContextField;
            public readonly FieldInfo IsParameterTypeContextField;
            public readonly MethodInfo IsMemberDeclarationContextMethod;
            public readonly MethodInfo IsTypeDeclarationContextMethod;
            public readonly FieldInfo IsInNonUserCodeField;
            public readonly FieldInfo IsIsOrAsContextField;
            public readonly MethodInfo IsTypeAttributeContextMethod;
            public readonly FieldInfo IsDefiniteCastTypeContextField;
            public readonly FieldInfo IsObjectCreationTypeContextField;
            public readonly FieldInfo IsGenericTypeArgumentContextField;
            public readonly FieldInfo IsLocalVariableDeclarationContextField;
            public readonly FieldInfo IsFixedVariableDeclarationContextField;
            public readonly FieldInfo IsPossibleLambdaOrAnonymousMethodParameterTypeContextField;
            public readonly FieldInfo IsImplicitOrExplicitOperatorTypeContextField;
            public readonly FieldInfo IsPrimaryFunctionExpressionContextField;
            public readonly FieldInfo IsCrefContextField;
            public readonly FieldInfo IsDelegateReturnTypeContextField;
            public readonly FieldInfo IsEnumBaseListContextField;
            public readonly FieldInfo IsConstantExpressionContextField;
            public readonly MethodInfo IsMemberAttributeContextMethod;
            public readonly FieldInfo PrecedingModifiersField;
            public readonly FieldInfo IsTypeOfExpressionContextField;
            public readonly FieldInfo ContainingTypeOrEnumDeclarationField;

            public CSharpSyntaxContextWrapper()
            {
                TypeInfo = Type.GetType("Microsoft.CodeAnalysis.CSharp.Extensions.ContextQuery.CSharpSyntaxContext" + ReflectionNamespaces.CSWorkspacesAsmName, true);

                CreateContextMethod = TypeInfo.GetMethod("CreateContext", BindingFlags.Static | BindingFlags.Public);
                IsIsOrAsTypeContextField = TypeInfo.GetField("IsIsOrAsTypeContext");
                IsInstanceContextField = TypeInfo.GetField("IsInstanceContext");
                IsNonAttributeExpressionContextField = TypeInfo.GetField("IsNonAttributeExpressionContext");
                IsPreProcessorKeywordContextField = TypeInfo.GetField("IsPreProcessorKeywordContext");
                IsPreProcessorExpressionContextField = TypeInfo.GetField("IsPreProcessorExpressionContext");
                ContainingTypeDeclarationField = TypeInfo.GetField("ContainingTypeDeclaration");
                IsGlobalStatementContextField = TypeInfo.GetField("IsGlobalStatementContext");
                IsParameterTypeContextField = TypeInfo.GetField("IsParameterTypeContext");
                IsMemberDeclarationContextMethod = TypeInfo.GetMethod("IsMemberDeclarationContext", BindingFlags.Instance | BindingFlags.Public);
                IsTypeDeclarationContextMethod = TypeInfo.GetMethod("IsTypeDeclarationContext", BindingFlags.Instance | BindingFlags.Public);
                IsInNonUserCodeField = TypeInfo.GetField("IsInNonUserCode");
                IsIsOrAsContextField = TypeInfo.GetField("IsIsOrAsContext");
                IsTypeAttributeContextMethod = TypeInfo.GetMethod("IsTypeAttributeContext", BindingFlags.Instance | BindingFlags.Public);
                IsDefiniteCastTypeContextField = TypeInfo.GetField("IsDefiniteCastTypeContext");
                IsObjectCreationTypeContextField = TypeInfo.GetField("IsObjectCreationTypeContext");
                IsGenericTypeArgumentContextField = TypeInfo.GetField("IsGenericTypeArgumentContext");
                IsLocalVariableDeclarationContextField = TypeInfo.GetField("IsLocalVariableDeclarationContext");
                IsFixedVariableDeclarationContextField = TypeInfo.GetField("IsFixedVariableDeclarationContext");
                IsPossibleLambdaOrAnonymousMethodParameterTypeContextField = TypeInfo.GetField("IsPossibleLambdaOrAnonymousMethodParameterTypeContext");
                IsImplicitOrExplicitOperatorTypeContextField = TypeInfo.GetField("IsImplicitOrExplicitOperatorTypeContext");
                IsPrimaryFunctionExpressionContextField = TypeInfo.GetField("IsPrimaryFunctionExpressionContext");
                IsCrefContextField = TypeInfo.GetField("IsCrefContext");
                IsDelegateReturnTypeContextField = TypeInfo.GetField("IsDelegateReturnTypeContext");
                IsEnumBaseListContextField = TypeInfo.GetField("IsEnumBaseListContext");
                IsConstantExpressionContextField = TypeInfo.GetField("IsConstantExpressionContext");
                IsMemberAttributeContextMethod = TypeInfo.GetMethod("IsMemberAttributeContext", BindingFlags.Instance | BindingFlags.Public);
                PrecedingModifiersField = TypeInfo.GetField("PrecedingModifiers");
                IsTypeOfExpressionContextField = TypeInfo.GetField("IsTypeOfExpressionContext");
                ContainingTypeOrEnumDeclarationField = TypeInfo.GetField("ContainingTypeOrEnumDeclaration");
            }
        }

        // ExpressionSyntaxExtensions
        public static ExpressionSyntaxExtensionsWrapper ExpressionSyntaxExtensions => expressionSyntaxExtensionsWrapper.Value;
        static readonly Lazy<ExpressionSyntaxExtensionsWrapper> expressionSyntaxExtensionsWrapper =
            new Lazy<ExpressionSyntaxExtensionsWrapper>(() => new ExpressionSyntaxExtensionsWrapper());
        public class ExpressionSyntaxExtensionsWrapper
        {
            public readonly MethodInfo CastIfPossibleMethod;
            public readonly MethodInfo TryReduceOrSimplifyExplicitNameMethod;

            public ExpressionSyntaxExtensionsWrapper()
            {
                var typeInfo = Type.GetType("Microsoft.CodeAnalysis.CSharp.Extensions.ExpressionSyntaxExtensions" + ReflectionNamespaces.CSWorkspacesAsmName, true);

                CastIfPossibleMethod = typeInfo.GetMethod("CastIfPossible", BindingFlags.Static | BindingFlags.Public);
                TryReduceOrSimplifyExplicitNameMethod = typeInfo.GetMethod("TryReduceOrSimplifyExplicitName", BindingFlags.Static | BindingFlags.Public);
            }
        }

        // ITypeSymbolExtensions/TypeExtensions
        public static CSharpITypeSymbolExtensionsWrapper CSharpITypeSymbolExtensions => cSharpITypeSymbolExtensionsWrapper.Value;
        static readonly Lazy<CSharpITypeSymbolExtensionsWrapper> cSharpITypeSymbolExtensionsWrapper =
            new Lazy<CSharpITypeSymbolExtensionsWrapper>(() => new CSharpITypeSymbolExtensionsWrapper());
        public class CSharpITypeSymbolExtensionsWrapper
        {
            public readonly MethodInfo GenerateTypeSyntaxMethod;
            public readonly MethodInfo ContainingTypesOrSelfHasUnsafeKeywordMethod;

            public CSharpITypeSymbolExtensionsWrapper()
            {
                var typeInfo = Type.GetType("Microsoft.CodeAnalysis.CSharp.Extensions.ITypeSymbolExtensions" + ReflectionNamespaces.CSWorkspacesAsmName, true);

#if RE2017
                // Since Roslyn 2.0 the parameter has the new type INamespaceOrTypeSymbol,
                // which has become a parent type of ITypeSymbol.
                GenerateTypeSyntaxMethod = typeInfo.GetMethod("GenerateTypeSyntax", new[] { typeof(INamespaceOrTypeSymbol) });
#else
                GenerateTypeSyntaxMethod = typeInfo.GetMethod("GenerateTypeSyntax", new[] { typeof(ITypeSymbol) });
#endif
                ContainingTypesOrSelfHasUnsafeKeywordMethod =
                    typeInfo.GetMethod("ContainingTypesOrSelfHasUnsafeKeyword", BindingFlags.Public | BindingFlags.Static);
            }
        }

        public static SharedITypeSymbolExtensionsWrapper SharedITypeSymbolExtensions => sharedITypeSymbolExtensionsWrapper.Value;
        static readonly Lazy<SharedITypeSymbolExtensionsWrapper> sharedITypeSymbolExtensionsWrapper =
            new Lazy<SharedITypeSymbolExtensionsWrapper>(() => new SharedITypeSymbolExtensionsWrapper());
        public class SharedITypeSymbolExtensionsWrapper
        {
            public readonly MethodInfo InheritsFromOrEqualsIgnoringConstructionMethod;
            public readonly MethodInfo RemoveUnavailableTypeParametersMethod;
            public readonly MethodInfo RemoveUnnamedErrorTypesMethod;
            public readonly MethodInfo ReplaceTypeParametersBasedOnTypeConstraintsMethod;
            public readonly MethodInfo SubstituteTypesMethod;
            public readonly MethodInfo SubstituteTypesMethod2;

            public SharedITypeSymbolExtensionsWrapper()
            {
                var typeInfo = Type.GetType("Microsoft.CodeAnalysis.Shared.Extensions.ITypeSymbolExtensions" + ReflectionNamespaces.WorkspacesAsmName, true);

                InheritsFromOrEqualsIgnoringConstructionMethod = typeInfo.GetMethod("InheritsFromOrEqualsIgnoringConstruction");
                RemoveUnavailableTypeParametersMethod = typeInfo.GetMethod("RemoveUnavailableTypeParameters");
                RemoveUnnamedErrorTypesMethod = typeInfo.GetMethod("RemoveUnnamedErrorTypes");
                ReplaceTypeParametersBasedOnTypeConstraintsMethod = typeInfo.GetMethod("ReplaceTypeParametersBasedOnTypeConstraints");
                foreach (var m in typeInfo.GetMethods(BindingFlags.Public | BindingFlags.Static))
                {
                    if (m.Name != "SubstituteTypes")
                        continue;
                    var parameters = m.GetParameters();
                    if (parameters.Length != 3)
                        continue;

                    if (parameters[2].Name == "typeGenerator")
                    {
                        SubstituteTypesMethod2 = m;
                    }
                    else if (parameters[2].Name == "compilation")
                    {
                        SubstituteTypesMethod = m;
                    }
                    break;
                }
            }
        }

        public static DependentTypeFinderWrapper DependentTypeFinder => dependentTypeFinderWrapper.Value;
        static readonly Lazy<DependentTypeFinderWrapper> dependentTypeFinderWrapper =
            new Lazy<DependentTypeFinderWrapper>(() => new DependentTypeFinderWrapper());
        public class DependentTypeFinderWrapper
        {
            public readonly MethodInfo FindDerivedClassesAsyncMethod;

            public DependentTypeFinderWrapper()
            {
                var type = Type.GetType("Microsoft.CodeAnalysis.FindSymbols.DependentTypeFinder" + ReflectionNamespaces.WorkspacesAsmName, true);

                FindDerivedClassesAsyncMethod = type.GetMethod("FindDerivedClassesAsync", new[] { typeof(INamedTypeSymbol), typeof(Solution), typeof(IImmutableSet<Project>), typeof(CancellationToken) });
            }
        }

        // SignatureComparer
        public static SignatureComparerWrapper SignatureComparer => signatureComparerWrapper.Value;
        static readonly Lazy<SignatureComparerWrapper> signatureComparerWrapper =
            new Lazy<SignatureComparerWrapper>(() => new SignatureComparerWrapper());
        public class SignatureComparerWrapper
        {
            public readonly Type TypeInfo;
            public readonly MethodInfo HaveSameSignatureMethod;
            public readonly MethodInfo HaveSameSignature2Method;
            public readonly MethodInfo HaveSameSignature3Method;
            public readonly MethodInfo HaveSameSignature4Method;
            public readonly MethodInfo HaveSameSignature5Method;
            public readonly MethodInfo HaveSameSignatureAndConstraintsAndReturnTypeAndAccessorsMethod;

            public SignatureComparerWrapper()
            {
                TypeInfo = Type.GetType("Microsoft.CodeAnalysis.Shared.Utilities.SignatureComparer" + ReflectionNamespaces.WorkspacesAsmName, false);

                if (TypeInfo != null)
                {
                    HaveSameSignatureMethod = TypeInfo.GetMethod("HaveSameSignature", new[] { typeof(IList<IParameterSymbol>), typeof(IList<IParameterSymbol>) });
                    HaveSameSignature2Method = TypeInfo.GetMethod("HaveSameSignature", new[] { typeof(IPropertySymbol), typeof(IPropertySymbol), typeof(bool) });
                    HaveSameSignature3Method = TypeInfo.GetMethod("HaveSameSignature", new[] { typeof(ISymbol), typeof(ISymbol), typeof(bool) });
                    HaveSameSignature4Method = TypeInfo.GetMethod("HaveSameSignature", new[] { typeof(IMethodSymbol), typeof(IMethodSymbol), typeof(bool), typeof(bool), typeof(bool) });
                    HaveSameSignature5Method = TypeInfo.GetMethod("HaveSameSignature", new[] { typeof(IList<IParameterSymbol>), typeof(IList<IParameterSymbol>), typeof(bool), typeof(bool) });
                    HaveSameSignatureAndConstraintsAndReturnTypeAndAccessorsMethod = TypeInfo.GetMethod("HaveSameSignatureAndConstraintsAndReturnTypeAndAccessors", BindingFlags.Public | BindingFlags.Instance);
                }
            }

            public bool IsAvailable() => TypeInfo != null;
        }

        // SyntaxExtensions
        public static ParenthesizedExpressionSyntaxExtensionsWrapper ParenthesizedExpressionSyntaxExtensions => parenthesizedExpressionSyntaxExtensionsWrapper.Value;
        static readonly Lazy<ParenthesizedExpressionSyntaxExtensionsWrapper> parenthesizedExpressionSyntaxExtensionsWrapper =
            new Lazy<ParenthesizedExpressionSyntaxExtensionsWrapper>(() => new ParenthesizedExpressionSyntaxExtensionsWrapper());
        public class ParenthesizedExpressionSyntaxExtensionsWrapper
        {
            public readonly MethodInfo CanRemoveParenthesesMethod;

            public ParenthesizedExpressionSyntaxExtensionsWrapper()
            {
                var typeInfo = Type.GetType("Microsoft.CodeAnalysis.CSharp.Extensions.ParenthesizedExpressionSyntaxExtensions" + ReflectionNamespaces.CSWorkspacesAsmName, true);
                CanRemoveParenthesesMethod = typeInfo.GetMethod("CanRemoveParentheses", new[] { typeof(ParenthesizedExpressionSyntax) });
            }
        }

        public static MemberDeclarationSyntaxExtensionsWrapper MemberDeclarationSyntaxExtensions => memberDeclarationSyntaxExtensionsWrapper.Value;
        static readonly Lazy<MemberDeclarationSyntaxExtensionsWrapper> memberDeclarationSyntaxExtensionsWrapper =
            new Lazy<MemberDeclarationSyntaxExtensionsWrapper>(() => new MemberDeclarationSyntaxExtensionsWrapper());
        public class MemberDeclarationSyntaxExtensionsWrapper
        {
            public readonly MethodInfo GetLocalDeclarationMapMethod;

            public MemberDeclarationSyntaxExtensionsWrapper()
            {
                var typeInfo = Type.GetType("Microsoft.CodeAnalysis.CSharp.Extensions.MemberDeclarationSyntaxExtensions" + ReflectionNamespaces.CSWorkspacesAsmName, true);
                GetLocalDeclarationMapMethod = typeInfo.GetMethod("GetLocalDeclarationMap", new[] { typeof(MemberDeclarationSyntax) });
            }
        }

        public static MemberDeclarationSyntaxExtensions_LocalDeclarationMapWrapper MemberDeclarationSyntaxExtensions_LocalDeclarationMap => memberDeclarationSyntaxExtensions_LocalDeclarationMapWrapper.Value;
        static readonly Lazy<MemberDeclarationSyntaxExtensions_LocalDeclarationMapWrapper> memberDeclarationSyntaxExtensions_LocalDeclarationMapWrapper =
            new Lazy<MemberDeclarationSyntaxExtensions_LocalDeclarationMapWrapper>(() => new MemberDeclarationSyntaxExtensions_LocalDeclarationMapWrapper());
        public class MemberDeclarationSyntaxExtensions_LocalDeclarationMapWrapper
        {
            public readonly PropertyInfo LocalDeclarationMapIndexer;

            public MemberDeclarationSyntaxExtensions_LocalDeclarationMapWrapper()
            {
                var typeInfo = Type.GetType("Microsoft.CodeAnalysis.CSharp.Extensions.MemberDeclarationSyntaxExtensions+LocalDeclarationMap" + ReflectionNamespaces.CSWorkspacesAsmName, true);
                LocalDeclarationMapIndexer = typeInfo.GetProperties().Single(p => p.GetIndexParameters().Any());
            }
        }

        public static SyntaxTokenExtensionsWrapper SyntaxTokenExtensions => syntaxTokenExtensionsWrapper.Value;
        static readonly Lazy<SyntaxTokenExtensionsWrapper> syntaxTokenExtensionsWrapper =
            new Lazy<SyntaxTokenExtensionsWrapper>(() => new SyntaxTokenExtensionsWrapper());
        public class SyntaxTokenExtensionsWrapper
        {
            public readonly MethodInfo GetAncestorsMethod;

            public SyntaxTokenExtensionsWrapper()
            {
                var typeInfo = Type.GetType("Microsoft.CodeAnalysis.Shared.Extensions.SyntaxTokenExtensions" + ReflectionNamespaces.WorkspacesAsmName, true);
                GetAncestorsMethod = typeInfo.GetMethods().Single(m => m.Name == "GetAncestors" && m.IsGenericMethod);
            }
        }

        // SyntaxNodeExtensions
        public static SyntaxNodeExtensionsWrapper SyntaxNodeExtensions => syntaxNodeExtensionsWrapper.Value;
        static readonly Lazy<SyntaxNodeExtensionsWrapper> syntaxNodeExtensionsWrapper =
            new Lazy<SyntaxNodeExtensionsWrapper>(() => new SyntaxNodeExtensionsWrapper());
        public class SyntaxNodeExtensionsWrapper
        {
            public readonly MethodInfo ContainsInterleavedDirectiveMethod;

            public SyntaxNodeExtensionsWrapper()
            {
                var typeInfo = Type.GetType("Microsoft.CodeAnalysis.CSharp.Extensions.SyntaxNodeExtensions" + ReflectionNamespaces.CSWorkspacesAsmName, true);
                ContainsInterleavedDirectiveMethod = typeInfo.GetMethod("ContainsInterleavedDirective", new[] { typeof(SyntaxNode), typeof(CancellationToken) });
            }
        }

        // SpeculationAnalyzer
        public static AbstractSpeculationAnalyzer_9Wrapper AbstractSpeculationAnalyzer_9 => abstractSpeculationAnalyzer_9Wrapper.Value;
        static readonly Lazy<AbstractSpeculationAnalyzer_9Wrapper> abstractSpeculationAnalyzer_9Wrapper =
            new Lazy<AbstractSpeculationAnalyzer_9Wrapper>(() => new AbstractSpeculationAnalyzer_9Wrapper());
        public class AbstractSpeculationAnalyzer_9Wrapper
        {
            public readonly Type type;

            public readonly MethodInfo SymbolsForOriginalAndReplacedNodesAreCompatibleMethod;
            public readonly MethodInfo ReplacementChangesSemanticsMethod;
            public readonly MethodInfo SymbolInfosAreCompatibleMethod;

            public AbstractSpeculationAnalyzer_9Wrapper()
            {
#if RE2017
				Type[] abstractSpeculationAnalyzerGenericParams = {
                    Type.GetType ("Microsoft.CodeAnalysis.SyntaxNode" + ReflectionNamespaces.CAAsmName, true),
                    Type.GetType ("Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax" + ReflectionNamespaces.CACSharpAsmName, true),
                    Type.GetType ("Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax" + ReflectionNamespaces.CACSharpAsmName, true),
                    Type.GetType ("Microsoft.CodeAnalysis.CSharp.Syntax.AttributeSyntax" + ReflectionNamespaces.CACSharpAsmName, true),
                    Type.GetType ("Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentSyntax" + ReflectionNamespaces.CACSharpAsmName, true),
                    Type.GetType ("Microsoft.CodeAnalysis.CSharp.Syntax.CommonForEachStatementSyntax" + ReflectionNamespaces.CACSharpAsmName, true),
                    Type.GetType ("Microsoft.CodeAnalysis.CSharp.Syntax.ThrowStatementSyntax" + ReflectionNamespaces.CACSharpAsmName, true),
                    Type.GetType ("Microsoft.CodeAnalysis.SemanticModel" + ReflectionNamespaces.CAAsmName, true),
                    Type.GetType ("Microsoft.CodeAnalysis.CSharp.Conversion" + ReflectionNamespaces.CACSharpAsmName, true)
                };
#else
				Type[] abstractSpeculationAnalyzerGenericParams = {
                    Type.GetType ("Microsoft.CodeAnalysis.SyntaxNode" + ReflectionNamespaces.CAAsmName, true),
                    Type.GetType ("Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax" + ReflectionNamespaces.CACSharpAsmName, true),
                    Type.GetType ("Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax" + ReflectionNamespaces.CACSharpAsmName, true),
                    Type.GetType ("Microsoft.CodeAnalysis.CSharp.Syntax.AttributeSyntax" + ReflectionNamespaces.CACSharpAsmName, true),
                    Type.GetType ("Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentSyntax" + ReflectionNamespaces.CACSharpAsmName, true),
                    Type.GetType ("Microsoft.CodeAnalysis.CSharp.Syntax.ForEachStatementSyntax" + ReflectionNamespaces.CACSharpAsmName, true),
                    Type.GetType ("Microsoft.CodeAnalysis.CSharp.Syntax.ThrowStatementSyntax" + ReflectionNamespaces.CACSharpAsmName, true),
                    Type.GetType ("Microsoft.CodeAnalysis.SemanticModel" + ReflectionNamespaces.CAAsmName, true),
                    Type.GetType ("Microsoft.CodeAnalysis.CSharp.Conversion" + ReflectionNamespaces.CACSharpAsmName, true)
                };
#endif
				type = Type.GetType("Microsoft.CodeAnalysis.Shared.Utilities.AbstractSpeculationAnalyzer`9" + ReflectionNamespaces.WorkspacesAsmName, true)
                    .MakeGenericType(abstractSpeculationAnalyzerGenericParams);

                SymbolsForOriginalAndReplacedNodesAreCompatibleMethod = type.GetMethod("SymbolsForOriginalAndReplacedNodesAreCompatible", BindingFlags.Public | BindingFlags.Instance);
                ReplacementChangesSemanticsMethod = type.GetMethod("ReplacementChangesSemantics", BindingFlags.Public | BindingFlags.Instance);
                SymbolInfosAreCompatibleMethod = type.GetMethod("SymbolInfosAreCompatible", BindingFlags.Public | BindingFlags.Static);
            }
        }

        public static SpeculationAnalyzerWrapper SpeculationAnalyzer => speculationAnalyzerWrapper.Value;
        readonly static Lazy<SpeculationAnalyzerWrapper> speculationAnalyzerWrapper =
            new Lazy<SpeculationAnalyzerWrapper>(() => new SpeculationAnalyzerWrapper());
        public class SpeculationAnalyzerWrapper
        {
            public readonly Type TypeInfo;
            public readonly MethodInfo CreateSpeculativeSemanticModelForNodeMethod;

            public SpeculationAnalyzerWrapper()
            {
                TypeInfo = Type.GetType("Microsoft.CodeAnalysis.CSharp.Utilities.SpeculationAnalyzer" + ReflectionNamespaces.CSWorkspacesAsmName, false);

                if (TypeInfo != null)
                {
                    CreateSpeculativeSemanticModelForNodeMethod = TypeInfo.GetMethod("CreateSpeculativeSemanticModelForNode", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(SyntaxNode), typeof(SyntaxNode), typeof(SemanticModel) }, null);
                }
            }

            public bool IsAvailable() => TypeInfo != null;
        }

        // TypeGenerator
        public static TypeGeneratorWrapper TypeGenerator => typeGeneratorWrapper.Value;
        static readonly Lazy<TypeGeneratorWrapper> typeGeneratorWrapper = new Lazy<TypeGeneratorWrapper>(() => new TypeGeneratorWrapper());
        public class TypeGeneratorWrapper
        {
            public readonly Type TypeInfo;

            public readonly MethodInfo CreateArrayTypeSymbolMethod;
            public readonly MethodInfo CreatePointerTypeSymbolMethod;
            public readonly MethodInfo ConstructMethod;

            public TypeGeneratorWrapper()
            {
                TypeInfo = Type.GetType("Microsoft.CodeAnalysis.CodeGeneration.TypeGenerator" + ReflectionNamespaces.WorkspacesAsmName, true);

                CreateArrayTypeSymbolMethod = TypeInfo.GetMethod("CreateArrayTypeSymbol");
                CreatePointerTypeSymbolMethod = TypeInfo.GetMethod("CreatePointerTypeSymbol");
                ConstructMethod = TypeInfo.GetMethod("Construct");
            }
        }
    }
}
