using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class EqualExpressionComparisonTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestEquality()
        {
            Analyze<EqualExpressionComparisonAnalyzer>(@"class Foo
{
    static int Bar (object o)
    {
        if ($o == o$) {
        }
        return 5;
    }
}", @"class Foo
{
    static int Bar (object o)
    {
        if (true) {
        }
        return 5;
    }
}");
        }


        [Test]
        public void TestInequality()
        {
            Analyze<EqualExpressionComparisonAnalyzer>(@"class Foo
{
    static int Bar (object o)
    {
        if ($o != o$) {
        }
        return 5;
    }
}", @"class Foo
{
    static int Bar (object o)
    {
        if (false) {
        }
        return 5;
    }
}");
        }


        [Test]
        public void TestEquals()
        {
            Analyze<EqualExpressionComparisonAnalyzer>(@"class Foo
{
    static int Bar (object o)
    {
        if ($(1 + 2).Equals(1 + 2)$) {
        }
        return 5;
    }
}", @"class Foo
{
    static int Bar (object o)
    {
        if (true) {
        }
        return 5;
    }
}");
        }

        [Test]
        public void TestNotEquals()
        {
            Analyze<EqualExpressionComparisonAnalyzer>(@"class Foo
{
    static int Bar (object o)
    {
        if ($!(1 + 2).Equals(1 + 2)$) {
        }
        return 5;
    }
}", @"class Foo
{
    static int Bar (object o)
    {
        if (false) {
        }
        return 5;
    }
}");
        }

        [Test]
        public void TestStaticEquals()
        {
            Analyze<EqualExpressionComparisonAnalyzer>(@"class Foo
{
    static int Bar (object o)
    {
        if ($Equals(o, o)$) {
        }
        return 5;
    }
}", @"class Foo
{
    static int Bar (object o)
    {
        if (true) {
        }
        return 5;
    }
}");
        }

        [Test]
        public void TestNotStaticEquals()
        {
            Analyze<EqualExpressionComparisonAnalyzer>(@"class Foo
{
    static int Bar (object o)
    {
        if ($!Equals(o, o)$) {
        }
        return 5;
    }
}", @"class Foo
{
    static int Bar (object o)
    {
        if (false) {
        }
        return 5;
    }
}");
        }

        [Test]
        public void TestDisable()
        {
            Analyze<EqualExpressionComparisonAnalyzer>(@"class Foo
{
    static int Bar (object o)
    {
#pragma warning disable " + CSharpDiagnosticIDs.EqualExpressionComparisonAnalyzerID + @"
        if (o == o) {
        }
        return 5;
    }
}");
        }

    }
}

