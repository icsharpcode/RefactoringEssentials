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
    public class ProhibitedModifiersAnalyzer : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "ProhibitedModifiersAnalyzer";
        const string Description = "Checks for prohibited modifiers";
        const string MessageFormat = "Static constructors can't have any other modifier";
        const string Category = DiagnosticAnalyzerCategories.CompilerErrors;

        static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, DiagnosticSeverity.Error, true, "Checks for prohibited modifiers");

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

        //		class GatherVisitor : GatherVisitorBase<ProhibitedModifiersAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			readonly Stack<TypeDeclaration> curType = new Stack<TypeDeclaration> ();
        ////			public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
        ////			{
        ////				curType.Push(typeDeclaration); 
        ////				base.VisitTypeDeclaration(typeDeclaration);
        ////				curType.Pop();
        ////			}
        ////
        ////			void AddStaticRequiredError (EntityDeclaration entity, AstNode node)
        ////			{
        ////				AddDiagnosticAnalyzer(new CodeIssue(
        ////					node,
        ////					ctx.TranslateString("'static' modifier is required inside a static class"),
        ////					ctx.TranslateString("Add 'static' modifier"), 
        ////					s => {
        ////						s.ChangeModifier(entity, (entity.Modifiers & ~(Modifiers.Virtual | Modifiers.Override)) | Modifiers.Static);
        ////					}
        ////				));
        ////			}
        ////
        ////			void CheckStaticRequired(EntityDeclaration entity)
        ////			{
        ////				if (!curType.Peek().HasModifier(Modifiers.Static) || entity.HasModifier(Modifiers.Static) || entity.HasModifier(Modifiers.Const))
        ////					return;
        ////				var fd = entity as FieldDeclaration;
        ////				if (fd != null) {
        ////					foreach (var init in fd.Variables)
        ////						AddStaticRequiredError(entity, init.NameToken);
        ////					return;
        ////				}
        ////
        ////				var ed = entity as EventDeclaration;
        ////				if (ed != null) {
        ////					foreach (var init in ed.Variables)
        ////						AddStaticRequiredError(entity, init.NameToken);
        ////					return;
        ////				}
        ////
        ////				AddStaticRequiredError(entity, entity.NameToken);
        ////			}
        ////
        ////			void CheckVirtual(EntityDeclaration entity)
        ////			{
        ////				if (!curType.Peek().HasModifier(Modifiers.Static) && !curType.Peek().HasModifier(Modifiers.Sealed) && entity.HasModifier(Modifiers.Virtual)) {
        ////					if (!entity.HasModifier(Modifiers.Public) && !entity.HasModifier(Modifiers.Protected)  && !entity.HasModifier(Modifiers.Internal)) {
        ////						AddDiagnosticAnalyzer(new CodeIssue(
        ////							entity.NameToken,
        ////							ctx.TranslateString("'virtual' members can't be private")
        ////						));
        ////						return;
        ////					}
        ////
        ////				}
        ////
        ////				if (entity.HasModifier(Modifiers.Sealed) && !entity.HasModifier(Modifiers.Override)) {
        ////					AddDiagnosticAnalyzer(new CodeIssue(
        ////						entity.ModifierTokens.First(t => t.Modifier == Modifiers.Sealed),
        ////						ctx.TranslateString("'sealed' modifier is not usable without override"),
        ////						ctx.TranslateString("Remove 'sealed' modifier"), 
        ////						s => {
        ////							s.ChangeModifier(entity, entity.Modifiers & ~Modifiers.Sealed);
        ////						}
        ////					));
        ////
        ////				}
        ////
        ////				if (!curType.Peek().HasModifier(Modifiers.Sealed) || !entity.HasModifier(Modifiers.Virtual))
        ////					return;
        ////				AddDiagnosticAnalyzer(new CodeIssue(
        ////					entity.ModifierTokens.First(t => t.Modifier == Modifiers.Virtual),
        ////					ctx.TranslateString("'virtual' modifier is not usable in a sealed class"),
        ////					ctx.TranslateString("Remove 'virtual' modifier"), 
        ////					s => s.ChangeModifier(entity, entity.Modifiers & ~Modifiers.Virtual)
        ////				));
        ////			}
        ////
        ////			public override void VisitMethodDeclaration(MethodDeclaration methodDeclaration)
        ////			{
        ////				CheckStaticRequired(methodDeclaration);
        ////				CheckVirtual(methodDeclaration);
        ////				if (methodDeclaration.IsExtensionMethod) {
        ////					var parent = methodDeclaration.Parent as TypeDeclaration;
        ////					if (parent != null && !parent.HasModifier(Modifiers.Static)) {
        ////						var actions = new List<CodeAction>();
        ////						var token = methodDeclaration.Parameters.First().FirstChild;
        ////						actions.Add(new CodeAction(
        ////							ctx.TranslateString("Make class 'static'"),
        ////							s => s.ChangeModifier(parent, (parent.Modifiers & ~Modifiers.Sealed) | Modifiers.Static),
        ////							token
        ////							));
        ////
        ////						actions.Add(new CodeAction(
        ////							ctx.TranslateString("Remove 'this'"),
        ////							s => s.ChangeModifier(methodDeclaration.Parameters.First(), ParameterModifier.None),
        ////							token
        ////						));
        ////
        ////						AddDiagnosticAnalyzer(new CodeIssue(
        ////							token,
        ////							ctx.TranslateString("Extension methods are only allowed in static classes"),
        ////							actions
        ////						));
        ////					}
        ////				}
        ////
        ////				base.VisitMethodDeclaration(methodDeclaration);
        ////			}
        ////
        ////			public override void VisitFieldDeclaration(FieldDeclaration fieldDeclaration)
        ////			{
        ////				CheckStaticRequired(fieldDeclaration);
        ////				base.VisitFieldDeclaration(fieldDeclaration);
        ////			}
        ////
        ////			public override void VisitFixedFieldDeclaration(FixedFieldDeclaration fixedFieldDeclaration)
        ////			{
        ////				CheckStaticRequired(fixedFieldDeclaration);
        ////				base.VisitFixedFieldDeclaration(fixedFieldDeclaration);
        ////			}
        ////
        ////			public override void VisitEventDeclaration(EventDeclaration eventDeclaration)
        ////			{
        ////				CheckStaticRequired(eventDeclaration);
        ////				base.VisitEventDeclaration(eventDeclaration);
        ////			}
        ////
        ////			public override void VisitCustomEventDeclaration(CustomEventDeclaration eventDeclaration)
        ////			{
        ////				CheckStaticRequired(eventDeclaration);
        ////				CheckVirtual(eventDeclaration);
        ////				base.VisitCustomEventDeclaration(eventDeclaration);
        ////			}
        ////
        ////			public override void VisitConstructorDeclaration(ConstructorDeclaration constructorDeclaration)
        ////			{
        ////				CheckStaticRequired(constructorDeclaration);
        ////
        ////				if (constructorDeclaration.HasModifier(Modifiers.Static)) {
        ////					if ((constructorDeclaration.Modifiers & Modifiers.Static) != 0) {
        ////						foreach (var mod in constructorDeclaration.ModifierTokens) {
        ////							if (mod.Modifier == Modifiers.Static)
        ////								continue;
        ////							AddDiagnosticAnalyzer(new CodeIssue(
        ////								mod,
        ////								ctx.TranslateString(""),
        ////								ctx.TranslateString(""), 
        ////								s => {
        ////									s.ChangeModifier(constructorDeclaration, Modifiers.Static);
        ////								}
        ////							));
        ////						}
        ////					}
        ////				}
        ////				base.VisitConstructorDeclaration(constructorDeclaration);
        ////			}
        ////
        ////			public override void VisitDestructorDeclaration(DestructorDeclaration destructorDeclaration)
        ////			{
        ////				base.VisitDestructorDeclaration(destructorDeclaration);
        ////			}
        ////
        ////			public override void VisitPropertyDeclaration(PropertyDeclaration propertyDeclaration)
        ////			{
        ////				CheckStaticRequired(propertyDeclaration);
        ////				CheckVirtual(propertyDeclaration);
        ////				base.VisitPropertyDeclaration(propertyDeclaration);
        ////			}
        ////
        ////			public override void VisitIndexerDeclaration(IndexerDeclaration indexerDeclaration)
        ////			{
        ////				CheckVirtual(indexerDeclaration);
        ////				base.VisitIndexerDeclaration(indexerDeclaration);
        ////			}
        ////
        ////			public override void VisitBlockStatement(BlockStatement blockStatement)
        ////			{
        ////				// SKIP
        ////			}
        //		}
    }

    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class ProhibitedModifiersFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(ProhibitedModifiersAnalyzer.DiagnosticId);
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
            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, "Remove prohibited modifier", document.WithSyntaxRoot(newRoot)), diagnostic);
        }
    }
}