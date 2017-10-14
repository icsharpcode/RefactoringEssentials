namespace RefactoringEssentials
{
    static class HelpLink
    {
        internal static string CreateFor(string diagnosticId)
        {
            return NRefactory6Host.GetHelpLinkForDiagnostic(diagnosticId);
        }
    }
}