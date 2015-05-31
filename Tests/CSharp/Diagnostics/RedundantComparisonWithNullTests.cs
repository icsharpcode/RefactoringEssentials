using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class RedundantComparisonWithNullTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestInspectorCase1()
        {
            Test<RedundantComparisonWithNullAnalyzer>(@"using System;class Test {public void test(){int a = 0;if(a is int && a != null){a = 1;}}}", @"using System;class Test {public void test(){int a = 0;
		if (a is int) {
			a = 1;
		}}}");
        }

        [Test]
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

        [Test]
        public void TestInspectorCase2()
        {
            Test<RedundantComparisonWithNullAnalyzer>(@"using System;class Test {public void test(){int a = 0;while(a != null && a is int){a = 1;}}}", @"using System;class Test {public void test(){int a = 0;
		while (a is int) {
			a = 1;
		}}}");
        }

        [Test]
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

        [Test]
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


        [Ignore("Extended version")]
        [Test]
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