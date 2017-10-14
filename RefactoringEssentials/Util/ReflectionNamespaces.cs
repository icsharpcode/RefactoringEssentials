namespace RefactoringEssentials
{
	public static class ReflectionNamespaces
    {
        public static readonly string WorkspacesAsmName;
        public static readonly string CSWorkspacesAsmName;
        public static readonly string VBWorkspacesAsmName;
        public static readonly string CAAsmName;
        public static readonly string CACSharpAsmName;
        public static readonly string VersionInfo;

        static ReflectionNamespaces()
        {
            const string namePart = "Microsoft.CodeAnalysis.LanguageNames, Microsoft.CodeAnalysis, ";
            VersionInfo = typeof(Microsoft.CodeAnalysis.LanguageNames).AssemblyQualifiedName.Substring(namePart.Length);
            WorkspacesAsmName = ", Microsoft.CodeAnalysis.Workspaces, " + VersionInfo;
            CSWorkspacesAsmName = ", Microsoft.CodeAnalysis.CSharp.Workspaces, " + VersionInfo;
            VBWorkspacesAsmName = ", Microsoft.CodeAnalysis.VisualBasic.Workspaces, " + VersionInfo;
            CAAsmName = ", Microsoft.CodeAnalysis, " + VersionInfo;
            CACSharpAsmName = ", Microsoft.CodeAnalysis.CSharp, " + VersionInfo;
        }
    }
}
