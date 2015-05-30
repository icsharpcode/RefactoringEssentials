using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Generate getter")]
    [NotPortedYet]
    public class GenerateGetterAction : CodeRefactoringProvider
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
        //			var initializer = GetVariableInitializer(context);
        //			if (initializer == null || !initializer.NameToken.Contains(context.Location)) {
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
        //			if (field == null) {
        //				yield break;
        //			}
        //			
        //			yield return new CodeAction(context.TranslateString("Create getter"), script => {
        //				script.InsertWithCursor(
        //					context.TranslateString("Create getter"),
        //					Script.InsertPosition.After,
        //					GeneratePropertyDeclaration(context, field, initializer));
        //			}, initializer);
        //		}
        //		
        //		static PropertyDeclaration GeneratePropertyDeclaration (SemanticModel context, FieldDeclaration field, VariableInitializer initializer)
        //		{
        //			var mod = RefactoringEssentials.Modifiers.Public;
        //			if (field.HasModifier (RefactoringEssentials.Modifiers.Static))
        //				mod |= RefactoringEssentials.Modifiers.Static;
        //			
        //			return new PropertyDeclaration () {
        //				Modifiers = mod,
        //				Name = context.GetNameProposal (initializer.Name, false),
        //				ReturnType = field.ReturnType.Clone (),
        //				Getter = new Accessor () {
        //					Body = new BlockStatement () {
        //						new ReturnStatement (new IdentifierExpression (initializer.Name))
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
        //		VariableInitializer GetVariableInitializer (SemanticModel context)
        //		{
        //			return context.GetNode<VariableInitializer> ();
        //		}
    }
}

