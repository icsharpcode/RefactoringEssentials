namespace RefactoringEssentials
{
    static class GettextCatalog
    {
        internal static string GetString(string str)
        {
            return NRefactory6Host.GetLocalizedString(str);
        }
    }
}