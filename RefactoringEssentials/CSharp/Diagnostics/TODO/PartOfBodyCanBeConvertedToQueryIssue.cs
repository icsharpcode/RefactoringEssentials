using System;
using ICSharpCode.NRefactory.Refactoring;

namespace ICSharpCode.NRefactory6.CSharp.Diagnostics
{
    //	[IssueDescription ("Part of the loop body can be converted into LINQ expression",
    //		Description = "Part of the loop body can be converted into LINQ expression",
    //		Category = IssueCategories.Opportunities,
    //		Severity = Severity.Hint)]
    public class PartOfBodyCanBeConvertedToQueryDiagnosticAnalyzer : GatherVisitorCodeIssueProvider
    {
        protected override IGatherVisitor CreateVisitor(BaseSemanticModel context)
        {
            return new GatherVisitor(context);
        }

        class GatherVisitor : GatherVisitorBase<PartOfBodyCanBeConvertedToQueryIssue>
        {
            public GatherVisitor(BaseSemanticModel ctx) : base(ctx)
            {
            }
        }
    }
}

