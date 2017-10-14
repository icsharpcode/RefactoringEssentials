using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;

namespace RefactoringEssentials.Tests.VB.Diagnostics
{
	public class VBDiagnosticTestBase : DiagnosticTestBase
    {
        public static VisualBasicCompilation CreateCompilation(
            IEnumerable<SyntaxTree> trees,
            IEnumerable<MetadataReference> references = null,
            VisualBasicCompilationOptions compOptions = null,
            string assemblyName = "")
        {
            if (compOptions == null)
            {
                compOptions = new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary, "a.dll");
            }

            return VisualBasicCompilation.Create(
                string.IsNullOrEmpty(assemblyName) ? GetUniqueName() : assemblyName,
                trees,
                references,
                compOptions);
        }


        public static VisualBasicCompilation CreateCompilationWithMscorlib(
            IEnumerable<SyntaxTree> source,
            IEnumerable<MetadataReference> references = null,
            VisualBasicCompilationOptions compOptions = null,
            string assemblyName = "")
        {
            var refs = new List<MetadataReference>();
            if (references != null)
            {
                refs.AddRange(references);
            }

            refs.AddRange(DefaultMetadataReferences);

            return CreateCompilation(source, refs, compOptions, assemblyName);
        }

        protected static void Analyze<T>(string input, string output = null, int issueToFix = -1, int actionToRun = 0, Action<int, Diagnostic> diagnosticCheck = null) where T : DiagnosticAnalyzer, new()
        {
            Analyze<T>(
                t => VisualBasicSyntaxTree.ParseText(t),
                s => CreateCompilationWithMscorlib(s),
                LanguageNames.VisualBasic,
                input, output, issueToFix, actionToRun, diagnosticCheck
                );
        }

        protected static void AnalyzeWithRule<T>(string input, string ruleId, string output = null, int issueToFix = -1, int actionToRun = 0, Action<int, Diagnostic> diagnosticCheck = null) where T : DiagnosticAnalyzer, new()
        {
            AnalyzeWithRule<T>(
                t => VisualBasicSyntaxTree.ParseText(t),
                s => CreateCompilationWithMscorlib(s),
                LanguageNames.VisualBasic,
                input, ruleId, output, issueToFix, actionToRun, diagnosticCheck
                );
        }
    }
}

