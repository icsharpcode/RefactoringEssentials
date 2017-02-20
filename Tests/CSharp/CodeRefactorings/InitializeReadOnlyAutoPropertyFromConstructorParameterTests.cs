using Xunit;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class InitializeReadOnlyAutoPropertyFromConstructorParameterTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void InitialiseInteger()
        {
            Test<InitializeReadOnlyAutoPropertyFromConstructorParameterCodeRefactoringProvider>(@"
class Foo
{
    public Foo(int $x, int y)
    {
    }
}", @"
class Foo
{
    public int X { get; }

    public Foo(int x, int y)
    {
        X = x;
    }
}");
        }

        [Fact]
        public void InitialiseInterface()
        {
            Test<InitializeReadOnlyAutoPropertyFromConstructorParameterCodeRefactoringProvider>(@"
class Foo
{
    public Foo(int x, ICedd $cedd)
    {
    }
}", @"
class Foo
{
    public ICedd Cedd { get; }

    public Foo(int x, ICedd cedd)
    {
        Cedd = cedd;
    }
}");
        }

        [Fact]
        public void NotInParameterList()
        {
            TestWrongContext<InitializeReadOnlyAutoPropertyFromConstructorParameterCodeRefactoringProvider>(@"
class Foo
{
    public $Foo(int x, int y)
    {
    }
}");
        }

        [Fact]
        public void NotInParameterValue()
        {
            TestWrongContext<InitializeReadOnlyAutoPropertyFromConstructorParameterCodeRefactoringProvider>(@"
class Foo
{
    public Foo($int x, int y)
    {
    }
}");
        }

        [Fact]
        public void NotInConstructor()
        {
            TestWrongContext<InitializeReadOnlyAutoPropertyFromConstructorParameterCodeRefactoringProvider>(@"
class Foo
{
    public void Foo(int $x, int y)
    {
    }
}");
        }
    }
}

