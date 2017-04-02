using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RefactoringEssentials.CSharp.Manipulations;


namespace RefactoringEssentials.CSharp
{
    internal class DocumentManipulator
    {
        readonly CompilationUnitSyntax root;
        readonly Document document;

        public DocumentManipulator(Document document, CompilationUnitSyntax root)
        {
            Contract.Requires(document != null);
            Contract.Requires(root != null);

            this.document = document;
            this.root = root;
        }

        internal string SystemDot => SystemNamespaceIfRequired;

        internal string SystemNamespaceIfRequired => NamespaceIfRequired("System");

        internal string NamespaceIfRequired(string usingNamespace) => 
            UsingNamespaceExists(usingNamespace)
                ? ""
                : usingNamespace + ".";

        internal bool UsingNamespaceExists(string usingNamespace) => 
            root.Usings.Any((UsingDirectiveSyntax u) => u.Name.ToString() == usingNamespace);

        internal BlockSyntax ThrowNotImplementedExceptionBlock => 
            ParseStatement($"throw new {SystemDot}NotImplementedException();").WrapInBlock();

        protected Task<Document> DocumentWithReplacedNode(SyntaxNode oldNode, SyntaxNode newNode) =>
            Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(oldNode, newNode)));

    }
}

