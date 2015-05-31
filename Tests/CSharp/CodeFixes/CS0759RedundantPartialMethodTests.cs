using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeFixes;

namespace RefactoringEssentials.Tests.CSharp.CodeFixes
{
    [TestFixture]
    public class CS0759RedundantPartialMethodTests : CSharpCodeFixTestBase
    {
        [Test]
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