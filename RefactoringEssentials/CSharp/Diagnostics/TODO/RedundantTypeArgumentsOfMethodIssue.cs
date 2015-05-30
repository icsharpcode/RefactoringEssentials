using System;
using ICSharpCode.NRefactory.Refactoring;

namespace ICSharpCode.NRefactory6.CSharp.Diagnostics
{
    //	[IssueDescription ("Redundant type arguments in method call",
    //		Description = "Explicit specifiction is redundant because they are inferred from arguments",
    //		Category = IssueCategories.RedundanciesInCode,
    //		Severity = Severity.Hint, 
    //		AnalysisDisableKeyword = "RedundantTypeArgumentsOfMethod" )]
    public class RedundantTypeArgumentsOfMethodDiagnosticAnalyzer : GatherVisitorCodeIssueProvider
    {
        protected override IGatherVisitor CreateVisitor(BaseSemanticModel context)
        {
            return new GatherVisitor(context);
        }

        class GatherVisitor : GatherVisitorBase<RedundantTypeArgumentsOfMethodIssue>
        {
            public GatherVisitor(BaseSemanticModel ctx) : base(ctx)
            {
            }
        }
    }
}

