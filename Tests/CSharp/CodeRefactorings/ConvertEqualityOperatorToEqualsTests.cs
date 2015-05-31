using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class ConvertEqualityOperatorToEqualsTests : CSharpCodeRefactoringTestBase
    {
        [Test]
        public void TestEquality()
        {
            Test<ConvertEqualityOperatorToEqualsCodeRefactoringProvider>(@"class FooBar
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
        if (Equals(x, y)) {
        }
    }
}");
        }

        [Test]
        public void TestInequality()
        {
            Test<ConvertEqualityOperatorToEqualsCodeRefactoringProvider>(@"class FooBar
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
        if (!Equals(x, y)) {
        }
    }
}");
        }

        [Test]
        public void TestEqualsFallback()
        {
            Test<ConvertEqualityOperatorToEqualsCodeRefactoringProvider>(@"class FooBar
{
    public void Foo(object x, object y)
    {
        if (x $== y) {
        }
    }
    public new static bool Equals(object o, object x)
    {
        return false;
    }
}", @"class FooBar
{
    public void Foo(object x, object y)
    {
        if (object.Equals(x, y)) {
        }
    }
    public new static bool Equals(object o, object x)
    {
        return false;
    }
}");
        }
    }
}

