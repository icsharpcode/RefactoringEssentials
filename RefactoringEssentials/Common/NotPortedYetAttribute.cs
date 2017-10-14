using System;

namespace RefactoringEssentials
{
    /// <summary>
    /// Marker attribute for not yet ported analyzers or refactorings.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class NotPortedYetAttribute : Attribute
	{
	}
}
