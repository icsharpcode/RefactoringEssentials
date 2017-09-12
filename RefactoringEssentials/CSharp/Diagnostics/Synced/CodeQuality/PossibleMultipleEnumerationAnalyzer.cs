//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.Diagnostics;
//using System.Collections.Immutable;

//namespace RefactoringEssentials.CSharp.Diagnostics
//{
//    [DiagnosticAnalyzer(LanguageNames.CSharp)]
//    [NotPortedYet]
//    public class PossibleMultipleEnumerationAnalyzer : DiagnosticAnalyzer
//    {
//        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
//            CSharpDiagnosticIDs.PossibleMultipleEnumerationAnalyzerID,
//            GettextCatalog.GetString("Possible multiple enumeration of IEnumerable"),
//            GettextCatalog.GetString("Possible multiple enumeration of IEnumerable"),
//            DiagnosticAnalyzerCategories.CodeQualityIssues,
//            DiagnosticSeverity.Warning,
//            isEnabledByDefault: true,
//            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.PossibleMultipleEnumerationAnalyzerID)
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

//        //		class AnalysisStatementCollector : DepthFirstAstVisitor
//        //		{
//        //			List<Statement> statements;
//        //			AstNode variableDecl;
//        //
//        //			AnalysisStatementCollector (AstNode variableDecl)
//        //			{
//        //				this.variableDecl = variableDecl;
//        //			}
//        //
//        //			IList<Statement> GetStatements ()
//        //			{
//        //				if (statements != null)
//        //					return statements;
//        //
//        //				statements = new List<Statement> ();
//        //				var parent = variableDecl.Parent;
//        //				while (parent != null) {
//        //					if (parent is BlockStatement || parent is MethodDeclaration ||
//        //						parent is AnonymousMethodExpression || parent is LambdaExpression) {
//        //						parent.AcceptVisitor (this);
//        //						if (parent is BlockStatement)
//        //							statements.Add ((BlockStatement)parent);
//        //						break;
//        //					}
//        //					parent = parent.Parent;
//        //				}
//        //				return statements;
//        //			}
//        //
//        //			public override void VisitMethodDeclaration (MethodDeclaration methodDeclaration)
//        //			{
//        //				statements.Add (methodDeclaration.Body);
//        //
//        //				base.VisitMethodDeclaration (methodDeclaration);
//        //			}
//        //
//        //			public override void VisitAnonymousMethodExpression (AnonymousMethodExpression anonymousMethodExpression)
//        //			{
//        //				statements.Add (anonymousMethodExpression.Body);
//        //
//        //				base.VisitAnonymousMethodExpression (anonymousMethodExpression);
//        //			}
//        //
//        //			public override void VisitLambdaExpression (LambdaExpression lambdaExpression)
//        //			{
//        //				var body = lambdaExpression.Body as BlockStatement;
//        //				if (body != null)
//        //					statements.Add (body);
//        //
//        //				base.VisitLambdaExpression (lambdaExpression);
//        //			}
//        //
//        //			public static IList<Statement> Collect (AstNode variableDecl)
//        //			{
//        //				return new AnalysisStatementCollector (variableDecl).GetStatements ();
//        //			}
//        //		}

//        //		class GatherVisitor : GatherVisitorBase<PossibleMultipleEnumerationAnalyzer>
//        //		{
//        //			//HashSet<AstNode> collectedAstNodes;

//        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
//        //				: base (semanticModel, addDiagnostic, cancellationToken)
//        //			{
//        //				//this.collectedAstNodes = new HashSet<AstNode> ();
//        //			}

//        ////			void ResolveIssue(Script s, AstNode node, Func<TypeSystemAstBuilder, AstType> type, string methodCall)
//        ////			{
//        ////				var rr = ctx.Resolve(node) as LocalResolveResult;
//        ////				if (rr == null || rr.IsError)
//        ////					return;
//        ////				var refs = ctx.FindReferences(ctx.RootNode, rr.Variable);
//        ////				var nodes = refs.Select(r => r.Node).Where(IsEnumeration).OfType<Expression>().ToList(); 
//        ////				if (nodes.Count == 0)
//        ////					return;
//        ////				var first = nodes.First().GetParent<Statement>();
//        ////				var varName = ctx.GetNameProposal("enumerable", first.StartLocation);
//        ////				var astBuilder = ctx.CreateTypeSystemAstBuilder(first);
//        ////
//        ////				var links = new List<AstNode>();
//        ////				var varDec = new VariableDeclarationStatement(new PrimitiveType("var"), varName, new BinaryOperatorExpression(new AsExpression((Expression)node.Clone(), type (astBuilder)), BinaryOperatorType.NullCoalescing, new InvocationExpression(new MemberReferenceExpression((Expression)node.Clone(), methodCall))));
//        ////
//        ////				links.Add(varDec.Variables.First().NameToken);
//        ////				s.InsertBefore(first, varDec);
//        ////				foreach (var n in nodes) {
//        ////					var id = new IdentifierExpression(varName);
//        ////					links.Add(id);
//        ////					s.Replace(n, id);
//        ////				}
//        ////				s.Link(links);
//        ////			}
//        ////
//        ////			void AddDiagnosticAnalyzer (AstNode node, IType elementType, IList<AstNode> nodes)
//        ////			{
//        ////				if (collectedAstNodes.Add(node)) {
//        ////					var actions = new List<CodeAction>();
//        ////					actions.Add(
//        ////						new CodeAction(
//        ////							ctx.TranslateString("Enumerate to array"),
//        ////							s => ResolveIssue(s, node, ab => new ComposedType { BaseType = ab.ConvertType(elementType), ArraySpecifiers =  { new ArraySpecifier() } }, "ToArray"),
//        ////							node
//        ////						)
//        ////					);
//        ////
//        ////					actions.Add(
//        ////						new CodeAction(
//        ////							ctx.TranslateString("Enumerate to list"),
//        ////							s => {
//        ////								var listType = ctx.Compilation.FindType(typeof(IList<>));
//        ////								ResolveIssue(s, node, ab => ab.ConvertType(new ParameterizedType(listType.GetDefinition(), new IType[]{ elementType })), "ToList");
//        ////							},
//        ////							node
//        ////						)
//        ////					);
//        ////
//        ////					AddDiagnosticAnalyzer(new CodeIssue(node, ctx.TranslateString("Possible multiple enumeration of IEnumerable"), actions));
//        ////				}
//        ////			}
//        ////
//        ////			void AddDiagnosticAnalyzers (IList<AstNode> nodes)
//        ////			{
//        ////				if (nodes.Count == 0)
//        ////					return;
//        ////				var rr = ctx.Resolve(nodes[0]);
//        ////				var elementType = TypeGuessing.GetElementType(ctx.Resolver, rr.Type);
//        ////
//        ////				foreach (var node in nodes)
//        ////					AddDiagnosticAnalyzer (node, elementType, nodes);
//        ////			}
//        ////
//        ////			public override void VisitParameterDeclaration (ParameterDeclaration parameterDeclaration)
//        ////			{
//        ////				base.VisitParameterDeclaration (parameterDeclaration);
//        ////
//        ////				var resolveResult = ctx.Resolve (parameterDeclaration) as LocalResolveResult;
//        ////				CollectIssues (parameterDeclaration, parameterDeclaration.Parent, resolveResult);
//        ////			}
//        ////
//        ////			public override void VisitVariableInitializer (VariableInitializer variableInitializer)
//        ////			{
//        ////				base.VisitVariableInitializer (variableInitializer);
//        ////
//        ////				var resolveResult = ctx.Resolve (variableInitializer) as LocalResolveResult;
//        ////				CollectIssues (variableInitializer, variableInitializer.Parent.Parent, resolveResult);
//        ////			}
//        ////
//        ////			static bool IsAssignment (AstNode node)
//        ////			{
//        ////				var assignment = node.Parent as AssignmentExpression;
//        ////				if (assignment != null)
//        ////					return assignment.Left == node;
//        ////
//        ////				var direction = node.Parent as DirectionExpression;
//        ////				if (direction != null)
//        ////					return direction.FieldDirection == FieldDirection.Out && direction.Expression == node;
//        ////
//        ////				return false;
//        ////			}
//        ////
//        ////			bool IsEnumeration (AstNode node)
//        ////			{
//        ////				var foreachStatement = node.Parent as ForeachStatement;
//        ////				if (foreachStatement != null && foreachStatement.InExpression == node) {
//        ////					return true;
//        ////				}
//        ////
//        ////				var memberRef = node.Parent as MemberReferenceExpression;
//        ////				if (memberRef != null && memberRef.Target == node) {
//        ////					var invocation = memberRef.Parent as InvocationExpression;
//        ////					if (invocation == null || invocation.Target != memberRef)
//        ////						return false;
//        ////
//        ////					var methodGroup = ctx.Resolve (memberRef) as MethodGroupResolveResult;
//        ////					if (methodGroup == null)
//        ////						return false;
//        ////
//        ////					var method = methodGroup.Methods.FirstOrDefault ();
//        ////					if (method != null) {
//        ////						var declaringTypeDef = method.DeclaringTypeDefinition;
//        ////						if (declaringTypeDef != null && declaringTypeDef.KnownTypeCode == KnownTypeCode.Object)
//        ////							return false;
//        ////					}
//        ////					return true;
//        ////				}
//        ////
//        ////				return false;
//        ////			}
//        ////
//        ////			HashSet<AstNode> references;
//        ////			HashSet<Statement> refStatements;
//        ////			HashSet<LambdaExpression> lambdaExpressions;
//        ////
//        ////			HashSet<VariableReferenceNode> visitedNodes;
//        ////			HashSet<VariableReferenceNode> collectedNodes;
//        ////			Dictionary<VariableReferenceNode, int> nodeDegree; // number of enumerations a node can reach
//        ////
//        ////			void FindReferences (AstNode variableDecl, AstNode rootNode, IVariable variable)
//        ////			{
//        ////				references = new HashSet<AstNode> ();
//        ////				refStatements = new HashSet<Statement> ();
//        ////				lambdaExpressions = new HashSet<LambdaExpression> ();
//        ////
//        ////				foreach (var result in ctx.FindReferences (rootNode, variable)) {
//        ////					var astNode = result.Node;
//        ////					if (astNode == variableDecl)
//        ////						continue;
//        ////
//        ////					var parent = astNode.Parent;
//        ////					while (!(parent == null || parent is Statement || parent is LambdaExpression))
//        ////						parent = parent.Parent;
//        ////					if (parent == null)
//        ////						continue;
//        ////
//        ////					// lambda expression with expression body, should be analyzed separately
//        ////					var expr = parent as LambdaExpression;
//        ////					if (expr != null) {
//        ////						if (IsAssignment (astNode) || IsEnumeration (astNode)) {
//        ////							references.Add (astNode);
//        ////							lambdaExpressions.Add (expr);
//        ////						}
//        ////						continue;
//        ////					}
//        ////
//        ////					if (IsAssignment (astNode) || IsEnumeration (astNode)) {
//        ////						references.Add (astNode);
//        ////						var statement = (Statement)parent;
//        ////						refStatements.Add (statement);
//        ////					}
//        ////				}
//        ////			}
//        ////
//        ////			void CollectIssues (AstNode variableDecl, AstNode rootNode, LocalResolveResult resolveResult)
//        ////			{
//        ////				if (resolveResult == null)
//        ////					return;
//        ////				var type = resolveResult.Type;
//        ////				var typeDef = type.GetDefinition ();
//        ////				if (typeDef == null ||
//        ////				    (typeDef.KnownTypeCode != KnownTypeCode.IEnumerable &&
//        ////				     typeDef.KnownTypeCode != KnownTypeCode.IEnumerableOfT))
//        ////					return;
//        ////
//        ////				FindReferences (variableDecl, rootNode, resolveResult.Variable);
//        ////
//        ////				var statements = AnalysisStatementCollector.Collect (variableDecl);
//        ////				var builder = new VariableReferenceGraphBuilder (ctx);
//        ////				foreach (var statement in statements) {
//        ////					var vrNode = builder.Build (statement, references, refStatements, ctx);
//        ////					FindMultipleEnumeration (vrNode);
//        ////				}
//        ////				foreach (var lambda in lambdaExpressions) {
//        ////					var vrNode = builder.Build (references, ctx.Resolver, (Expression)lambda.Body);
//        ////					FindMultipleEnumeration (vrNode);
//        ////				}
//        ////			}
//        ////
//        ////			/// <summary>
//        ////			/// split references in the specified node into sub nodes according to the value they uses
//        ////			/// </summary>
//        ////			/// <param name="node">node to split</param>
//        ////			/// <returns>list of sub nodes</returns>
//        ////			static IList<VariableReferenceNode> SplitNode (VariableReferenceNode node)
//        ////			{
//        ////				var subNodes = new List<VariableReferenceNode> ();
//        ////				// find indices of all assignments in node and use them to split references
//        ////				var assignmentIndices = new List<int> { -1 };
//        ////				for (int i = 0; i < node.References.Count; i++) {
//        ////					if (IsAssignment (node.References [i]))
//        ////						assignmentIndices.Add (i);
//        ////				}
//        ////				assignmentIndices.Add (node.References.Count);
//        ////				for (int i = 0; i < assignmentIndices.Count - 1; i++) {
//        ////					var index1 = assignmentIndices [i];
//        ////					var index2 = assignmentIndices [i + 1];
//        ////					if (index1 + 1 >= index2)
//        ////						continue;
//        ////					var subNode = new VariableReferenceNode ();
//        ////					for (int refIndex = index1 + 1; refIndex < index2; refIndex++)
//        ////						subNode.References.Add (node.References [refIndex]);
//        ////					subNodes.Add (subNode);
//        ////				}
//        ////				if (subNodes.Count == 0)
//        ////					subNodes.Add (new VariableReferenceNode ());
//        ////
//        ////				var firstNode = subNodes [0];
//        ////				foreach (var prevNode in node.PreviousNodes) {
//        ////					prevNode.NextNodes.Remove (node);
//        ////					// connect two nodes if the first ref is not an assignment
//        ////					if (firstNode.References.FirstOrDefault () == node.References.FirstOrDefault ())
//        ////						prevNode.NextNodes.Add (firstNode);
//        ////				}
//        ////
//        ////				var lastNode = subNodes [subNodes.Count - 1];
//        ////				foreach (var nextNode in node.NextNodes) {
//        ////					nextNode.PreviousNodes.Remove (node);
//        ////					lastNode.AddNextNode (nextNode);
//        ////				}
//        ////
//        ////				return subNodes;
//        ////			}
//        ////
//        ////			/// <summary>
//        ////			/// convert a variable reference graph starting from the specified node to an assignment usage graph,
//        ////			/// in which nodes are connect if and only if they contains references using the same assigned value
//        ////			/// </summary>
//        ////			/// <param name="startNode">starting node of the variable reference graph</param>
//        ////			/// <returns>
//        ////			/// list of VariableReferenceNode, each of which is a starting node of a sub-graph in which references all
//        ////			/// use the same assigned value
//        ////			/// </returns>
//        ////			static IEnumerable<VariableReferenceNode> GetAssignmentUsageGraph (VariableReferenceNode startNode)
//        ////			{
//        ////				var graph = new List<VariableReferenceNode> ();
//        ////				var visited = new HashSet<VariableReferenceNode> ();
//        ////				var stack = new Stack<VariableReferenceNode> ();
//        ////				stack.Push (startNode);
//        ////				while (stack.Count > 0) {
//        ////					var node = stack.Pop ();
//        ////					if (!visited.Add (node))
//        ////						continue;
//        ////
//        ////					var nodes = SplitNode (node);
//        ////					graph.AddRange (nodes);
//        ////					foreach (var addedNode in nodes)
//        ////						visited.Add (addedNode);
//        ////
//        ////					foreach (var nextNode in nodes.Last ().NextNodes)
//        ////						stack.Push (nextNode);
//        ////				}
//        ////				return graph;
//        ////			}
//        ////
//        ////			void FindMultipleEnumeration (VariableReferenceNode startNode)
//        ////			{
//        ////				var vrg = GetAssignmentUsageGraph (startNode);
//        ////				visitedNodes = new HashSet<VariableReferenceNode> ();
//        ////				collectedNodes = new HashSet<VariableReferenceNode> ();
//        ////
//        ////				// degree of a node is the number of references that can be reached by the node
//        ////				nodeDegree = new Dictionary<VariableReferenceNode, int> ();
//        ////
//        ////				foreach (var node in vrg) {
//        ////					if (node.References.Count == 0 || !visitedNodes.Add (node))
//        ////						continue;
//        ////					ProcessNode (node);
//        ////					if (nodeDegree [node] > 1)
//        ////						collectedNodes.Add (node);
//        ////				}
//        ////				foreach (var node in collectedNodes)
//        ////					AddDiagnosticAnalyzers (node.References);
//        ////			}
//        ////
//        ////			void ProcessNode (VariableReferenceNode node)
//        ////			{
//        ////				var degree = nodeDegree [node] = 0;
//        ////				foreach (var nextNode in node.NextNodes) {
//        ////					collectedNodes.Add (nextNode);
//        ////					if (visitedNodes.Add (nextNode))
//        ////						ProcessNode (nextNode);
//        ////					degree = Math.Max (degree, nodeDegree [nextNode]);
//        ////				}
//        ////				nodeDegree [node] = degree + node.References.Count;
//        ////			}
//        //		}
//    }
//}