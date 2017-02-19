using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Reflection;
using Microsoft.CodeAnalysis.CodeFixes;

namespace RefactoringEssentials.DocGenerator
{
	class Program
    {
        static void Main(string[] args)
        {
            const string BasePath = @"..\..\RefactoringEssentials";

            using (var missingMDWriter = new StreamWriter(Path.Combine(BasePath, "missing.md"), false, Encoding.UTF8))
            {
                missingMDWriter.WriteLine("Things not ported yet");
                missingMDWriter.WriteLine("=====================");
                missingMDWriter.WriteLine("");

                var codeRefactorings = typeof(NotPortedYetAttribute).Assembly.GetTypes()
                    .Where(t => !t.FullName.StartsWith("RefactoringEssentials.Samples.") && t.CustomAttributes.Any(a => a.AttributeType.FullName == typeof(ExportCodeRefactoringProviderAttribute).FullName))
                    .OrderBy(t => t.Name);

                var codeAnalyzers = typeof(NotPortedYetAttribute).Assembly.GetTypes()
                    .Where(t => !t.FullName.StartsWith("RefactoringEssentials.Samples.") && t.CustomAttributes.Any(a => a.AttributeType.FullName == typeof(DiagnosticAnalyzerAttribute).FullName))
                    .OrderBy(t => t.Name);

                var codeFixes = typeof(NotPortedYetAttribute).Assembly.GetTypes()
                    .Where(t => !t.FullName.StartsWith("RefactoringEssentials.Samples.") && t.CustomAttributes.Any(a => a.AttributeType.FullName == typeof(ExportCodeFixProviderAttribute).FullName) && CodeFixUnrelatedToNRAnalyzer(t))
                    .OrderBy(t => t.Name);

                // Code Refactorings
                missingMDWriter.WriteLine("*Refactorings*");
                missingMDWriter.WriteLine("");
                missingMDWriter.WriteLine("**C#**");
                missingMDWriter.WriteLine("");
                WriteTypeList(BasePath, "CodeRefactorings.html.template", "CodeRefactorings.CSharp.html", "{0} code refactorings for C#",
                    codeRefactorings.Where(t => IsCSharpRelatedElement(t)),
                    GetRefactoringDescription, missingMDWriter);
                missingMDWriter.WriteLine("");
                WriteTypeList(BasePath, "CodeRefactorings.html.template", "CodeRefactorings.VB.html", "{0} code refactorings for Visual Basic",
                    codeRefactorings.Where(t => IsVBRelatedElement(t)),
                    GetRefactoringDescription, null);

                // Diagnostics (Analyzers)
                missingMDWriter.WriteLine("*Analyzers*");
                missingMDWriter.WriteLine("");
                missingMDWriter.WriteLine("**C#**");
                missingMDWriter.WriteLine("");
                WriteTypeList(BasePath, "CodeAnalyzers.html.template", "CodeAnalyzers.CSharp.html", "{0} code analyzers for C#",
                    codeAnalyzers.Where(t => IsCSharpRelatedElement(t)),
                    GetAnalyzerDescription, missingMDWriter);
                missingMDWriter.WriteLine("");
                WriteTypeList(BasePath, "CodeAnalyzers.html.template", "CodeAnalyzers.VB.html", "{0} code analyzers for Visual Basic",
                    codeAnalyzers.Where(t => IsVBRelatedElement(t)),
                    GetAnalyzerDescription, null);

                // Code Fixes
                WriteTypeList(BasePath, "CodeFixes.html.template", "CodeFixes.CSharp.html", "{0} code fixes for C#",
                    codeFixes.Where(t => IsCSharpRelatedElement(t)),
                    NoDescription, missingMDWriter);
                WriteTypeList(BasePath, "CodeFixes.html.template", "CodeFixes.VB.html", "{0} code fixes for Visual Basic",
                    codeFixes.Where(t => IsVBRelatedElement(t)),
                    NoDescription, null);
            }
        }

        static void WriteTypeList(string basePath, string templateFile, string targetFile, string titleFormat, IEnumerable<Type> types, Func<Type, string> descriptionGetter, StreamWriter missingMDWriter)
        {
            var document = XDocument.Load(Path.Combine(basePath, templateFile));
            var node = document.Descendants("{http://www.w3.org/1999/xhtml}ul").First();
            var countNode = document.Descendants("{http://www.w3.org/1999/xhtml}p").First();
            int count = 0;

            foreach (var type in types)
            {
                if (type.CustomAttributes.Any(a => a.AttributeType.FullName == typeof(NotPortedYetAttribute).FullName))
                {
                    if (missingMDWriter != null)
                        missingMDWriter.WriteLine(string.Format("* {0}", type.Name));
                }
                else
                {
                    var description = descriptionGetter(type);
                    var line = (description == null) ? string.Format("{0}", type.Name) : string.Format("{0} ({1})", description, type.Name);
                    node.Add(new XElement("{http://www.w3.org/1999/xhtml}li", line));
                    count++;
                }
            }

            countNode.Value = string.Format(titleFormat, count);
            document.Save(Path.Combine(basePath, targetFile));
        }

        static string GetRefactoringDescription(Type t)
        {
            var exportAttribute = t.GetCustomAttributes(false).OfType<ExportCodeRefactoringProviderAttribute>().First();
            return exportAttribute.Name;
        }

        static string GetAnalyzerDescription(Type t)
        {
            var descriptor = t.GetField("descriptor", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null) as Microsoft.CodeAnalysis.DiagnosticDescriptor;
            return descriptor?.Title.ToString();
        }

        static string NoDescription(Type t)
        {
            return null;
        }

        static bool CodeFixUnrelatedToNRAnalyzer(Type codeFixType)
        {
            var codeFixInstance = Activator.CreateInstance(codeFixType) as CodeFixProvider;
            if (codeFixInstance != null)
            {
                if (!codeFixInstance.FixableDiagnosticIds.Any(id => id.StartsWith("RE")))
                {
                    // Try to find an appropriate analyzer class in NR6Pack
                    string analyzerClassName = codeFixType.FullName.Replace("CodeFixProvider", "Analyzer");
                    var analyzerClass = codeFixType.Assembly.GetType(analyzerClassName, false);
                    return analyzerClass == null;
                }
            }

            return false;
        }

        static bool IsCSharpRelatedElement(Type type)
        {
            return type.FullName.Contains("RefactoringEssentials.CSharp.");
        }

        static bool IsVBRelatedElement(Type type)
        {
            return type.FullName.Contains("RefactoringEssentials.VB.");
        }
    }
}
