using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class JoinStringTests : CSharpCodeRefactoringTestBase
    {
        public void Test(string input, string output)
        {
            Test<JoinStringCodeRefactoringProvider>(@"
class TestClass
{
	string TestMethod (string arg)
	{
		return " + input + @";
	}
}", @"
class TestClass
{
	string TestMethod (string arg)
	{
		return " + output + @";
	}
}");
        }

        [Test]
        public void TestRegularString()
        {
            Test("\"a\" $+ \"a\"", "\"aa\"");
            Test("arg + \"a\" $+ \"a\"", "arg + \"aa\"");
        }

        [Test]
        public void TestVerbatimString()
        {
            Test("@\"a\" $+ @\"a\"", "@\"aa\"");
        }

        public void TestWrongContext(string input)
        {
            TestWrongContext<JoinStringCodeRefactoringProvider>(@"
class TestClass
{
	string TestMethod ()
	{
		return " + input + @";
	}
}");
        }

        [Test]
        public void TestWrongContext()
        {
            TestWrongContext("@\"a\" $+ \"a\"");
            TestWrongContext("\"a\" $+ @\"a\"");
        }
    }
}
