using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class RedundantLambdaSignatureParenthesesTests : CSharpDiagnosticTestBase
    {

        [Test]
        public void SimpleCase()
        {
            Test<RedundantLambdaSignatureParenthesesAnalyzer>(@"
class Program
{
	public delegate int MyDel(int j);

	public static void Foo()
	{
		MyDel increase = (j) => (j * 42);
	}
}
", 2, @"
class Program
{
	public delegate int MyDel(int j);

	public static void Foo()
	{
		MyDel increase = j => (j * 42);
	}
}
", 0);
        }

        [Test]
        public void InvalidCase1()
        {
            Analyze<RedundantLambdaSignatureParenthesesAnalyzer>(@"
class Program
{
	public delegate int MyDel(int j);

	public static void Foo()
	{
		MyDel increase = (int j) => (j * 42);
	}
}
");
        }

        [Test]
        public void InvalidCase2()
        {
            Analyze<RedundantLambdaSignatureParenthesesAnalyzer>(@"
using System;

class Program
{
	public static void Foo()
	{
		Action<int, int> act = (i, j) => (j * j);
	}
}
");
        }


        [Test]
        public void TestInvalid()
        {
            Analyze<RedundantLambdaSignatureParenthesesAnalyzer>(@"
class Program
{
	public delegate int MyDel(int j);

	public static void Foo()
	{
		MyDel increase = j => (j * 42);
	}
}
");
        }


        [Test]
        public void TestDisable()
        {
            Analyze<RedundantLambdaSignatureParenthesesAnalyzer>(@"
class Program
{
	public delegate int MyDel(int j);

	public static void Foo()
	{
// ReSharper disable RedundantLambdaSignatureParentheses
		MyDel increase = (j) => (j * 42);
// ReSharper restore RedundantLambdaSignatureParentheses
	}
}
");
        }

    }
}

