using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
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
		for (int i = 0, j = 0; i < 10; j++)
		{
		}
	}
}";
            Test<ForControlVariableIsNeverModifiedAnalyzer>(input, 1);
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
            Test<ForControlVariableIsNeverModifiedAnalyzer>(input, 0);
        }

        [Test]
        public void TestUnaryOpConditionNotModified()
        {
            var input = @"
class TestClass
{
	void TestMethod ()
	{
		for (bool x = true; !x;)
		{
		}
	}
}";
            Test<ForControlVariableIsNeverModifiedAnalyzer>(input, 1);
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
            Test<ForControlVariableIsNeverModifiedAnalyzer>(input, 0);
        }

        [Test]
        public void TestIdentifierConditionNotModified()
        {
            var input = @"
class TestClass
{
	void TestMethod ()
	{
		for (bool x = true; x;)
		{
		}
	}
}";
            Test<ForControlVariableIsNeverModifiedAnalyzer>(input, 1);
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
            Test<ForControlVariableIsNeverModifiedAnalyzer>(input, 0);
        }
    }
}
