using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class ConstantConditionTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestConditionalExpression()
        {
            Analyze<ConstantConditionAnalyzer>(@"
class TestClass
{
	void TestMethod ()
	{
		var a = $1 > 0$ ? 1 : 0;
		var b = $1 < 0$ ? 1 : 0;
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		var a = 1;
		var b = 0;
	}
}");
        }

        [Fact]
        public void TestIf()
        {
            Analyze<ConstantConditionAnalyzer>(@"
class TestClass
{
    void TestMethod ()
    {
        int i;
        if ($1 > 0$)
        	i = 1;
        if ($1 > 0$) {
        	i = 1;
        }
        if ($1 < 0$)
        	i = 1;
        if ($1 == 0$) {
        	i = 1;
        } else {
        	i = 0;
        }
        if ($1 == 0$) {
        	i = 1;
        } else
        	i = 0;
    }
}", @"
class TestClass
{
    void TestMethod ()
    {
        int i;
        i = 1;
        i = 1;
        i = 0;
        i = 0;
    }
}");
        }

        [Fact]
        public void TestFor()
        {
            Analyze<ConstantConditionAnalyzer>(@"
class TestClass
{
	void TestMethod ()
	{
		for (int i = 0; $1 > 0$; i++) ;
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		for (int i = 0; true; i++) ;
	}
}");
        }

        [Fact]
        public void TestWhile()
        {
            Analyze<ConstantConditionAnalyzer>(@"
class TestClass
{
	void TestMethod ()
	{
		while ($1 > 0$)
			;
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		while (true)
			;
	}
}");
        }

        [Fact]
        public void TestDoWhile()
        {
            Analyze<ConstantConditionAnalyzer>(@"
class TestClass
{
	void TestMethod ()
	{
		do {
		} while ($1 < 0$);
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		do {
		} while (false);
	}
}");
        }

        [Fact]
        public void TestNoIssue()
        {
            Analyze<ConstantConditionAnalyzer>(@"
class TestClass
{
	void TestMethod (int x = true)
	{
		while (true) ;
		if (false) ;
		if (x) ;
	}
}");
        }
    }
}
