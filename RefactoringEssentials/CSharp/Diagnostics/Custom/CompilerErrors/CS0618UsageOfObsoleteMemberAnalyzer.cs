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
    public class CS0618UsageOfObsoleteMemberAnalyzer : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "CS0618UsageOfObsoleteMemberAnalyzer";
        const string Description = "CS0618: Member is obsolete";
        const string MessageFormat = "";
        const string Category = DiagnosticAnalyzerCategories.CompilerWarnings;

        static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, DiagnosticSeverity.Warning, true, "CS0618: Member is obsolete");

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

        //		class GatherVisitor : GatherVisitorBase<CS0618UsageOfObsoleteMemberAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base(semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}
        ////
        ////			void Check(ResolveResult rr, AstNode nodeToMark)
        ////			{
        ////				if (rr == null || rr.IsError)
        ////					return;
        ////				IMember member = null;
        ////				var memberRR = rr as MemberResolveResult;
        ////				if (memberRR != null)
        ////					member = memberRR.Member;
        ////
        ////				var operatorRR = rr as OperatorResolveResult;
        ////				if (operatorRR != null)
        ////					member = operatorRR.UserDefinedOperatorMethod;
        ////
        ////				if (member == null)
        ////					return;
        ////
        ////				var attr = member.Attributes.FirstOrDefault(a => a.AttributeType.Name == "ObsoleteAttribute" && a.AttributeType.Namespace == "System");
        ////				if (attr == null)
        ////					return;
        ////				AddDiagnosticAnalyzer(new CodeIssue(nodeToMark, string.Format(ctx.TranslateString("'{0}' is obsolete"), member.FullName)));
        ////			}
        ////
        ////			public override void VisitMemberReferenceExpression(MemberReferenceExpression memberReferenceExpression)
        ////			{
        ////				base.VisitMemberReferenceExpression(memberReferenceExpression);
        ////				Check(ctx.Resolve(memberReferenceExpression), memberReferenceExpression.MemberNameToken);
        ////			}
        ////
        ////			public override void VisitInvocationExpression(InvocationExpression invocationExpression)
        ////			{
        ////				base.VisitInvocationExpression(invocationExpression);
        ////				Check(ctx.Resolve(invocationExpression), invocationExpression.Target);
        ////			}
        ////
        ////			public override void VisitIdentifierExpression(IdentifierExpression identifierExpression)
        ////			{
        ////				base.VisitIdentifierExpression(identifierExpression);
        ////				Check(ctx.Resolve(identifierExpression), identifierExpression);
        ////			}
        ////
        ////			public override void VisitIndexerExpression(IndexerExpression indexerExpression)
        ////			{
        ////				base.VisitIndexerExpression(indexerExpression);
        ////				Check(ctx.Resolve(indexerExpression), indexerExpression);
        ////			}
        ////
        ////			public override void VisitBinaryOperatorExpression(BinaryOperatorExpression binaryOperatorExpression)
        ////			{
        ////				base.VisitBinaryOperatorExpression(binaryOperatorExpression);
        ////				Check(ctx.Resolve(binaryOperatorExpression), binaryOperatorExpression.OperatorToken);
        ////			}
        ////
        ////			bool IsObsolete(EntityDeclaration entity)
        ////			{
        ////				foreach (var section in entity.Attributes) {
        ////					foreach (var attr in section.Attributes) {
        ////						var rr = ctx.Resolve(attr); 
        ////						if (rr.Type.Name == "ObsoleteAttribute" && rr.Type.Namespace == "System")
        ////							return true;
        ////					}
        ////				}
        ////				return false;
        ////			}
        ////
        ////			public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
        ////			{
        ////				if (IsObsolete(typeDeclaration))
        ////					return;
        ////				base.VisitTypeDeclaration(typeDeclaration);
        ////			}
        ////
        ////			public override void VisitMethodDeclaration(MethodDeclaration methodDeclaration)
        ////			{
        ////				if (IsObsolete(methodDeclaration))
        ////					return;
        ////				base.VisitMethodDeclaration(methodDeclaration);
        ////			}
        ////
        ////			public override void VisitPropertyDeclaration(PropertyDeclaration propertyDeclaration)
        ////			{
        ////				if (IsObsolete(propertyDeclaration))
        ////					return;
        ////				base.VisitPropertyDeclaration(propertyDeclaration);
        ////			}
        ////
        ////			public override void VisitAccessor(Accessor accessor)
        ////			{
        ////				if (IsObsolete(accessor))
        ////					return;
        ////				base.VisitAccessor(accessor);
        ////			}
        ////
        ////			public override void VisitIndexerDeclaration(IndexerDeclaration indexerDeclaration)
        ////			{
        ////				if (IsObsolete(indexerDeclaration))
        ////					return;
        ////				base.VisitIndexerDeclaration(indexerDeclaration);
        ////			}
        ////
        ////			public override void VisitCustomEventDeclaration(CustomEventDeclaration eventDeclaration)
        ////			{
        ////				if (IsObsolete(eventDeclaration))
        ////					return;
        ////				base.VisitCustomEventDeclaration(eventDeclaration);
        ////			}
        ////
        ////			public override void VisitFieldDeclaration(FieldDeclaration fieldDeclaration)
        ////			{
        ////				if (IsObsolete(fieldDeclaration))
        ////					return;
        ////				base.VisitFieldDeclaration(fieldDeclaration);
        ////			}
        ////
        ////			public override void VisitConstructorDeclaration(ConstructorDeclaration constructorDeclaration)
        ////			{
        ////				if (IsObsolete(constructorDeclaration))
        ////					return;
        ////				base.VisitConstructorDeclaration(constructorDeclaration);
        ////			}
        ////
        ////			public override void VisitDestructorDeclaration(DestructorDeclaration destructorDeclaration)
        ////			{
        ////				if (IsObsolete(destructorDeclaration))
        ////					return;
        ////				base.VisitDestructorDeclaration(destructorDeclaration);
        ////			}
        ////
        ////			public override void VisitOperatorDeclaration(OperatorDeclaration operatorDeclaration)
        ////			{
        ////				if (IsObsolete(operatorDeclaration))
        ////					return;
        ////				base.VisitOperatorDeclaration(operatorDeclaration);
        ////			}
        //		}
    }

    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    [NotPortedYet]
    public class CS0618UsageOfObsoleteMemberFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CS0618UsageOfObsoleteMemberAnalyzer.DiagnosticId);
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