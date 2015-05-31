using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeFixes;

namespace RefactoringEssentials.Tests.CSharp.CodeFixes
{
    [TestFixture]
    public class CS0164LabelHasNotBeenReferencedTests : CSharpCodeFixTestBase
    {
        [Test]
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

