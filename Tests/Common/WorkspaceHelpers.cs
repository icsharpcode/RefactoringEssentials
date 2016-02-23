using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace RefactoringEssentials.Tests.Common
{
    /// <summary>
    /// Helps create workspace objects suitable for testing; solutions, projects and documents.
    /// </summary>
    static class WorkspaceHelpers
    {
        /// <summary>
        /// Default CSharp parse options.
        /// </summary>
        static readonly ParseOptions DefaultCSharpParseOptions = new CSharpParseOptions(
                    Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp6,
                    DocumentationMode.Diagnose | DocumentationMode.Parse,
                    SourceCodeKind.Regular,
                    ImmutableArray.Create("DEBUG", "TEST")
                );

        /// <summary>
        /// Default CSharp compilation options.
        /// </summary>
        static readonly CSharpCompilationOptions DefaultCSharpCompilationOptions = new CSharpCompilationOptions(
                        OutputKind.DynamicallyLinkedLibrary,
                        "",
                        "",
                        "Script",
                        null,
                        OptimizationLevel.Debug,
                        false,
                        true
                    );

        /// <summary>
        /// Default VB Parse Options.
        /// </summary>
        static readonly ParseOptions DefaultVBParseOptions = new VisualBasicParseOptions(
                    Microsoft.CodeAnalysis.VisualBasic.LanguageVersion.VisualBasic14,
                    DocumentationMode.Diagnose | DocumentationMode.Parse,
                    SourceCodeKind.Regular
                );

        /// <summary>
        /// Default VB compilation options.
        /// </summary>
        static readonly VisualBasicCompilationOptions DefaultVisualBasicCompilationOptions = new VisualBasicCompilationOptions(
                        OutputKind.DynamicallyLinkedLibrary,
                        "",
                        ""
                    );

        static readonly MetadataReference mscorlib = MetadataReference.CreateFromFile(typeof(Console).Assembly.Location);
        static readonly MetadataReference systemAssembly = MetadataReference.CreateFromFile(typeof(System.ComponentModel.BrowsableAttribute).Assembly.Location);
        static readonly MetadataReference systemXmlLinq = MetadataReference.CreateFromFile(typeof(System.Xml.Linq.XElement).Assembly.Location);
        static readonly MetadataReference systemCore = MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);

        /// <summary>
        /// Default references.
        /// </summary>
        static readonly MetadataReference[] DefaultMetadataReferences = {
                mscorlib,
                systemAssembly,
                systemCore,
                systemXmlLinq
            };

        /// <summary>
        /// Create a solution.
        /// </summary>
        /// <param name="solutionId">The ID of the solution.</param>
        /// <returns></returns>
        public static SolutionInfo CreateSolution(SolutionId solutionId)
        {
            return CreateSolution(solutionId, null);
        }

        /// <summary>
        /// Create a solution with projects.
        /// </summary>
        /// <param name="solutionId">The ID of the solution.</param>
        /// <param name="projects">The solution's projects.</param>
        /// <returns></returns>
        public static SolutionInfo CreateSolution(SolutionId solutionId, IEnumerable<ProjectInfo> projects)
        {
            return SolutionInfo.Create(
                solutionId,
                VersionStamp.Create(),
                null,
                projects);
        }

        /// <summary>
        /// Create a <see cref="ProjectInfo"/> suitable for CSharp code.
        /// </summary>
        /// <param name="projectId">The ID of the project.</param>
        /// <param name="name">The name of the project.</param>
        /// <returns></returns>
        public static ProjectInfo CreateCSharpProject(ProjectId projectId, string name)
        {
            return CreateCSharpProject(projectId, name, null);
        }

        /// <summary>
        /// Create a <see cref="ProjectInfo"/> suitable for CSharp code.
        /// </summary>
        /// <param name="projectId">The ID of the project.</param>
        /// <param name="name">The name of the project.</param>
        /// <param name="documents">The project's documents.</param>
        /// <returns></returns>
        public static ProjectInfo CreateCSharpProject(ProjectId projectId, string name, IEnumerable<DocumentInfo> documents)
        {
            return ProjectInfo.Create(
                    projectId,
                    VersionStamp.Create(),
                    name,
                    name,
                    LanguageNames.CSharp,
                    null,
                    null,
                    new CSharpCompilationOptions(
                        OutputKind.DynamicallyLinkedLibrary,
                        "",
                        "",
                        "Script",
                        null,
                        OptimizationLevel.Debug,
                        false,
                        true
                    ),
                    DefaultCSharpParseOptions,
                    documents,
                    null,
                    DiagnosticTestBase.DefaultMetadataReferences
                ).WithMetadataReferences(DefaultMetadataReferences);
        }

        /// <summary>
        /// Create a <see cref="ProjectInfo"/> suitable for Visual Basic code.
        /// </summary>
        /// <param name="projectId">The ID of the project.</param>
        /// <param name="name">The name of the project.</param>
        /// <returns></returns>
        public static ProjectInfo CreateVisualBasicProject(ProjectId projectId, string name)
        {
            return CreateVisualBasicProject(projectId, name, null);
        }

        /// <summary>
        /// Create a <see cref="ProjectInfo"/> suitable for Visual Basic code.
        /// </summary>
        /// <param name="projectId">The ID of the project.</param>
        /// <param name="name">The name of the project.</param>
        /// <param name="documents">The project's documents.</param>
        /// <returns></returns>
        public static ProjectInfo CreateVisualBasicProject(ProjectId projectId, string name, IEnumerable<DocumentInfo> documents)
        {
            return ProjectInfo.Create(
                    projectId,
                    VersionStamp.Create(),
                    name,
                    name,
                    LanguageNames.VisualBasic,
                    null,
                    null,
                    DefaultVisualBasicCompilationOptions,
                    DefaultVBParseOptions,
                    documents,
                    null,
                    DiagnosticTestBase.DefaultMetadataReferences
                ).WithMetadataReferences(DefaultMetadataReferences);
        }

        /// <summary>
        /// Creates a <see cref="DocumentInfo"/> suitable for testing.
        /// </summary>
        /// <param name="documentId">The ID of the document.</param>
        /// <param name="name">The filename of the document.</param>
        /// <param name="text">The test content of the file.</param>
        /// <returns></returns>
        public static DocumentInfo CreateDocument(DocumentId documentId, string name, string text)
        {
            return DocumentInfo.Create(
                documentId,
                name,
                null,
                SourceCodeKind.Regular,
                TextLoader.From(TextAndVersion.Create(SourceText.From(text), VersionStamp.Create()))
                );
        }
                
    }
}
