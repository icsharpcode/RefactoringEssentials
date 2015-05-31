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
    public class UnassignedReadonlyFieldAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.UnassignedReadonlyFieldAnalyzerID,
            GettextCatalog.GetString("Unassigned readonly field"),
            GettextCatalog.GetString("Readonly field is never assigned"),
            DiagnosticAnalyzerCategories.CompilerWarnings,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.UnassignedReadonlyFieldAnalyzerID)
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

        //		class GatherVisitor : GatherVisitorBase<UnassignedReadonlyFieldAnalyzer>
        //		{
        //			//readonly Stack<List<Tuple<VariableInitializer, IVariable>>> fieldStack = new Stack<List<Tuple<VariableInitializer, IVariable>>>();

        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			void Collect()
        ////			{
        ////				foreach (var varDecl in fieldStack.Peek()) {
        ////					var resolveResult = ctx.Resolve(varDecl.Item1) as MemberResolveResult;
        ////					if (resolveResult == null || resolveResult.IsError)
        ////						continue;
        ////					AddDiagnosticAnalyzer(new CodeIssue(
        ////						varDecl.Item1.NameToken,
        ////						string.Format(ctx.TranslateString(""), varDecl.Item1.Name),
        ////						ctx.TranslateString(""),
        ////						script => {
        ////						script.InsertWithCursor(
        ////							ctx.TranslateString("Create constructor"),
        ////							resolveResult.Member.DeclaringTypeDefinition,
        ////							(s, c) => {
        ////							return new ConstructorDeclaration {
        ////								Name = resolveResult.Member.DeclaringTypeDefinition.Name,
        ////								Modifiers = Modifiers.Public,
        ////								Body = new BlockStatement {
        ////										new AssignmentExpression(
        ////											new MemberReferenceExpression(new ThisReferenceExpression(), varDecl.Item1.Name),
        ////											new IdentifierExpression(varDecl.Item1.Name)
        ////										)
        ////								},
        ////								Parameters = {
        ////									new ParameterDeclaration(c.CreateShortType(resolveResult.Type), varDecl.Item1.Name)
        ////								}
        ////							};
        ////						}
        ////						);
        ////					}
        ////					));
        ////				}
        ////			}
        ////
        ////			public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
        ////			{
        ////				var list = new List<Tuple<VariableInitializer, IVariable>>();
        ////				fieldStack.Push(list);
        ////				foreach (var fieldDeclaration in ConvertToConstantAnalyzer.CollectFields(this, typeDeclaration)) {
        ////					if (!fieldDeclaration.HasModifier(Modifiers.Readonly))
        ////						continue;
        //////					var rr = ctx.Resolve(fieldDeclaration.ReturnType);
        ////				
        ////					if (fieldDeclaration.Variables.Count() > 1)
        ////						continue;
        ////					if (!fieldDeclaration.Variables.First().Initializer.IsNull)
        ////						continue;
        ////					var variable = fieldDeclaration.Variables.First();
        ////					var mr = ctx.Resolve(variable) as MemberResolveResult;
        ////					if (mr == null)
        ////						continue;
        ////					list.Add(Tuple.Create(variable, mr.Member as IVariable)); 
        ////				}
        ////				base.VisitTypeDeclaration(typeDeclaration);
        ////				Collect();
        ////				fieldStack.Pop();
        ////			}
        ////
        ////			public override void VisitBlockStatement(BlockStatement blockStatement)
        ////			{
        ////				var assignmentAnalysis = new ConvertToConstantAnalyzer.VariableUsageAnalyzation(ctx);
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
    public class UnassignedReadonlyFieldFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.UnassignedReadonlyFieldAnalyzerID);
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
            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, "Initialize field from constructor parameter", document.WithSyntaxRoot(newRoot)), diagnostic);
        }
    }
}