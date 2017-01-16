using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    /// <summary>
    /// Type is either mentioned in the base type list of other part, or it is interface and appears as other's type base and contains no explicit implementation.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class RedundantExtendsListEntryAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantExtendsListEntryAnalyzerID,
            GettextCatalog.GetString("Type is either mentioned in the base type list of another part or in another base type"),
            "{0}",
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Hidden,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantExtendsListEntryAnalyzerID),
            customTags: DiagnosticCustomTags.Unnecessary
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
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
            //var node = nodeContext.Node as ;
            //diagnostic = Diagnostic.Create (descriptor, node.GetLocation ());
            //return true;
            return false;
        }

        //		class GatherVisitor : GatherVisitorBase<RedundantExtendsListEntryAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
        ////			{
        ////				if (typeDeclaration == null)
        ////					return;
        ////				
        ////				base.VisitTypeDeclaration(typeDeclaration);
        ////				
        ////				if (typeDeclaration.BaseTypes.Count == 0)
        ////					return;
        ////				
        ////				List<AstNode> redundantBase = new List<AstNode>();
        ////				var type = ctx.Resolve(typeDeclaration).Type;
        ////
        ////				if (typeDeclaration.HasModifier(Modifiers.Partial) && type.GetDefinition() != null) {
        ////					var parts = type.GetDefinition().Parts;
        ////					foreach (var node in typeDeclaration.BaseTypes) {
        ////						int count = 0;
        ////						foreach (var unresolvedTypeDefinition in parts) {
        ////							var baseTypes = unresolvedTypeDefinition.BaseTypes;
        ////							
        ////							if (baseTypes.Any(f => f.ToString().Equals(node.ToString()))) {
        ////								count++;
        ////								if (count > 1) {
        ////									if (!redundantBase.Contains(node))
        ////										redundantBase.Add(node);
        ////									break;
        ////								}
        ////							}
        ////						}
        ////					}
        ////				}
        ////				
        ////				var directBaseType = type.DirectBaseTypes.Where(f => f.Kind == TypeKind.Class);
        ////				if (directBaseType.Count() != 1)
        ////					return;
        ////				var members = type.GetMembers();
        ////				var memberDeclaration = typeDeclaration.Members;
        ////				var interfaceBase = typeDeclaration.BaseTypes.Where(delegate(AstType f) {
        ////					var resolveResult = ctx.Resolve(f);
        ////					if (resolveResult.IsError || resolveResult.Type.GetDefinition() == null)
        ////						return false;
        ////					return resolveResult.Type.GetDefinition().Kind == TypeKind.Interface;
        ////				});
        ////				foreach (var node in interfaceBase) {
        ////					if (directBaseType.Single().GetDefinition().GetAllBaseTypeDefinitions().Any(f => f.Name.Equals(node.ToString()))) {
        ////						bool flag = false;
        ////						foreach (var member in members) {
        ////							if (!memberDeclaration.Any(f => f.Name.Equals(member.Name))) {
        ////								continue;
        ////							}
        ////							if (
        ////								member.ImplementedInterfaceMembers.Any(
        ////								g => g.DeclaringType.Name.Equals(node.ToString()))) {
        ////								flag = true;
        ////								break;
        ////							}
        ////						}
        ////						if (!flag) {
        ////							if (!redundantBase.Contains(node))
        ////								redundantBase.Add(node);
        ////						}
        ////					}			
        ////				}
        ////				foreach (var node in redundantBase) {
        ////					var nodeType = ctx.Resolve(node).Type;
        ////					var issueText = nodeType.Kind == TypeKind.Interface ?
        //			//						ctx.TranslateString("") :
        ////						ctx.TranslateString("");
        ////
        ////					AddDiagnosticAnalyzer(new CodeIssue(
        ////						node,
        ////						string.Format(issueText, nodeType.Name), 
        ////						new CodeAction (
        ////							ctx.TranslateString(""),
        ////							script => {
        ////								if (typeDeclaration.GetCSharpNodeBefore(node).ToString().Equals(":")) {
        ////									if (node.GetNextNode().Role != Roles.BaseType) {
        ////										script.Remove(typeDeclaration.GetCSharpNodeBefore(node));
        ////									}
        ////								}
        ////								if (typeDeclaration.GetCSharpNodeBefore(node).ToString().Equals(",")) {
        ////									script.Remove(typeDeclaration.GetCSharpNodeBefore(node));
        ////								}
        ////								script.Remove(node);
        ////							},
        ////						node)
        ////					) { IssueMarker = IssueMarker.GrayOut });
        ////				}
        ////			}

        //		}
    }
}