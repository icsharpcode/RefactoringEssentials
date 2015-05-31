using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class CS1573ParameterHasNoMatchingParamTagTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestMethodMissesParameter()
        {
            Test<CS1573ParameterHasNoMatchingParamTagAnalyzer>(@"
class Foo {
	/// <summary/>
	/// <param name = ""y""></param>
	/// <param name = ""z""></param>
	public void FooBar(int x, int y, int z)
	{
	}
}
", @"
class Foo {
	/// <summary/>
	/// <param name = ""x""></param>
	/// <param name = ""y""></param>
	/// <param name = ""z""></param>
	public void FooBar(int x, int y, int z)
	{
	}
}
");
        }

        [Test]
        public void TestNoParamDocs()
        {
            Analyze<CS1573ParameterHasNoMatchingParamTagAnalyzer>(@"
class Foo {
	/// <summary/>
	public void FooBar(int x, int y, int z)
	{
	}
}
");
        }

        [Test]
        public void TestDisable()
        {
            Analyze<CS1573ParameterHasNoMatchingParamTagAnalyzer>(@"
class Foo {
	/// <summary/>
	/// <param name = ""y""></param>
	/// <param name = ""z""></param>
// ReSharper disable once CSharpWarnings::CS1573
	public void FooBar(int x, int y, int z)
	{
	}
}
");
        }

        [Test]
        public void TestPragmaDisable()
        {
            Analyze<CS1573ParameterHasNoMatchingParamTagAnalyzer>(@"
class Foo {
	/// <summary/>
	/// <param name = ""y""></param>
	/// <param name = ""z""></param>
#pragma warning disable 1573
	public void FooBar(int x, int y, int z)
#pragma warning restore 1573
	{
	}
}
");
        }

    }
}

