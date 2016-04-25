using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class OptionalParameterHierarchyMismatchAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.OptionalParameterHierarchyMismatchAnalyzerID,
            GettextCatalog.GetString("The value of an optional parameter in a method does not match the base method"),
            GettextCatalog.GetString("Optional parameter value {0} differs from base {1} '{2}'"),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.OptionalParameterHierarchyMismatchAnalyzerID)
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

        //		class GatherVisitor : GatherVisitorBase<OptionalParameterHierarchyMismatchAnalyzer>
        //		{
        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}
        ////
        ////			//Delegate declarations are not visited even though they can have optional
        ////			//parameters because they can not be overriden.
        ////
        ////			public override void VisitMethodDeclaration(MethodDeclaration methodDeclaration)
        ////			{
        ////				VisitParameterizedEntityDeclaration("method", methodDeclaration, methodDeclaration.Parameters);
        ////			}
        ////
        ////			void VisitParameterizedEntityDeclaration(string memberType, EntityDeclaration entityDeclaration, AstNodeCollection<ParameterDeclaration> parameters)
        ////			{
        ////				// Ignore explicit interface implementations (those should have no optional parameters as there can't be any direct calls) 
        ////				if (!entityDeclaration.GetChildByRole(EntityDeclaration.PrivateImplementationTypeRole).IsNull)
        ////					return;
        ////				//Override is not strictly necessary because methodDeclaration
        ////				//might still implement an interface member
        ////				var memberResolveResult = ctx.Resolve(entityDeclaration) as MemberResolveResult;
        ////				if (memberResolveResult == null) {
        ////					return;
        ////				}
        ////				var member = (IParameterizedMember)memberResolveResult.Member;
        ////				var baseMembers = InheritanceHelper.GetBaseMembers(member, true).ToList();
        ////				foreach (IParameterizedMember baseMember in baseMembers) {
        ////					if (baseMember.IsOverride || baseMember.DeclaringType.Kind == TypeKind.Interface)
        ////						continue;
        ////					CompareMethods(memberType, parameters, member, baseMember);
        ////					return;
        ////				}
        ////				// only check 1 interface method -> multiple interface implementations could lead to deault value conflicts
        ////				// possible other solutions: Skip the interface check entirely
        ////				var interfaceBaseMethods = baseMembers.Where(b => b.DeclaringType.Kind == TypeKind.Interface).ToList();
        ////				if (interfaceBaseMethods.Count == 1) {
        ////					foreach (IParameterizedMember baseMember in interfaceBaseMethods) {
        ////						if (baseMember.DeclaringType.Kind == TypeKind.Interface) {
        ////							CompareMethods(memberType, parameters, member, baseMember);
        ////						}
        ////					}
        ////				}
        ////			}
        ////
        ////			static Expression CreateDefaultValueExpression(BaseSemanticModel ctx, AstNode node, IType type, object constantValue)
        ////			{
        ////				var astBuilder = ctx.CreateTypeSystemAstBuilder(node);
        ////				return astBuilder.ConvertConstantValue(type, constantValue); 
        ////			}
        ////
        ////			void CompareMethods(string memberType, AstNodeCollection<ParameterDeclaration> parameters, IParameterizedMember overridenMethod, IParameterizedMember baseMethod)
        ////			{
        ////				var parameterEnumerator = parameters.GetEnumerator();
        ////				for (int parameterIndex = 0; parameterIndex < overridenMethod.Parameters.Count; parameterIndex++) {
        ////					parameterEnumerator.MoveNext();
        ////
        ////					var baseParameter = baseMethod.Parameters [parameterIndex];
        ////
        ////					var overridenParameter = overridenMethod.Parameters [parameterIndex];
        ////
        ////					string parameterName = overridenParameter.Name;
        ////					var parameterDeclaration = parameterEnumerator.Current;
        ////
        ////					if (overridenParameter.IsOptional) {
        ////						if (!baseParameter.IsOptional) {
        ////							AddDiagnosticAnalyzer(new CodeIssue(parameterDeclaration,
        ////							         string.Format(ctx.TranslateString("Optional parameter value {0} differs from base " + memberType + " '{1}'"), parameterName, baseMethod.DeclaringType.FullName),
        ////							         ctx.TranslateString("Remove parameter default value"),
        ////							         script => {
        ////								script.Remove(parameterDeclaration.AssignToken);
        ////								script.Remove(parameterDeclaration.DefaultExpression);
        ////								script.FormatText(parameterDeclaration);
        ////								}));
        ////						} else if (!object.Equals(overridenParameter.ConstantValue, baseParameter.ConstantValue)) {
        ////							AddDiagnosticAnalyzer(new CodeIssue(parameterDeclaration,
        ////							         string.Format(ctx.TranslateString("Optional parameter value {0} differs from base " + memberType + " '{1}'"), parameterName, baseMethod.DeclaringType.FullName),
        ////							         string.Format(ctx.TranslateString("Change default value to {0}"), baseParameter.ConstantValue),
        ////								script => script.Replace(parameterDeclaration.DefaultExpression, CreateDefaultValueExpression(ctx, parameterDeclaration, baseParameter.Type, baseParameter.ConstantValue))));
        ////						}
        ////					} else {
        ////						if (!baseParameter.IsOptional)
        ////							continue;
        ////						AddDiagnosticAnalyzer(new CodeIssue(parameterDeclaration,
        ////							string.Format(ctx.TranslateString("Parameter {0} has default value in base method '{1}'"), parameterName, baseMethod.FullName),
        ////							string.Format(ctx.TranslateString("Add default value from base '{0}'"), CreateDefaultValueExpression(ctx, parameterDeclaration, baseParameter.Type, baseParameter.ConstantValue)),
        ////							script => {
        ////								var newParameter = (ParameterDeclaration)parameterDeclaration.Clone();
        ////								newParameter.DefaultExpression = CreateDefaultValueExpression(ctx, parameterDeclaration, baseParameter.Type, baseParameter.ConstantValue);
        ////								script.Replace(parameterDeclaration, newParameter);
        ////							}
        ////						));
        ////					}
        ////				}
        ////			}
        ////
        ////			public override void VisitIndexerDeclaration(IndexerDeclaration indexerDeclaration)
        ////			{
        ////				VisitParameterizedEntityDeclaration("indexer", indexerDeclaration, indexerDeclaration.Parameters);
        ////			}
        ////
        ////			public override void VisitBlockStatement(BlockStatement blockStatement)
        ////			{
        ////				//No need to visit statements
        ////			}
        //		}
    }
}