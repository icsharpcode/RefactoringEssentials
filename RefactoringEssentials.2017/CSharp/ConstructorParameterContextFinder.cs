using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactoringEssentials.CSharp
{
    public class ConstructorParameterContextFinder
    {
        public async Task<ConstructorParameterContext> Find(CodeRefactoringContext context)
        {
            var document = context.Document;
            if (document.Project.Solution.Workspace.Kind == WorkspaceKind.MiscellaneousFiles)
                return null;
            var span = context.Span;
            if (!span.IsEmpty)
                return null;
            var cancellationToken = context.CancellationToken;
            if (cancellationToken.IsCancellationRequested)
                return null;
            var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (model.IsFromGeneratedCode(cancellationToken))
                return null;
            var root = await model.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            var token = root.FindToken(span.Start);
            var parameter = token.Parent as ParameterSyntax;

            if (parameter == null)
                return null;

            var ctor = parameter.Parent.Parent as ConstructorDeclarationSyntax;
            if (ctor == null)
                return null;

            return new ConstructorParameterContext(document, parameter.Identifier.ToString(), GetPropertyName(parameter.Identifier.ToString()), parameter.Type, ctor, span, root);
        }

        static string GetPropertyName(string v)
        {
            return char.ToUpper(v[0]) + v.Substring(1);
        }
    }
}
