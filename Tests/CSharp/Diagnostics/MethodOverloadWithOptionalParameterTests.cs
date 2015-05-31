using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class MethodOverloadWithOptionalParameterTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestSingleMethod()
        {
            TestIssue<MethodOverloadWithOptionalParameterAnalyzer>(@"
using System;

public class FooBar
{
	public void Print(string message)
	{
		Console.WriteLine(message);
	}

	private void Print(string message, string messageDelimiter = ""==="")
	{
		Console.WriteLine(message + messageDelimiter);
	}
}
");
        }

        [Test]
        public void TestTwoParameters()
        {
            TestIssue<MethodOverloadWithOptionalParameterAnalyzer>(@"
using System;

public class FooBar
{
    public void Print(string message)
    {
        Console.WriteLine(message);
    }

    public void Print(string message, string str2)
    {
        Console.WriteLine(message);
    }

    private void Print(string message, string messageDelimiter = ""==="", string secondmessage = ""==="")
	{
		Console.WriteLine(message + messageDelimiter);
	}
}", 2);
        }

        [Test]
        public void TestIndexer()
        {
            TestIssue<MethodOverloadWithOptionalParameterAnalyzer>(@"
using System;

public class FooBar
{
	public string this[string message]
	{
		get { return message; }
	}

	private string this[string message, string messageDelimiter = ""===""]
	{
		get { return message + messageDelimiter; }
	}
}
");
        }


        [Test]
        public void TestDisable()
        {
            Analyze<MethodOverloadWithOptionalParameterAnalyzer>(@"
using System;

public class FooBar
{
	public void Print(string message)
	{
		Console.WriteLine(message);
	}

	// ReSharper disable once MethodOverloadWithOptionalParameter
	private void Print(string message, string messageDelimiter = ""==="")
	{
		Console.WriteLine(message + messageDelimiter);
	}
}
");
        }

        [Test]
        public void Test()
        {
            var input = @"
class TestClass
{
	void TestMethod (int a)
	{ }
	void TestMethod (int a, int b = 1)
	{ }
}";
            Test<MethodOverloadWithOptionalParameterAnalyzer>(input, 1);
        }

        [Test]
        public void Test2()
        {
            var input = @"
class TestClass
{
	void TestMethod (int a, int b)
	{ }
	void TestMethod (int a, int b = 1, int c = 1)
	{ }
}";
            Test<MethodOverloadWithOptionalParameterAnalyzer>(input, 1);
        }

        [Test]
        public void TestNoIssue()
        {
            var input = @"
class TestClass
{
	void TestMethod (int a, int b = 1, int c = 1)
	{ }
}";
            Test<MethodOverloadWithOptionalParameterAnalyzer>(input, 0);
        }

        [Test]
        public void TestNoIssue_Generics()
        {
            var input = @"
class TestClass
{
	void TestMethod (object obj) { }
	void TestMethod<T> (object obj, int arg = 0) { }
}";
            Test<MethodOverloadWithOptionalParameterAnalyzer>(input, 0);
        }

    }
}

