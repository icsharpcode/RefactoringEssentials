using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory6.CSharp.Analysis;
using ICSharpCode.NRefactory.Refactoring;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory6.CSharp.Resolver;
using ICSharpCode.NRefactory.Semantics;

namespace ICSharpCode.NRefactory6.CSharp.Diagnostics
{
    //	[IssueDescription("Detects if fields & simple getters are used at the same time inside a class.",
    //	                  Description = "Detects if fields & simple getters are used at the same time inside a class.",
    //	                  Category = IssueCategories.CodeQualityIssues,
    //	                  Severity = Severity.Suggestion)]
    //	public class MixedUseOfFieldsAndGettersDiagnosticAnalyzer : GatherVisitorCodeIssueProvider
    //	{
    //		protected override IGatherVisitor CreateVisitor(BaseSemanticModel context)
    //		{
    //			return new GatherVisitor(context);
    //		}
    //
    //		class GatherVisitor : GatherVisitorBase<LockThisIssue>
    //		{
    //			public GatherVisitor (BaseSemanticModel context) : base (context)
    //			{
    //			}
    //
    //			public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
    //			{
    //				foreach (var entity in typeDeclaration.Members) {
    //					var propertyDeclaration = entity as PropertyDeclaration;
    //					if (propertyDeclaration == null)
    //						continue;
    //
    //				}
    //
    //				base.VisitTypeDeclaration(typeDeclaration);
    //
    //
    //
    //			}
    //
    //		}
    //	}
}

