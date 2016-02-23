using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace RefactoringEssentials.Tests.Common
{
    /// <summary>
    /// Utility methods for testing actions with workspaces.
    /// </summary>
    static class WorkspaceTestUtil
    {
        /// <summary>
        /// Refactor a workspace with a <see cref="CodeRefactoringProvider"/> type.
        /// The workspace will be searched for an entry point to test refactoring using well known entry point syntax.
        /// </summary>
        /// <typeparam name="T">A <see cref="CodeRefactoringProvider"/> type.</typeparam>
        /// <param name="workspace">A workspace to refactor.</param>
        /// <returns>
        /// A new workspace which is the result of the refactoring.
        /// </returns>
        public static Workspace RunRefactoringProvider<T>(Workspace workspace) where T : CodeRefactoringProvider, new()
        {
            var sln = workspace.CurrentSolution;

            Document doc = null;
            var emptySpan = new TextSpan();
            var selectedSpan = new TextSpan();
            var markedSpan = new TextSpan();

            // Search each project and document for selected and marked text.
            foreach (var project in sln.Projects)
            {
                foreach (var document in project.Documents)
                {
                    TextSpan newSelectedSpan;
                    TextSpan newMarkedSpan;
                    var txt = Utils.ParseText(document.GetTextAsync().Result.ToString(), out newSelectedSpan, out newMarkedSpan);
                    if (newSelectedSpan != emptySpan || markedSpan != emptySpan)
                    {
                        // Fail if multiple documents are demarcated.
                        if (doc != null)
                            Assert.Fail("Multiple text spans must not be chosen.");

                        sln = sln.WithDocumentText(document.Id, SourceText.From(txt));
                        workspace.TryApplyChanges(sln);

                        doc = workspace.CurrentSolution.GetDocument(document.Id);
                        selectedSpan = newSelectedSpan;
                        markedSpan = newMarkedSpan;
                    }
                }
            }

            // Fail if no documents are demarcated.
            if (doc == null)
                Assert.Fail("Text spans must be chosen.");

            // Prepare refactoring context and action.
            var actions = new List<CodeAction>();
            var context = new CodeRefactoringContext(
                doc,
                selectedSpan,
                actions.Add,
                default(CancellationToken));

            var action = new T();
            action.ComputeRefactoringsAsync(context).Wait();

            if (markedSpan.Start > 0)
            {
                foreach (var nra in actions.OfType<NRefactoryCodeAction>())
                {
                    Assert.AreEqual(markedSpan, nra.TextSpan, "Activation span does not match.");
                }
            }

            // Apply code actions to workspace.
            var a = actions[0];
            foreach (var op in a.GetOperationsAsync(default(CancellationToken)).Result)
            {
                op.Apply(workspace, default(CancellationToken));
            }

            return workspace;
        }

        /// <summary>
        /// Assert workspaces are equivalent.
        /// </summary>
        /// <param name="expected">The expected workspace.</param>
        /// <param name="actual">The actual workspace.</param>
        public static void AssertEqual(Workspace expected, Workspace actual)
        {
            var expectedSln = expected.CurrentSolution;
            var actualSln = actual.CurrentSolution;

            AssertEqual(expectedSln, actualSln);
        }

        /// <summary>
        /// Assert solutions are equivalent.
        /// </summary>
        /// <param name="expected">The expected <see cref="Solution"/>.</param>
        /// <param name="actual">The actual <see cref="Solution"/>.</param>
        public static void AssertEqual(Solution expected, Solution actual)
        {
            var expectedSln = expected;
            var actualSln = actual;

            // Assert project counts match.
            if (expectedSln.Projects.Count() != actualSln.Projects.Count())
                Assert.Fail($"Project counts do not match. Expected; {expectedSln.Projects.Count()}, Actual: {actualSln.Projects.Count()}.");

            // Loop through projects, ensure it exists in both solutions and test equivalence.
            foreach (var expProject in expectedSln.Projects)
            {
                var actProject = actualSln.GetProject(expProject.Id);
                if (actProject == null)
                    Assert.Fail($"Project with ID \"{ expProject.Id}\" and name\"{expProject.Name}\" does not exist.");

                AssertEqual(expProject, actProject);
            }
        }

        /// <summary>
        /// Assert projects are equivalent.
        /// </summary>
        /// <param name="expected">The expected <see cref="Project"/>.</param>
        /// <param name="actual">The actual <see cref="Project"/>.</param>
        public static void AssertEqual(Project expected, Project actual)
        {
            // Assert document counts match.
            if (expected.Documents.Count() != actual.Documents.Count())
                Assert.Fail($"Document counts do not match for project named \"{expected.Name}\". Expected; {expected.Documents.Count()}, Actual: {actual.Documents.Count()}.");

            // Loop through documents, ensure it exists in both projects and test equivalence.
            foreach (var expDoc in expected.Documents)
            {
                var actDoc = actual.GetDocument(expDoc.Id);
                if (actDoc == null)
                    Assert.Fail($"Document with ID \"{ expDoc.Id}\" and name\"{expDoc.Name}\" does not exist.");

                AssertEqual(expDoc, actDoc);
            }
        }

        /// <summary>
        /// Assert documents are equivalent.
        /// </summary>
        /// <param name="expected">The expected <see cref="Document"/>.</param>
        /// <param name="actual">The actual <see cref="Document"/>.</param>
        public static void AssertEqual(Document expected, Document actual)
        {
            var expText = Utils.HomogenizeEol(expected.GetTextAsync().Result.ToString());
            var actText = Utils.HomogenizeEol(actual.GetTextAsync().Result.ToString());

            if (!string.Equals(expText, actText))
                Assert.Fail($"Document with name\"{expected.Name}\" does not match. Expected:\n{expText}\nActual:\n{actText}\n");
        }
    }
}
