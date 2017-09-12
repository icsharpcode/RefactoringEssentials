//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.Diagnostics;
//using System.Collections.Immutable;

//namespace RefactoringEssentials.CSharp.Diagnostics
//{
//    [DiagnosticAnalyzer(LanguageNames.CSharp)]
//    [NotPortedYet]
//    public class ForCanBeConvertedToForeachAnalyzer : DiagnosticAnalyzer
//    {
//        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
//            CSharpDiagnosticIDs.ForCanBeConvertedToForeachAnalyzerID,
//            GettextCatalog.GetString("Foreach loops are more efficient"),
//            GettextCatalog.GetString("'for' loop can be converted to 'foreach'"),
//            DiagnosticAnalyzerCategories.Opportunities,
//            DiagnosticSeverity.Warning,
//            isEnabledByDefault: true,
//            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.ForCanBeConvertedToForeachAnalyzerID)
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

//        //		class GatherVisitor : GatherVisitorBase<ForCanBeConvertedToForeachAnalyzer>
//        //		{
//        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
//        //				: base (semanticModel, addDiagnostic, cancellationToken)
//        //			{
//        //			}
//        ////
//        ////			static readonly AstNode forPattern =
//        ////				new Choice {
//        ////					new ForStatement {
//        ////						Initializers = {
//        ////							new VariableDeclarationStatement {
//        ////								Type = new AnyNode("int"),
//        ////								Variables = {
//        ////									new NamedNode("iteratorInitialzer", new VariableInitializer(Pattern.AnyString, new PrimitiveExpression(0)))
//        ////								}
//        ////							}
//        ////						},
//        ////						Condition = PatternHelper.OptionalParentheses(
//        ////							new BinaryOperatorExpression(
//        ////								PatternHelper.OptionalParentheses(new AnyNode("iterator")), 
//        ////								BinaryOperatorType.LessThan,
//        ////								PatternHelper.OptionalParentheses(
//        ////									new NamedNode("upperBound", new MemberReferenceExpression(new AnyNode (), Pattern.AnyString))
//        ////								)
//        ////							)
//        ////						),
//        ////						Iterators = {
//        ////							new ExpressionStatement(
//        ////								new Choice {
//        ////									new UnaryOperatorExpression(UnaryOperatorType.Increment, new Backreference("iterator")), 
//        ////									new UnaryOperatorExpression(UnaryOperatorType.PostIncrement, new Backreference("iterator")) 
//        ////								}
//        ////							)
//        ////						},
//        ////						EmbeddedStatement = new AnyNode("body")
//        ////					},
//        ////					new ForStatement {
//        ////						Initializers = {
//        ////							new VariableDeclarationStatement {
//        ////								Type = new AnyNode("int"),
//        ////								Variables = {
//        ////									new NamedNode("iteratorInitialzer", new VariableInitializer(Pattern.AnyString, new PrimitiveExpression(0))),
//        ////									new NamedNode("upperBoundInitializer", new VariableInitializer(Pattern.AnyString, new NamedNode("upperBound", new MemberReferenceExpression(new AnyNode (), Pattern.AnyString)))),
//        ////								}
//        ////							}
//        ////						},
//        ////						Condition = PatternHelper.OptionalParentheses(
//        ////							new BinaryOperatorExpression(
//        ////								PatternHelper.OptionalParentheses(new AnyNode("iterator")), 
//        ////								BinaryOperatorType.LessThan,
//        ////								PatternHelper.OptionalParentheses(
//        ////									new AnyNode("upperBoundInitializerName")
//        ////								)
//        ////							)
//        ////						),
//        ////						Iterators = {
//        ////							new ExpressionStatement(
//        ////								new Choice {
//        ////									new UnaryOperatorExpression(UnaryOperatorType.Increment, new Backreference("iterator")), 
//        ////									new UnaryOperatorExpression(UnaryOperatorType.PostIncrement, new Backreference("iterator")) 
//        ////								}
//        ////							)
//        ////						},
//        ////						EmbeddedStatement = new AnyNode("body")
//        ////					},
//        ////				};
//        ////			static readonly AstNode varDeclPattern =
//        ////				new VariableDeclarationStatement {
//        ////					Type = new AnyNode(),
//        ////					Variables = {
//        ////						new VariableInitializer(Pattern.AnyString, new NamedNode("indexer", new IndexerExpression(new AnyNode(), new IdentifierExpression(Pattern.AnyString))))
//        ////					}
//        ////				};
//        ////			static readonly AstNode varTypePattern =
//        ////				new SimpleType("var");
//        ////
//        ////			static bool IsEnumerable(IType type)
//        ////			{
//        ////				return type.Name == "IEnumerable" && (type.Namespace == "System.Collections.Generic" || type.Namespace == "System.Collections");
//        ////			}
//        ////
//        ////			public override void VisitForStatement(ForStatement forStatement)
//        ////			{
//        ////				base.VisitForStatement(forStatement);
//        ////				var forMatch = forPattern.Match(forStatement);
//        ////				if (!forMatch.Success)
//        ////					return;
//        ////				var body = forStatement.EmbeddedStatement as BlockStatement;
//        ////				if (body == null || !body.Statements.Any())
//        ////					return;
//        ////				var varDeclStmt = body.Statements.First() as VariableDeclarationStatement;
//        ////				if (varDeclStmt == null)
//        ////					return;
//        ////				var varMatch = varDeclPattern.Match(varDeclStmt);
//        ////				if (!varMatch.Success)
//        ////					return;
//        ////				var typeNode = forMatch.Get<AstNode>("int").FirstOrDefault();
//        ////				var varDecl = forMatch.Get<VariableInitializer>("iteratorInitialzer").FirstOrDefault();
//        ////				var iterator = forMatch.Get<IdentifierExpression>("iterator").FirstOrDefault();
//        ////				var upperBound = forMatch.Get<MemberReferenceExpression>("upperBound").FirstOrDefault();
//        ////				if (typeNode == null || varDecl == null || iterator == null || upperBound == null)
//        ////					return;
//        ////
//        ////				// Check iterator type
//        ////				if (!varTypePattern.IsMatch(typeNode)) {
//        ////					var typeRR = ctx.Resolve(typeNode);
//        ////					if (!typeRR.Type.IsKnownType(KnownTypeCode.Int32))
//        ////						return;
//        ////				}
//        ////
//        ////				if (varDecl.Name != iterator.Identifier)
//        ////					return;
//        ////
//        ////				var upperBoundInitializer = forMatch.Get<VariableInitializer>("upperBoundInitializer").FirstOrDefault();
//        ////				var upperBoundInitializerName = forMatch.Get<IdentifierExpression>("upperBoundInitializerName").FirstOrDefault();
//        ////				if (upperBoundInitializer != null) {
//        ////					if (upperBoundInitializerName == null || upperBoundInitializer.Name != upperBoundInitializerName.Identifier)
//        ////						return;
//        ////				}
//        ////
//        ////				var indexer = varMatch.Get<IndexerExpression>("indexer").Single();
//        ////				if (((IdentifierExpression)indexer.Arguments.First()).Identifier != iterator.Identifier)
//        ////					return;
//        ////				if (!indexer.Target.IsMatch(upperBound.Target))
//        ////					return;
//        ////
//        ////				var rr = ctx.Resolve(upperBound) as MemberResolveResult;
//        ////				if (rr == null || rr.IsError)
//        ////					return;
//        ////
//        ////				if (!(rr.Member.Name == "Length" && rr.Member.DeclaringType.Name == "Array" && rr.Member.DeclaringType.Namespace == "System") &&
//        ////				!(rr.Member.Name == "Count" && (IsEnumerable(rr.TargetResult.Type) || rr.TargetResult.Type.GetAllBaseTypes().Any(IsEnumerable))))
//        ////					return;
//        ////
//        ////				var variableInitializer = varDeclStmt.Variables.First();
//        ////				var lr = ctx.Resolve(variableInitializer) as LocalResolveResult;
//        ////				if (lr == null)
//        ////					return;
//        ////
//        ////				var ir = ctx.Resolve(varDecl) as LocalResolveResult;
//        ////				if (ir == null)
//        ////					return;
//        ////
//        ////				var analyze = new ConvertToConstantAnalyzer.VariableUsageAnalyzation(ctx);
//        ////				analyze.SetAnalyzedRange(
//        ////					varDeclStmt,
//        ////					forStatement.EmbeddedStatement,
//        ////					false
//        ////				);
//        ////				forStatement.EmbeddedStatement.AcceptVisitor(analyze);
//        ////				if (analyze.GetStatus(lr.Variable) == RefactoringEssentials.Refactoring.ExtractMethod.VariableState.Changed ||
//        ////				    analyze.GetStatus(ir.Variable) == RefactoringEssentials.Refactoring.ExtractMethod.VariableState.Changed ||
//        ////				    analyze.GetStatus(ir.Variable) == RefactoringEssentials.Refactoring.ExtractMethod.VariableState.Used)
//        ////					return;
//        ////
//        ////				AddDiagnosticAnalyzer(new CodeIssue(
//        ////					forStatement.ForToken,
//        ////					ctx.TranslateString(""),
//        ////					ctx.TranslateString(""),
//        ////					script => {
//        ////						var foreachBody = (BlockStatement)forStatement.EmbeddedStatement.Clone();
//        ////						foreachBody.Statements.First().Remove();
//        ////
//        ////						var fe = new ForeachStatement {
//        ////							VariableType = new PrimitiveType("var"),
//        ////							VariableName = variableInitializer.Name,
//        ////							InExpression = upperBound.Target.Clone(),
//        ////							EmbeddedStatement = foreachBody
//        ////						};
//        ////						script.Replace(forStatement, fe); 
//        ////					}
//        ////				) { IssueMarker = IssueMarker.DottedLine });
//        ////
//        ////			}
//        //		}
//    }



//}

