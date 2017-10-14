using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RefactoringEssentials
{
	public interface INameProposalStrategy
    {
        string GetNameProposal(string baseName, SyntaxKind syntaxKindHint, Accessibility accessibility, bool isStatic, Document document, int position);
    }

    public class NameProposalService
    {
        static INameProposalStrategy instance = new DefaultNameProposalStrategy();

        class DefaultNameProposalStrategy : INameProposalStrategy
        {
            static readonly char[] s_underscoreCharArray = new[] { '_' };
            static readonly CultureInfo EnUSCultureInfo = new CultureInfo("en-US");

            public virtual string GetNameProposal(string baseName, SyntaxKind syntaxKindHint, Accessibility accessibility, bool isStatic, Document document, int position)
            {
                switch (syntaxKindHint)
                {
                    case SyntaxKind.ClassDeclaration:
                    case SyntaxKind.StructDeclaration:
                    case SyntaxKind.InterfaceDeclaration:
                    case SyntaxKind.EnumDeclaration:
                    case SyntaxKind.DelegateDeclaration:
                    case SyntaxKind.MethodDeclaration:
                    case SyntaxKind.PropertyDeclaration:
                    case SyntaxKind.EventDeclaration:
                    case SyntaxKind.EventFieldDeclaration:
                    case SyntaxKind.EnumMemberDeclaration:

                        // Trim leading underscores
                        var newBaseName = baseName.TrimStart(s_underscoreCharArray);

                        // Trim leading "m_"
                        if (newBaseName.Length >= 2 && newBaseName[0] == 'm' && newBaseName[1] == '_')
                        {
                            newBaseName = newBaseName.Substring(2);
                        }

                        // Take original name if no characters left
                        if (newBaseName.Length == 0)
                        {
                            newBaseName = baseName;
                        }

                        // Make the first character upper case using the "en-US" culture.  See discussion at
                        // https://github.com/dotnet/roslyn/issues/5524.
                        var firstCharacter = EnUSCultureInfo.TextInfo.ToUpper(newBaseName[0]);
                        return firstCharacter.ToString() + newBaseName.Substring(1);

                    case SyntaxKind.Parameter:
                    case SyntaxKind.FieldDeclaration:
                    case SyntaxKind.VariableDeclaration:
                    case SyntaxKind.LocalDeclarationStatement:
                        return char.ToLower(baseName[0]).ToString() + baseName.Substring(1);

                }
                return baseName;
            }
        }

        public static void Replace (INameProposalStrategy newService)
        {
            instance = newService;
        }

        public static string GetNameProposal(string baseName, SyntaxKind syntaxKindHint, Accessibility accessibility = Accessibility.Private, bool isStatic = false, Document document = null, int position = 0)
        {
            return instance.GetNameProposal(baseName, syntaxKindHint, accessibility, isStatic, document, position);
        }
    }
}