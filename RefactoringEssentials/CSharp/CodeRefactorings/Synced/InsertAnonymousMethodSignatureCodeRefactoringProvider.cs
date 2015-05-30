using System.Collections.Generic;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Insert anonymous method signature")]
    public class InsertAnonymousMethodSignatureCodeRefactoringProvider : CodeRefactoringProvider
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
            var anonymousMethodExpression = root.FindNode(span) as AnonymousMethodExpressionSyntax;
            if (anonymousMethodExpression == null || !anonymousMethodExpression.DelegateKeyword.Span.Contains(span) || anonymousMethodExpression.ParameterList != null)
                return;

            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    anonymousMethodExpression.Span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("Insert signature"),
                    t2 =>
                    {
                        var typeInfo = model.GetTypeInfo(anonymousMethodExpression);
                        var type = typeInfo.ConvertedType ?? typeInfo.Type;
                        if (type == null)
                            return Task.FromResult(document);
                        var method = type.GetDelegateInvokeMethod();

                        if (method == null)
                            return Task.FromResult(document);
                        var parameters = new List<ParameterSyntax>();

                        foreach (var param in method.Parameters)
                        {
                            var t = SyntaxFactory.ParseTypeName(param.Type.ToMinimalDisplayString(model, anonymousMethodExpression.SpanStart));
                            parameters.Add(SyntaxFactory.Parameter(SyntaxFactory.Identifier(param.Name)).WithType(t));
                        }

                        var newRoot = root.ReplaceNode((SyntaxNode)anonymousMethodExpression, anonymousMethodExpression.WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters))).WithAdditionalAnnotations(Formatter.Annotation));
                        return Task.FromResult(document.WithSyntaxRoot(newRoot));
                    }
                )
            );
        }
    }
}