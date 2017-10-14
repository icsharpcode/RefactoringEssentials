using RefactoringEssentials.CSharp.CodeFixes;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeFixes
{
    public class CS0132StaticConstructorParameterTests : CSharpCodeFixTestBase
    {
        [Fact]
        public void TestSimpleCase()
        {
            Test<CS0132StaticConstructorParameterCodeFixProvider>(@"class Foo
{
    static $Foo$(int bar)
    {
    }
}", @"class Foo
{
    static Foo()
    {
    }
}");
        }

        [Fact]
        public void TestNoIssue()
        {
            TestWrongContext<CS0132StaticConstructorParameterCodeFixProvider>(@"
        class Foo
        {
            static Foo ()
            {
            }
        }
        ");
        }

    }
}

