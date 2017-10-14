using RefactoringEssentials.CSharp.CodeFixes;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeFixes
{
    public class CS0759RedundantPartialMethodTests : CSharpCodeFixTestBase
    {
        [Fact]
        public void TestRedundantModifier()
        {
            var input = @"
partial class TestClass
{
	partial void TestMethod ()
	{
		int i = 1;
	}
}";
            var output = @"
partial class TestClass
{
	void TestMethod ()
	{
		int i = 1;
	}
}";
            Test<CS0759RedundantPartialMethodCodeFixProvider>(input, output);
        }
    }
}