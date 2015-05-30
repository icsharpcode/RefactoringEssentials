using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using System.Linq;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class CS0126ReturnMustBeFollowedByAnyExpression : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "CS0126ReturnMustBeFollowedByAnyExpression";
        const string Description = "Since 'function' doesn't return void, a return keyword must be followed by an object expression";
        const string MessageFormat = "";
        const string Category = DiagnosticAnalyzerCategories.CompilerErrors;

        static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, DiagnosticSeverity.Error, true, "CS0126: A method with return type cannot return without value.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
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
            if (nodeContext.IsFromGeneratedCode())
                return false;
            //var node = nodeContext.Node as ;
            //diagnostic = Diagnostic.Create (descriptor, node.GetLocation ());
            //return true;
            return false;
        }


        //		internal static IType GetRequestedReturnType (BaseSemanticModel ctx, AstNode returnStatement, out AstNode entityNode)
        //		{
        //			entityNode = returnStatement.GetParent(p => p is LambdaExpression || p is AnonymousMethodExpression || !(p is Accessor) && p is EntityDeclaration);
        //			if (entityNode == null)
        //				return SpecialType.UnknownType;
        //			if (entityNode is EntityDeclaration) {
        //				var rr = ctx.Resolve(entityNode) as MemberResolveResult;
        //				if (rr == null)
        //					return SpecialType.UnknownType;
        //				if (((EntityDeclaration)entityNode).HasModifier(Modifiers.Async))
        //					return TaskType.UnpackTask(ctx.Compilation, rr.Member.ReturnType);
        //				return rr.Member.ReturnType;
        //			}
        //			bool isAsync = false;
        //			if (entityNode is LambdaExpression)
        //				isAsync = ((LambdaExpression)entityNode).IsAsync;
        //			if (entityNode is AnonymousMethodExpression)
        //				isAsync = ((AnonymousMethodExpression)entityNode).IsAsync;
        //			foreach (var type in TypeGuessing.GetValidTypes(ctx.Resolver, entityNode)) {
        //				if (type.Kind != TypeKind.Delegate)
        //					continue;
        //				var invoke = type.GetDelegateInvokeMethod();
        //				if (invoke != null) {
        //					return isAsync ? TaskType.UnpackTask(ctx.Compilation, invoke.ReturnType) : invoke.ReturnType;
        //				}
        //			}
        //			return SpecialType.UnknownType;
        //		}


        //		class GatherVisitor : GatherVisitorBase<CS0126ReturnMustBeFollowedByAnyExpression>
        //		{
        //			//string currentMethodName;

        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base(semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			bool skip;
        ////
        ////			public override void VisitMethodDeclaration(MethodDeclaration methodDeclaration)
        ////			{
        ////				var primitiveType = methodDeclaration.ReturnType as PrimitiveType;
        ////				skip = (primitiveType != null && primitiveType.Keyword == "void");
        ////				currentMethodName = methodDeclaration.Name;
        ////				base.VisitMethodDeclaration(methodDeclaration);
        ////			}
        ////
        ////			public override void VisitConstructorDeclaration(ConstructorDeclaration constructorDeclaration)
        ////			{
        ////				currentMethodName = constructorDeclaration.Name;
        ////				skip = true;
        ////				base.VisitConstructorDeclaration(constructorDeclaration);
        ////			}
        ////
        ////			public override void VisitDestructorDeclaration(DestructorDeclaration destructorDeclaration)
        ////			{
        ////				currentMethodName = "~" + destructorDeclaration.Name;
        ////				skip = true;
        ////				base.VisitDestructorDeclaration(destructorDeclaration);
        ////			}
        ////
        ////			public override void VisitAccessor(Accessor accessor)
        ////			{
        ////				bool old = skip; 
        ////				skip = accessor.Role != PropertyDeclaration.GetterRole && accessor.Role != IndexerDeclaration.GetterRole;
        ////				base.VisitAccessor(accessor);
        ////				skip = old;
        ////			}
        ////
        ////			static bool AnonymousMethodMayReturnVoid(BaseSemanticModel ctx, Expression anonymousMethodExpression)
        ////			{
        ////				foreach (var type in TypeGuessing.GetValidTypes(ctx.Resolver, anonymousMethodExpression)) {
        ////					if (type.Kind != TypeKind.Delegate)
        ////						continue;
        ////					var invoke = type.GetDelegateInvokeMethod();
        ////					if (invoke != null && invoke.ReturnType.IsKnownType(KnownTypeCode.Void))
        ////						return true;
        ////				}
        ////				return false;
        ////			}
        ////
        ////
        ////			public override void VisitAnonymousMethodExpression(AnonymousMethodExpression anonymousMethodExpression)
        ////			{
        ////				bool old = skip;
        ////				skip = AnonymousMethodMayReturnVoid(ctx, anonymousMethodExpression);
        ////				base.VisitAnonymousMethodExpression(anonymousMethodExpression);
        ////				skip = old;
        ////			}
        ////
        ////			public override void VisitLambdaExpression(LambdaExpression lambdaExpression)
        ////			{
        ////				bool old = skip;
        ////				skip = AnonymousMethodMayReturnVoid(ctx, lambdaExpression);
        ////				base.VisitLambdaExpression(lambdaExpression);
        ////				skip = old;
        ////			}
        ////
        ////			public override void VisitReturnStatement(ReturnStatement returnStatement)
        ////			{
        ////				base.VisitReturnStatement(returnStatement);
        ////				if (skip)
        ////					return;
        ////
        ////				if (returnStatement.Expression.IsNull) {
        ////					var entity = returnStatement.GetParent<EntityDeclaration>();
        ////					if (entity is Accessor)
        ////						entity = entity.GetParent<EntityDeclaration>();
        ////					if (entity == null)
        ////						return;
        ////					AstNode entityNode;
        ////					var rr = GetRequestedReturnType (ctx, returnStatement, out entityNode);
        ////					if (rr.Kind == TypeKind.Void)
        ////						return;
        ////					var actions = new List<CodeAction>();
        ////					if (rr.Kind != TypeKind.Unknown) {
        ////						actions.Add(new CodeAction(ctx.TranslateString("Return default value"), script => {
        ////							Expression p;
        ////							if (rr.IsKnownType(KnownTypeCode.Boolean)) {
        ////								p = new PrimitiveExpression(false);
        ////							} else if (rr.IsKnownType(KnownTypeCode.String)) {
        ////								p = new PrimitiveExpression("");
        ////							} else if (rr.IsKnownType(KnownTypeCode.Char)) {
        ////								p = new PrimitiveExpression(' ');
        ////							} else if (rr.IsReferenceType == true) {
        ////								p = new NullReferenceExpression();
        ////							} else if (rr.GetDefinition() != null &&
        ////								rr.GetDefinition().KnownTypeCode < KnownTypeCode.DateTime) {
        ////								p = new PrimitiveExpression(0x0);
        ////							} else {
        ////								p = new DefaultValueExpression(ctx.CreateTypeSystemAstBuilder(returnStatement).ConvertType(rr));
        ////							}
        ////
        ////							script.Replace(returnStatement, new ReturnStatement(p));
        ////						}, returnStatement));
        ////					}
        ////					var method = returnStatement.GetParent<MethodDeclaration>();
        ////					if (method != null) {
        ////						actions.Add(new CodeAction(ctx.TranslateString("Change method return type to 'void'"), script => {
        ////							script.Replace(method.ReturnType, new PrimitiveType("void"));
        ////						}, returnStatement));
        ////					}
        ////
        ////					AddDiagnosticAnalyzer(new CodeIssue(
        ////						returnStatement, 
        ////						string.Format(ctx.TranslateString("`{0}': A return keyword must be followed by any expression when method returns a value"), currentMethodName),
        ////						actions
        ////					));
        ////				}
        ////			}
        //		}
    }

    [ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    [NotPortedYet]
    public class CS0126ReturnMustBeFollowedByAnyExpressionFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CS0126ReturnMustBeFollowedByAnyExpression.DiagnosticId);
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
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var diagnostic = diagnostics.First();
            var node = root.FindNode(context.Span);
            //if (!node.IsKind(SyntaxKind.BaseList))
            //	continue;
            var newRoot = root.RemoveNode(node, SyntaxRemoveOptions.KeepNoTrivia);
            context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, diagnostic.GetMessage(), document.WithSyntaxRoot(newRoot)), diagnostic);
        }
    }
}