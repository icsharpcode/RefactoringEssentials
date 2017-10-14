using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Samples.CSharp
{
    // PLEASE UNCOMMENT THIS LINE TO REGISTER ANALYZER IN IDE.
    //[DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SampleAnalyzer : DiagnosticAnalyzer
    {
        // An analyzer is responsible for finding code elements that are marked in IDE or the build process as
        // potential code issues or suggestions for improvement.
        // An analyzer itself does not offer a fix for the potential problem, it just marks code.
        // But its possible to implement an optional code fix (a CodeFixProvider, see SampleCodeFixProvider.cs) and link
        // it to an analyzer. Then user is able to fix a code issue in place.
        //
        //
        // The DiagnosticDescriptor defines how a suggested fix is presented to user:
        //
        // ID:
        //     Every analyzer has a unique ID. RefactoringEssentials defines all IDs for C# analyzers at one place: CSharpDiagnosticIDs.
        //     The ID is used to link a CodeFixProvider to an analyzer.
        // Title & Message:
        //     This text is shown in compiler output, Visual Studio's error list or in popup shown when hovering the finding in editor.
        // Category:
        //     Freely definable category, but RefactoringEssentials uses the list defined in DiagnosticAnalyzerCategories.
        // Severity:
        //     This defines how to mark the finding: Fading, if a code element is redundant, showing as warning, underlining as error etc.

        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.SampleAnalyzerID,
            GettextCatalog.GetString("Sample analyzer: Class name should not have a 'C' prefix."),
            GettextCatalog.GetString("Sample analyzer: Class name should not have a 'C' prefix."),
            DiagnosticAnalyzerCategories.CodeQualityIssues,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.SampleAnalyzerID)
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            // Use this configuration to improve analyzer's performance by allowing Roslyn to execute it concurrently.
            // Be careful if the analyzer instance has any dependency on an external state (like configuration or other analyzers).
            context.EnableConcurrentExecution();

            // Use this configuration to prevent the analyzer from creating warnings on generated code
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            // With SyntaxNodeActions we can register a delegate executed for
            // all nodes of given SyntaxKinds. In this delegate we create a Diagnostic
            // object for our analyzer.
            //
            // In this example we want to mark all class names which are prefixed with a "C"
            // as bad naming style. Therefore we register our delegate with class declaration
            // nodes.
            context.RegisterSyntaxNodeAction(
                (nodeContext) =>
                {
                    Diagnostic diagnostic;
                    if (TryGetDiagnostic(nodeContext, out diagnostic))
                        nodeContext.ReportDiagnostic(diagnostic);
                },
                SyntaxKind.ClassDeclaration
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            // Don't forget to initialize the out parameter
            diagnostic = default(Diagnostic);

            // nodeContext is used to retrieve the SyntaxNode for which the delegate has been called.
            var node = nodeContext.Node as ClassDeclarationSyntax;

            string className = node.Identifier.ValueText;
            if (!className.StartsWith("C") || !char.IsUpper(className[1]))
            {
                // Exit if class name doesn't start with a "C" or that "C" is just the start of a word (like in "Char")
                return false;
            }

            // Now we can create the Diagnostic instance using the DiagnosticDescriptor defined earlier.
            // 2nd parameter defines the area which has to be marked. In this example we mark only the class name identifier.
            diagnostic = Diagnostic.Create(
                descriptor,
                node.Identifier.GetLocation()
            );
            return true;
        }
    }
}