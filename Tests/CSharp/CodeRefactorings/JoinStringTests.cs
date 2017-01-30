using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
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

        [Fact]
        public void TestRegularString()
        {
            Test("\"a\" $+ \"a\"", "\"aa\"");
            Test("arg + \"a\" $+ \"a\"", "arg + \"aa\"");
        }

        [Fact]
        public void TestVerbatimString()
        {
            Test("@\"a\" $+ @\"a\"", "@\"aa\"");
        }

		[Theory]
		[InlineData("@\"a\" $+ \"a\"")]
		[InlineData("\"a\" $+ @\"a\"")]
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
    }
}
