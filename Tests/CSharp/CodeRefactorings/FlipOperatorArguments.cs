using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class FlipOperatorArguments : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestEquals()
        {
            Test<FlipOperatorArgumentsCodeRefactoringProvider>(@"
class Foo
{
    public void FooFoo (int x, int y)
    {
        if (x $== y))
            Console.WriteLine (x);
    }
}", @"
class Foo
{
    public void FooFoo (int x, int y)
    {
        if (y == x))
            Console.WriteLine (x);
    }
}");
        }


        [Fact]
        public void TestNotEquals()
        {
            Test<FlipOperatorArgumentsCodeRefactoringProvider>(@"
class Foo
{
    public void FooFoo (int x, int y)
    {
        if (x $!= y))
            Console.WriteLine (x);
    }
}", @"
class Foo
{
    public void FooFoo (int x, int y)
    {
        if (y != x))
            Console.WriteLine (x);
    }
}");
        }
    }
}