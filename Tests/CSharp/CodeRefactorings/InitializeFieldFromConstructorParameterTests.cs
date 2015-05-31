using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class InitializeFieldFromConstructorParameterTests : CSharpCodeRefactoringTestBase
    {
        [Test]
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

