//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.Diagnostics;
//using System.Collections.Immutable;

//namespace RefactoringEssentials.CSharp.Diagnostics
//{
//    [DiagnosticAnalyzer(LanguageNames.CSharp)]
//    [NotPortedYet]
//    public class RedundantArgumentDefaultValueAnalyzer : DiagnosticAnalyzer
//    {
//        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
//            CSharpDiagnosticIDs.RedundantArgumentDefaultValueAnalyzerID,
//            GettextCatalog.GetString("Default argument value is redundant"),
//            GettextCatalog.GetString("The parameter is optional with the same default value"),
//            DiagnosticAnalyzerCategories.RedundanciesInCode,
//            DiagnosticSeverity.Hidden,
//            isEnabledByDefault: true,
//            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantArgumentDefaultValueAnalyzerID),
//            customTags: DiagnosticCustomTags.Unnecessary
//        );

//        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

//        public override void Initialize(AnalysisContext context)
//        {
//            context.EnableConcurrentExecution();
//            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
//            //context.RegisterSyntaxNodeAction(
//            //	(nodeContext) => {
//            //		Diagnostic diagnostic;
//            //		if (TryGetDiagnostic (nodeContext, out diagnostic)) {
//            //			nodeContext.ReportDiagnostic(diagnostic);
//            //		}
//            //	}, 
//            //	new SyntaxKind[] { SyntaxKind.None }
//            //);
//        }

//        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
//        {
//            diagnostic = default(Diagnostic);
//            //var node = nodeContext.Node as ;
//            //diagnostic = Diagnostic.Create (descriptor, node.GetLocation ());
//            //return true;
//            return false;
//        }

//        //		class GatherVisitor : GatherVisitorBase<RedundantArgumentDefaultValueAnalyzer>
//        //		{
//        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
//        //				: base (semanticModel, addDiagnostic, cancellationToken)
//        //			{
//        //			}

//        ////			bool IsDefaultValue(Expression arg, RefactoringEssentials.TypeSystem.IParameter par)
//        ////			{
//        ////				var ne = arg as NamedArgumentExpression;
//        ////				if (ne != null) {
//        ////					if (ne.Name != par.Name)
//        ////						return false;
//        ////					arg = ne.Expression;
//        ////				}
//        ////				var cr = ctx.Resolve(arg);
//        ////				if (cr == null || !cr.IsCompileTimeConstant || !par.IsOptional)
//        ////					return false;
//        ////				return 
//        ////					cr.ConstantValue == null && par.ConstantValue == null || 
//        ////					cr.ConstantValue.Equals(par.ConstantValue);
//        ////			}
//        ////
//        ////			public override void VisitObjectCreateExpression(ObjectCreateExpression objectCreateExpression)
//        ////			{
//        ////				base.VisitObjectCreateExpression(objectCreateExpression);
//        ////				Check(objectCreateExpression, objectCreateExpression.Arguments.ToList());
//        ////			}
//        ////
//        ////			public override void VisitInvocationExpression(InvocationExpression invocationExpression)
//        ////			{
//        ////				base.VisitInvocationExpression(invocationExpression);
//        ////				Check(invocationExpression, invocationExpression.Arguments.ToList());
//        ////			}
//        ////
//        ////			public override void VisitIndexerExpression(IndexerExpression indexerExpression)
//        ////			{
//        ////				base.VisitIndexerExpression(indexerExpression);
//        ////				Check(indexerExpression, indexerExpression.Arguments.ToList());
//        ////			}
//        ////
//        ////			void Check(AstNode invocationExpression, List<Expression> arguments)
//        ////			{
//        ////				var rr = ctx.Resolve(invocationExpression) as CSharpInvocationResolveResult;
//        ////				if (rr == null || rr.IsError)
//        ////					return;
//        ////
//        ////				for (int i = 0; i < arguments.Count && i < rr.Member.Parameters.Count; i++) {
//        ////
//        ////					if (!IsDefaultValue(arguments[i], rr.Member.Parameters[i]))
//        ////						continue;
//        ////					bool nextAreAllDefault = true;
//        ////					for (int j = i + 1; j < arguments.Count && j < rr.Member.Parameters.Count; j++) {
//        ////						if (!IsDefaultValue(arguments[j], rr.Member.Parameters[j])) {
//        ////							nextAreAllDefault = false;
//        ////							break;
//        ////						}
//        ////					}
//        ////					if (!nextAreAllDefault)
//        ////						continue;
//        ////					for (int j = i; j < arguments.Count && j < rr.Member.Parameters.Count; j++) {
//        ////						var _i = j;
//        ////						AddDiagnosticAnalyzer(new CodeIssue(
//        ////							arguments[j],
//        ////							i + 1 < arguments.Count ? ctx.TranslateString("Argument values are redundant") : ctx.TranslateString("Argument value is redundant"),
//        ////							i + 1 < arguments.Count ? ctx.TranslateString("Remove redundant arguments") : ctx.TranslateString("Remove redundant argument"),
//        ////							script => {
//        ////								var invoke = invocationExpression.Clone();
//        ////
//        ////								var argCollection = invoke.GetChildrenByRole<Expression>(Roles.Argument);
//        ////								argCollection.Clear();
//        ////								for (int k = 0; k < _i; k++)
//        ////									argCollection.Add(arguments[k].Clone());
//        ////								script.Replace(invocationExpression, invoke);
//        ////							}
//        ////						) { IssueMarker = IssueMarker.GrayOut });
//        ////					}
//        ////					break;
//        ////				}
//        ////			}
//        //		}
//    }

//    /* TODO: Merge:
//[IssueDescription("Optional argument has default value and can be skipped",
//	                  Description = "Finds calls to functions where optional parameters are used and the passed argument is the same as the default.",
//	                  Category = IssueCategories.RedundanciesInCode,
//	                  Severity = Severity.Hint)]
//	public class OptionalParameterCouldBeSkippedAnalyzer : GatherVisitorCodeIssueProvider
//	{
//		protected override IGatherVisitor CreateVisitor(BaseSemanticModel context)
//		{
//			return new GatherVisitor(context);
//		}
		
//		class GatherVisitor : GatherVisitorBase<OptionalParameterCouldBeSkippedAnalyzer>
//		{
//			static readonly object removeAllRedundantArgumentsKey = new object ();

//			public GatherVisitor(BaseSemanticModel context) : base (context)
//			{
//			}

//			public override void VisitObjectCreateExpression(ObjectCreateExpression objectCreateExpression)
//			{
//				base.VisitObjectCreateExpression(objectCreateExpression);
				
//				CheckMethodCall(objectCreateExpression, objectCreateExpression.Arguments,
//				                (objectCreation, args) => new ObjectCreateExpression(objectCreation.Type.Clone(), args));
//			}

//			public override void VisitInvocationExpression(InvocationExpression invocationExpression)
//			{
//				base.VisitInvocationExpression(invocationExpression);
				
//				CheckMethodCall(invocationExpression, invocationExpression.Arguments,
//				                (invocation, args) => new InvocationExpression(invocation.Target.Clone(), args));
//			}

//			void CheckMethodCall<T> (T node, IEnumerable<Expression> args, Func<T, IEnumerable<Expression>, T> generateReplacement) where T: AstNode
//			{
//				// The first two checks are unnecessary, but eliminates the majority of calls early,
//				// improving performance.
//				var arguments = args.ToArray();
//				if (arguments.Length == 0)
//					return;
//				var lastArg = arguments[arguments.Length - 1];
//				if (!(lastArg is PrimitiveExpression || lastArg is NamedArgumentExpression))
//					return;

//				var invocationResolveResult = ctx.Resolve(node) as CSharpInvocationResolveResult;
//				if (invocationResolveResult == null)
//					return;
				
//				string actionMessage = ctx.TranslateString("Remove redundant arguments");
				
//				var redundantArguments = GetRedundantArguments(arguments, invocationResolveResult);
//				var action = new CodeAction(actionMessage, script => {
//					var newArgumentList = arguments
//						.Where(arg => !redundantArguments.Contains(arg))
//						.Select(arg => arg.Clone());
//					var newInvocation = generateReplacement(node, newArgumentList);
//					script.Replace(node, newInvocation);
//				}, node, removeAllRedundantArgumentsKey);
//				var issueMessage = ctx.TranslateString("Argument is identical to the default value");
//				var lastPositionalArgument = redundantArguments.FirstOrDefault(expression => !(expression is NamedArgumentExpression));

//				foreach (var argument in redundantArguments) {
//					var localArgument = argument;
//					var actions = new List<CodeAction>();
//					actions.Add(action);

//					if (localArgument is NamedArgumentExpression || localArgument == lastPositionalArgument) {
//						var title = ctx.TranslateString("Remove this argument");
//						actions.Add(new CodeAction(title, script => {
//							var newArgumentList = arguments
//								.Where(arg => arg != localArgument)
//								.Select(arg => arg.Clone());
//							var newInvocation = generateReplacement(node, newArgumentList);
//							script.Replace(node, newInvocation);
//						}, node, null));
//					} else {
//						var title = ctx.TranslateString("Remove this and the following positional arguments");
//						actions.Add(new CodeAction(title, script => {
//							var newArgumentList = arguments
//								.Where(arg => arg.StartLocation < localArgument.StartLocation && !(arg is NamedArgumentExpression))
//								.Select(arg => arg.Clone());
//							var newInvocation = generateReplacement(node, newArgumentList);
//							script.Replace(node, newInvocation);
//						}, node, null));
//					}

//					AddDiagnosticAnalyzer(new CodeIssue(localArgument, issueMessage, actions) { IssueMarker = IssueMarker.GrayOut });
//				}
//			}

//			IList<Expression> GetRedundantArguments(Expression[] arguments, CSharpInvocationResolveResult invocationResolveResult)
//			{
//				var argumentToParameterMap = invocationResolveResult.GetArgumentToParameterMap();
//				var resolvedParameters = invocationResolveResult.Member.Parameters;

//				IList<Expression> redundantArguments = new List<Expression>();

//				for (int i = arguments.Length - 1; i >= 0; i--) {
//					var parameterIndex = argumentToParameterMap[i];
//					if (parameterIndex == -1)
//						// This particular parameter is an error, but keep trying the other ones
//						continue;
//					var parameter = resolvedParameters[parameterIndex];
//					var argument = arguments[i];
//					if (argument is PrimitiveExpression) {
//						if (parameter.IsParams)
//							// before positional params arguments all optional arguments are needed, otherwise some of the
//							// param arguments will be shifted out of the params into the fixed parameters
//							break;
//						if (!parameter.IsOptional)
//							// There can be no optional parameters preceding a required one
//							break;
//						var argumentResolveResult = ctx.Resolve(argument) as ConstantResolveResult;
//						if (argumentResolveResult == null || parameter.ConstantValue != argumentResolveResult.ConstantValue)
//							// Stop here since any arguments before this one has to be there
//							// to enable the passing of this argument
//							break;
//						redundantArguments.Add(argument);
//					} else if (argument is NamedArgumentExpression) {
//						var expression = ((NamedArgumentExpression)argument).Expression as PrimitiveExpression;
//						if (expression == null)
//							continue;
//						var expressionResolveResult = ctx.Resolve(expression) as ConstantResolveResult;
//						if (expressionResolveResult == null || parameter.ConstantValue != expressionResolveResult.ConstantValue)
//							// continue, since there can still be more arguments that are redundant
//							continue;
//						redundantArguments.Add(argument);
//					} else {
//						// This is a non-constant positional argument => no more redundancies are possible
//						break;
//					}
//				}
//				return redundantArguments;
//			}
//		}
//	}
//*/
//}