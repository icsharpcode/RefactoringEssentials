using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class ForControlVariableIsNeverModifiedTests : CSharpDiagnosticTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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
