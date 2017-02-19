using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class RedundantLambdaSignatureParenthesesTests : CSharpDiagnosticTestBase
    {

        [Fact(Skip="TODO: Issue not ported yet")]
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

        [Fact(Skip="TODO: Issue not ported yet")]
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

        [Fact(Skip="TODO: Issue not ported yet")]
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


        [Fact(Skip="TODO: Issue not ported yet")]
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


        [Fact(Skip="TODO: Issue not ported yet")]
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

