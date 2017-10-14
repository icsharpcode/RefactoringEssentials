using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using System.Linq;

namespace RefactoringEssentials.CSharp.Diagnostics
{

    //	public class IsTypeCriterion : ITypeCriterion
    //	{
    //		IType isType;
    //
    //		public IsTypeCriterion(IType isType)
    //		{
    //			this.isType = isType;
    //		}
    //
    //		#region ITypeCriterion implementation
    //		public bool SatisfiedBy (IType type)
    //		{
    //			return isType == type ||
    //				type.GetAllBaseTypes().Any(t => t == isType);
    //		}
    //		#endregion
    //	}
    //
    //	public class IsArrayTypeCriterion : ITypeCriterion
    //	{
    //		#region ITypeCriterion implementation
    //
    //		public bool SatisfiedBy(IType type)
    //		{
    //			return type is ArrayType;
    //		}
    //
    //		#endregion
    //	}
    //
    //	public class HasMemberCriterion : ITypeCriterion
    //	{
    ////		IMember neededMember;
    //		IList<IMember> acceptableMembers;
    //
    //		public HasMemberCriterion(IMember neededMember)
    //		{
    ////			this.neededMember = neededMember;
    //
    //			if (neededMember.ImplementedInterfaceMembers.Any()) {
    //				acceptableMembers = neededMember.ImplementedInterfaceMembers.ToList();
    //			} else if (neededMember.IsOverride) {
    //				acceptableMembers = new List<IMember>();
    //				foreach (var member in InheritanceHelper.GetBaseMembers(neededMember, true)) {
    //					acceptableMembers.Add(member);
    //					if (member.IsShadowing)
    //						break;
    //				}
    //				acceptableMembers.Add(neededMember);
    //			} else {
    //				acceptableMembers = new List<IMember> { neededMember };
    //			}
    //		}
    //
    //		#region ITypeCriterion implementation
    //		public bool SatisfiedBy (IType type)
    //		{
    //			if (type == null)
    //				throw new ArgumentNullException("type");
    //
    //			var typeMembers = type.GetMembers();
    //			return typeMembers.Any(member => HasCommonMemberDeclaration(acceptableMembers, member));
    //		}
    //		#endregion
    //
    //		static bool HasCommonMemberDeclaration(IEnumerable<IMember> acceptableMembers, IMember member)
    //		{
    //			var implementedInterfaceMembers = member.MemberDefinition.ImplementedInterfaceMembers;
    //			if (implementedInterfaceMembers.Any()) {
    //				return ContainsAny(acceptableMembers, implementedInterfaceMembers);
    //			} else {
    //				return acceptableMembers.Contains(member/*				.MemberDefinition*/);
    //			}
    //		}
    //
    //		static bool ContainsAny<T>(IEnumerable<T> collection, IEnumerable<T> items)
    //		{
    //			foreach (var item in items) {
    //				if (collection.Contains(item))
    //					return true;
    //			}
    //			return false;
    //		}
    //	}

    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    public class ParameterCanBeDeclaredWithBaseTypeCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CSharpDiagnosticIDs.ParameterCanBeDeclaredWithBaseTypeAnalyzerID);
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
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var diagnostic = diagnostics.First();
            var node = root.FindNode(context.Span);
            //if (!node.IsKind(SyntaxKind.BaseList))
            //	continue;
            var newRoot = root.RemoveNode(node, SyntaxRemoveOptions.KeepNoTrivia);
            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, diagnostic.GetMessage(), document.WithSyntaxRoot(newRoot)), diagnostic);
        }
    }
}