using System;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
	public class CSharpDiagnosticTestBase : DiagnosticTestBase
    {
        public static CSharpCompilation CreateCompilation(
            IEnumerable<SyntaxTree> trees,
            IEnumerable<MetadataReference> references = null,
            CSharpCompilationOptions compOptions = null,
            string assemblyName = "")
        {
            if (compOptions == null)
            {
                compOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, false, "a.dll");
            }

            return CSharpCompilation.Create(
                string.IsNullOrEmpty(assemblyName) ? GetUniqueName() : assemblyName,
                trees,
                references,
                compOptions);
        }


        public static CSharpCompilation CreateCompilationWithMscorlib(
            IEnumerable<SyntaxTree> source,
            IEnumerable<MetadataReference> references = null,
            CSharpCompilationOptions compOptions = null,
            string assemblyName = "")
        {
            var refs = new List<MetadataReference>();

            refs.AddRange(DefaultMetadataReferences);

            if (references != null)
            {
                refs.AddRange(references);
            }

            return CreateCompilation(source, refs, compOptions, assemblyName);
        }

        protected static void Analyze<T>(string input, string output = null, int issueToFix = -1, int actionToRun = 0, Action<int, Diagnostic> diagnosticCheck = null, IEnumerable<MetadataReference> references = null) where T : DiagnosticAnalyzer, new()
        {
            Analyze<T>(
                t => CSharpSyntaxTree.ParseText(t),
                s => CreateCompilationWithMscorlib(s, references),
                LanguageNames.CSharp,
                input, output, issueToFix, actionToRun, diagnosticCheck
                );
        }

        protected static void AnalyzeWithRule<T>(string input, string ruleId, string output = null, int issueToFix = -1, int actionToRun = 0, Action<int, Diagnostic> diagnosticCheck = null) where T : DiagnosticAnalyzer, new()
        {
            AnalyzeWithRule<T>(
                t => CSharpSyntaxTree.ParseText(t),
                s => CreateCompilationWithMscorlib(s),
                LanguageNames.CSharp,
                input, ruleId, output, issueToFix, actionToRun, diagnosticCheck
                );
        }
    }
}

