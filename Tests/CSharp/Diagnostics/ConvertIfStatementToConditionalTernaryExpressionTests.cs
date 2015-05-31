using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class ConvertIfStatementToConditionalTernaryExpressionTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestIfElse()
        {
            Analyze<ConvertIfStatementToConditionalTernaryExpressionAnalyzer>(@"class Foo
{
	static int Bar (int x)
	{
		int result;
		$if$ (x > 10)
			result = 10;
		else
			result = 20;
		return result;
	}
}");
        }

        [Test]
        public void TestSkipComplexCondition()
        {
            Analyze<ConvertIfStatementToConditionalTernaryExpressionAnalyzer>(@"class Foo
{
	static int Bar (int x)
	{
		int result;
		if (x > 10 ||
		    x < -10)
			result = 10;
		else
			result = 20;
		return result;
	}
}");
        }

        [Test]
        public void TestSkipIfElseIf()
        {
            Analyze<ConvertIfStatementToConditionalTernaryExpressionAnalyzer>(@"class Foo
{
	static int Bar (int x)
	{
		int result;
		if (x < 10)
			result = -10;
		else $if$ (x > 10)
			result = 10;
		else
			result = 20;
		return result;
	}
}");
        }

        [Test]
        public void TestSkipComplexTrueExpression()
        {
            Analyze<ConvertIfStatementToConditionalTernaryExpressionAnalyzer>(@"class Foo
{
	static int Bar (int x)
	{
		int result;
		if (x > 10)
			result = 10 +
					 12;
		else
			result = 20;
		return result;
	}
}");
        }

        [Test]
        public void TestSkipComplexFalseExpression()
        {
            Analyze<ConvertIfStatementToConditionalTernaryExpressionAnalyzer>(@"class Foo
{
	static int Bar (int x)
	{
		int result;
		if (x > 10)
			result = 10;
		else
			result = 20 +

12;
		return result;
	}
}");
        }

        [Test]
        public void TestDisable()
        {
            Analyze<ConvertIfStatementToConditionalTernaryExpressionAnalyzer>(@"class Foo
{
	static int Bar (int x)
	{
		int result;
#pragma warning disable " + CSharpDiagnosticIDs.ConvertIfStatementToConditionalTernaryExpressionAnalyzerID + @"
		if (x > 10)
			result = 10;
		else
			result = 20;
		return result;
	}
}");
        }

        [Test]
        public void TestSkipAnnoyingCase1()
        {
            Analyze<ConvertIfStatementToConditionalTernaryExpressionAnalyzer>(@"class Foo
{
	int Bar(string example)
	{
		if (!string.IsNullOrEmpty (example)) {
			text = Environment.NewLine != ""\n"" ? example.Replace (""\n"", Environment.NewLine) : example;
		} else {
			text = """";
		}
	}
}");
        }


    }
}

