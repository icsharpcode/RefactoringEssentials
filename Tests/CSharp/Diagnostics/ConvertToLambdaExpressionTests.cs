using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class ConvertToLambdaExpressionTests : CSharpDiagnosticTestBase
    {
        [Fact(Skip="TODO: Issue not ported yet")]
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

        [Fact(Skip="TODO: Issue not ported yet")]
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

        [Fact(Skip="TODO: Issue not ported yet")]
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


        [Fact(Skip="TODO: Issue not ported yet")]
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
        [Fact(Skip="TODO: Issue not ported yet")]
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

        [Fact(Skip="TODO: Issue not ported yet")]
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


        [Fact(Skip="TODO: Issue not ported yet")]
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

        [Fact(Skip="TODO: Issue not ported yet")]
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

