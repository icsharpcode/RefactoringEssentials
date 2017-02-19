using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class ForControlVariableIsNeverModifiedTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestBinaryOpConditionNotModified()
        {
            var input = @"
class TestClass
{
	void TestMethod ()
	{
		for (int $i$ = 0, j = 0; i < 10; j++)
		{
		}
	}
}";
            Analyze<ForControlVariableIsNeverModifiedAnalyzer>(input);
        }

        [Fact]
        public void TestBinaryOpConditionModified()
        {
            var input = @"
class TestClass
{
	void TestMethod ()
	{
		for (int i = 0, j = 0; i < 10; i++)
		{
		}
	}
}";
            Analyze<ForControlVariableIsNeverModifiedAnalyzer>(input);
        }

        [Fact]
        public void TestUnaryOpConditionNotModified()
        {
            var input = @"
class TestClass
{
	void TestMethod ()
	{
		for (bool $x$ = true; !x;)
		{
		}
	}
}";
            Analyze<ForControlVariableIsNeverModifiedAnalyzer>(input);
        }

        [Fact]
        public void TestUnaryOpConditionModified()
        {
            var input = @"
class TestClass
{
	void TestMethod ()
	{
		for (bool x = true; !x;)
		{
			x = false;
		}
	}
}";
            Analyze<ForControlVariableIsNeverModifiedAnalyzer>(input);
        }

        [Fact]
        public void TestIdentifierConditionNotModified()
        {
            var input = @"
class TestClass
{
	void TestMethod ()
	{
		for (bool $x$ = true; x;)
		{
		}
	}
}";
            Analyze<ForControlVariableIsNeverModifiedAnalyzer>(input);
        }

        [Fact]
        public void TestIdentifierConditionModified()
        {
            var input = @"
class TestClass
{
	void TestMethod ()
	{
		for (bool x = false; x;)
		{
			x = true;
		}
	}
}";
            Analyze<ForControlVariableIsNeverModifiedAnalyzer>(input);
        }
    }
}
