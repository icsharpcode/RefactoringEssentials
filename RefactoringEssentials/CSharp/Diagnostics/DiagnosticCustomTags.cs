using Microsoft.CodeAnalysis;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    static class DiagnosticCustomTags
    {
        public static readonly string[] Unnecessary = { WellKnownDiagnosticTags.Unnecessary, WellKnownDiagnosticTags.Telemetry };
    }
}

