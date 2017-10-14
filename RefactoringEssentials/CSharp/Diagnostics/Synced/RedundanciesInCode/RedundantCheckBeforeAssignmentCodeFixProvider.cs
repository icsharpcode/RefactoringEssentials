using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.Diagnostics
{
	[ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
	public class RedundantCheckBeforeAssignmentCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds
		{
			get
			{
				return ImmutableArray.Create(CSharpDiagnosticIDs.RedundantCheckBeforeAssignmentAnalyzerID);
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
			var diagnostics = context.Diagnostics;
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var diagnostic = diagnostics.First();
			var node = root.FindNode(context.Span) as BinaryExpressionSyntax;
			if (node == null)
				return;
			StatementSyntax expression = null;
			var ifStatement = node.Parent as IfStatementSyntax;
			if (ifStatement == null)
				return;
            var statement = ifStatement.Statement;
			if (statement is BlockSyntax)
				expression = ((BlockSyntax)statement).Statements[0];
			else
				expression = statement;

			if (expression == null)
				return;

            expression = expression
                    .WithOrderedTriviaFromSubTree(ifStatement)
                    .WithAdditionalAnnotations(Formatter.Annotation);
            var newRoot = root.ReplaceNode(ifStatement, expression);

            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, "Remove redundant check", document.WithSyntaxRoot(newRoot)), diagnostic);
		}
	}
}