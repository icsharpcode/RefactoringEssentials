using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
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
		$i = 1$;
	}
}";

            var output = @"
class TestClass
{
	void TestMethod(int i)
	{
	}
}";
            Analyze<ParameterOnlyAssignedAnalyzer>(input,output);
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
            Analyze<ParameterOnlyAssignedAnalyzer>(input);
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
            Analyze<ParameterOnlyAssignedAnalyzer>(input);
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
            Analyze<ParameterOnlyAssignedAnalyzer>(input);
        }
    }
}
