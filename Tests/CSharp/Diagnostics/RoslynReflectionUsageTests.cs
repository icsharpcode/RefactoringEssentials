using NUnit.Framework;
using RefactoringEssentials.CSharp;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class RoslynReflectionUsageTests : CSharpDiagnosticTestBase
    {
        const string attributeFakes = @"
using System;
using RefactoringEssentials;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefactoringEssentials
{
    [Flags]
    enum RoslynReflectionAllowedContext
    {
        None = 0,
        Analyzers = 1,
        CodeFixes = 2
    }
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
namespace Microsoft.CodeAnalysis.Diagnostics
{
    [AttributeUsage(AttributeTargets.Class)]
    class DiagnosticAnalyzerAttribute : Attribute
    {
    }
}
namespace Microsoft.CodeAnalysis.CodeFixes
{
    [AttributeUsage(AttributeTargets.Class)]
    class ExportCodeFixProviderAttribute : Attribute
    {
    }
}
";

        [Test]
        public void ForbiddenMethodInAnalyzer()
        {
            Analyze<RoslynReflectionUsageAnalyzer>(attributeFakes + @"
static class SomeUtilityClass
{
    [RoslynReflectionUsageAttribute(RoslynReflectionAllowedContext.CodeFixes)]
    public static void ForbiddenMethod(this int i)
    {
    }
}

[DiagnosticAnalyzerAttribute]
public class AnalyzerWithForbiddenCode : DiagnosticAnalyzer
{
    public AnalyzerWithForbiddenCode()
    {
        int i = 0;
        i.$ForbiddenMethod$();
    }
}");
        }

        [Test]
        public void ForbiddenClassInAnalyzer()
        {
            Analyze<RoslynReflectionUsageAnalyzer>(attributeFakes + @"
[RoslynReflectionUsageAttribute(RoslynReflectionAllowedContext.CodeFixes)]
static class SomeUtilityClass
{
    public static void ForbiddenMethod(this int i)
    {
    }
}

[DiagnosticAnalyzerAttribute]
public class AnalyzerWithForbiddenCode : DiagnosticAnalyzer
{
    public AnalyzerWithForbiddenCode()
    {
        int i = 0;
        i.$ForbiddenMethod$();
    }
}");
        }

        [Test]
        public void AllowedMethodInAnalyzer()
        {
            Analyze<RoslynReflectionUsageAnalyzer>(attributeFakes + @"
static class SomeUtilityClass
{
    [RoslynReflectionUsageAttribute(RoslynReflectionAllowedContext.Analyzers)]
    public static void ForbiddenMethod(this int i)
    {
    }
}

[DiagnosticAnalyzerAttribute(LanguageNames.CSharp)]
public class AnalyzerWithForbiddenCode : DiagnosticAnalyzer
{
    public AnalyzerWithForbiddenCode()
    {
        int i = 0;
        i.ForbiddenMethod();
    }
}");
        }

        [Test]
        public void MethodNotInAnalyzer()
        {
            Analyze<RoslynReflectionUsageAnalyzer>(attributeFakes + @"
static class SomeUtilityClass
{
    [RoslynReflectionUsageAttribute(RoslynReflectionAllowedContext.Analyzers)]
    public static void ForbiddenMethod(this int i)
    {
    }
}

public class AnalyzerWithForbiddenCode : DiagnosticAnalyzer
{
    public AnalyzerWithForbiddenCode()
    {
        int i = 0;
        i.ForbiddenMethod();
    }
}");
        }
    }
}

