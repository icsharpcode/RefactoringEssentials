using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Create indexer")]
    [NotPortedYet]
    public class CreateIndexerAction : CodeRefactoringProvider
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
        //			var indexer = context.GetNode<IndexerExpression>();
        //			if (indexer == null)
        //				yield break;
        //			if (!(context.Resolve(indexer).IsError))
        //				yield break;
        //
        //			var state = context.GetResolverStateBefore(indexer);
        //			if (state.CurrentTypeDefinition == null)
        //				yield break;
        //			var guessedType = TypeGuessing.GuessAstType(context, indexer);
        //
        //			bool createInOtherType = false;
        //			ResolveResult targetResolveResult = null;
        //			targetResolveResult = context.Resolve(indexer.Target);
        //			createInOtherType = !state.CurrentTypeDefinition.Equals(targetResolveResult.Type.GetDefinition());
        //
        //			bool isStatic;
        //			if (createInOtherType) {
        //				if (targetResolveResult.Type.GetDefinition() == null || targetResolveResult.Type.GetDefinition().Region.IsEmpty)
        //					yield break;
        //				isStatic = targetResolveResult is TypeResolveResult;
        //				if (isStatic && targetResolveResult.Type.Kind == TypeKind.Interface || targetResolveResult.Type.Kind == TypeKind.Enum)
        //					yield break;
        //			} else {
        //				isStatic = indexer.Target is IdentifierExpression && state.CurrentMember.IsStatic;
        //			}
        //
        //			yield return new CodeAction(context.TranslateString("Create indexer"), script => {
        //				var decl = new IndexerDeclaration() {
        //					ReturnType = guessedType,
        //					Getter = new Accessor() {
        //						Body = new BlockStatement() {
        //							new ThrowStatement(new ObjectCreateExpression(context.CreateShortType("System", "NotImplementedException")))
        //						}
        //					},
        //					Setter = new Accessor() {
        //						Body = new BlockStatement() {
        //							new ThrowStatement(new ObjectCreateExpression(context.CreateShortType("System", "NotImplementedException")))
        //						}
        //					},
        //				};
        //				decl.Parameters.AddRange(CreateMethodDeclarationAction.GenerateParameters(context, indexer.Arguments));
        //				if (isStatic)
        //					decl.Modifiers |= Modifiers.Static;
        //				
        //				if (createInOtherType) {
        //					if (targetResolveResult.Type.Kind == TypeKind.Interface) {
        //						decl.Getter.Body = null;
        //						decl.Setter.Body = null;
        //						decl.Modifiers = Modifiers.None;
        //					} else {
        //						decl.Modifiers |= Modifiers.Public;
        //					}
        //
        //					script.InsertWithCursor(context.TranslateString("Create indexer"), targetResolveResult.Type.GetDefinition(), (s, c) => decl);
        //					return;
        //				}
        //
        //				script.InsertWithCursor(context.TranslateString("Create indexer"), Script.InsertPosition.Before, decl);
        //			}, indexer) { Severity = RefactoringEssentials.Refactoring.Severity.Error };
        //		}

    }
}