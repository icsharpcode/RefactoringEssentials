using NUnit.Framework;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [Ignore("Should be a code fix")]
    [TestFixture]
    public class StaticConstructorParameterTests : CSharpDiagnosticTestBase
    {
        //		[Test]
        //		public void TestSimpleCase()
        //		{
        //			Analyze<StaticConstructorParameterAnalyzer>(@"
        //class Foo
        //{
        //	static $Foo$(int bar)
        //	{
        //	}
        //}
        //", @"
        //class Foo
        //{
        //	static Foo()
        //	{
        //	}
        //}
        //");
        //		}

        //		[Test]
        //		public void TestNoIssue()
        //		{
        //			Analyze<StaticConstructorParameterAnalyzer>(@"
        //class Foo
        //{
        //	static Foo ()
        //	{
        //	}
        //}
        //");
        //		}

    }
}

