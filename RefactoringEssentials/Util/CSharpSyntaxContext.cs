using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Runtime.ExceptionServices;

namespace RefactoringEssentials
{
    [RoslynReflectionUsage(RoslynReflectionAllowedContext.CodeFixes)]
#if NR6
    public
#endif
    class CSharpSyntaxContext
    {
        object instance;

        public SyntaxToken LeftToken
        {
            get
            {
                return (SyntaxToken)RoslynReflection.AbstractSyntaxContext.LeftTokenProperty.GetValue(instance);
            }
        }

        public SyntaxToken TargetToken
        {
            get
            {
                return (SyntaxToken)RoslynReflection.AbstractSyntaxContext.TargetTokenProperty.GetValue(instance);
            }
        }

        public bool IsIsOrAsTypeContext
        {
            get
            {
                return (bool)RoslynReflection.CSharpSyntaxContext.IsIsOrAsTypeContextField.GetValue(instance);
            }
        }

        public bool IsInstanceContext
        {
            get
            {
                return (bool)RoslynReflection.CSharpSyntaxContext.IsInstanceContextField.GetValue(instance);
            }
        }

        public bool IsNonAttributeExpressionContext
        {
            get
            {
                return (bool)RoslynReflection.CSharpSyntaxContext.IsNonAttributeExpressionContextField.GetValue(instance);
            }
        }

        public bool IsPreProcessorKeywordContext
        {
            get
            {
                return (bool)RoslynReflection.CSharpSyntaxContext.IsPreProcessorKeywordContextField.GetValue(instance);
            }
        }

        public bool IsPreProcessorExpressionContext
        {
            get
            {
                return (bool)RoslynReflection.CSharpSyntaxContext.IsPreProcessorExpressionContextField.GetValue(instance);
            }
        }

        public TypeDeclarationSyntax ContainingTypeDeclaration
        {
            get
            {
                return (TypeDeclarationSyntax)RoslynReflection.CSharpSyntaxContext.ContainingTypeDeclarationField.GetValue(instance);
            }
        }

        public bool IsGlobalStatementContext
        {
            get
            {
                return (bool)RoslynReflection.CSharpSyntaxContext.IsGlobalStatementContextField.GetValue(instance);
            }
        }

        public bool IsParameterTypeContext
        {
            get
            {
                return (bool)RoslynReflection.CSharpSyntaxContext.IsParameterTypeContextField.GetValue(instance);
            }
        }

        public SyntaxTree SyntaxTree
        {
            get
            {
                return (SyntaxTree)RoslynReflection.AbstractSyntaxContext.SyntaxTreeProperty.GetValue(instance);
            }
        }

        public bool IsMemberDeclarationContext(
            ISet<SyntaxKind> validModifiers = null,
            ISet<SyntaxKind> validTypeDeclarations = null,
            bool canBePartial = false,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                return (bool)RoslynReflection.CSharpSyntaxContext.IsMemberDeclarationContextMethod.Invoke(instance, new object[] {
                    validModifiers,
                    validTypeDeclarations,
                    canBePartial,
                    cancellationToken
                });
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                return false;
            }
        }

        public bool IsTypeDeclarationContext(
            ISet<SyntaxKind> validModifiers = null,
            ISet<SyntaxKind> validTypeDeclarations = null,
            bool canBePartial = false,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                return (bool)RoslynReflection.CSharpSyntaxContext.IsTypeDeclarationContextMethod.Invoke(instance, new object[] {
                    validModifiers,
                    validTypeDeclarations,
                    canBePartial,
                    cancellationToken
                });
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                return false;
            }
        }

        public bool IsPreProcessorDirectiveContext
        {
            get
            {
                return (bool)RoslynReflection.AbstractSyntaxContext.IsPreProcessorDirectiveContextProperty.GetValue(instance);
            }
        }

        public bool IsInNonUserCode
        {
            get
            {
                return (bool)RoslynReflection.CSharpSyntaxContext.IsInNonUserCodeField.GetValue(instance);
            }
        }

        public bool IsIsOrAsContext
        {
            get
            {
                return (bool)RoslynReflection.CSharpSyntaxContext.IsIsOrAsContextField.GetValue(instance);
            }
        }

        public bool IsTypeAttributeContext(CancellationToken cancellationToken)
        {
            try
            {
                return (bool)RoslynReflection.CSharpSyntaxContext.IsTypeAttributeContextMethod.Invoke(instance, new object[] { cancellationToken });
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                return false;
            }
        }

        public bool IsAnyExpressionContext
        {
            get
            {
                return (bool)RoslynReflection.AbstractSyntaxContext.IsAnyExpressionContextProperty.GetValue(instance);
            }
        }

        public bool IsStatementContext
        {
            get
            {
                return (bool)RoslynReflection.AbstractSyntaxContext.IsStatementContextProperty.GetValue(instance);
            }
        }

        public bool IsDefiniteCastTypeContext
        {
            get
            {
                return (bool)RoslynReflection.CSharpSyntaxContext.IsDefiniteCastTypeContextField.GetValue(instance);
            }
        }

        public bool IsObjectCreationTypeContext
        {
            get
            {
                return (bool)RoslynReflection.CSharpSyntaxContext.IsObjectCreationTypeContextField.GetValue(instance);
            }
        }

        public bool IsGenericTypeArgumentContext
        {
            get
            {
                return (bool)RoslynReflection.CSharpSyntaxContext.IsGenericTypeArgumentContextField.GetValue(instance);
            }
        }

        public bool IsLocalVariableDeclarationContext
        {
            get
            {
                return (bool)RoslynReflection.CSharpSyntaxContext.IsLocalVariableDeclarationContextField.GetValue(instance);
            }
        }

        public bool IsFixedVariableDeclarationContext
        {
            get
            {
                return (bool)RoslynReflection.CSharpSyntaxContext.IsFixedVariableDeclarationContextField.GetValue(instance);
            }
        }

        public bool IsPossibleLambdaOrAnonymousMethodParameterTypeContext
        {
            get
            {
                return (bool)RoslynReflection.CSharpSyntaxContext.IsPossibleLambdaOrAnonymousMethodParameterTypeContextField.GetValue(instance);
            }
        }

        public bool IsImplicitOrExplicitOperatorTypeContext
        {
            get
            {
                return (bool)RoslynReflection.CSharpSyntaxContext.IsImplicitOrExplicitOperatorTypeContextField.GetValue(instance);
            }
        }

        public bool IsPrimaryFunctionExpressionContext
        {
            get
            {
                return (bool)RoslynReflection.CSharpSyntaxContext.IsPrimaryFunctionExpressionContextField.GetValue(instance);
            }
        }

        public bool IsCrefContext
        {
            get
            {
                return (bool)RoslynReflection.CSharpSyntaxContext.IsCrefContextField.GetValue(instance);
            }
        }

        public bool IsDelegateReturnTypeContext
        {
            get
            {
                return (bool)RoslynReflection.CSharpSyntaxContext.IsDelegateReturnTypeContextField.GetValue(instance);
            }
        }

        public bool IsEnumBaseListContext
        {
            get
            {
                return (bool)RoslynReflection.CSharpSyntaxContext.IsEnumBaseListContextField.GetValue(instance);
            }
        }

        public bool IsConstantExpressionContext
        {
            get
            {
                return (bool)RoslynReflection.CSharpSyntaxContext.IsConstantExpressionContextField.GetValue(instance);
            }
        }

        public bool IsMemberAttributeContext(ISet<SyntaxKind> validTypeDeclarations, CancellationToken cancellationToken)
        {
            try
            {
                return (bool)RoslynReflection.CSharpSyntaxContext.IsMemberAttributeContextMethod.Invoke(instance, new object[] {
                    validTypeDeclarations,
                    cancellationToken
                });
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                return false;
            }

        }

        public ISet<SyntaxKind> PrecedingModifiers
        {
            get
            {
                return (ISet<SyntaxKind>)RoslynReflection.CSharpSyntaxContext.PrecedingModifiersField.GetValue(instance);
            }
        }

        public bool IsTypeOfExpressionContext
        {
            get
            {
                return (bool)RoslynReflection.CSharpSyntaxContext.IsTypeOfExpressionContextField.GetValue(instance);
            }
        }

        public BaseTypeDeclarationSyntax ContainingTypeOrEnumDeclaration
        {
            get
            {
                return (BaseTypeDeclarationSyntax)RoslynReflection.CSharpSyntaxContext.ContainingTypeOrEnumDeclarationField.GetValue(instance);
            }
        }

        public bool IsAttributeNameContext
        {
            get
            {
                return (bool)RoslynReflection.AbstractSyntaxContext.IsAttributeNameContextProperty.GetValue(instance);
            }
        }

        public bool IsInQuery
        {
            get
            {
                return (bool)RoslynReflection.AbstractSyntaxContext.IsInQueryProperty.GetValue(instance);
            }
        }

        public SemanticModel SemanticModel
        {
            get;
            private set;
        }

        public int Position
        {
            get;
            private set;
        }

        CSharpSyntaxContext(object instance)
        {
            this.instance = instance;
        }

        public static CSharpSyntaxContext CreateContext(Workspace workspace, SemanticModel semanticModel, int position, CancellationToken cancellationToken)
        {
            try
            {
                return new CSharpSyntaxContext(RoslynReflection.CSharpSyntaxContext.CreateContextMethod.Invoke(null, new object[] {
                    workspace,
                    semanticModel,
                    position,
                    cancellationToken
                }))
                {
                    SemanticModel = semanticModel,
                    Position = position
                };
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                return null;
            }
        }
    }

#if NR6
    public
#endif
    class CSharpTypeInferenceService
    {
        readonly static Type typeInfo, baseTypeInfo;
        readonly static MethodInfo inferTypesMethod;
        readonly static MethodInfo inferTypes2Method;
        readonly object instance;

        static CSharpTypeInferenceService()
        {
            typeInfo = Type.GetType("Microsoft.CodeAnalysis.CSharp.CSharpTypeInferenceService" + ReflectionNamespaces.CSWorkspacesAsmName, true);
            baseTypeInfo = Type.GetType("Microsoft.CodeAnalysis.LanguageServices.TypeInferenceService.AbstractTypeInferenceService`1" + ReflectionNamespaces.WorkspacesAsmName, true)
                .MakeGenericType(typeof(ExpressionSyntax));
            inferTypesMethod = baseTypeInfo.GetMethod("InferTypes", new[] {
                typeof(SemanticModel),
                typeof(int),
#if RE2017
                typeof(string),
#endif
                typeof(CancellationToken)
            });
            inferTypes2Method = baseTypeInfo.GetMethod("InferTypes", new[] {
                typeof(SemanticModel),
                typeof(SyntaxNode),
#if RE2017
                typeof(string),
#endif
                typeof(CancellationToken)
            });
        }

        public CSharpTypeInferenceService()
        {
            instance = Activator.CreateInstance(typeInfo);
        }

        public IEnumerable<ITypeSymbol> InferTypes(SemanticModel semanticModel, int position, string nameOpt, CancellationToken cancellationToken)
        {
            try
            {
                return (IEnumerable<ITypeSymbol>)inferTypesMethod.Invoke(instance, new object[] {
                    semanticModel,
                    position,
#if RE2017
                    nameOpt,
#endif
                    cancellationToken
                });
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                return null;
            }
        }

        public IEnumerable<ITypeSymbol> InferTypes(SemanticModel semanticModel, SyntaxNode expression, string nameOpt, CancellationToken cancellationToken)
        {
            try
            {
                return (IEnumerable<ITypeSymbol>)inferTypes2Method.Invoke(instance, new object[] {
                    semanticModel,
                    expression,
#if RE2017
                    nameOpt,
#endif
                    cancellationToken
                });
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                return null;
            }
        }

        // see https://github.com/dotnet/roslyn/blob/master/src/Workspaces/Core/Portable/Shared/Extensions/ITypeInferenceServiceExtensions.cs
        public ITypeSymbol InferType(
            SemanticModel semanticModel,
            SyntaxNode expression,
            bool objectAsDefault,
            CancellationToken cancellationToken)
        {
            var types = InferTypes(semanticModel, expression, null, cancellationToken)
                .WhereNotNull();

            if (!types.Any())
            {
                return objectAsDefault ? semanticModel.Compilation.ObjectType : null;
            }

            return types.FirstOrDefault();
        }


        //public INamedTypeSymbol InferDelegateType(
        //	SemanticModel semanticModel,
        //	SyntaxNode expression,
        //	CancellationToken cancellationToken)
        //{
        //	var type = this.InferType(semanticModel, expression, objectAsDefault: false, cancellationToken: cancellationToken);
        //	return type.GetDelegateType(semanticModel.Compilation);
        //}


        public ITypeSymbol InferType(
            SemanticModel semanticModel,
            int position,
            bool objectAsDefault,
            CancellationToken cancellationToken)
        {
            var types = this.InferTypes(semanticModel, position, null, cancellationToken)
                .WhereNotNull();

            if (!types.Any())
            {
                return objectAsDefault ? semanticModel.Compilation.ObjectType : null;
            }

            return types.FirstOrDefault();
        }
    }
}
