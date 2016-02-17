using System;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using RefactoringEssentials.VB.Converter;

namespace RefactoringEssentials
{
	[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(ConvertUnitTestToVB)), Shared]
	internal class ConvertUnitTestToVB : CodeRefactoringProvider
	{
		public async sealed override Task ComputeRefactoringsAsync(CodeRefactoringContext context)
		{
			var document = context.Document;
			if (document.Project.Solution.Workspace.Kind == WorkspaceKind.MiscellaneousFiles)
				return;
			var span = context.Span;
			if (!span.IsEmpty)
				return;
			var cancellationToken = context.CancellationToken;
			if (cancellationToken.IsCancellationRequested)
				return;
			var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
			if (model.IsFromGeneratedCode(cancellationToken))
				return;
			var root = await model.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
			var token = root.FindToken(span.Start);
			if (!token.IsKind(SyntaxKind.IdentifierToken))
				return;
			var node = token.Parent as ClassDeclarationSyntax;
			if (node == null)
				return;
			if (!node.AttributeLists.Any(l => HasTestFixtureAttribute(l, model)))
				return;
            if (model.GetDeclaredSymbol(node).ContainingNamespace.GetFullName().Contains(".VB."))
                return;
			context.RegisterRefactoring(
				CodeActionFactory.Create(
					token.Span,
					DiagnosticSeverity.Info,
					"Convert Unit Test to VB",
					t2 => {
                        root = root.ReplaceNodes(root.DescendantNodes().OfType<SimpleNameSyntax>().Where(n => n.Identifier.Text.StartsWith("CSharp", StringComparison.Ordinal)), ConvertIdentifier);
                        root = root.ReplaceNodes(root.DescendantNodes().OfType<LiteralExpressionSyntax>().Where(l => l.IsKind(SyntaxKind.StringLiteralExpression)), ConvertContent);
						var vbDocument = document.Project.AddDocument(document.Name, root);
                        return Task.FromResult(vbDocument);
					}
				)
			);
		}

		bool HasTestFixtureAttribute(AttributeListSyntax list, SemanticModel model)
		{
			return list.Attributes.Any(a => model.GetTypeInfo(a.Name).Type.GetFullName() == "NUnit.Framework.TestFixtureAttribute");
        }

		string ConvertCodeToVB(string code)
        {
            if (IsVBCode(code))
                return code;
			var tree = SyntaxFactory.ParseSyntaxTree(SourceText.From(code));

			var references = new[]
			{
				MetadataReference.CreateFromFile(typeof(Action).GetAssemblyLocation()),
				MetadataReference.CreateFromFile(typeof(System.ComponentModel.EditorBrowsableAttribute).GetAssemblyLocation()),
				MetadataReference.CreateFromFile(typeof(Enumerable).GetAssemblyLocation())
			};
			var compilation = CSharpCompilation.Create("Conversion", new[] { tree }, references);

			return CSharpConverter.Convert((CSharpSyntaxNode)tree.GetRoot(), compilation.GetSemanticModel(tree, true), null).NormalizeWhitespace().ToFullString();
		}

        bool IsVBCode(string code)
        {
            foreach (var ch in code)
            {
                if (ch == '}' || ch == '{')
                    return false;
            }
            return true;
        }

        SyntaxNode ConvertContent(LiteralExpressionSyntax originalNode, LiteralExpressionSyntax withConvertedDescendants)
        {
            var value = ConvertCodeToVB((string)originalNode.Token.Value);
			return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("@\"" + value + '"', value));
        }

        SyntaxNode ConvertIdentifier(SimpleNameSyntax originalNode, SimpleNameSyntax withConvertedDescendants)
        {
            return SyntaxFactory.ParseName(originalNode.ToFullString().Replace("CSharp", "VB"));
        }
	}
}