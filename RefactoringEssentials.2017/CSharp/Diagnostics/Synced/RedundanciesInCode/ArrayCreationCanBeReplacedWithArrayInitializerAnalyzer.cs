using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ArrayCreationCanBeReplacedWithArrayInitializerAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.ArrayCreationCanBeReplacedWithArrayInitializerAnalyzerID,
            GettextCatalog.GetString("When initializing explicitly typed local variable or array type, array creation expression can be replaced with array initializer."),
            GettextCatalog.GetString("Redundant array creation expression"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.ArrayCreationCanBeReplacedWithArrayInitializerAnalyzerID),
            customTags: DiagnosticCustomTags.Unnecessary
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(
                nodeContext =>
                {
                    Diagnostic diagnostic;
                    if (TryGetDiagnostic(nodeContext, out diagnostic))
                    {
                        nodeContext.ReportDiagnostic(diagnostic);
                    }
                },
                new SyntaxKind[] { SyntaxKind.ArrayCreationExpression, SyntaxKind.ImplicitArrayCreationExpression }
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);

            InitializerExpressionSyntax initializer = null;
            var node = nodeContext.Node as ArrayCreationExpressionSyntax;
            if (node != null) initializer = node.Initializer;
            var inode = nodeContext.Node as ImplicitArrayCreationExpressionSyntax;
            if (inode != null) initializer = inode.Initializer;

            if (initializer == null)
                return false;
            var varInitializer = nodeContext.Node.Parent.Parent;
            if (varInitializer == null)
                return false;
            var variableDeclaration = varInitializer.Parent as VariableDeclarationSyntax;
            if (variableDeclaration != null)
            {
                if (!variableDeclaration.Type.IsKind(SyntaxKind.ArrayType))
                    return false;
                diagnostic = Diagnostic.Create(
                    descriptor,
                    Location.Create(nodeContext.SemanticModel.SyntaxTree, TextSpan.FromBounds((node != null ? node.NewKeyword : inode.NewKeyword).Span.Start, initializer.Span.Start))
                );
                return true;
            }
            return false;
        }
    }
}