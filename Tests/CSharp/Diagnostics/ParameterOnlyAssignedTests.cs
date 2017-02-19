using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class ParameterOnlyAssignedTests : CSharpDiagnosticTestBase
    {
        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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
