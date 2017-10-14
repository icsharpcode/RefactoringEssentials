using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ConvertEqualityOperatorToReferenceEqualsTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestEquality()
        {
            Test<ConvertEqualityOperatorToReferenceEqualsCodeRefactoringProvider>(@"class FooBar
{
    public void Foo(object x, object y)
    {
        if (x $== y) {
        }
    }
}", @"class FooBar
{
    public void Foo(object x, object y)
    {
        if (ReferenceEquals(x, y)) {
        }
    }
}");
        }

        [Fact]
        public void TestInequality()
        {
            Test<ConvertEqualityOperatorToReferenceEqualsCodeRefactoringProvider>(@"class FooBar
{
    public void Foo(object x, object y)
    {
        if (x $!= y) {
        }
    }
}", @"class FooBar
{
    public void Foo(object x, object y)
    {
        if (!ReferenceEquals(x, y)) {
        }
    }
}");
        }

        [Fact]
        public void TestStruct()
        {
            TestWrongContext<ConvertEqualityOperatorToReferenceEqualsCodeRefactoringProvider>(@"
struct MyStruct {}
class FooBar
{
    public void Foo(MyStruct x, MyStruct y)
    {
        if (x $== y) {
        }
    }
}");
        }

        [Fact]
        public void TestEqualsFallback()
        {
            Test<ConvertEqualityOperatorToReferenceEqualsCodeRefactoringProvider>(@"class FooBar
{
    public void Foo(object x, object y)
    {
        if (x $== y) {
        }
    }
    public new static bool ReferenceEquals(object o, object x)
    {
        return false;
    }
}", @"class FooBar
{
    public void Foo(object x, object y)
    {
        if (object.ReferenceEquals(x, y)) {
        }
    }
    public new static bool ReferenceEquals(object o, object x)
    {
        return false;
    }
}");
        }
    }
}

