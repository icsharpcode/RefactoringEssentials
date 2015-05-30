using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Generate property")]
    [NotPortedYet]
    public class GeneratePropertyAction : CodeRefactoringProvider
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
        //			var initializer = context.GetNode<VariableInitializer>();
        //			if (initializer == null || !initializer.NameToken.Contains(context.Location.Line, context.Location.Column)) {
        //				yield break;
        //			}
        //			var type = initializer.Parent.Parent as TypeDeclaration;
        //			if (type == null) {
        //				yield break;
        //			}
        //			foreach (var member in type.Members) {
        //				if (member is PropertyDeclaration && ContainsGetter((PropertyDeclaration)member, initializer)) {
        //					yield break;
        //				}
        //			}
        //			var field = initializer.Parent as FieldDeclaration;
        //			if (field == null || field.HasModifier(Modifiers.Readonly) || field.HasModifier(Modifiers.Const)) {
        //				yield break;
        //			}
        //			var resolveResult = context.Resolve(initializer) as MemberResolveResult;
        //			if (resolveResult == null)
        //				yield break;
        //			yield return new CodeAction(context.TranslateString("Create property"), script => {
        //				var fieldName = context.GetNameProposal(initializer.Name, true);
        //				if (initializer.Name == context.GetNameProposal(initializer.Name, false)) {
        //					script.Rename(resolveResult.Member, fieldName);
        //				}
        //				script.InsertWithCursor(
        //					context.TranslateString("Create property"),
        //					Script.InsertPosition.After, GeneratePropertyDeclaration(context, field, fieldName));
        //			}, initializer);
        //		}
        //		
        //		static PropertyDeclaration GeneratePropertyDeclaration (SemanticModel context, FieldDeclaration field, string fieldName)
        //		{
        //			var mod = RefactoringEssentials.Modifiers.Public;
        //			if (field.HasModifier (RefactoringEssentials.Modifiers.Static))
        //				mod |= RefactoringEssentials.Modifiers.Static;
        //			
        //			return new PropertyDeclaration () {
        //				Modifiers = mod,
        //				Name = context.GetNameProposal (fieldName, false),
        //				ReturnType = field.ReturnType.Clone (),
        //				Getter = new Accessor {
        //					Body = new BlockStatement {
        //						new ReturnStatement (new IdentifierExpression (fieldName))
        //					}
        //				},
        //				Setter = new Accessor {
        //					Body = new BlockStatement {
        //						new AssignmentExpression (new IdentifierExpression (fieldName), new IdentifierExpression ("value"))
        //					}
        //				}
        //			};
        //		}
        //		
        //		bool ContainsGetter (PropertyDeclaration property, VariableInitializer initializer)
        //		{
        //			if (property.Getter.IsNull || property.Getter.Body.Statements.Count () != 1)
        //				return false;
        //			var ret = property.Getter.Body.Statements.Single () as ReturnStatement;
        //			if (ret == null)
        //				return false;
        //			return ret.Expression.IsMatch (new IdentifierExpression (initializer.Name)) || 
        //				ret.Expression.IsMatch (new MemberReferenceExpression (new ThisReferenceExpression (), initializer.Name));
        //		}
        //		

    }
}

