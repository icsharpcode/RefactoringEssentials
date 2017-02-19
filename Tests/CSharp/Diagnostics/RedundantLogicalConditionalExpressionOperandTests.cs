using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class RedundantLogicalConditionalExpressionOperandTests : CSharpDiagnosticTestBase
    {
        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestTrue()
        {
            Test<RedundantLogicalConditionalExpressionOperandAnalyzer>(@"
class Test
{
	void Foo ()
	{
		if (true || b) {
		}
	}
}
", @"
class Test
{
	void Foo ()
	{
		if (b) {
		}
	}
}
");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestTrueCase2()
        {
            Test<RedundantLogicalConditionalExpressionOperandAnalyzer>(@"
class Test
{
	void Foo ()
	{
		if (b || (((true)))) {
		}
	}
}
", @"
class Test
{
	void Foo ()
	{
		if (b) {
		}
	}
}
");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestFalse()
        {
            Test<RedundantLogicalConditionalExpressionOperandAnalyzer>(@"
class Test
{
	void Foo ()
	{
		if (false && b) {
		}
	}
}
", @"
class Test
{
	void Foo ()
	{
		if (b) {
		}
	}
}
");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestFalseCase2()
        {
            Test<RedundantLogicalConditionalExpressionOperandAnalyzer>(@"
class Test
{
	void Foo ()
	{
		if (b && (((false)))) {
		}
	}
}
", @"
class Test
{
	void Foo ()
	{
		if (b) {
		}
	}
}
");
        }


        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestInvalid()
        {
            Analyze<RedundantLogicalConditionalExpressionOperandAnalyzer>(@"
class Test
{
	void Foo ()
	{
		if (false || b) {
		}
		if (true && b) {
		}
	}
}
");
        }


        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestDisable()
        {
            Analyze<RedundantLogicalConditionalExpressionOperandAnalyzer>(@"
class Test
{
	void Foo ()
	{
		// ReSharper disable once RedundantLogicalConditionalExpressionOperand
		if (true || b) {
		}
	}
}
");
        }

    }
}

