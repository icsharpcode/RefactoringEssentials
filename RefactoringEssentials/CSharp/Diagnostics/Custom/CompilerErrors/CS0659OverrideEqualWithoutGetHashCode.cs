namespace RefactoringEssentials.CSharp.Diagnostics
{
    //[DiagnosticAnalyzer(LanguageNames.CSharp)]
    //public class CS0659ClassOverrideEqualsWithoutGetHashCode : DiagnosticAnalyzer
    //{
    //	internal const string DiagnosticId = "CS0659ClassOverrideEqualsWithoutGetHashCode";
    //	const string Description = "If two objects are equal then they must both have the same hash code";
    //	const string MessageFormat = "Add 'GetHashCode' method";
    //	const string Category = DiagnosticAnalyzerCategories.CompilerWarnings;

    //	static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, DiagnosticSeverity.Warning, true, "CS0659: Class overrides Object.Equals but not Object.GetHashCode.");

    //	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    //	{
    //		get
    //		{
    //			return ImmutableArray.Create(Rule);
    //		}
    //	}

    //	public override void Initialize(AnalysisContext context)
    //	{
    //		//context.RegisterSyntaxNodeAction(
    //		//	(nodeContext) => {
    //		//		Diagnostic diagnostic;
    //		//		if (TryGetDiagnostic (nodeContext, out diagnostic)) {
    //		//			nodeContext.ReportDiagnostic(diagnostic);
    //		//		}
    //		//	}, 
    //		//	new SyntaxKind[] {  SyntaxKind.None }
    //		//);
    //	}

    //	static bool TryGetDiagnostic (SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
    //	{
    //		diagnostic = default(Diagnostic);
    //		//var node = nodeContext.Node as ;
    //		//diagnostic = Diagnostic.Create (descriptor, node.GetLocation ());
    //		//return true;
    //		return false;
    //	}

    //	//class GatherVisitor : GatherVisitorBase<CS0659ClassOverrideEqualsWithoutGetHashCode>
    //	//{
    //	//	public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
    //	//		: base(semanticModel, addDiagnostic, cancellationToken)
    //	//	{
    //	//	}

    //	//	public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    //	//	{
    //	//		base.VisitMethodDeclaration(node);

    //	//		var methodSymbol = semanticModel.GetDeclaredSymbol(node);
    //	//		if (methodSymbol == null || !methodSymbol.Name.Equals("Equals") || !methodSymbol.IsOverride ||
    //	//			methodSymbol.Parameters.Count() != 1 || (!methodSymbol.Parameters.Single().Type.GetFullName().Equals("object") &&
    //	//			!methodSymbol.Parameters.Single().Type.GetFullName().Equals("System.Object")))
    //	//			return;

    //	//		var classSymbol = methodSymbol.ContainingType;
    //	//		if (classSymbol == null)
    //	//			return;

    //	//		var hashCode = classSymbol.GetMembers().OfType<IMethodSymbol>().Where(m => m.Name.Equals("GetHashCode"));
    //	//		if (hashCode.Count() == 0 || !hashCode.Any(h => (h.IsOverride && (h.ReturnType.GetFullName().Equals("System.Int32") || h.ReturnType.GetFullName().Equals("int")))))
    //	//			AddDiagnosticAnalyzer(Diagnostic.Create(Rule, node.Identifier.GetLocation()));
    //	//	}
    //	//}
    //}

    //[ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
    //public class CS0659ClassOverrideEqualsWithoutGetHashCodeFixProvider : CodeFixProvider
    //{
    //	public override ImmutableArray<string> FixableDiagnosticIds {
    //		get {
    //			return ImmutableArray.Create (CS0659ClassOverrideEqualsWithoutGetHashCode.DiagnosticId);
    //		}
    //	}

    //	public override FixAllProvider GetFixAllProvider()
    //	{
    //		return WellKnownFixAllProviders.BatchFixer;
    //	}

    //	public async override Task RegisterCodeFixesAsync(CodeFixContext context)
    //	{
    //		var document = context.Document;
    //		var cancellationToken = context.CancellationToken;
    //		var span = context.Span;
    //		var diagnostics = context.Diagnostics;
    //		var root = await document.GetSyntaxRootAsync(cancellationToken);
    //		var diagnostic = diagnostics.First ();
    //		var node = root.FindNode(context.Span);
    //		var hashCode = SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)), "GetHashCode").WithModifiers(
    //			new SyntaxTokenList().Add(SyntaxFactory.Token(SyntaxKind.PublicKeyword)).Add(SyntaxFactory.Token(SyntaxKind.OverrideKeyword)))
    //			.WithBody(SyntaxFactory.Block(
    //			SyntaxFactory.ReturnStatement().WithExpression(SyntaxFactory.ParseExpression("base.GetHashCode()")))).WithAdditionalAnnotations(Formatter.Annotation);
    //		var newRoot = root.InsertNodesAfter(node, new List<SyntaxNode>() { hashCode });
    //		context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, diagnostic.GetMessage(), document.WithSyntaxRoot(newRoot)), diagnostic);
    //	}
    //}
}