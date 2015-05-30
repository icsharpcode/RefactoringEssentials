using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Extract anonymous method")]
    [NotPortedYet]
    public class ExtractAnonymousMethodAction : CodeRefactoringProvider
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
        //		public override IEnumerable<CodeAction> GetActions (SemanticModel context)
        //		{
        //			// lambda
        //			var lambda = context.GetNode<LambdaExpression> ();
        //			if (lambda != null && lambda.ArrowToken.Contains(context.Location)) {
        //				if (ContainsLocalReferences (context, lambda, lambda.Body))
        //					yield break;
        //
        //				bool noReturn = false;
        //				BlockStatement body;
        //				if (lambda.Body is BlockStatement) {
        //					body = (BlockStatement)lambda.Body.Clone ();
        //				} else {
        //					if (!(lambda.Body is Expression))
        //						yield break;
        //					body = new BlockStatement ();
        //
        //					var type = LambdaHelper.GetLambdaReturnType (context, lambda);
        //					if (type == null || type.ReflectionName == "System.Void") {
        //						noReturn = true;
        //						body.Add ((Expression)lambda.Body.Clone ());
        //					} else {
        //						body.Add (new ReturnStatement ((Expression)lambda.Body.Clone ()));
        //					}
        //				}
        //				var method = GetMethod (context, (LambdaResolveResult)context.Resolve (lambda), body, noReturn);
        //				yield return GetAction (context, lambda, method);
        //			}
        //
        //			// anonymous method
        //			var anonymousMethod = context.GetNode<AnonymousMethodExpression> ();
        //			if (anonymousMethod != null && anonymousMethod.DelegateToken.Contains(context.Location)) {
        //				if (ContainsLocalReferences (context, anonymousMethod, anonymousMethod.Body))
        //					yield break;
        //
        //				var method = GetMethod (context, (LambdaResolveResult)context.Resolve (anonymousMethod), 
        //										(BlockStatement)anonymousMethod.Body.Clone ());
        //				yield return GetAction (context, anonymousMethod, method);
        //			}
        //		}
        //
        //		CodeAction GetAction (SemanticModel context, AstNode node, MethodDeclaration method)
        //		{
        //			return new CodeAction (context.TranslateString ("Extract anonymous method"),
        //				script =>
        //				{
        //					var identifier = new IdentifierExpression ("Method");
        //					script.Replace (node, identifier);
        //					script.InsertBefore (node.GetParent<EntityDeclaration> (), method);
        //					script.Link (method.NameToken, identifier);
        //				}, method.NameToken);
        //		}
        //
        //		static MethodDeclaration GetMethod (SemanticModel context, LambdaResolveResult lambda, BlockStatement body,
        //			bool noReturnValue = false)
        //		{
        //			var method = new MethodDeclaration { Name = "Method" };
        //
        //			if (noReturnValue) {
        //				method.ReturnType = new PrimitiveType ("void"); 
        //			} else {
        //				var type = lambda.ReturnType;
        //				method.ReturnType = type.Kind == TypeKind.Unknown ? new PrimitiveType ("void") : context.CreateShortType (type);
        //			}
        //
        //			foreach (var param in lambda.Parameters)
        //				method.Parameters.Add (new ParameterDeclaration (context.CreateShortType (param.Type), param.Name));
        //
        //			method.Body = body;
        //			if (lambda.IsAsync)
        //				method.Modifiers |= Modifiers.Async;
        //
        //			return method;
        //		}
        //
        //		static bool ContainsLocalReferences (SemanticModel context, AstNode expr, AstNode body)
        //		{
        //			var visitor = new ExtractMethod.VariableLookupVisitor (context);
        //			body.AcceptVisitor (visitor);
        //			return visitor.UsedVariables.Any (variable => !expr.Contains (variable.Region.Begin));
        //		}
    }
}
