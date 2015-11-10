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
            Analyze<ParameterOnlyAssignedAnalyzer>(@"
class TestClass
{
	void TestMethod(int i)
	{
		$i = 1$;
	}
}", @"
class TestClass
{
	void TestMethod(int i)
	{
	}
}");
        }

        [Test]
        public void TestUsedValue()
        {
            Analyze<ParameterOnlyAssignedAnalyzer>(@"
class TestClass
{
	int TestMethod(int i)
	{
		i = 1;
		return i;
	}
}");
        }

        [Test]
        public void TestOutParameter()
        {
            Analyze<ParameterOnlyAssignedAnalyzer>(@"
class TestClass
{
	void TestMethod(out int i)
	{
		i = 1;
	}
}");
        }

        [Test]
        public void TestRefParameter()
        {
            Analyze<ParameterOnlyAssignedAnalyzer>(@"
class TestClass
{
	void TestMethod(ref int i)
	{
		i = 1;
	}
}");
        }

        [Test]
        public void TestMultipleParameters()
        {
            Analyze<ParameterOnlyAssignedAnalyzer>(@"
class TestClass
{
    public static void TestMethod<T, T2, T3>(out T argument, ref T2 argument2, T3 argument3) where T : class where T2 : struct
    {
        argument = null;
        argument2 = default(T2);
        $argument3 = default(T3)$;
    }
}");
        }
    }
}
