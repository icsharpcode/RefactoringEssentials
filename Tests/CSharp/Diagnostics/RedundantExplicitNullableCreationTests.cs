using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class RedundantExplicitNullableCreationTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestVariableCreation()
        {
            Analyze<RedundantExplicitNullableCreationAnalyzer>(@"
class FooBar
{
    void Test()
    {
        int? i = $new int?$(5);
    }
}
", @"
class FooBar
{
    void Test()
    {
        int? i = 5;
    }
}
");
        }

        [Fact]
        public void TestLongForm()
        {
            Analyze<RedundantExplicitNullableCreationAnalyzer>(@"
class FooBar
{
    void Test()
    {
        int? i = $new System.Nullable<int>$(5);
    }
}
", @"
class FooBar
{
    void Test()
    {
        int? i = 5;
    }
}
");
        }

        [Fact]
        public void TestCreationInArgument()
        {
            Analyze<RedundantExplicitNullableCreationAnalyzer>(@"
class FooBar
{
    void NullableMethod(int? param)
    {
    }

    void Test()
    {
        NullableMethod($new int?$(5));
    }
}
", @"
class FooBar
{
    void NullableMethod(int? param)
    {
    }

    void Test()
    {
        NullableMethod(5);
    }
}
");
        }

        [Fact]
        public void TestInvalid()
        {
            Analyze<RedundantExplicitNullableCreationAnalyzer>(@"
class FooBar
{
    void Test()
    {
        var i = new int?(5);
    }
}
");
        }

        /// <summary>
        /// RECS0138 fix produces bad code when one of expressions is an explicit nullable type creation #185
        /// </summary>
        [Fact]
        public void TestIssue185()
        {
            Analyze<RedundantExplicitNullableCreationAnalyzer>(@"
enum FooBar { Foo, Bar }

class Test
{
    FooBar GetFooBar(object o) { return (FooBar)o; }

    FooBar? Foo(object o)
    {
        return o == null ? null : new FooBar?(GetFooBar(o));
    }
}
");
        }

        [Fact]
        public void TestDisable()
        {
            Analyze<RedundantExplicitNullableCreationAnalyzer>(@"
class FooBar
{
    void Test()
    {
        // ReSharper disable once RedundantExplicitNullableCreation
+#pragma warning disable " + CSharpDiagnosticIDs.RedundantExplicitNullableCreationAnalyzerID + @"
        int? i = new int?(5);
    }
}
");
        }
    }
}

