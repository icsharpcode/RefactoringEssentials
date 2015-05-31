using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class RedundantLogicalConditionalExpressionOperandTests : CSharpDiagnosticTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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


        [Test]
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


        [Test]
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

