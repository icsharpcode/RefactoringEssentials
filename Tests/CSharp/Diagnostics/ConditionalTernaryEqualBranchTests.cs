using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class ConditionalTernaryEqualBranchTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestInspectorCase1()
        {
            Analyze<ConditionalTernaryEqualBranchAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		string c = $str != null ? ""default"" : ""default""$;
	}
}", @"class Foo
{
	void Bar (string str)
	{
		string c = ""default"";
	}
}");

        }

        [Fact]
        public void TestMoreComplexBranch()
        {
            Analyze<ConditionalTernaryEqualBranchAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		var c = $str != null ? 3 + (3 * 4) - 12 * str.Length : 3 + (3 * 4) - 12 * str.Length$;
	}
}", @"class Foo
{
	void Bar (string str)
	{
		var c = 3 + (3 * 4) - 12 * str.Length;
	}
}");

        }

        [Fact]
        public void TestNotEqualBranches()
        {
            Analyze<ConditionalTernaryEqualBranchAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		string c = str != null ? ""default"" : ""default2"";
	}
}");

        }

        [Fact]
        public void TestNotEqualBranchesWithLambdas()
        {
            Analyze<ConditionalTernaryEqualBranchAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		List<string> someList;
		string c = str != null ? someList.FirstOrDefault(s => { return s == s.ToLower(); }) : someList.FirstOrDefault(s => { return s == s.ToUpper(); });
	}
}");

        }

        [Fact]
        public void TestNotEqualBranchesWithInterpolatedStrings()
        {
            Analyze<ConditionalTernaryEqualBranchAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		string c = str != null ? $$""default"" : $$""default2"";
	}
}");

        }

        [Fact]
        public void TestDisable()
        {
            Analyze<ConditionalTernaryEqualBranchAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
#pragma warning disable " + CSharpDiagnosticIDs.ConditionalTernaryEqualBranchAnalyzerID + @"
		string c = str != null ? ""default"" : ""default"";
	}
}");

        }

        [Fact]
        public void Test()
        {
            Analyze<ConditionalTernaryEqualBranchAnalyzer>(@"
class TestClass
{
	void TestMethod (int i)
	{
		var a = $i > 0 ? 1 + 1 : 1 + 1$;
		var b = i > 1 ? 1 : 2;
	}
}", @"
class TestClass
{
	void TestMethod (int i)
	{
		var a = 1 + 1;
		var b = i > 1 ? 1 : 2;
	}
}");
        }
    }
}

