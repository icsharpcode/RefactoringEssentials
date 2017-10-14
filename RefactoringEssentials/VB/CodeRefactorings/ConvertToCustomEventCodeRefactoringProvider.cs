using System.Linq;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.VB.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.VisualBasic, Name = "Convert to custom event")]
    public class ConvertToCustomEventCodeRefactoringProvider : CodeRefactoringProvider
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

            var eventStatement = root.FindNode(span) as EventStatementSyntax;
            if (eventStatement == null)
                return;

            // No action on event blocks
            if (eventStatement.Parent is EventBlockSyntax)
                return;

            // Conversion to custom event only possible for delegate-based events
            if (eventStatement.AsClause == null)
                return;

            var delegateType = model.GetTypeInfo(eventStatement.AsClause.Type).Type;
            if (delegateType.IsErrorType() || !delegateType.IsDelegateType())
                return;
            var delegateParameters = delegateType.GetDelegateInvokeMethod().Parameters;

            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("Convert to custom event"),
                    t2 =>
                    {
                        var customEventStatement = SyntaxFactory.EventBlock(
                            eventStatement.WithCustomKeyword(SyntaxFactory.Token(SyntaxKind.CustomKeyword)),
                            SyntaxFactory.List(new AccessorBlockSyntax[] {
                                SyntaxFactory.AddHandlerAccessorBlock(
                                    SyntaxFactory.AddHandlerAccessorStatement(
                                        SyntaxFactory.List<AttributeListSyntax>(),
                                        SyntaxFactory.TokenList(),
                                        SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(new[] {
                                            SyntaxFactory.Parameter(SyntaxFactory.List<AttributeListSyntax>(), SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ByValKeyword)), SyntaxFactory.ModifiedIdentifier("value"), SyntaxFactory.SimpleAsClause(eventStatement.AsClause.Type().WithoutTrailingTrivia()), null)
                                        }))),
                                    SyntaxFactory.List<StatementSyntax>(new[] { GetNotImplementedThrowStatement() })),
                                SyntaxFactory.RemoveHandlerAccessorBlock(
                                    SyntaxFactory.RemoveHandlerAccessorStatement(
                                        SyntaxFactory.List<AttributeListSyntax>(),
                                        SyntaxFactory.TokenList(),
                                        SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(new[] {
                                            SyntaxFactory.Parameter(SyntaxFactory.List<AttributeListSyntax>(), SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ByValKeyword)), SyntaxFactory.ModifiedIdentifier("value"), SyntaxFactory.SimpleAsClause(eventStatement.AsClause.Type().WithoutTrailingTrivia()), null)
                                        }))),
                                    SyntaxFactory.List<StatementSyntax>(new[] { GetNotImplementedThrowStatement() })),
                                SyntaxFactory.RaiseEventAccessorBlock(
                                    SyntaxFactory.RaiseEventAccessorStatement(
                                        SyntaxFactory.List<AttributeListSyntax>(),
                                        SyntaxFactory.TokenList(),
                                        SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(
                                            delegateParameters.Select(
                                                p => SyntaxFactory.Parameter(SyntaxFactory.List<AttributeListSyntax>(), SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ByValKeyword)), SyntaxFactory.ModifiedIdentifier(p.Name), SyntaxFactory.SimpleAsClause(SyntaxFactory.ParseTypeName(p.Type.GetFullName())), null)
                                            )))),
                                    SyntaxFactory.List<StatementSyntax>(new[] { GetNotImplementedThrowStatement() }))
                            })
                        ).WithTrailingTrivia(eventStatement.GetTrailingTrivia());
                        
                        return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(eventStatement, customEventStatement.WithAdditionalAnnotations(Formatter.Annotation))));
                    }
                )
            );
        }

        public static ThrowStatementSyntax GetNotImplementedThrowStatement()
        {
            return SyntaxFactory.ThrowStatement(SyntaxFactory.ObjectCreationExpression(
                SyntaxFactory.QualifiedName(SyntaxFactory.IdentifierName(@"System"), SyntaxFactory.IdentifierName(@"NotImplementedException")))
                .WithArgumentList(SyntaxFactory.ArgumentList()));
        }
    }
}