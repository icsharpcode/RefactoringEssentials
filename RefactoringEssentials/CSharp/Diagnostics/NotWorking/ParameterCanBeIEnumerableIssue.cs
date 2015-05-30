using System.Collections.Generic;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.Semantics;
using System.Linq;

namespace ICSharpCode.NRefactory6.CSharp.Diagnostics
{
    // Does this even make sense ? We've a parameter can be demoted issue which does mostly the same.
    //	[IssueDescription("A parameter can IEnumerable/ICollection/IList<T>",
    //	                  Description = "Finds parameters that can be demoted to a generic list.",
    //	                  Category = IssueCategories.Opportunities,
    //	                  Severity = Severity.Suggestion
    //	                  )]
    public class ParameterCanBeIEnumerableDiagnosticAnalyzer : GatherVisitorCodeIssueProvider
    {
        readonly bool tryResolve;

        public ParameterCanBeIEnumerableIssue() : this (true)
		{
        }

        public ParameterCanBeIEnumerableIssue(bool tryResolve)
        {
            this.tryResolve = tryResolve;
        }

        #region ICodeIssueProvider implementation
        protected override IGatherVisitor CreateVisitor(BaseSemanticModel context)
        {
            return new GatherVisitor(context, tryResolve);
        }
        #endregion

        class GatherVisitor : GatherVisitorBase<ParameterCanBeIEnumerableIssue>
        {
            bool tryResolve;

            public GatherVisitor(BaseSemanticModel context, bool tryResolve) : base(context)
            {
                this.tryResolve = tryResolve;
            }

            public override void VisitMethodDeclaration(MethodDeclaration methodDeclaration)
            {
                methodDeclaration.Attributes.AcceptVisitor(this);
                if (HasEntryPointSignature(methodDeclaration))
                    return;
                var eligibleParameters = methodDeclaration.Parameters
                    .Where(p => p.ParameterModifier != ParameterModifier.Out && p.ParameterModifier != ParameterModifier.Ref)
                        .ToList();
                if (eligibleParameters.Count == 0)
                    return;
                var declarationResolveResult = ctx.Resolve(methodDeclaration) as MemberResolveResult;
                if (declarationResolveResult == null)
                    return;
                var member = declarationResolveResult.Member;
                if (member.IsOverride || member.IsOverridable || member.ImplementedInterfaceMembers.Any())
                    return;

                var collector = new TypeCriteriaCollector(ctx);
                methodDeclaration.AcceptVisitor(collector);

                foreach (var parameter in eligibleParameters)
                {
                    ProcessParameter(parameter, methodDeclaration.Body, collector);
                }
            }

            bool HasEntryPointSignature(MethodDeclaration methodDeclaration)
            {
                if (!methodDeclaration.Modifiers.HasFlag(Modifiers.Static))
                    return false;
                var returnType = ctx.Resolve(methodDeclaration.ReturnType).Type;
                if (!returnType.IsKnownType(KnownTypeCode.Int32) && !returnType.IsKnownType(KnownTypeCode.Void))
                    return false;
                var parameterCount = methodDeclaration.Parameters.Count;
                if (parameterCount == 0)
                    return true;
                if (parameterCount != 1)
                    return false;
                var parameterType = ctx.Resolve(methodDeclaration.Parameters.First()).Type as ArrayType;
                return parameterType != null && parameterType.ElementType.IsKnownType(KnownTypeCode.String);
            }

            void ProcessParameter(ParameterDeclaration parameter, AstNode rootResolutionNode, TypeCriteriaCollector collector)
            {
                var localResolveResult = ctx.Resolve(parameter) as LocalResolveResult;
                if (localResolveResult == null)
                    return;
                var variable = localResolveResult.Variable;
                var typeKind = variable.Type.Kind;
                if (!(typeKind == TypeKind.Class ||
                      typeKind == TypeKind.Struct ||
                      typeKind == TypeKind.Interface ||
                      typeKind == TypeKind.Array) ||
                    !collector.UsedVariables.Contains(variable))
                {
                    return;
                }

                var candidateTypes = localResolveResult.Type.GetAllBaseTypes()
                    .Where(t => t.IsParameterized)
                    .ToList();

                var validTypes =
                    (from type in candidateTypes
                     where !tryResolve || ParameterCanBeDeclaredWithBaseTypeIssue.TypeChangeResolvesCorrectly(ctx, parameter, rootResolutionNode, type)
                     select type).ToList();
                if (!validTypes.Any()) return;

                var foundType = validTypes.FirstOrDefault();
                if (foundType == null)
                    return;

                AddDiagnosticAnalyzer(new CodeIssue(parameter.NameToken, string.Format(ctx.TranslateString("Parameter can be {0}"),
                                                            foundType.Name), string.Format(ctx.TranslateString("Parameter can be {0}"),
                                              foundType.Name),
                    script => script.Replace(parameter.Type, CreateShortType(ctx, foundType, parameter))));
            }

            public static AstType CreateShortType(BaseSemanticModel SemanticModel, IType expressionType, AstNode node)
            {

                var csResolver = SemanticModel.Resolver.GetResolverStateBefore(node);
                var builder = new TypeSystemAstBuilder(csResolver);
                return builder.ConvertType(expressionType);
            }
        }
    }
}
