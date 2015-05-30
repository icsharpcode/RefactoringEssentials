using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class InitializeAutoPropertyFromConstructorParameterTests : ContextActionTestBase
    {
        [Test]
        public void TestSimple()
        {
            Test<InitializeAutoPropertyFromConstructorParameterCodeRefactoringProvider>(@"
class Foo
{
    public Foo(int $x, int y)
    {
    }
}", @"
class Foo
{
    public int X { get; set; }

    public Foo(int x, int y)
    {
        X = x;
    }
}");
        }
    }
}

