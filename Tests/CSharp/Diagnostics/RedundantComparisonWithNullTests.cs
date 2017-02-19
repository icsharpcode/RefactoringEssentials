using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class RedundantComparisonWithNullTests : CSharpDiagnosticTestBase
    {
        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestInspectorCase1()
        {
            Test<RedundantComparisonWithNullAnalyzer>(@"using System;class Test {public void test(){int a = 0;if(a is int && a != null){a = 1;}}}", @"using System;class Test {public void test(){int a = 0;
		if (a is int) {
			a = 1;
		}}}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestResharperDisable()
        {
            Analyze<RedundantComparisonWithNullAnalyzer>(@"using System;
class Test {
	public void test(){
	int a = 0;
	//Resharper disable RedundantComparisonWithNull
	if(a is int && a != null)
	{a = 1;}
	//Resharper restore RedundantComparisonWithNull
	}	
	}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestInspectorCase2()
        {
            Test<RedundantComparisonWithNullAnalyzer>(@"using System;class Test {public void test(){int a = 0;while(a != null && a is int){a = 1;}}}", @"using System;class Test {public void test(){int a = 0;
		while (a is int) {
			a = 1;
		}}}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestCaseWithFullParens()
        {
            Test<RedundantComparisonWithNullAnalyzer>(@"using System;
class TestClass
{
	public void Test(object o)
	{
		if (!((o is int) && (o != null))) {
		}
	}
}", @"using System;
class TestClass
{
	public void Test(object o)
	{
		if (!(o is int)) {
		}
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestDisable()
        {
            Analyze<RedundantComparisonWithNullAnalyzer>(
                @"using System;
class TestClass
{
	public void Test(object o)
	{
// ReSharper disable once RedundantComparisonWithNull
		if (!((o is int) && (o != null))) {
		}
	}
}");
        }


        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestNegatedCase()
        {
            Test<RedundantComparisonWithNullAnalyzer>(@"using System;
class TestClass
{
	public void Test(object o)
	{
		if (null == o || !(o is int)) {
		}
	}
}", @"using System;
class TestClass
{
	public void Test(object o)
	{
		if (!(o is int)) {
		}
	}
}");
        }
    }
}