using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NotPortedYet]
    public class RedundantToStringCallAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor1 = new DiagnosticDescriptor(
            NRefactoryDiagnosticIDs.RedundantToStringCallAnalyzerID,
            GettextCatalog.GetString("Finds calls to ToString() which would be generated automatically by the compiler"),
            GettextCatalog.GetString("Redundant 'ToString()' call"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(NRefactoryDiagnosticIDs.RedundantToStringCallAnalyzerID),
            customTags: DiagnosticCustomTags.Unnecessary
        );

        static readonly DiagnosticDescriptor descriptor2 = new DiagnosticDescriptor(
            NRefactoryDiagnosticIDs.RedundantToStringCallAnalyzer_ValueTypesID,
            GettextCatalog.GetString("Finds calls to ToString() which would be generated automatically by the compiler"),
            GettextCatalog.GetString("Redundant 'ToString()' call"),
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(NRefactoryDiagnosticIDs.RedundantToStringCallAnalyzerID),
            customTags: DiagnosticCustomTags.Unnecessary
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor1, descriptor2);

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

        //		class GatherVisitor : GatherVisitorBase<RedundantToStringCallAnalyzer>
        //		{
        ////			static Tuple<int, int> onlyFirst = Tuple.Create (0, 0);
        ////
        ////			static IDictionary<Tuple<string, int>, Tuple<int, int>> membersCallingToString = new Dictionary<Tuple<string, int>, Tuple<int, int>> {
        ////				{ Tuple.Create("System.IO.TextWriter.Write", 1), onlyFirst },
        ////				{ Tuple.Create("System.IO.TextWriter.WriteLine", 1), onlyFirst },
        ////				{ Tuple.Create("System.Console.Write", 1), onlyFirst },
        ////				{ Tuple.Create("System.Console.WriteLine", 1), onlyFirst }
        ////			};

        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //				//binOpVisitor = new BinaryExpressionVisitor (this);
        //			}

        ////			HashSet<AstNode> processedNodes = new HashSet<AstNode>();
        ////
        ////			void CheckExpressionInAutoCallContext(Expression expression)
        ////			{
        ////				if (expression is InvocationExpression && !processedNodes.Contains(expression)) {
        ////					CheckInvocationInAutoCallContext((InvocationExpression)expression);
        ////				}
        ////			}
        ////
        ////			void CheckInvocationInAutoCallContext(InvocationExpression invocationExpression)
        ////			{
        ////				var memberExpression = invocationExpression.Target as MemberReferenceExpression;
        ////				if (memberExpression == null) {
        ////					return;
        ////				}
        ////				if (memberExpression.MemberName != "ToString" || invocationExpression.Arguments.Any ()) {
        ////					return;
        ////				}
        ////
        ////				var resolveResult = ctx.Resolve(invocationExpression) as CSharpInvocationResolveResult;
        ////				if (resolveResult == null) {
        ////					return;
        ////				}
        ////				if (ctx.Resolve(memberExpression.Target).Type.Kind != TypeKind.Struct) 
        ////					AddRedundantToStringIssue(memberExpression, invocationExpression);
        ////			}
        ////			
        ////			void AddRedundantToStringIssue(MemberReferenceExpression memberExpression, InvocationExpression invocationExpression)
        ////			{
        ////				// Simon Lindgren 2012-09-14: Previously there was a check here to see if the node had already been processed
        ////				// This has been moved out to the callers, to check it earlier for a 30-40% run time reduction
        ////				processedNodes.Add(invocationExpression);
        ////				
        ////				AddDiagnosticAnalyzer(new CodeIssue(memberExpression.DotToken.StartLocation, invocationExpression.RParToken.EndLocation,
        ////				         ctx.TranslateString(""), 
        ////				         ctx.TranslateString(""), script =>  {
        ////					script.Replace(invocationExpression, memberExpression.Target.Clone());
        ////					}) { IssueMarker = IssueMarker.GrayOut });
        ////			}
        ////
        ////			#region Binary operator
        ////			public override void VisitBinaryOperatorExpression(BinaryOperatorExpression binaryOperatorExpression)
        ////			{
        ////				base.VisitBinaryOperatorExpression(binaryOperatorExpression);
        ////
        ////				if (binaryOperatorExpression.Operator != BinaryOperatorType.Add)
        ////					return;
        ////				binOpVisitor.Reset();
        ////				binaryOperatorExpression.AcceptVisitor(binOpVisitor);
        ////			}
        ////
        ////			BinaryExpressionVisitor binOpVisitor;
        ////			class BinaryExpressionVisitor : DepthFirstAstVisitor
        ////			{
        ////				GatherVisitor issue;
        ////				int stringExpressionCount;
        ////				Expression firstStringExpression;
        ////
        ////				public BinaryExpressionVisitor(GatherVisitor issue)
        ////				{
        ////					this.issue = issue;
        ////				}
        ////
        ////				public void Reset()
        ////				{
        ////					stringExpressionCount = 0;
        ////					firstStringExpression = null;
        ////				}
        ////
        ////				void Check (Expression expression)
        ////				{
        ////					if (expression is BinaryOperatorExpression) {
        ////						expression.AcceptVisitor(this);
        ////						return;
        ////					}
        ////					if (stringExpressionCount <= 1) {
        ////						var resolveResult = issue.ctx.Resolve(expression);
        ////						if (resolveResult.Type.IsKnownType(KnownTypeCode.String)) {
        ////							stringExpressionCount++;
        ////							if (stringExpressionCount == 1) {
        ////								firstStringExpression = expression;
        ////							} else {
        ////								issue.CheckExpressionInAutoCallContext(firstStringExpression);
        ////								issue.CheckExpressionInAutoCallContext(expression);
        ////							}
        ////						}
        ////					} else {
        ////						issue.CheckExpressionInAutoCallContext(expression);
        ////					}
        ////				}
        ////				
        ////				public override void VisitBinaryOperatorExpression(BinaryOperatorExpression binaryOperatorExpression)
        ////				{
        ////					Check(binaryOperatorExpression.Left);
        ////					Check(binaryOperatorExpression.Right);
        ////				}
        ////			}
        ////
        ////			#endregion
        ////
        ////			#region Invocation expression
        ////			public override void VisitInvocationExpression(InvocationExpression invocationExpression)
        ////			{
        ////				base.VisitInvocationExpression(invocationExpression);
        ////
        ////				//var target = invocationExpression.Target as MemberReferenceExpression;
        ////
        ////				var invocationResolveResult = ctx.Resolve(invocationExpression) as CSharpInvocationResolveResult;
        ////				if (invocationResolveResult == null) {
        ////					return;
        ////				}
        ////				IMember member = invocationResolveResult.Member;
        ////
        ////				// "".ToString()
        ////                CheckTargetedObject(invocationExpression, invocationResolveResult.TargetResult.Type, member);
        ////
        ////				// Check list of members that call ToString() automatically
        ////				CheckAutomaticToStringCallers(invocationExpression, member);
        ////
        ////				// Check formatting calls
        ////				CheckFormattingCall(invocationExpression, invocationResolveResult);
        ////			}
        ////
        ////			void CheckTargetedObject(InvocationExpression invocationExpression, IType type, IMember member)
        ////			{
        ////				var memberExpression = invocationExpression.Target as MemberReferenceExpression;
        ////				if (memberExpression != null && !processedNodes.Contains(invocationExpression)) {
        ////					if (type.IsKnownType(KnownTypeCode.String) && member.Name == "ToString") {
        ////						AddRedundantToStringIssue(memberExpression, invocationExpression);
        ////					}
        ////				}
        ////			}
        ////			
        ////			void CheckAutomaticToStringCallers(InvocationExpression invocationExpression, IMember member)
        ////			{
        ////				if (member.IsOverride) {
        ////					member = InheritanceHelper.GetBaseMember(member);
        ////					if (member == null) {
        ////						return;
        ////					}
        ////				}
        ////				var key = new Tuple<string, int>(member.ReflectionName, invocationExpression.Arguments.Count);
        ////				Tuple<int, int> checkInfo;
        ////				if (membersCallingToString.TryGetValue(key, out checkInfo)) {
        ////					var arguments = invocationExpression.Arguments.ToList();
        ////					for (int i = checkInfo.Item1; i < Math.Min(invocationExpression.Arguments.Count, checkInfo.Item2 + 1); ++i) {
        ////						CheckExpressionInAutoCallContext(arguments[i]);
        ////					}
        ////				}
        ////			}
        ////
        ////			void CheckFormattingCall(InvocationExpression invocationExpression, CSharpInvocationResolveResult invocationResolveResult)
        ////			{
        ////				Expression formatArgument;
        ////				IList<Expression> formatArguments;
        ////				// Only check parameters that are of type object: String means it is neccessary, others
        ////				// means that there is another problem (ie no matching overload of the method).
        ////				Func<IParameter, Expression, bool> predicate = (parameter, argument) => {
        ////					var type = parameter.Type;
        ////					if (type is TypeWithElementType && parameter.IsParams) {
        ////						type = ((TypeWithElementType)type).ElementType;
        ////					}
        ////					var typeDefinition = type.GetDefinition();
        ////					if (typeDefinition == null)
        ////						return false;
        ////					return typeDefinition.IsKnownType(KnownTypeCode.Object);
        ////				};
        ////				if (FormatStringHelper.TryGetFormattingParameters(invocationResolveResult, invocationExpression,
        ////				                                                  out formatArgument, out formatArguments, predicate)) {
        ////					foreach (var argument in formatArguments) {
        ////						CheckExpressionInAutoCallContext(argument);
        ////					}
        ////				}
        ////			}
        ////			#endregion
        //		}
    }
}