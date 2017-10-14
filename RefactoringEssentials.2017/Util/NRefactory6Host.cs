using System;

namespace RefactoringEssentials
{
    /// <summary>
    /// Needs to be implemented from IDE/host side.
    /// </summary>
    public static class NRefactory6Host
    {
        public static Func<string, string> GetLocalizedString = s => s;
        public static Func<string, string> GetHelpLinkForDiagnostic = id => null;
    }
}

