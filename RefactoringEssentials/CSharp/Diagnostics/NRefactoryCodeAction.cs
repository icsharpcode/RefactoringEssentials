using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;

namespace RefactoringEssentials
{
    public abstract class NRefactoryCodeAction : CodeAction
    {
        public TextSpan TextSpan
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the severity of the code action. 
        /// Actions are sorted according to their Severity.
        /// </summary>
        /// <value>The severity.</value>
        public DiagnosticSeverity Severity
        {
            get;
            private set;
        }

        protected NRefactoryCodeAction(TextSpan textSpan, DiagnosticSeverity severity)
        {
            TextSpan = textSpan;
            Severity = severity;
        }
    }
}

