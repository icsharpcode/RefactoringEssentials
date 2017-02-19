using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class RedundantTernaryExpressionTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestTrueFalseCase()
        {
            Analyze<RedundantTernaryExpressionAnalyzer>(@"
class Foo
{
	void Bar ()
	{
		var a = 1 < 2 $? true : false$;
	}
}
", @"
class Foo
{
	void Bar ()
	{
		var a = 1 < 2;
	}
}
");
        }

        [Fact]
        public void TestDisable()
        {
            Analyze<RedundantTernaryExpressionAnalyzer>(@"
class Foo
{
	void Bar ()
	{
#pragma warning disable " + CSharpDiagnosticIDs.RedundantTernaryExpressionAnalyzerID + @"
		var a = 1 < 2 ? true : false;
	}
}
");
        }

        [Fact]
        public void TestExceptionCastNotValid()
        {
            Analyze<RedundantTernaryExpressionAnalyzer>(@"namespace Test
{
    class Foo
    {
        internal void Bar(string str)
        {
            var description = str.EndsWith(""a"") ? ""Add"" : ""Remove"";
        }
	}
}");
        }
    }
}

