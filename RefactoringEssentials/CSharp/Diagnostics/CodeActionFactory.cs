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
                throw new ArgumentNullException(nameof(description));
            if (changedDocument == null)
                throw new ArgumentNullException(nameof(changedDocument));
            return new DocumentChangeAction(textSpan, severity, description, ct => Task.FromResult<Document>(changedDocument));
        }

        public static CodeAction Create(TextSpan textSpan, DiagnosticSeverity severity, string description, Func<CancellationToken, Task<Document>> createChangedDocument)
        {
            if (description == null)
                throw new ArgumentNullException(nameof(description));
            if (createChangedDocument == null)
                throw new ArgumentNullException(nameof(createChangedDocument));
            return new DocumentChangeAction(textSpan, severity, description, createChangedDocument);
        }

        public static CodeAction Create(TextSpan textSpan, DiagnosticSeverity severity, string description, Solution changedSolution)
        {
            if (description == null)
                throw new ArgumentNullException(nameof(description));
            if (changedSolution == null)
                throw new ArgumentNullException(nameof(changedSolution));
            return new DocumentChangeAction(textSpan, severity, description, ct => Task.FromResult<Solution>(changedSolution));
        }

        public static CodeAction Create(TextSpan textSpan, DiagnosticSeverity severity, string description, Func<CancellationToken, Task<Solution>> createChangedSolution)
        {
            if (description == null)
                throw new ArgumentNullException(nameof(description));
            if (createChangedSolution == null)
                throw new ArgumentNullException(nameof(createChangedSolution));
            return new DocumentChangeAction(textSpan, severity, description, createChangedSolution);
        }

        public static CodeAction CreateInsertion(TextSpan textSpan, DiagnosticSeverity severity, string description, Func<CancellationToken, Task<InsertionResult>> createInsertion)
        {
            if (description == null)
                throw new ArgumentNullException(nameof(description));
            if (createInsertion == null)
                throw new ArgumentNullException(nameof(createInsertion));
            return new InsertionAction(textSpan, severity, description, createInsertion);
        }
    }
}