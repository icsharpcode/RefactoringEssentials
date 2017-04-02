using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;
using System.Threading;
using ICSharpCode.NRefactory6.CSharp.Refactoring;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.FindSymbols;

namespace ICSharpCode.NRefactory6.CSharp.Diagnostics
{
    [DiagnosticAnalyzer]
    [ExportDiagnosticAnalyzer("", LanguageNames.CSharp)]
    [NRefactoryCodeDiagnosticAnalyzer(Description = "", AnalysisDisableKeyword = "")]
    [IssueDescription("Type does not implement IDisposable despite having a Dispose method",
                      Description = "This type declares a method named Dispose, but it does not implement the System.IDisposable interface",
                      Category = IssueCategories.CodeQualityIssues,
                      Severity = Severity.Warning)]
    public class DisposeMethodInNonIDisposableTypeDiagnosticAnalyzer : GatherVisitorCodeIssueProvider
    {
        internal const string DiagnosticId = "";
        const string Description = "";
        const string MessageFormat = "";
        const string Category = IssueCategories.CodeQualityIssues;

        static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, DiagnosticSeverity.Warning);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        protected override CSharpSyntaxWalker CreateVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        {
            return new GatherVisitor(semanticModel, addDiagnostic, cancellationToken);
        }

        private class GatherVisitor : GatherVisitorBase<DisposeMethodInNonIDisposableTypeIssue>
        {
            public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
                : base(semanticModel, addDiagnostic, cancellationToken)
            {
            }

            static bool IsDisposeMethod(MethodDeclaration methodDeclaration)
            {
                if (!methodDeclaration.PrivateImplementationType.IsNull)
                {
                    //Ignore explictly implemented methods
                    return false;
                }
                if (methodDeclaration.Name != "Dispose")
                {
                    return false;
                }
                if (methodDeclaration.Parameters.Count != 0)
                {
                    return false;
                }

                if (methodDeclaration.HasModifier(Modifiers.Static))
                {
                    return false;
                }

                var primitiveType = methodDeclaration.ReturnType as PrimitiveType;
                if (primitiveType == null || primitiveType.KnownTypeCode != KnownTypeCode.Void)
                {
                    return false;
                }

                return true;
            }

            public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
            {
                if (typeDeclaration.ClassType == ClassType.Enum)
                {
                    //enums have no methods
                    return;
                }

                var resolve = ctx.Resolve(typeDeclaration) as TypeResolveResult;
                if (resolve != null && Implements(resolve.Type, "System.IDisposable"))
                {
                    return;
                }

                base.VisitTypeDeclaration(typeDeclaration);
            }

            public override void VisitMethodDeclaration(MethodDeclaration methodDeclaration)
            {
                if (!IsDisposeMethod(methodDeclaration))
                {
                    return;
                }

                var type = methodDeclaration.GetParent<TypeDeclaration>();
                if (type == null)
                {
                    return;
                }

                AddDiagnosticAnalyzer(new CodeIssue(methodDeclaration.NameToken,
                         ctx.TranslateString("Type does not implement IDisposable despite having a Dispose method"),
                         ctx.TranslateString("Implement IDisposable"),
                    script => Fix(script, methodDeclaration, type)));
            }

            static IEnumerable<MethodDeclaration> DisposeMethods(TypeDeclaration newTypeDeclaration)
            {
                return newTypeDeclaration.Members
                    .OfType<MethodDeclaration>()
                        .Where(IsDisposeMethod);
            }

            void Fix(Script script, MethodDeclaration methodDeclaration, TypeDeclaration typeDeclaration)
            {
                var newTypeDeclaration = (TypeDeclaration)typeDeclaration.Clone();

                var resolver = ctx.GetResolverStateAfter(typeDeclaration.LBraceToken);

                var typeResolve = resolver.ResolveSimpleName("IDisposable", new List<IType>()) as TypeResolveResult;
                bool canShortenIDisposable = typeResolve != null && typeResolve.Type.FullName == "System.IDisposable";

                string interfaceName = (canShortenIDisposable ? string.Empty : "System.") + "IDisposable";

                newTypeDeclaration.BaseTypes.Add(new SimpleType(interfaceName));

                foreach (var method in DisposeMethods(newTypeDeclaration).ToList())
                {
                    if (typeDeclaration.ClassType == ClassType.Interface)
                    {
                        method.Remove();
                    }
                    else
                    {
                        method.Modifiers &= ~Modifiers.Private;
                        method.Modifiers &= ~Modifiers.Protected;
                        method.Modifiers &= ~Modifiers.Internal;
                        method.Modifiers |= Modifiers.Public;
                    }
                }

                if (typeDeclaration.ClassType == ClassType.Interface)
                {
                    var disposeMember = ((MemberResolveResult)ctx.Resolve(methodDeclaration)).Member;
                    script.DoGlobalOperationOn(new List<IEntity>() { disposeMember }, (nCtx, nScript, nodes) =>
                    {
                        List<Tuple<AstType, AstType>> pendingChanges = new List<Tuple<AstType, AstType>>();
                        foreach (var node in nodes)
                        {
                            var method = node as MethodDeclaration;
                            if (method != null && !method.PrivateImplementationType.IsNull)
                            {
                                var nResolver = ctx.GetResolverStateAfter(typeDeclaration.LBraceToken);

                                var nTypeResolve = nResolver.ResolveSimpleName("IDisposable", new List<IType>()) as TypeResolveResult;
                                bool nCanShortenIDisposable = nTypeResolve != null && nTypeResolve.Type.FullName == "System.IDisposable";

                                string nInterfaceName = (nCanShortenIDisposable ? string.Empty : "System.") + "IDisposable";

                                pendingChanges.Add(Tuple.Create(method.PrivateImplementationType, AstType.Create(nInterfaceName)));
                            }
                        }

                        foreach (var change in pendingChanges)
                        {
                            nScript.Replace(change.Item1, change.Item2);
                        }
                    }, "Fix explicitly implemented members");
                }

                script.Replace(typeDeclaration, newTypeDeclaration);
            }

            static bool Implements(IType type, string fullName)
            {
                return type.GetAllBaseTypes().Any(baseType => baseType.FullName == fullName);
            }

            //Ignore entities that are not methods -- don't visit children
            public override void VisitFieldDeclaration(FieldDeclaration fieldDeclaration)
            {
            }

            public override void VisitFixedFieldDeclaration(FixedFieldDeclaration fixedFieldDeclaration)
            {
            }

            public override void VisitConstructorDeclaration(ConstructorDeclaration constructorDeclaration)
            {
            }

            public override void VisitDestructorDeclaration(DestructorDeclaration destructorDeclaration)
            {
            }

            public override void VisitPropertyDeclaration(PropertyDeclaration propertyDeclaration)
            {
            }

            public override void VisitEventDeclaration(EventDeclaration eventDeclaration)
            {
            }

            public override void VisitIndexerDeclaration(IndexerDeclaration indexerDeclaration)
            {
            }

            public override void VisitOperatorDeclaration(OperatorDeclaration operatorDeclaration)
            {
            }

            public override void VisitCustomEventDeclaration(CustomEventDeclaration eventDeclaration)
            {
            }
        }
    }

    [ExportCodeFixProvider(.DiagnosticId, LanguageNames.CSharp)]
    public class FixProvider : ICodeFixProvider
    {
        public IEnumerable<string> GetFixableDiagnosticIds()
        {
            yield return .DiagnosticId;
        }

        public async Task<IEnumerable<CodeAction>> GetFixesAsync(Document document, TextSpan span, IEnumerable<Diagnostic> diagnostics, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var result = new List<CodeAction>();
            foreach (var diagonstic in diagnostics)
            {
                var node = root.FindNode(diagonstic.Location.SourceSpan);
                //if (!node.IsKind(SyntaxKind.BaseList))
                //	continue;
                var newRoot = root.RemoveNode(node, SyntaxRemoveOptions.KeepNoTrivia);
                result.Add(CodeActionFactory.Create(node.Span, diagonstic.Severity, diagonstic.GetMessage(), document.WithSyntaxRoot(newRoot)));
            }
            return result;
        }
    }
}