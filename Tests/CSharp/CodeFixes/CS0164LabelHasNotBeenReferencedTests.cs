using RefactoringEssentials.CSharp.CodeFixes;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeFixes
{
    public class CS0164LabelHasNotBeenReferencedTests : CSharpCodeFixTestBase
    {
        [Fact]
        public void TestUnusedLabelInMethod()
        {
            Test<CS0164LabelHasNotBeenReferencedCodeFixProvider>(@"
class Foo
{
	void Test()
	{
		bar: ;
	}
}
", @"
class Foo
{
	void Test()
	{
		;
	}
}
");
        }
    }
}

