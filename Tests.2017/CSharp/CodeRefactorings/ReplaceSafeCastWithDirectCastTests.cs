using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ReplaceSafeCastWithDirectCastTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void Test()
        {
            Test<ReplaceSafeCastWithDirectCastCodeRefactoringProvider>(@"
using System;
class TestClass
{
	void Test (object a)
	{
		var b = a $as Exception;
	}
}", @"
using System;
class TestClass
{
	void Test (object a)
	{
		var b = (Exception)a;
	}
}");
        }

        [Fact]
        public void TestWithComment1()
        {
            Test<ReplaceSafeCastWithDirectCastCodeRefactoringProvider>(@"
using System;
class TestClass
{
	void Test (object a)
	{
		// Some comment
		var b = a $as Exception;
	}
}", @"
using System;
class TestClass
{
	void Test (object a)
	{
		// Some comment
		var b = (Exception)a;
	}
}");
        }

        [Fact]
        public void TestWithComment2()
        {
            Test<ReplaceSafeCastWithDirectCastCodeRefactoringProvider>(@"
using System;
class TestClass
{
	void Test (object a)
	{
		var b = a $as Exception; // Some comment
	}
}", @"
using System;
class TestClass
{
	void Test (object a)
	{
		var b = (Exception)a; // Some comment
	}
}");
        }

        [Fact]
        public void TestRemoveParentheses()
        {
            string input = @"
class TestClass {
	void TestMethod (object o)
	{
		var b = 1 + (o $as TestClass);
	}
}";
            string output = @"
class TestClass {
	void TestMethod (object o)
	{
		var b = 1 + (TestClass)o;
	}
}";
            Test<ReplaceSafeCastWithDirectCastCodeRefactoringProvider>(input, output);
        }

        [Fact]
        public void TestInsertParentheses()
        {
            string input = @"
class TestClass {
	void TestMethod (object o)
	{
		var b = 1 + o $as TestClass;
	}
}";
            string output = @"
class TestClass {
	void TestMethod (object o)
	{
		var b = (TestClass)(1 + o);
	}
}";
            Test<ReplaceSafeCastWithDirectCastCodeRefactoringProvider>(input, output);
        }
    }
}
