using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class FlipOperatorArguments : CSharpCodeRefactoringTestBase
    {
        [Test]
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


        [Test]
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