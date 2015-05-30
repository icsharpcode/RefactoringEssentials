using System;
using ICSharpCode.NRefactory.Refactoring;

namespace ICSharpCode.NRefactory6.CSharp.Diagnostics
{
    //	[IssueDescription ("Loop can be converted into LINQ expression",
    //		Description = "Loop can be converted into LINQ expression",
    //		Category = IssueCategories.Opportunities,
    //		Severity = Severity.Suggestion,
    //		AnalysisDisableKeyword = "LoopCanBeConvertedToQuery")]
    public class LoopCanBeConvertedToQueryDiagnosticAnalyzer : GatherVisitorCodeIssueProvider
    {
        protected override IGatherVisitor CreateVisitor(BaseSemanticModel context)
        {
            return new GatherVisitor(context);
        }

        class GatherVisitor : GatherVisitorBase<LoopCanBeConvertedToQueryIssue>
        {
            public GatherVisitor(BaseSemanticModel ctx) : base(ctx)
            {
            }
        }
    }
}

