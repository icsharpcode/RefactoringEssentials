using System.Threading;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Create overload without parameter")]
    [NotPortedYet]
    public class CreateOverloadWithoutParameterAction : SpecializedCodeRefactoringProvider<ParameterSyntax>
    {
        protected override IEnumerable<CodeAction> GetActions(Document document, SemanticModel semanticModel, SyntaxNode root, TextSpan span, ParameterSyntax node, CancellationToken cancellationToken)
        {
            yield break;
        }
        //		protected override CodeAction GetAction (SemanticModel context, ParameterDeclaration node)
        //		{
        //			if (!node.DefaultExpression.IsNull)
        //				return null;
        //			if (node.ParameterModifier == ParameterModifier.This || node.ParameterModifier == ParameterModifier.Params)
        //				return null;
        //			if (!node.NameToken.Contains(context.Location))
        //				return null;
        //
        //			var methodDecl = node.Parent as MethodDeclaration;
        //			if (methodDecl == null)
        //				return null;
        //
        //			// explicit implementation
        //			if (!methodDecl.PrivateImplementationType.IsNull)
        //				return null;
        //
        //			// find existing method
        //			var method = (IMethod)((MemberResolveResult)context.Resolve (methodDecl)).Member;
        //			var parameters = new List<IParameter> (method.Parameters.Where (param => param.Name != node.Name));
        //			if (method.DeclaringType.GetMethods (
        //				m => m.Name == method.Name && m.TypeParameters.Count == method.TypeParameters.Count)
        //				.Any (m => ParameterListComparer.Instance.Equals (m.Parameters, parameters)))
        //				return null;
        //			
        //			return new CodeAction (context.TranslateString ("Create overload without parameter"),
        //				script =>
        //				{
        //					var defaultExpr = GetDefaultValueExpression (context, node.Type);
        //
        //					var body = new BlockStatement ();
        //					Expression argExpr;
        //					if (node.ParameterModifier == ParameterModifier.Ref) {
        //						body.Add (new VariableDeclarationStatement (node.Type.Clone (), node.Name, defaultExpr));
        //						argExpr = GetArgumentExpression (node);
        //					} else if (node.ParameterModifier == ParameterModifier.Out) {
        //						body.Add (new VariableDeclarationStatement (node.Type.Clone (), node.Name));
        //						argExpr = GetArgumentExpression (node);
        //					} else {
        //						argExpr = defaultExpr;
        //					}
        //					body.Add (new InvocationExpression (new IdentifierExpression (methodDecl.Name),
        //						methodDecl.Parameters.Select (param => param == node ? argExpr : GetArgumentExpression(param))));
        //
        //					var decl = (MethodDeclaration)methodDecl.Clone ();
        //					decl.Parameters.Remove (decl.Parameters.First (param => param.Name == node.Name));
        //					decl.Body = body;
        //
        //					script
        //						.InsertWithCursor ("Create overload without parameter", Script.InsertPosition.Before, decl)
        //						.ContinueScript (() => script.Select(argExpr));
        //				}, node.NameToken); 
        //		}
        //
        //		static Expression GetArgumentExpression(ParameterDeclaration parameter)
        //		{
        //			var identifierExpr = new IdentifierExpression(parameter.Name);
        //			switch (parameter.ParameterModifier) {
        //				case ParameterModifier.Out:
        //					return new DirectionExpression (FieldDirection.Out, identifierExpr);
        //				case ParameterModifier.Ref:
        //					return new DirectionExpression (FieldDirection.Ref, identifierExpr);
        //			}
        //			return identifierExpr;
        //		}
        //
        //		static Expression GetDefaultValueExpression (SemanticModel context, AstType astType)
        //		{
        //			var type = context.ResolveType (astType);
        //
        //			// array
        //			if (type.Kind == TypeKind.Array)
        //				return new ObjectCreateExpression (astType.Clone ());
        //
        //			// enum
        //			if (type.Kind == TypeKind.Enum) {
        //				var members = type.GetMembers ().ToArray();
        //				if (members.Length == 0)
        //					return new DefaultValueExpression (astType.Clone ());
        //				return astType.Member(members[0].Name).Clone ();
        //			}
        //
        //			if ((type.IsReferenceType ?? false) || type.Kind == TypeKind.Dynamic)
        //				return new NullReferenceExpression ();
        //
        //			var typeDefinition = type.GetDefinition ();
        //			if (typeDefinition != null) {
        //				switch (typeDefinition.KnownTypeCode) {
        //					case KnownTypeCode.Boolean:
        //						return new PrimitiveExpression (false);
        //
        //					case KnownTypeCode.Char:
        //						return new PrimitiveExpression ('\0');
        //
        //					case KnownTypeCode.SByte:
        //					case KnownTypeCode.Byte:
        //					case KnownTypeCode.Int16:
        //					case KnownTypeCode.UInt16:
        //					case KnownTypeCode.Int32:
        //					case KnownTypeCode.UInt32:
        //					case KnownTypeCode.Int64:
        //					case KnownTypeCode.UInt64:
        //					case KnownTypeCode.Single:
        //					case KnownTypeCode.Double:
        //					case KnownTypeCode.Decimal:
        //						return new PrimitiveExpression (0);
        //
        //					case KnownTypeCode.NullableOfT:
        //						return new NullReferenceExpression ();
        //				}
        //				if (type.Kind == TypeKind.Struct)
        //					return new ObjectCreateExpression (astType.Clone ());
        //			}
        //			return new DefaultValueExpression (astType.Clone ());
        //		}
    }
}
