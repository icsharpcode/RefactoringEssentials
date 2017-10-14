using System;

namespace RefactoringEssentials
{
	/// <summary>
	/// Defines contexts where usage of reflection is allowed.
	/// </summary>
	[Flags]
    enum RoslynReflectionAllowedContext
    {
        None = 0,
        Analyzers = 1,
        CodeFixes = 2
    }

    /// <summary>
    /// Marker attribute for types or members providing access to internal Roslyn functionality.
    /// </summary>
    /// <remarks>
    /// This marker is mainly intended for automatic checking of reflection calls to Roslyn,
    /// warnings about wrong usage etc.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    class RoslynReflectionUsageAttribute : Attribute
    {
        public RoslynReflectionUsageAttribute(RoslynReflectionAllowedContext allowedContexts = RoslynReflectionAllowedContext.None)
        {
            AllowedContexts = allowedContexts;
        }

        public RoslynReflectionAllowedContext AllowedContexts { get; set; }
    }
}
