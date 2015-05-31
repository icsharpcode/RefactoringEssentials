using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class MemberHidesStaticFromOuterClassAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.MemberHidesStaticFromOuterClassAnalyzerID,
            GettextCatalog.GetString("Member hides static member from outer class"),
            GettextCatalog.GetString("{0} '{1}' hides {2} from outer class"),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.MemberHidesStaticFromOuterClassAnalyzerID)
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            //context.RegisterSyntaxNodeAction(
            //	(nodeContext) => {
            //		Diagnostic diagnostic;
            //		if (TryGetDiagnostic (nodeContext, out diagnostic)) {
            //			nodeContext.ReportDiagnostic(diagnostic);
            //		}
            //	}, 
            //	new SyntaxKind[] { SyntaxKind.None }
            //);
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            if (nodeContext.IsFromGeneratedCode())
                return false;
            //var node = nodeContext.Node as ;
            //diagnostic = Diagnostic.Create (descriptor, node.GetLocation ());
            //return true;
            return false;
        }

        //		class GatherVisitor : GatherVisitorBase<MemberHidesStaticFromOuterClassAnalyzer>
        //		{
        //			//readonly List<List<IMember>> staticMembers = new List<List<IMember>>();

        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			public override void VisitBlockStatement(BlockStatement blockStatement)
        ////			{
        ////				// SKIP
        ////			}
        ////
        ////			public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
        ////			{
        ////				var rr = ctx.Resolve(typeDeclaration);
        ////
        ////				staticMembers.Add(new List<IMember>(rr.Type.GetMembers(m => m.IsStatic)));
        ////				base.VisitTypeDeclaration(typeDeclaration);
        ////				staticMembers.RemoveAt(staticMembers.Count - 1); 
        ////			}
        ////
        ////			void Check(string name, AstNode nodeToMark, string memberType)
        ////			{
        ////				for (int i = 0; i < staticMembers.Count - 1; i++) {
        ////					var member = staticMembers[i].FirstOrDefault(m => m.Name == name);
        ////					if (member == null)
        ////						continue;
        ////					string outerMemberType;
        ////					switch (member.SymbolKind) {
        ////						case SymbolKind.Field:
        ////							outerMemberType = ctx.TranslateString("field");
        ////							break;
        ////						case SymbolKind.Property:
        ////							outerMemberType = ctx.TranslateString("property");
        ////							break;
        ////						case SymbolKind.Event:
        ////							outerMemberType = ctx.TranslateString("event");
        ////							break;
        ////						case SymbolKind.Method:
        ////							outerMemberType = ctx.TranslateString("method");
        ////							break;
        ////						default:
        ////							outerMemberType = ctx.TranslateString("member");
        ////							break;
        ////					}
        ////					AddDiagnosticAnalyzer(new CodeIssue(nodeToMark,
        //			//						string.Format(ctx.TranslateString("{0} '{1}' hides {2} from outer class"),
        ////							memberType, member.Name, outerMemberType)));
        ////					return;
        ////				}
        ////			}
        ////
        ////			public override void VisitEventDeclaration(EventDeclaration eventDeclaration)
        ////			{
        ////				foreach (var init in eventDeclaration.Variables) {
        ////					Check(init.Name, init.NameToken, ctx.TranslateString("Event"));
        ////				}
        ////			}
        ////
        ////			public override void VisitCustomEventDeclaration(CustomEventDeclaration eventDeclaration)
        ////			{
        ////				Check(eventDeclaration.Name, eventDeclaration.NameToken, ctx.TranslateString("Event"));
        ////			}
        ////
        ////			public override void VisitFieldDeclaration(FieldDeclaration fieldDeclaration)
        ////			{
        ////				foreach (var init in fieldDeclaration.Variables) {
        ////					Check(init.Name, init.NameToken, ctx.TranslateString("Field"));
        ////				}
        ////			}
        ////
        ////			public override void VisitPropertyDeclaration(PropertyDeclaration propertyDeclaration)
        ////			{
        ////				Check(propertyDeclaration.Name, propertyDeclaration.NameToken, ctx.TranslateString("Property"));
        ////			}
        ////
        ////			public override void VisitFixedFieldDeclaration(FixedFieldDeclaration fixedFieldDeclaration)
        ////			{
        ////				Check(fixedFieldDeclaration.Name, fixedFieldDeclaration.NameToken, ctx.TranslateString("Fixed field"));
        ////			}
        ////
        ////			public override void VisitMethodDeclaration(MethodDeclaration methodDeclaration)
        ////			{
        ////				Check(methodDeclaration.Name, methodDeclaration.NameToken, ctx.TranslateString("Method"));
        ////			}
        //		}
    }
}