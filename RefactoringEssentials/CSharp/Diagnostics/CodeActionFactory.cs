using System;
using Microsoft.CodeAnalysis.CodeActions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace RefactoringEssentials
{
    static class CodeActionFactory
    {
        public static CodeAction Create(TextSpan textSpan, DiagnosticSeverity severity, string description, Document changedDocument)
        {
            if (description == null)
                throw new ArgumentNullException("description");
            if (changedDocument == null)
                throw new ArgumentNullException("changedDocument");
            return new DocumentChangeAction(textSpan, severity, description, ct => Task.FromResult<Document>(changedDocument));
        }

        public static CodeAction Create(TextSpan textSpan, DiagnosticSeverity severity, string description, Func<CancellationToken, Task<Document>> createChangedDocument)
        {
            if (description == null)
                throw new ArgumentNullException("description");
            if (createChangedDocument == null)
                throw new ArgumentNullException("createChangedDocument");
            return new DocumentChangeAction(textSpan, severity, description, createChangedDocument);
        }

        public static CodeAction CreateInsertion(TextSpan textSpan, DiagnosticSeverity severity, string description, Func<CancellationToken, Task<InsertionResult>> createInsertion)
        {
            if (description == null)
                throw new ArgumentNullException("description");
            if (createInsertion == null)
                throw new ArgumentNullException("createInsertion");
            return new InsertionAction(textSpan, severity, description, createInsertion);
        }
    }
}