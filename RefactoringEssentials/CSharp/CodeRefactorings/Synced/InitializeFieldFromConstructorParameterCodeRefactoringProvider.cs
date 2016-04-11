using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Initialize field from constructor parameter")]
    public class InitializeFieldFromConstructorParameterCodeRefactoringProvider : CodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
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
            var parameter = token.Parent as ParameterSyntax;

            if (parameter != null)
            {
                var ctor = parameter.Parent.Parent as ConstructorDeclarationSyntax;
                if (ctor == null)
                    return;
                
                context.RegisterRefactoring(
                    CodeActionFactory.Create(
                        parameter.Span,
                        DiagnosticSeverity.Info,
                        GettextCatalog.GetString("Initialize field from parameter"),
                        t2 =>
                        {
                    var newFieldName = NameProposalService.GetNameProposal(parameter.Identifier.ValueText, SyntaxKind.FieldDeclaration, Accessibility.Private, false, context.Document, ctor.SpanStart);
                            var newField = SyntaxFactory.FieldDeclaration(
                                SyntaxFactory.VariableDeclaration(
                                    parameter.Type,
                                    SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(SyntaxFactory.VariableDeclarator(newFieldName)))
                            ).WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)))
                            .WithAdditionalAnnotations(Formatter.Annotation);

                            var assignmentStatement = SyntaxFactory.ExpressionStatement(
                                SyntaxFactory.AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ThisExpression(), SyntaxFactory.IdentifierName(newFieldName)),
                            SyntaxFactory.IdentifierName(parameter.Identifier)
                                )
                            ).WithAdditionalAnnotations(Formatter.Annotation);

                            var trackedRoot = root.TrackNodes(ctor);
                            var newRoot = trackedRoot.InsertNodesBefore(trackedRoot.GetCurrentNode(ctor), new List<SyntaxNode>() {
                                newField
                            });
                            newRoot = newRoot.ReplaceNode(newRoot.GetCurrentNode(ctor), ctor.WithBody(
                                ctor.Body.WithStatements(SyntaxFactory.List<StatementSyntax>(new[] { assignmentStatement }.Concat(ctor.Body.Statements)))
                            ));

                            return Task.FromResult(document.WithSyntaxRoot(newRoot));
                        })
                );
            }
        }
    }
}