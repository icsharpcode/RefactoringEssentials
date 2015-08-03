using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class RedundantUnsafeContextAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantUnsafeContextAnalyzerID,
            GettextCatalog.GetString("Unsafe modifier in redundant in unsafe context or when no unsafe constructs are used"),
            GettextCatalog.GetString("'unsafe' modifier is redundant"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantUnsafeContextAnalyzerID),
            customTags: DiagnosticCustomTags.Unnecessary
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

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

        //		class GatherVisitor : GatherVisitorBase<RedundantUnsafeContextAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			class UnsafeState 
        ////			{
        ////				public bool InUnsafeContext;
        ////				public bool UseUnsafeConstructs;
        ////
        ////				public UnsafeState(bool inUnsafeContext)
        ////				{
        ////					this.InUnsafeContext = inUnsafeContext;
        ////					this.UseUnsafeConstructs = false;
        ////				}
        ////
        ////				public override string ToString()
        ////				{
        ////					return string.Format("[UnsafeState: InUnsafeContext={0}, UseUnsafeConstructs={1}]", InUnsafeContext, UseUnsafeConstructs);
        ////				}
        ////			}
        ////
        ////			readonly Stack<UnsafeState> unsafeStateStack = new Stack<UnsafeState> ();
        ////
        ////			public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
        ////			{
        ////				bool unsafeIsRedundant = false;
        ////				if (unsafeStateStack.Count > 0) {
        ////					var curState = unsafeStateStack.Peek();
        ////
        ////					unsafeIsRedundant |= typeDeclaration.HasModifier(Modifiers.Unsafe);
        ////
        ////					unsafeStateStack.Push(new UnsafeState (curState.InUnsafeContext)); 
        ////				} else {
        ////					unsafeStateStack.Push(new UnsafeState (typeDeclaration.HasModifier(Modifiers.Unsafe))); 
        ////				}
        ////
        ////				base.VisitTypeDeclaration(typeDeclaration);
        ////
        ////				var state = unsafeStateStack.Pop();
        ////				unsafeIsRedundant = typeDeclaration.HasModifier(Modifiers.Unsafe) && !state.UseUnsafeConstructs;
        ////				if (unsafeIsRedundant) {
        ////					AddDiagnosticAnalyzer(new CodeIssue(
        ////						typeDeclaration.ModifierTokens.First (t => t.Modifier == Modifiers.Unsafe),
        ////						ctx.TranslateString(""), 
        ////						ctx.TranslateString(""), 
        ////						script => script.ChangeModifier(typeDeclaration, typeDeclaration.Modifiers & ~Modifiers.Unsafe)
        ////					) { IssueMarker = IssueMarker.GrayOut });
        ////				}
        ////			}
        ////
        ////			public override void VisitFixedFieldDeclaration(FixedFieldDeclaration fixedFieldDeclaration)
        ////			{
        ////				base.VisitFixedFieldDeclaration(fixedFieldDeclaration);
        ////				unsafeStateStack.Peek().UseUnsafeConstructs = true;
        ////			}
        ////
        ////			public override void VisitComposedType(ComposedType composedType)
        ////			{
        ////				base.VisitComposedType(composedType);
        ////				if (composedType.PointerRank > 0)
        ////					unsafeStateStack.Peek().UseUnsafeConstructs = true;
        ////			}
        ////
        ////			public override void VisitFixedStatement(FixedStatement fixedStatement)
        ////			{
        ////				base.VisitFixedStatement(fixedStatement);
        ////
        ////				unsafeStateStack.Peek().UseUnsafeConstructs = true;
        ////			}
        ////
        ////			public override void VisitSizeOfExpression(SizeOfExpression sizeOfExpression)
        ////			{
        ////				base.VisitSizeOfExpression(sizeOfExpression);
        ////				unsafeStateStack.Peek().UseUnsafeConstructs = true;
        ////			}
        ////
        ////			public override void VisitUnaryOperatorExpression(UnaryOperatorExpression unaryOperatorExpression)
        ////			{
        ////				base.VisitUnaryOperatorExpression(unaryOperatorExpression);
        ////				if (unaryOperatorExpression.Operator == UnaryOperatorType.AddressOf ||
        ////				    unaryOperatorExpression.Operator == UnaryOperatorType.Dereference)
        ////					unsafeStateStack.Peek().UseUnsafeConstructs = true;
        ////			}
        ////		
        ////			public override void VisitUnsafeStatement(UnsafeStatement unsafeStatement)
        ////			{
        ////				unsafeStateStack.Peek().UseUnsafeConstructs = true;
        ////				bool isRedundant = unsafeStateStack.Peek().InUnsafeContext;
        ////				unsafeStateStack.Push(new UnsafeState (true)); 
        ////				base.VisitUnsafeStatement(unsafeStatement);
        ////				isRedundant |= !unsafeStateStack.Pop().UseUnsafeConstructs;
        ////
        ////				if (isRedundant) {
        ////					AddDiagnosticAnalyzer(new CodeIssue(
        ////						unsafeStatement.UnsafeToken,
        ////						ctx.TranslateString("'unsafe' statement is redundant."), 
        ////						ctx.TranslateString("Replace 'unsafe' statement with it's body"), 
        ////						s => {
        ////							s.Remove(unsafeStatement.UnsafeToken);
        ////							s.Remove(unsafeStatement.Body.LBraceToken);
        ////							s.Remove(unsafeStatement.Body.RBraceToken);
        ////							s.FormatText(unsafeStatement.Parent);
        ////						}
        ////					));
        ////				}
        ////			}
        //		}
    }
}