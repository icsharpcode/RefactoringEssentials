using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class UnusedTypeParameterTests : CSharpDiagnosticTestBase
    {

        [Fact]
        public void TestUnusedTypeParameter()
        {
            var input = @"
class TestClass {
    void TestMethod<$T$> ()
    {
    }
}";
            Analyze<UnusedTypeParameterAnalyzer>(input);
        }

        [Fact]
        public void TestUsedTypeParameter()
        {
            var input = @"
class TestClass {
	void TestMethod<T> (T i)
	{
	}
}";
            var input2 = @"
class TestClass {
	T TestMethod<T> ()
	{
	}
}";
            Analyze<UnusedTypeParameterAnalyzer>(input);
            Analyze<UnusedTypeParameterAnalyzer>(input2);
        }


        [Fact]
        public void TestInterface()
        {
            var input = @"
interface ITest {
    void TestMethod<T> (T i);
}";
            Analyze<UnusedTypeParameterAnalyzer>(input);
        }

        [Fact]
        public void TestInterfaceImplementation()
        {
            var input = @"
interface ITest {
    void TestMethod<T> (T i);
}
class TestClass : ITest {
    void TestMethod<$T$> ()
    {
    }
}
";
            Analyze<UnusedTypeParameterAnalyzer>(input);
        }


        [Fact]
        public void TestAbstractClass()
        {
            var input = @"
abstract class TestClass<T> {
}";
            Analyze<UnusedTypeParameterAnalyzer>(input);
        }

        [Fact]
        public void TestAbstractMethod()
        {
            var input = @"
abstract class TestClass {
    public abstract void TestMethod<T> ()
    {
    }
}";
            Analyze<UnusedTypeParameterAnalyzer>(input);
        }


    }
}
