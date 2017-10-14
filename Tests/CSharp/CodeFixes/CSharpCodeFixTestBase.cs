using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using RefactoringEssentials.Tests.CSharp.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Xunit;
using System.Text;

namespace RefactoringEssentials.Tests.CSharp.CodeFixes
{
	public abstract class CSharpCodeFixTestBase : CodeFixTestBase
    {
        public void Test<T>(string input, string output, int action = 0, bool expectErrors = false, CSharpParseOptions parseOptions = null)
            where T : CodeFixProvider, new()
        {
            Test(new T(), input, output, action, expectErrors, parseOptions);
        }

        public void Test(CodeFixProvider provider, string input, string output, int action = 0, bool expectErrors = false, CSharpParseOptions parseOptions = null)
        {
            string result = HomogenizeEol(RunContextAction(provider, HomogenizeEol(input), action, expectErrors, parseOptions));
            output = HomogenizeEol(output);
            bool passed = result == output;
            if (!passed)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("-----------Expected:");
                sb.AppendLine(output);
                sb.AppendLine("-----------Got:");
                sb.AppendLine(result);
                Assert.True(passed, sb.ToString());
            }
        }

        internal static List<Microsoft.CodeAnalysis.CodeActions.CodeAction> GetActions<T>(string input) where T : CodeFixProvider, new()
        {
            DiagnosticTestBase.TestWorkspace workspace;
            Document doc;
            return GetActions(new T(), input, out workspace, out doc);
        }

        static List<CodeAction> GetActions(CodeFixProvider action, string input, out DiagnosticTestBase.TestWorkspace workspace, out Document doc, CSharpParseOptions parseOptions = null)
        {
            TextSpan selectedSpan;
            string text = ParseText(input, out selectedSpan);
            workspace = new DiagnosticTestBase.TestWorkspace();
            var projectId = ProjectId.CreateNewId();
            var documentId = DocumentId.CreateNewId(projectId);
            if (parseOptions == null)
            {
                parseOptions = new CSharpParseOptions(
                    LanguageVersion.CSharp6,
                    DocumentationMode.Diagnose | DocumentationMode.Parse,
                    SourceCodeKind.Regular,
                    ImmutableArray.Create("DEBUG", "TEST")
                );
            }
            workspace.Options.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInControlBlocks, false);
            workspace.Open(ProjectInfo.Create(
                projectId,
                VersionStamp.Create(),
                "TestProject",
                "TestProject",
                LanguageNames.CSharp,
                null,
                null,
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    false,
                    "",
                    "",
                    "Script",
                    null,
                    OptimizationLevel.Debug,
                    false,
                    true
                ),
                parseOptions,
                new[] {
                    DocumentInfo.Create(
                        documentId,
                        "a.cs",
                        null,
                        SourceCodeKind.Regular,
                        TextLoader.From(TextAndVersion.Create(SourceText.From(text), VersionStamp.Create()))
                    )
                },
                null,
                CSharpDiagnosticTestBase.DefaultMetadataReferences
            )
            );
            doc = workspace.CurrentSolution.GetProject(projectId).GetDocument(documentId);
            var actions = new List<Tuple<CodeAction, ImmutableArray<Diagnostic>>>();
            var model = doc.GetSemanticModelAsync().GetAwaiter().GetResult();
            var diagnostics = model.GetDiagnostics();
            if (diagnostics.Length == 0)
                return new List<CodeAction>();

            foreach (var d in diagnostics)
            {
                if (action.FixableDiagnosticIds.Contains(d.Id))
                {
                    if (selectedSpan.Start > 0)
                        Assert.True(selectedSpan == d.Location.SourceSpan, "Activation span does not match.");

                    var context = new CodeFixContext(doc, d.Location.SourceSpan, diagnostics.Where(d2 => d2.Location.SourceSpan == d.Location.SourceSpan).ToImmutableArray(), (arg1, arg2) => actions.Add(Tuple.Create(arg1, arg2)), default(CancellationToken));
                    action.RegisterCodeFixesAsync(context);
                }
            }


            return actions.Select(a => a.Item1).ToList();
        }

        protected string RunContextAction(CodeFixProvider action, string input, int actionIndex = 0, bool expectErrors = false, CSharpParseOptions parseOptions = null)
        {
            Document doc;
            DiagnosticTestBase.TestWorkspace workspace;
            var actions = GetActions(action, input, out workspace, out doc, parseOptions);
            if (actions.Count < actionIndex)
                Console.WriteLine("invalid input is:" + input);
            var a = actions[actionIndex];
            foreach (var op in a.GetOperationsAsync(default(CancellationToken)).GetAwaiter().GetResult())
            {
                op.Apply(workspace, default(CancellationToken));
            }
            return workspace.CurrentSolution.GetDocument(doc.Id).GetTextAsync().GetAwaiter().GetResult().ToString();
        }

        protected void TestWrongContext<T>(string input) where T : CodeFixProvider, new()
        {
            TestWrongContext(new T(), input);
        }

        protected void TestWrongContext(CodeFixProvider action, string input)
        {
            Document doc;
            DiagnosticTestBase.TestWorkspace workspace;
            var actions = GetActions(action, input, out workspace, out doc);
            Assert.True(actions == null || actions.Count == 0, action.GetType() + " shouldn't be valid there.");
        }
    }
}
