using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using RefactoringEssentials.Tests.CSharp.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Simplification;
using Xunit;
using System.Text;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
	public abstract class CSharpCodeRefactoringTestBase : CodeRefactoringTestBase
    {
        public void Test<T>(string input, string output, int action = 0, bool expectErrors = false, CSharpParseOptions parseOptions = null)
            where T : CodeRefactoringProvider, new()
        {
            Test(new T(), input, output, action, expectErrors, parseOptions);
        }

        public void Test(CodeRefactoringProvider provider, string input, string output, int action = 0, bool expectErrors = false, CSharpParseOptions parseOptions = null)
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

        internal static List<CodeAction> GetActions<T>(string input) where T : CodeRefactoringProvider, new()
        {
            DiagnosticTestBase.TestWorkspace workspace;
            Document doc;
            return GetActions(new T(), input, out workspace, out doc);
        }

        static List<CodeAction> GetActions(CodeRefactoringProvider action, string input, out CSharpDiagnosticTestBase.TestWorkspace workspace, out Document doc, CSharpParseOptions parseOptions = null)
        {
            TextSpan selectedSpan;
            TextSpan markedSpan;
            string text = ParseText(input, out selectedSpan, out markedSpan);
            workspace = new CSharpDiagnosticTestBase.TestWorkspace();
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
            workspace.Options = workspace.Options
                .WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInControlBlocks, true)
                .WithChangedOption(SimplificationOptions.PreferIntrinsicPredefinedTypeKeywordInDeclaration, LanguageNames.CSharp, true)
                .WithChangedOption(SimplificationOptions.PreferIntrinsicPredefinedTypeKeywordInMemberAccess, LanguageNames.CSharp, true);
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
                DiagnosticTestBase.DefaultMetadataReferences
            )
            );
            doc = workspace.CurrentSolution.GetProject(projectId).GetDocument(documentId);
            var actions = new List<CodeAction>();
            var context = new CodeRefactoringContext(doc, selectedSpan, actions.Add, default(CancellationToken));
            action.ComputeRefactoringsAsync(context).GetAwaiter().GetResult();
            if (markedSpan.Start > 0)
            {
                foreach (var nra in actions.OfType<NRefactoryCodeAction>())
                {
                    Assert.True(markedSpan == nra.TextSpan, "Activation span does not match.");
                }
            }
            return actions;
        }

        protected string RunContextAction(CodeRefactoringProvider action, string input, int actionIndex = 0, bool expectErrors = false, CSharpParseOptions parseOptions = null)
        {
            Document doc;
            CSharpDiagnosticTestBase.TestWorkspace workspace;
            var actions = GetActions(action, input, out workspace, out doc, parseOptions);
            if (actions.Count < actionIndex)
                Console.WriteLine("invalid input is:" + input);
            var a = actions[actionIndex];
            foreach (var op in a.GetOperationsAsync(default(CancellationToken)).GetAwaiter().GetResult())
            {
                op.Apply(workspace, default(CancellationToken));
            }
            var result = workspace.CurrentSolution.GetDocument(doc.Id).GetTextAsync().GetAwaiter().GetResult().ToString();
            if (Environment.NewLine != "\r\n")
                result = result.Replace("\r\n", Environment.NewLine);
            return result;
        }


        protected void TestWrongContext(CodeRefactoringProvider action, string input)
        {
            Document doc;
            CSharpDiagnosticTestBase.TestWorkspace workspace;
            var actions = GetActions(action, input, out workspace, out doc);
            Assert.True(actions == null || actions.Count == 0, action.GetType() + " shouldn't be valid there.");
        }

        protected void TestWrongContext<T>(string input) where T : CodeRefactoringProvider, new()
        {
            TestWrongContext(new T(), input);
        }
    }
}
