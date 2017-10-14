using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactoringEssentials.CSharp
{
    public class ConstructorParameterContext
    {
        public string ParameterName { get; }
        public string PropertyName { get; }
        public TypeSyntax Type { get; }
        public ConstructorDeclarationSyntax Constructor { get; }
        public TextSpan TextSpan { get; }
        public SyntaxNode Root { get; }
        public Document Document { get; }

        public ConstructorParameterContext(Document document, string parameterName, string propertyName, TypeSyntax type, ConstructorDeclarationSyntax constructor, TextSpan textSpan, SyntaxNode root)
        {
            Contract.Requires(document != null);
            Contract.Requires(parameterName != null);
            Contract.Requires(propertyName != null);
            Contract.Requires(type != null);
            Contract.Requires(constructor != null);
            Contract.Requires(root != null);

            Document = document;
            Constructor = constructor;
            Type = type;
            ParameterName = parameterName;
            PropertyName = propertyName;
            TextSpan = textSpan;
            Root = root;
        }
    }
}
