using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class RedundantExplicitNullableCreationTests : CSharpDiagnosticTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

