using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using CS = Microsoft.CodeAnalysis.CSharp;

namespace RefactoringEssentials.VB.Converter
{
    static class ConversionExtensions
    {
        public static CS.SyntaxKind CSKind(this SyntaxToken token)
        {
            return CS.CSharpExtensions.Kind(token);
        }
    }
}
