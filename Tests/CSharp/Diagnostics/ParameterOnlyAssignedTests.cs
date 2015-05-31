using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class ParameterOnlyAssignedTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestUnusedValue()
        {
            var input = @"
class TestClass
{
	void TestMethod(int i)
	{
		i = 1;
	}
}";
            Test<ParameterOnlyAssignedAnalyzer>(input, 1);
        }

        [Test]
        public void TestUsedValue()
        {
            var input = @"
class TestClass
{
	int TestMethod(int i)
	{
		i = 1;
		return i;
	}
}";
            Test<ParameterOnlyAssignedAnalyzer>(input, 0);
        }

        [Test]
        public void TestOutParametr()
        {
            var input = @"
class TestClass
{
	void TestMethod(out int i)
	{
		i = 1;
	}
}";
            Test<ParameterOnlyAssignedAnalyzer>(input, 0);
        }

        [Test]
        public void TestRefParametr()
        {
            var input = @"
class TestClass
{
	void TestMethod(ref int i)
	{
		i = 1;
	}
}";
            Test<ParameterOnlyAssignedAnalyzer>(input, 0);
        }
    }
}
