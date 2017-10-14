using System;
using System.Linq;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using System.Threading;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.CodeActions;
using RefactoringEssentials.Tests.Common;
using Xunit;

namespace RefactoringEssentials.Tests
{
	public abstract class DiagnosticTestBase
    {
        static MetadataReference mscorlib;
        static MetadataReference systemAssembly;
        static MetadataReference systemXmlLinq;
        static MetadataReference systemCore;

        internal static MetadataReference[] DefaultMetadataReferences;

        static Dictionary<string, CodeFixProvider> providers = new Dictionary<string, CodeFixProvider>();

        static DiagnosticTestBase()
        {
            try
            {
                mscorlib = MetadataReference.CreateFromFile(typeof(Console).Assembly.Location);
                systemAssembly = MetadataReference.CreateFromFile(typeof(System.ComponentModel.BrowsableAttribute).Assembly.Location);
                systemXmlLinq = MetadataReference.CreateFromFile(typeof(System.Xml.Linq.XElement).Assembly.Location);
                systemCore = MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);
                DefaultMetadataReferences = new[] {
                    mscorlib,
                    systemAssembly,
                    systemCore,
                    systemXmlLinq
                };

                foreach (var provider in typeof(DiagnosticAnalyzerCategories).Assembly.GetTypes().Where(t => t.GetCustomAttributes(typeof(ExportCodeFixProviderAttribute), false).Length > 0))
                {
                    //var attr = (ExportCodeFixProviderAttribute)provider.GetCustomAttributes(typeof(ExportCodeFixProviderAttribute), false) [0];
                    var codeFixProvider = (CodeFixProvider)Activator.CreateInstance(provider);
                    foreach (var id in codeFixProvider.FixableDiagnosticIds)
                    {
                        if (providers.ContainsKey(id))
                        {
                            Console.WriteLine("Provider " + id + " already added.");
                            continue;
                        }
                        providers.Add(id, codeFixProvider);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static string GetUniqueName()
        {
            return Guid.NewGuid().ToString("D");
        }

        internal class TestWorkspace : Workspace
        {
            readonly static HostServices services = Microsoft.CodeAnalysis.Host.Mef.MefHostServices.DefaultHost;/* MefHostServices.Create(new [] { 
				typeof(MefHostServices).Assembly,
				typeof(Microsoft.CodeAnalysis.CSharp.Formatting.CSharpFormattingOptions).Assembly
			});*/


            public TestWorkspace(string workspaceKind = "Test") : base(services, workspaceKind)
            {
                /*
                foreach (var a in MefHostServices.DefaultAssemblies)
                {
                    Console.WriteLine(a.FullName);
                }*/
            }

            public void ChangeDocument(DocumentId id, SourceText text)
            {
                ApplyDocumentTextChanged(id, text);
            }

            protected override void ApplyDocumentTextChanged(DocumentId id, SourceText text)
            {
                base.ApplyDocumentTextChanged(id, text);
                var document = CurrentSolution.GetDocument(id);
                if (document != null)
                    OnDocumentTextChanged(id, text, PreservationMode.PreserveValue);
            }

            public override bool CanApplyChange(ApplyChangesKind feature)
            {
                return true;
            }

            public void Open(ProjectInfo projectInfo)
            {
                var sInfo = SolutionInfo.Create(
                                SolutionId.CreateNewId(),
                                VersionStamp.Create(),
                                null,
                                new[] { projectInfo }
                            );
                OnSolutionAdded(sInfo);
            }
        }

        protected static void RunFix(Workspace workspace, ProjectId projectId, DocumentId documentId, Diagnostic diagnostic, int index = 0)
        {
            CodeFixProvider provider;
            if (providers.TryGetValue(diagnostic.Id, out provider))
            {
                Assert.True(provider != null, "null provider for : " + diagnostic.Id);
                var document = workspace.CurrentSolution.GetProject(projectId).GetDocument(documentId);
                var actions = new List<CodeAction>();
                var context = new CodeFixContext(document, diagnostic, (fix, diags) => actions.Add(fix), default(CancellationToken));
                provider.RegisterCodeFixesAsync(context).GetAwaiter().GetResult();
                if (!actions.Any())
                {
                    Assert.True(false, "Provider has no fix for " + diagnostic.Id + " at " + diagnostic.Location.SourceSpan);
                    return;
                }
                foreach (var op in actions[index].GetOperationsAsync(default(CancellationToken)).GetAwaiter().GetResult())
                {
                    op.Apply(workspace, default(CancellationToken));
                }
            }
            else
            {
                Assert.True(false, "No code fix provider found for :" + diagnostic.Id);
            }
        }

        protected static void Test<T>(string input, int expectedDiagnostics = 1, string output = null, int issueToFix = -1, int actionToRun = 0) where T : DiagnosticAnalyzer, new()
        {
            Assert.True(false, "Use Analyze");
        }

        protected static void Test<T>(string input, string output, int fixIndex = 0)
            where T : DiagnosticAnalyzer, new()
        {
            Assert.True(false, "Use Analyze");
        }

        protected static void TestIssue<T>(string input, int issueCount = 1)
            where T : DiagnosticAnalyzer, new()
        {
            Assert.True(false, "Use Analyze");
        }

        protected static void TestWrongContextWithSubIssue<T>(string input, string id) where T : DiagnosticAnalyzer, new()
        {
            Assert.True(false, "Use AnalyzeWithRule");
        }

        protected static void TestWithSubIssue<T>(string input, string output, string subIssue, int fixIndex = 0) where T : DiagnosticAnalyzer, new()
        {
            Assert.True(false, "Use AnalyzeWithRule");
        }

        class TestDiagnosticAnalyzer<T> : DiagnosticAnalyzer
        {
            readonly DiagnosticAnalyzer t;

            public TestDiagnosticAnalyzer(DiagnosticAnalyzer t)
            {
                this.t = t;
            }

            #region IDiagnosticAnalyzer implementation
            public override void Initialize(AnalysisContext context)
            {
                t.Initialize(context);
            }

            public override System.Collections.Immutable.ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return t.SupportedDiagnostics;
                }
            }
            #endregion
        }

        protected static TextSpan GetWholeSpan(Diagnostic d)
        {
            int start = d.Location.SourceSpan.Start;
            int end = d.Location.SourceSpan.End;
            foreach (var a in d.AdditionalLocations)
            {
                start = Math.Min(start, a.SourceSpan.Start);
                end = Math.Max(start, a.SourceSpan.End);
            }
            return TextSpan.FromBounds(start, end);
        }

        protected static void Analyze<T>(Func<string, SyntaxTree> parseTextFunc, Func<SyntaxTree[], Compilation> createCompilationFunc, string language, string input, string output = null, int issueToFix = -1, int actionToRun = 0, Action<int, Diagnostic> diagnosticCheck = null) where T : DiagnosticAnalyzer, new()
        {
            var text = new StringBuilder();

            var expectedDiagnosics = new List<TextSpan>();
            int start = -1;
            for (int i = 0; i < input.Length; i++)
            {
                char ch = input[i];
                if (ch == '$' && ((i > 0) && (input[i - 1] == '$')))
                {
                    // Ignore 2nd "$" in "$$"
                }
                else if (ch == '$' && (i + 1 >= input.Length || input[i + 1] != '$'))
                {
                    if (start < 0)
                    {
                        start = text.Length;
                        continue;
                    }
                    expectedDiagnosics.Add(TextSpan.FromBounds(start, text.Length));
                    start = -1;
                }
                else
                {
                    text.Append(ch);
                }
            }

            var syntaxTree = parseTextFunc(text.ToString());

            Compilation compilation = createCompilationFunc(new[] { syntaxTree });

            var diagnostics = new List<Diagnostic>();

            var compilationWithAnalyzers = compilation.WithAnalyzers(System.Collections.Immutable.ImmutableArray<DiagnosticAnalyzer>.Empty.Add(new T()));
            var result = compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult();
            diagnostics.AddRange(result);

            diagnostics.Sort((d1, d2) => d1.Location.SourceSpan.Start.CompareTo(d2.Location.SourceSpan.Start));
            expectedDiagnosics.Sort((d1, d2) => d1.Start.CompareTo(d2.Start));

            if (expectedDiagnosics.Count != diagnostics.Count)
            {
                foreach (var diag in diagnostics)
                {
                    Console.WriteLine(diag.Id + "/" + diag.GetMessage() + "/" + diag.Location.SourceSpan);
                }
                Assert.True(false, "Diagnostic count mismatch expected: " + expectedDiagnosics.Count + " was " + diagnostics.Count);
            }

            for (int i = 0; i < expectedDiagnosics.Count; i++)
            {
                var d = diagnostics[i];
                var wholeSpan = GetWholeSpan(d);
                if (wholeSpan != expectedDiagnosics[i])
                {
                    Assert.True(false, "Diagnostic " + i + " span mismatch expected: " + expectedDiagnosics[i] + " but was " + wholeSpan);
                }
                if (diagnosticCheck != null)
                    diagnosticCheck(i, d);
            }

            if (output == null)
                return;

            var workspace = new TestWorkspace();
            var projectId = ProjectId.CreateNewId();
            var documentId = DocumentId.CreateNewId(projectId);
            workspace.Open(ProjectInfo.Create(
                projectId,
                VersionStamp.Create(),
                "a", "a.exe", language, null, null, null, null,
                new[] {
                    DocumentInfo.Create(
                        documentId,
                        "a.cs",
                        null,
                        SourceCodeKind.Regular,
                        TextLoader.From(TextAndVersion.Create(SourceText.From(text.ToString()), VersionStamp.Create())))
                }
            ));
            if (issueToFix < 0)
            {
                diagnostics.Reverse();
                foreach (var v in diagnostics)
                {
                    RunFix(workspace, projectId, documentId, v);
                }
            }
            else
            {
                RunFix(workspace, projectId, documentId, diagnostics.ElementAt(issueToFix), actionToRun);
            }

            var txt = workspace.CurrentSolution.GetProject(projectId).GetDocument(documentId).GetTextAsync().GetAwaiter().GetResult().ToString();
            output = Utils.HomogenizeEol(output);
            txt =  Utils.HomogenizeEol(txt);
            if (output != txt)
            {
				StringBuilder sb = new StringBuilder();
                sb.AppendLine("expected:");
				sb.AppendLine(output);
				sb.AppendLine("got:");
				sb.AppendLine(txt);
				sb.AppendLine("-----Mismatch:");
                for (int i = 0; i < txt.Length; i++)
                {
                    if (i >= output.Length)
                    {
                        sb.Append("#");
                        continue;
                    }
                    if (txt[i] != output[i])
                    {
						sb.Append("#");
                        continue;
                    }
					sb.Append(txt[i]);
                }
                Assert.True(false, sb.ToString());
            }
        }

        protected static void AnalyzeWithRule<T>(Func<string, SyntaxTree> parseTextFunc, Func<SyntaxTree[], Compilation> createCompilationFunc, string language, string input, string ruleId, string output = null, int issueToFix = -1, int actionToRun = 0, Action<int, Diagnostic> diagnosticCheck = null) where T : DiagnosticAnalyzer, new()
        {
            var text = new StringBuilder();

            var expectedDiagnosics = new List<TextSpan>();
            int start = -1;
            for (int i = 0; i < input.Length; i++)
            {
                char ch = input[i];
                if (ch == '$')
                {
                    if (start < 0)
                    {
                        start = text.Length;
                        continue;
                    }
                    expectedDiagnosics.Add(TextSpan.FromBounds(start, text.Length));
                    start = -1;
                }
                else
                {
                    text.Append(ch);
                }
            }

            var syntaxTree = parseTextFunc(text.ToString());

            Compilation compilation = createCompilationFunc(new[] { syntaxTree });

            var diagnostics = new List<Diagnostic>();
            var compilationWithAnalyzers = compilation.WithAnalyzers(System.Collections.Immutable.ImmutableArray<DiagnosticAnalyzer>.Empty.Add(new T()));
            diagnostics.AddRange(compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult());


            if (expectedDiagnosics.Count != diagnostics.Count)
            {
                Console.WriteLine("Diagnostics: " + diagnostics.Count);
                foreach (var diag in diagnostics)
                {
                    Console.WriteLine(diag.Id + "/" + diag.GetMessage());
                }
                Assert.True(false, "Diagnostic count mismatch expected: " + expectedDiagnosics.Count + " but was:" + diagnostics.Count);
            }

            for (int i = 0; i < expectedDiagnosics.Count; i++)
            {
                var d = diagnostics[i];
                var wholeSpan = GetWholeSpan(d);
                if (wholeSpan != expectedDiagnosics[i])
                {
                    Assert.True(false, "Diagnostic " + i + " span mismatch expected: " + expectedDiagnosics[i] + " but was " + wholeSpan);
                }
                if (diagnosticCheck != null)
                    diagnosticCheck(i, d);
            }

            if (output == null)
                return;

            var workspace = new TestWorkspace();
            var projectId = ProjectId.CreateNewId();
            var documentId = DocumentId.CreateNewId(projectId);
            workspace.Open(ProjectInfo.Create(
                projectId,
                VersionStamp.Create(),
                "", "", language, null, null, null, null,
                new[] {
                    DocumentInfo.Create(
                        documentId,
                        "a.cs",
                        null,
                        SourceCodeKind.Regular,
                        TextLoader.From(TextAndVersion.Create(SourceText.From(text.ToString()), VersionStamp.Create())))
                }
            ));
            if (issueToFix < 0)
            {
                diagnostics.Reverse();
                foreach (var v in diagnostics)
                {
                    RunFix(workspace, projectId, documentId, v);
                }
            }
            else
            {
                RunFix(workspace, projectId, documentId, diagnostics.ElementAt(issueToFix), actionToRun);
            }

            var txt = workspace.CurrentSolution.GetProject(projectId).GetDocument(documentId).GetTextAsync().GetAwaiter().GetResult().ToString();
            txt = Utils.HomogenizeEol(txt);
            output = Utils.HomogenizeEol(output);
            if (output != txt)
            {
				StringBuilder sb = new StringBuilder();
				sb.AppendLine("expected:");
				sb.AppendLine(output);
				sb.AppendLine("got:");
				sb.AppendLine(txt);
                Assert.True(false, sb.ToString());
            }
        }
    }
}

