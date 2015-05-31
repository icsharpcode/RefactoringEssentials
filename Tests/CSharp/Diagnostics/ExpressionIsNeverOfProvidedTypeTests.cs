using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

class TestClass2 { }
class TestClass
{
    void TestMethod<T>(T x) where T : TestClass2
    {
        if (x is TestClass) ;
    }
}

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class ExpressionIsNeverOfProvidedTypeTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestClass()
        {
            var input = @"
class AnotherClass { }
class TestClass
{
	void TestMethod (AnotherClass x)
	{
		if (x is TestClass) ;
	}
}";
            Test<ExpressionIsNeverOfProvidedTypeAnalyzer>(input, 1);
        }

        [Test]
        public void TestClassNoIssue()
        {
            var input = @"
interface ITest { }
class TestClass
{
	void TestMethod (object x)
	{
		if (x is ITest) ;
		if (x is TestClass) ;
	}
}";
            Test<ExpressionIsNeverOfProvidedTypeAnalyzer>(input, 0);
        }

        [Test]
        public void TestSealedClass()
        {
            var input = @"
interface ITest { }
sealed class TestClass
{
	void TestMethod (TestClass x)
	{
		if (x is ITest) ;
	}
}";
            Test<ExpressionIsNeverOfProvidedTypeAnalyzer>(input, 1);
        }

        [Test]
        public void TestNull()
        {
            var input = @"
class TestClass
{
	void TestMethod ()
	{
		if (null is object) ;
	}
}";
            Test<ExpressionIsNeverOfProvidedTypeAnalyzer>(input, 1);
        }

        [Test]
        public void TestObjectToInt()
        {
            var input = @"
sealed class TestClass
{
	void TestMethod (object x)
	{
		if (x is int) ;
	}
}";
            Test<ExpressionIsNeverOfProvidedTypeAnalyzer>(input, 0);
        }

        [Test]
        public void TestClassIsTypeParameter()
        {
            var input = @"
class TestClass2 { }
class TestClass
{
	void TestMethod<T, T2> (TestClass x) where T : TestClass2 where T2 : struct
	{
		if (x is T) ;
		if (x is T2) ;
	}
}";
            Test<ExpressionIsNeverOfProvidedTypeAnalyzer>(input, 2);
        }

        [Test]
        public void TestClassIsTypeParameter2()
        {
            var input = @"
interface ITest { }
class TestBase { }
class TestClass2 : TestClass { }
class TestClass : TestBase
{
	void TestMethod<T, T2, T3> (TestClass x) where T : TestBase where T2 : ITest where T3 : TestClass2
	{
		if (x is T3) ;
	}
}";
            Test<ExpressionIsNeverOfProvidedTypeAnalyzer>(input, 0);
        }

        [Test]
        public void TestStructIsTypeParameter()
        {
            var input = @"
interface ITest { }
struct TestStruct : ITest { }
class TestClass
{
	void TestMethod<T> (TestStruct x) where T : ITest
	{
		if (x is T) ;
	}
}";
            Test<ExpressionIsNeverOfProvidedTypeAnalyzer>(input, 0);
        }

        [Test]
        public void TestStructIsTypeParameter2()
        {
            var input = @"
struct TestStruct { }
class TestClass
{
	void TestMethod<T> (TestStruct x) where T : class
	{
		if (x is T) ;
	}
}";
            // this is possible with T==object
            Test<ExpressionIsNeverOfProvidedTypeAnalyzer>(input, 0);
        }

        [Test]
        public void TestTypeParameter()
        {
            var input = @"
class TestClass2 { }
class TestClass
{
	void TestMethod<T> (T x) where T : TestClass2
	{
		if (x is TestClass) ;
	}
}";
            Test<ExpressionIsNeverOfProvidedTypeAnalyzer>(input, 1);
        }

        [Test]
        public void TestTypeParameter2()
        {
            var input = @"
class TestClass
{
	void TestMethod<T> (T x) where T : struct
	{
		if (x is TestClass) ;
	}
}";
            Test<ExpressionIsNeverOfProvidedTypeAnalyzer>(input, 1);
        }

        [Test]
        public void TestTypeParameter3()
        {
            var input = @"
interface ITest { }
class TestClass
{
	void TestMethod<T> (T x) where T : ITest, new()
	{
		if (x is TestClass) ;
	}
}";
            Test<ExpressionIsNeverOfProvidedTypeAnalyzer>(input, 0);
        }

        [Test]
        public void TestTypeParameter4()
        {
            var input = @"
interface ITest { }
sealed class TestClass
{
	void TestMethod<T> (T x) where T : ITest
	{
		if (x is TestClass) ;
	}
}";
            Test<ExpressionIsNeverOfProvidedTypeAnalyzer>(input, 1);
        }

        [Test]
        public void TestTypeParameterIsTypeParameter()
        {
            var input = @"
class TestClass2 { }
class TestClass
{
	void TestMethod<T, T2> (T x) where T : TestClass where T2 : TestClass2
	{
		if (x is T2) ;
	}
}";
            Test<ExpressionIsNeverOfProvidedTypeAnalyzer>(input, 1);
        }

        [Test]
        public void TestTypeParameterIsTypeParameter2()
        {
            var input = @"
interface ITest { }
class TestClass
{
	void TestMethod<T, T2> (T x) where T : TestClass where T2 : ITest, new()
	{
		if (x is T2) ;
	}
}";
            Test<ExpressionIsNeverOfProvidedTypeAnalyzer>(input, 0);
        }

        [Test]
        public void TestObjectArrayToStringArray()
        {
            var input = @"
sealed class TestClass
{
	void TestMethod (object[] x)
	{
		if (x is string[]) ;
	}
}";
            Test<ExpressionIsNeverOfProvidedTypeAnalyzer>(input, 0);
        }

        [Test]
        public void UnknownExpression()
        {
            var input = @"
sealed class TestClass
{
	void TestMethod ()
	{
		if (unknown is string) ;
	}
}";
            Test<ExpressionIsNeverOfProvidedTypeAnalyzer>(input, 0);
        }

        [Test]
        public void UnknownType()
        {
            var input = @"
sealed class TestClass
{
	void TestMethod (int x)
	{
		if (x is unknown) ;
	}
}";
            Test<ExpressionIsNeverOfProvidedTypeAnalyzer>(input, 0);
        }
    }
}
