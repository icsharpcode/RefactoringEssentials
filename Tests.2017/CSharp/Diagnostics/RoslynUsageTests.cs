using RefactoringEssentials.CSharp;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class RoslynUsageTests : CSharpDiagnosticTestBase
    {
        const string attributeFakes = @"
using System;
using RefactoringEssentials;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeRefactorings;

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
namespace Microsoft.CodeAnalysis.CodeRefactorings
{
    [AttributeUsage(AttributeTargets.Class)]
    class ExportCodeRefactoringProviderAttribute : Attribute
    {
    }
}
";

        [Fact]
        public void ForbiddenMethodInAnalyzer()
        {
            Analyze<RoslynUsageAnalyzer>(attributeFakes + @"
static class SomeUtilityClass
{
    [RoslynReflectionUsage(RoslynReflectionAllowedContext.CodeFixes)]
    public static void ForbiddenMethod(this int i)
    {
    }
}

[DiagnosticAnalyzer]
public class TestAnalyzer : DiagnosticAnalyzer
{
    public TestAnalyzer()
    {
        int i = 0;
        i.$ForbiddenMethod$();
    }
}");
        }

        [Fact]
        public void ForbiddenClassInAnalyzer()
        {
            Analyze<RoslynUsageAnalyzer>(attributeFakes + @"
[RoslynReflectionUsage(RoslynReflectionAllowedContext.CodeFixes)]
static class SomeUtilityClass
{
    public static void ForbiddenMethod(this int i)
    {
    }
}

[DiagnosticAnalyzer]
public class TestAnalyzer : DiagnosticAnalyzer
{
    public TestAnalyzer()
    {
        int i = 0;
        i.$ForbiddenMethod$();
    }
}");
        }

        [Fact]
        public void ForbiddenMethodInCodeFix()
        {
            Analyze<RoslynUsageAnalyzer>(attributeFakes + @"
static class SomeUtilityClass
{
    [RoslynReflectionUsage(RoslynReflectionAllowedContext.Analyzers)]
    public static void ForbiddenMethod(this int i)
    {
    }
}

[ExportCodeFixProvider]
public class TestCodeFix : CodeFixProvider
{
    public TestCodeFix()
    {
        int i = 0;
        i.$ForbiddenMethod$();
    }
}");
        }

        [Fact]
        public void ForbiddenMethodInRefactoring()
        {
            Analyze<RoslynUsageAnalyzer>(attributeFakes + @"
static class SomeUtilityClass
{
    [RoslynReflectionUsage(RoslynReflectionAllowedContext.Analyzers)]
    public static void ForbiddenMethod(this int i)
    {
    }
}

[ExportCodeRefactoringProvider]
public class TestRefactoring : CodeRefactoringProvider
{
    public TestRefactoring()
    {
        int i = 0;
        i.$ForbiddenMethod$();
    }
}");
        }

        [Fact]
        public void ForbiddenCallToCodeFixMethodInAnalyzer()
        {
            Analyze<RoslynUsageAnalyzer>(attributeFakes + @"
[ExportCodeRefactoringProvider]
public class TestRefactoring : CodeRefactoringProvider
{
    public static void SomeUtilityMethod()
    {
    }
}

[DiagnosticAnalyzer]
public class TestAnalyzer : DiagnosticAnalyzer
{
    public TestAnalyzer()
    {
        TestRefactoring.$SomeUtilityMethod$();
    }
}");
        }

        [Fact]
        public void AllowedCallToAnalyzerMethodInCodeFix()
        {
            Analyze<RoslynUsageAnalyzer>(attributeFakes + @"
[ExportCodeRefactoringProvider]
public class TestRefactoring : CodeRefactoringProvider
{
    public TestRefactoring()
    {
        TestAnalyzer.SomeUtilityMethod();
    }
}

[DiagnosticAnalyzer]
public class TestAnalyzer : DiagnosticAnalyzer
{
    public static void SomeUtilityMethod()
    {
    }
}");
        }

        [Fact]
        public void AllowedMethodInAnalyzer1()
        {
            Analyze<RoslynUsageAnalyzer>(attributeFakes + @"
static class SomeUtilityClass
{
    [RoslynReflectionUsage(RoslynReflectionAllowedContext.Analyzers)]
    public static void ForbiddenMethod(this int i)
    {
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TestAnalyzer : DiagnosticAnalyzer
{
    public TestAnalyzer()
    {
        int i = 0;
        i.ForbiddenMethod();
    }
}");
        }

        [Fact]
        public void AllowedMethodInAnalyzer2()
        {
            Analyze<RoslynUsageAnalyzer>(attributeFakes + @"
static class SomeUtilityClass
{
    [RoslynReflectionUsage(RoslynReflectionAllowedContext.Analyzers | RoslynReflectionAllowedContext.CodeFixes)]
    public static void ForbiddenMethod(this int i)
    {
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TestAnalyzer : DiagnosticAnalyzer
{
    public TestAnalyzer()
    {
        int i = 0;
        i.ForbiddenMethod();
    }
}");
        }

        [Fact]
        public void AllowedMethodInAnalyzer3()
        {
            Analyze<RoslynUsageAnalyzer>(attributeFakes + @"
static class SomeUtilityClass
{
    public static void ForbiddenMethod(this int i)
    {
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TestAnalyzer : DiagnosticAnalyzer
{
    public TestAnalyzer()
    {
        int i = 0;
        i.ForbiddenMethod();
    }
}");
        }

        [Fact]
        public void MethodNotInAnalyzer()
        {
            Analyze<RoslynUsageAnalyzer>(attributeFakes + @"
static class SomeUtilityClass
{
    [RoslynReflectionUsage(RoslynReflectionAllowedContext.Analyzers)]
    public static void ForbiddenMethod(this int i)
    {
    }
}

public class TestAnalyzer : DiagnosticAnalyzer
{
    public TestAnalyzer()
    {
        int i = 0;
        i.ForbiddenMethod();
    }
}");
        }
    }
}

