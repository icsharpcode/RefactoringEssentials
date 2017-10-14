using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ReplaceDirectCastWithSafeCastTest : CSharpCodeRefactoringTestBase
    {
        void TestType(string type)
        {
            string input = @"
using System;
class TestClass
{
	void Test (object a)
	{
		var b = ($" + type + @")a;
	}
}";
            string output = @"
using System;
class TestClass
{
	void Test (object a)
	{
		var b = a as " + type + @";
	}
}";
            Test<ReplaceDirectCastWithSafeCastCodeRefactoringProvider>(input, output);
        }

        [Fact]
        public void Test()
        {
            TestType("Exception");
        }

        [Fact]
        public void TestNullable()
        {
            TestType("int?");
        }

        [Fact]
        public void TestWithComment()
        {
            string input = @"
using System;
class TestClass
{
	void Test (object a)
	{
		// Some comment
		var b = ($Exception)a;
	}
}";
            string output = @"
using System;
class TestClass
{
	void Test (object a)
	{
		// Some comment
		var b = a as Exception;
	}
}";

            Test<ReplaceDirectCastWithSafeCastCodeRefactoringProvider>(input, output);
        }

        [Fact]
        public void TestNonReferenceType()
        {
            TestWrongContext<ReplaceDirectCastWithSafeCastCodeRefactoringProvider>(@"
using System;
class TestClass
{
	void Test (object a)
	{
		var b = ($int)a;
	}
}");
        }

        [Fact]
        public void TestInsertParentheses()
        {
            string input = @"
class TestClass {
	void TestMethod (object o)
	{
		var b = 1 + ($TestClass)o;
	}
}";
            string output = @"
class TestClass {
	void TestMethod (object o)
	{
		var b = 1 + (o as TestClass);
	}
}";
            Test<ReplaceDirectCastWithSafeCastCodeRefactoringProvider>(input, output);
        }
    }
}
