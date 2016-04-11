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
    public class CS0183ExpressionIsAlwaysOfProvidedTypeAnalyzer : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "CS0183ExpressionIsAlwaysOfProvidedTypeAnalyzer";
        const string Description = "CS0183:Given expression is always of the provided type";
        const string MessageFormat = "Given expression is always of the provided type. Consider comparing with 'null' instead";
        const string Category = DiagnosticAnalyzerCategories.CompilerWarnings;

        static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, DiagnosticSeverity.Warning, true, "CS0183:Given expression is always of the provided type");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
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
            //var node = nodeContext.Node as ;
            //diagnostic = Diagnostic.Create (descriptor, node.GetLocation ());
            //return true;
            return false;
        }

        //		class GatherVisitor : GatherVisitorBase<CS0183ExpressionIsAlwaysOfProvidedTypeAnalyzer>
        //		{
        //			//			readonly CSharpConversions conversions;

        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //				// conversions = CSharpConversions.Get(ctx.Compilation);
        //			}
        ////
        ////			public override void VisitIsExpression(IsExpression isExpression)
        ////			{
        ////				base.VisitIsExpression(isExpression);
        ////
        ////				var type = ctx.Resolve(isExpression.Expression).Type;
        ////				var providedType = ctx.ResolveType(isExpression.Type);
        ////
        ////				if (type.Kind == TypeKind.Unknown || providedType.Kind == TypeKind.Unknown)
        ////					return;
        //////				var foundConversion = conversions.ImplicitConversion(type, providedType);
        ////				if (!IsValidReferenceOrBoxingConversion(type, providedType))
        ////					return;
        ////
        ////				var action = new CodeAction(
        ////					             ctx.TranslateString(""), 
        ////					             script => script.Replace(isExpression, new BinaryOperatorExpression(
        ////						             isExpression.Expression.Clone(), BinaryOperatorType.InEquality, new PrimitiveExpression(null))),
        ////					             isExpression
        ////				             );
        ////				AddDiagnosticAnalyzer(new CodeIssue(isExpression, ctx.TranslateString(""), new [] { action }));
        ////			}
        ////
        ////			bool IsValidReferenceOrBoxingConversion(IType fromType, IType toType)
        ////			{
        ////				Conversion c = conversions.ImplicitConversion(fromType, toType);
        ////				return c.IsValid && (c.IsIdentityConversion || c.IsReferenceConversion || c.IsBoxingConversion);
        ////			}
        //		}
    }

    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class CS0183ExpressionIsAlwaysOfProvidedTypeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CS0183ExpressionIsAlwaysOfProvidedTypeAnalyzer.DiagnosticId);
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
            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, "Compare with 'null'", document.WithSyntaxRoot(newRoot)), diagnostic);
        }
    }
}