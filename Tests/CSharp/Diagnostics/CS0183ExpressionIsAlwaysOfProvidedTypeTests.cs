using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class CS0183ExpressionIsAlwaysOfProvidedTypeTests : CSharpDiagnosticTestBase
    {
        public void Test(string variableType, string providedType)
        {
            var input = @"
class TestClass
{
	void TestMethod (" + variableType + @" x)
	{
		if (x is " + providedType + @")
			;
	}
}";
            var output = @"
class TestClass
{
	void TestMethod (" + variableType + @" x)
	{
		if (x != null)
			;
	}
}";
            Test<CS0183ExpressionIsAlwaysOfProvidedTypeAnalyzer>(input, 1, output);
        }

        [Test]
        public void TestSameType()
        {
            Test("int", "int");
        }

        [Test]
        public void TestBaseType()
        {
            Test("int", "object");
        }

        [Test]
        public void TestTypeParameter()
        {
            var input = @"
class TestClass
{
	void TestMethod<T> (T x) where T : TestClass
	{
		if (x is TestClass)
			;
	}
}";
            var output = @"
class TestClass
{
	void TestMethod<T> (T x) where T : TestClass
	{
		if (x != null)
			;
	}
}";
            Test<CS0183ExpressionIsAlwaysOfProvidedTypeAnalyzer>(input, 1, output);
        }

        [Test]
        public void IntIsNotDouble()
        {
            var input = @"
sealed class TestClass
{
	void TestMethod (int x)
	{
		if (x is double) ;
	}
}";
            Test<CS0183ExpressionIsAlwaysOfProvidedTypeAnalyzer>(input, 0);
        }

        [Test]
        public void MissingTypes()
        {
            var input = @"
sealed class TestClass
{
	void TestMethod (object x)
	{
		if (x.MissingMethod() is MissingClass) ;
	}
}";
            Test<CS0183ExpressionIsAlwaysOfProvidedTypeAnalyzer>(input, 0);
        }


        [Test]
        public void TestDisable()
        {
            Analyze<CS0183ExpressionIsAlwaysOfProvidedTypeAnalyzer>(@"
class TestClass
{
	void TestMethod (TestClass x)
	{
		// ReSharper disable once CSharpWarnings::CS0183
		if (x is TestClass)
			;
	}
}");
        }

        [Test]
        public void TestPragmaDisable()
        {
            Analyze<CS0183ExpressionIsAlwaysOfProvidedTypeAnalyzer>(@"
class TestClass
{
	void TestMethod (TestClass x)
	{
#pragma warning disable 183
		if (x is TestClass)
#pragma warning restore 183
			;
	}
}");
        }
    }
}
