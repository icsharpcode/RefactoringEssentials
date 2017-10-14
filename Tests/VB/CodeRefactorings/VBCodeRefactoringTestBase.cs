using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.VisualBasic;
using Xunit;

namespace RefactoringEssentials.Tests.VB.CodeRefactorings
{
	public abstract class VBCodeRefactoringTestBase : CodeRefactoringTestBase
    {
        public void Test<T>(string input, string output, int action = 0, bool expectErrors = false, VisualBasicParseOptions parseOptions = null)
            where T : CodeRefactoringProvider, new()
        {
            Test(new T(), input, output, action, expectErrors, parseOptions);
        }

        public void Test(CodeRefactoringProvider provider, string input, string output, int action = 0, bool expectErrors = false, VisualBasicParseOptions parseOptions = null)
        {
            string result = HomogenizeEol(RunContextAction(provider, HomogenizeEol(input), action, expectErrors, parseOptions));
            output = HomogenizeEol(output);
            bool passed = result == output;
            if (!passed)
            {
                Console.WriteLine("-----------Expected:");
                Console.WriteLine(output);
                Console.WriteLine("-----------Got:");
                Console.WriteLine(result);
            }
            Assert.Equal(output, result);
        }

        internal static List<Microsoft.CodeAnalysis.CodeActions.CodeAction> GetActions<T>(string input) where T : CodeRefactoringProvider, new()
        {
            DiagnosticTestBase.TestWorkspace workspace;
            Document doc;
            return GetActions(new T(), input, out workspace, out doc);
        }

        static List<CodeAction> GetActions(CodeRefactoringProvider action, string input, out DiagnosticTestBase.TestWorkspace workspace, out Document doc, VisualBasicParseOptions parseOptions = null)
        {
            TextSpan selectedSpan;
            TextSpan markedSpan;
            string text = ParseText(input, out selectedSpan, out markedSpan);
            workspace = new DiagnosticTestBase.TestWorkspace();
            var projectId = ProjectId.CreateNewId();
            var documentId = DocumentId.CreateNewId(projectId);
            if (parseOptions == null)
            {
                parseOptions = new VisualBasicParseOptions(
                    LanguageVersion.VisualBasic14,
                    DocumentationMode.Diagnose | DocumentationMode.Parse,
                    SourceCodeKind.Regular,
                    ImmutableArray.Create(
                        new KeyValuePair<string, object>("DEBUG", null),
                        new KeyValuePair<string, object>("TEST", null))
                );
            }
            //workspace.Options.WithChangedOption(VisualBasicFormattingOptions.NewLinesForBracesInControlBlocks, false);
            workspace.Open(ProjectInfo.Create(
                projectId,
                VersionStamp.Create(),
                "TestProject",
                "TestProject",
                LanguageNames.VisualBasic,
                null,
                null,
                new VisualBasicCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    "",
                    "",
                    "Script",
                    null,
                    null,
                    OptionStrict.Off,
                    true,
                    true,
                    false,
                    parseOptions
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

        protected string RunContextAction(CodeRefactoringProvider action, string input, int actionIndex = 0, bool expectErrors = false, VisualBasicParseOptions parseOptions = null)
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


        protected void TestWrongContext(CodeRefactoringProvider action, string input)
        {
            Document doc;
            DiagnosticTestBase.TestWorkspace workspace;
            var actions = GetActions(action, input, out workspace, out doc);
            Assert.True(actions == null || actions.Count == 0, action.GetType() + " shouldn't be valid there.");
        }


        protected void TestWrongContext<T>(string input) where T : CodeRefactoringProvider, new()
        {
            TestWrongContext(new T(), input);
        }

        //		protected List<CodeAction> GetActions<T> (string input) where T : CodeActionProvider, new ()
        //		{
        //			var ctx = TestRefactoringContext.Create(input);
        //			ctx.FormattingOptions = formattingOptions;
        //			return new T().GetActions(ctx).ToList();
        //		}
        //
        //		protected void TestActionDescriptions (CodeActionProvider provider, string input, params string[] expected)
        //		{
        //			var ctx = TestRefactoringContext.Create(input);
        //			ctx.FormattingOptions = formattingOptions;
        //			var actions = provider.GetActions(ctx).ToList();
        //			Assert.Equal(
        //				expected,
        //				actions.Select(a => a.Description).ToArray());
        //		}
    }
}
