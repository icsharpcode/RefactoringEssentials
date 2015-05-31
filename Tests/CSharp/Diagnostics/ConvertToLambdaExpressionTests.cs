using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class ConvertToLambdaExpressionTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestReturn()
        {
            Test<ConvertToLambdaExpressionAnalyzer>(@"
class TestClass
{
	void TestMethod ()
	{
		System.Func<int, int> f = i => {
			return i + 1;
		};
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		System.Func<int, int> f = i => i + 1;
	}
}");
        }

        [Test]
        public void TestExpressionStatement()
        {
            Test<ConvertToLambdaExpressionAnalyzer>(@"
class TestClass
{
	void TestMethod ()
	{
		System.Action<int> f = i => {
			System.Console.Write (i);
		};
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		System.Action<int> f = i => System.Console.Write (i);
	}
}");
        }

        [Test]
        public void TestDisable()
        {
            Analyze<ConvertToLambdaExpressionAnalyzer>(@"
class TestClass
{
	void TestMethod ()
	{
		// ReSharper disable once ConvertToLambdaExpression
		System.Func<int, int> f = i => {
			return i + 1;
		};
	}
}");
        }


        [Test]
        public void TestAssignmentExpression()
        {
            Analyze<ConvertToLambdaExpressionAnalyzer>(@"
using System;
public class Test
{
	void Foo ()
	{
		int i;
		Action<int> foo = x => {
			i = x + x;
		};
		foo(5);
	}
}
");
        }


        /// <summary>
        /// Bug 14840 - Incorrect "can be converted to expression" suggestion
        /// </summary>
        [Test]
        public void TestBug14840()
        {
            Analyze<ConvertToLambdaExpressionAnalyzer>(@"using System;
using System.Collections.Generic;

class C
{
	void Foo (Action<int> a) {}
	void Foo (Func<int,int> a) {}

	void Test ()
	{
		int t = 0;
		Foo (c => { t = c; });
	}
}");
        }

        [Test]
        public void TestAnonymousMethod()
        {
            Test<ConvertToLambdaExpressionAnalyzer>(@"
class TestClass
{
	void TestMethod ()
	{
		System.Action a = delegate () {
			System.Console.WriteLine ();
		};
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		System.Action a = () => System.Console.WriteLine ();
	}
}");
        }


        [Test]
        public void TestAnonymousFunction()
        {
            Test<ConvertToLambdaExpressionAnalyzer>(@"
class TestClass
{
	void TestMethod ()
	{
		System.Func<int, int> f = delegate (int i) {
			return i + 1;
		};
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		System.Func<int, int> f = (int i) => i + 1;
	}
}");
        }

        [Test]
        public void TestAnonymousMethodWithoutParameterList()
        {
            Analyze<ConvertToLambdaExpressionAnalyzer>(@"
class TestClass
{
	void TestMethod ()
	{
		System.Func<int, int> f = delegate {
			return 123;
		};
	}
}");
        }


    }
}

