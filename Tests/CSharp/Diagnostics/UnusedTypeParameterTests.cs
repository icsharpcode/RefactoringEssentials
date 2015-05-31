using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class UnusedTypeParameterTests : CSharpDiagnosticTestBase
    {

        [Test]
        public void TestUnusedTypeParameter()
        {
            var input = @"
class TestClass {
	void TestMethod<T> ()
	{
	}
}";
            Test<UnusedTypeParameterAnalyzer>(input, 1);
        }

        [Test]
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
            Test<UnusedTypeParameterAnalyzer>(input, 0);
            Test<UnusedTypeParameterAnalyzer>(input2, 0);
        }

    }
}
