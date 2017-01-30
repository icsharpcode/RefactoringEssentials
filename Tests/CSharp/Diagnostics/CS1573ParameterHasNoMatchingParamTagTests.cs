using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class CS1573ParameterHasNoMatchingParamTagTests : CSharpDiagnosticTestBase
    {
        [Fact(Skip="TODO: Issue not ported yet")]
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

        [Fact(Skip="TODO: Issue not ported yet")]
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

        [Fact(Skip="TODO: Issue not ported yet")]
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

        [Fact(Skip="TODO: Issue not ported yet")]
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

