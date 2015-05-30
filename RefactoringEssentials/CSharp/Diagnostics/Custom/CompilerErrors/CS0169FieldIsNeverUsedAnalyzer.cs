using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using System.Linq;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class CS0169FieldIsNeverUsedAnalyzer : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "CS0169FieldIsNeverUsedAnalyzer";
        const string Description = "CS0169: Field is never used";
        const string MessageFormat = "";
        const string Category = DiagnosticAnalyzerCategories.CompilerWarnings;

        static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, DiagnosticSeverity.Warning, true, "CS0169: Field is never used");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

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


        //		class GatherVisitor : GatherVisitorBase<CS0169FieldIsNeverUsedAnalyzer>
        //		{
        //			//readonly Stack<List<Tuple<VariableInitializer, IVariable>>> fieldStack = new Stack<List<Tuple<VariableInitializer, IVariable>>>();

        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base(semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			void Collect()
        ////			{
        ////				foreach (var varDecl in fieldStack.Peek()) {
        ////					AddDiagnosticAnalyzer(new CodeIssue(
        ////						varDecl.Item1.NameToken,
        ////						string.Format(ctx.TranslateString("The private field '{0}' is never assigned"), varDecl.Item2.Name)
        ////					));
        ////				}
        ////			}
        ////
        ////			public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
        ////			{	
        ////				var list = new List<Tuple<VariableInitializer, IVariable>>();
        ////				fieldStack.Push(list);
        ////
        ////				foreach (var fieldDeclaration in ConvertToConstantAnalyzer.CollectFields (this, typeDeclaration)) {
        ////					if (fieldDeclaration.HasModifier(Modifiers.Const))
        ////						continue;
        ////					if (fieldDeclaration.HasModifier(Modifiers.Public) || fieldDeclaration.HasModifier(Modifiers.Protected) || fieldDeclaration.HasModifier(Modifiers.Internal))
        ////						continue;
        ////					if (fieldDeclaration.Variables.Count() > 1)
        ////						continue;
        ////					var variable = fieldDeclaration.Variables.First();
        ////					if (!variable.Initializer.IsNull)
        ////						continue;
        ////					var rr = ctx.Resolve(fieldDeclaration.ReturnType);
        ////					if (rr.Type.IsReferenceType == false) {
        ////						// Value type:
        ////						var def = rr.Type.GetDefinition();
        ////						if (def != null && def.KnownTypeCode == KnownTypeCode.None) {
        ////							// user-defined value type -- might be mutable
        ////							continue;
        ////						} else if (ctx.Resolve (variable.Initializer).IsCompileTimeConstant) {
        ////							// handled by ConvertToConstantIssue
        ////							continue;
        ////						}
        ////					}
        ////
        ////					var mr = ctx.Resolve(variable) as MemberResolveResult;
        ////					if (mr == null || !(mr.Member is IVariable))
        ////						continue;
        ////					list.Add(Tuple.Create(variable, (IVariable)mr.Member)); 
        ////				}
        ////				base.VisitTypeDeclaration(typeDeclaration);
        ////				Collect();
        ////				fieldStack.Pop();
        ////			}
        ////
        ////			public override void VisitBlockStatement(BlockStatement blockStatement)
        ////			{
        ////				var assignmentAnalysis = new ConvertToConstantAnalyzer.VariableUsageAnalyzation (ctx);
        ////				var newVars = new List<Tuple<VariableInitializer, IVariable>>();
        ////				blockStatement.AcceptVisitor(assignmentAnalysis); 
        ////				foreach (var variable in fieldStack.Pop()) {
        ////					if (assignmentAnalysis.GetStatus(variable.Item2) == VariableState.Changed)
        ////						continue;
        ////					newVars.Add(variable);
        ////				}
        ////				fieldStack.Push(newVars);
        ////			}
        //		}
    }

    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    [NotPortedYet]
    public class CS0169FieldIsNeverUsedFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CS0169FieldIsNeverUsedAnalyzer.DiagnosticId);
            }
        }

        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public async override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var cancellationToken = context.CancellationToken;
            var span = context.Span;
            var diagnostics = context.Diagnostics;
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var diagnostic = diagnostics.First();
            var node = root.FindNode(context.Span);
            //if (!node.IsKind(SyntaxKind.BaseList))
            //	continue;
            var newRoot = root.RemoveNode(node, SyntaxRemoveOptions.KeepNoTrivia);
            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, diagnostic.GetMessage(), document.WithSyntaxRoot(newRoot)), diagnostic);
        }
    }
}