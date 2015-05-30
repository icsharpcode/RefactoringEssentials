using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using System.Linq;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class MissingInterfaceMemberImplementationAnalyzer : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "MissingInterfaceMemberImplementationAnalyzer";
        const string Description = "Searches for missing interface implementations";
        const string MessageFormat = "";
        const string Category = DiagnosticAnalyzerCategories.CompilerErrors;

        static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, DiagnosticSeverity.Error, true, "Missing interface members");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            //context.RegisterSyntaxNodeAction(
            //	(nodeContext) => {
            //		Diagnostic diagnostic;
            //		if (TryGetDiagnostic (nodeContext, out diagnostic)) {
            //			nodeContext.ReportDiagnostic(diagnostic);
            //		}
            //	}, 
            //	new SyntaxKind[] { SyntaxKind.None }
            //);
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            if (nodeContext.IsFromGeneratedCode())
                return false;
            //var node = nodeContext.Node as ;
            //diagnostic = Diagnostic.Create (descriptor, node.GetLocation ());
            //return true;
            return false;
        }

        //		class GatherVisitor : GatherVisitorBase<MissingInterfaceMemberImplementationAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base(semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
        ////			{
        ////				if (typeDeclaration.ClassType == ClassType.Interface || typeDeclaration.ClassType == ClassType.Enum)
        ////					return;
        ////				base.VisitTypeDeclaration(typeDeclaration);
        ////				var rr = ctx.Resolve(typeDeclaration);
        ////				if (rr.IsError)
        ////					return;
        ////				foreach (var baseType in typeDeclaration.BaseTypes) {
        ////					var bt = ctx.Resolve(baseType);
        ////					if (bt.IsError || bt.Type.Kind != TypeKind.Interface)
        ////						continue;
        ////					bool interfaceMissing;
        ////					var toImplement = ImplementInterfaceAction.CollectMembersToImplement(rr.Type.GetDefinition(), bt.Type, false, out interfaceMissing);
        ////					if (toImplement.Count == 0)
        ////						continue;
        ////
        ////					AddDiagnosticAnalyzer(new CodeIssue(baseType, ctx.TranslateString("Missing interface member implementations")) { ActionProvider = { typeof(ImplementInterfaceAction), typeof(ImplementInterfaceExplicitAction)} });
        ////				}
        ////			}
        //		}
    }

    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    [NotPortedYet]
    public class MissingInterfaceMemberImplementationFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(MissingInterfaceMemberImplementationAnalyzer.DiagnosticId);
            }
        }

        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public async override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var cancellationToken = context.CancellationToken;
            var span = context.Span;
            var diagnostics = context.Diagnostics;
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var diagnostic = diagnostics.First();
            var node = root.FindNode(context.Span);
            //if (!node.IsKind(SyntaxKind.BaseList))
            //	continue;
            var newRoot = root.RemoveNode(node, SyntaxRemoveOptions.KeepNoTrivia);
            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, diagnostic.GetMessage(), document.WithSyntaxRoot(newRoot)), diagnostic);
        }
    }
}