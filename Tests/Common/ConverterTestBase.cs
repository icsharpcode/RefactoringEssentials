﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.VisualBasic;
using RefactoringEssentials.Tests.CSharp.Diagnostics;
using RefactoringEssentials.VB.Converter;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using RefactoringEssentials.Tests.Common;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.Tests.VB.Converter
{
	public class ConverterTestBase
	{
		void CSharpWorkspaceSetup(string text, out CSharpDiagnosticTestBase.TestWorkspace workspace, out Document doc, CSharpParseOptions parseOptions = null)
		{
			workspace = new CSharpDiagnosticTestBase.TestWorkspace();
			var projectId = ProjectId.CreateNewId();
			var documentId = DocumentId.CreateNewId(projectId);
			if (parseOptions == null)
			{
				parseOptions = new CSharpParseOptions(
					Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp6,
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
					new[] { "System", "System.Collections.Generic", "System.Linq" },
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
		}

		void VBWorkspaceSetup(out CSharpDiagnosticTestBase.TestWorkspace workspace, out Document doc, VisualBasicParseOptions parseOptions = null)
		{
			workspace = new CSharpDiagnosticTestBase.TestWorkspace();
			var projectId = ProjectId.CreateNewId();
			var documentId = DocumentId.CreateNewId(projectId);
			if (parseOptions == null)
			{
				parseOptions = new VisualBasicParseOptions(
					Microsoft.CodeAnalysis.VisualBasic.LanguageVersion.VisualBasic14,
					DocumentationMode.Diagnose | DocumentationMode.Parse,
					SourceCodeKind.Regular
				);
			}
			workspace.Options.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInControlBlocks, false);
            var compilationOptions = new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithRootNamespace("TestProject")
                .WithGlobalImports(GlobalImport.Parse("System", "System.Collections.Generic", "System.Linq", "Microsoft.VisualBasic"));
            workspace.Open(ProjectInfo.Create(
				projectId,
				VersionStamp.Create(),
				"TestProject",
				"TestProject",
				LanguageNames.VisualBasic,
				null,
				null,
                compilationOptions,
				parseOptions,
				new[] {
					DocumentInfo.Create(
						documentId,
						"a.vb",
						null,
						SourceCodeKind.Regular
					)
				},
				null,
				DiagnosticTestBase.DefaultMetadataReferences
			)
			);
			doc = workspace.CurrentSolution.GetProject(projectId).GetDocument(documentId);
		}

		public void TestConversionCSharpToVisualBasic(string csharpCode, string expectedVisualBasicCode, CSharpParseOptions csharpOptions = null, VisualBasicParseOptions vbOptions = null)
		{
			DiagnosticTestBase.TestWorkspace csharpWorkspace, vbWorkspace;
			Document inputDocument, outputDocument;
			CSharpWorkspaceSetup(csharpCode, out csharpWorkspace, out inputDocument, csharpOptions);
			VBWorkspaceSetup(out vbWorkspace, out outputDocument, vbOptions);
            var outputNode = Convert((CSharpSyntaxNode)inputDocument.GetSyntaxRootAsync().Result, inputDocument.GetSemanticModelAsync().Result, outputDocument);
			
			var txt = outputDocument.WithSyntaxRoot(Formatter.Format(outputNode, vbWorkspace)).GetTextAsync().Result.ToString();
			txt = Utils.HomogenizeEol(txt).TrimEnd();
			expectedVisualBasicCode = Utils.HomogenizeEol(expectedVisualBasicCode).TrimEnd();
			if (expectedVisualBasicCode != txt)
			{
				Console.WriteLine("expected:");
				Console.WriteLine(expectedVisualBasicCode);
				Console.WriteLine("got:");
				Console.WriteLine(txt);
                Console.WriteLine("diff:");
                int l = Math.Max(expectedVisualBasicCode.Length, txt.Length);
                StringBuilder diff = new StringBuilder(l);
                for (int i = 0; i < l; i++)
                {
                    if (i >= expectedVisualBasicCode.Length || i >= txt.Length || expectedVisualBasicCode[i] != txt[i])
                        diff.Append('x');
                    else
                        diff.Append(expectedVisualBasicCode[i]);
                }
                Console.WriteLine(diff.ToString());
				Assert.Fail();
			}
		}

        VisualBasicSyntaxNode Convert(CSharpSyntaxNode input, SemanticModel semanticModel, Document targetDocument)
		{
			return CSharpConverter.Convert(input, semanticModel, targetDocument);
		}
	}
}
