using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Create delegate")]
    [NotPortedYet]
    public class CreateDelegateAction : CodeRefactoringProvider
    {
        public override Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            //var document = context.Document;
            //var span = context.Span;
            //var cancellationToken = context.CancellationToken;
            //var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            //if (model.IsFromGeneratedCode(cancellationToken))
            //	return;
            //var root = await model.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            return Task.FromResult(0);
        }
        //		public async Task ComputeRefactoringsAsync(Document document, TextSpan span, CancellationToken cancellationToken)
        //		{
        //			var simpleType = context.GetNode<SimpleType>();
        //			if (simpleType != null && (simpleType.Parent is EventDeclaration || simpleType.Parent is CustomEventDeclaration)) 
        //				return GetActions(context, simpleType);
        //
        //			return;
        //		}
        //
        //		static IEnumerable<CodeAction> GetActions(SemanticModel context, SimpleType node)
        //		{
        //			var resolveResult = context.Resolve(node) as UnknownIdentifierResolveResult;
        //			if (resolveResult == null)
        //				yield break;
        //
        //			yield return new CodeAction(context.TranslateString("Create delegate"), script => {
        //				script.CreateNewType(CreateType(context,  node));
        //			}, node);
        //
        //		}
        //
        //		static DelegateDeclaration CreateType(SemanticModel context, SimpleType simpleType)
        //		{
        //			var result = new DelegateDeclaration() {
        //				Name = simpleType.Identifier,
        //				Modifiers = ((EntityDeclaration)simpleType.Parent).Modifiers,
        //				ReturnType = new PrimitiveType("void"),
        //				Parameters = {
        //					new ParameterDeclaration(new PrimitiveType("object"), "sender"),
        //					new ParameterDeclaration(context.CreateShortType("System", "EventArgs"), "e")
        //				}
        //			};
        //			return result;
        //		}
    }
}