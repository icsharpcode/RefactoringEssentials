using System.Collections.Generic;
using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory.Refactoring;
using System.Linq;
using ICSharpCode.NRefactory6.CSharp.Analysis;

namespace ICSharpCode.NRefactory6.CSharp.Diagnostics
{
    //	[IssueDescription ("Tail recursive call may be replaced by loop",
    //		Description = "Tail recursive calls should be avoided.",
    //		Category = IssueCategories.CodeQualityIssues,
    //		Severity = Severity.Hint)]
    public class ConvertTailRecursiveCallToLoopDiagnosticAnalyzer : GatherVisitorCodeIssueProvider
    {
        protected override IGatherVisitor CreateVisitor(BaseSemanticModel context)
        {
            return new GatherVisitor(context);
        }

        class GatherVisitor : GatherVisitorBase<ConvertTailRecursiveCallToLoopIssue>
        {
            public GatherVisitor(BaseSemanticModel ctx) : base(ctx)
            {
            }

            public override void VisitMethodDeclaration(MethodDeclaration methodDeclaration)
            {
                // TODO
            }
        }
    }
}

