using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class InitializeFieldFromConstructorParameterTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestSimple()
        {
            Test<InitializeFieldFromConstructorParameterCodeRefactoringProvider>(@"
class Foo
{
    public Foo(int $x, int y)
    {
    }
}", @"
class Foo
{
    readonly int x;

    public Foo(int x, int y)
    {
        this.x = x;
    }
}");
        }
    }
}

