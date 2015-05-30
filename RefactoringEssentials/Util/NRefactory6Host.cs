using System;

namespace RefactoringEssentials
{
    /// <summary>
    /// Needs to be implemented from IDE/host side.
    /// </summary>
#if NR6
	public
#endif
    static class NRefactory6Host
    {
        public static Func<string, string> GetLocalizedString = s => s;
        public static Func<string, string> GetHelpLinkForDiagnostic = id => null;
    }
}

