using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class ConvertEqualsToEqualityOperatorTests : CSharpCodeRefactoringTestBase
    {
        [Test]
        public void TestEquality()
        {
            Test<ConvertEqualsToEqualityOperatorCodeRefactoringProvider>(@"class FooBar
{
	public void Foo(object x, object y)
	{
		if ($Equals(x, y)) {
		}
	}
}", @"class FooBar
{
	public void Foo(object x, object y)
	{
		if (x == y) {
		}
	}
}");
        }

        [Test]
        public void TestInequality()
        {
            Test<ConvertEqualsToEqualityOperatorCodeRefactoringProvider>(@"class FooBar
{
	public void Foo(object x, object y)
	{
		if (!$Equals(x, y)) {
		}
	}
}", @"class FooBar
{
	public void Foo(object x, object y)
	{
		if (x != y) {
		}
	}
}");
        }

        [Test]
        public void TestEqualsFallback()
        {
            Test<ConvertEqualsToEqualityOperatorCodeRefactoringProvider>(@"class FooBar
{
	public void Foo(object x, object y)
	{
		if (object.$Equals(x, y)) {
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
		if (x == y) {
		}
	}
	public new static bool Equals(object o, object x)
	{
		return false;
	}
}");
        }

        [Test]
        public void TestMemberReferencEquals()
        {
            Test<ConvertEqualsToEqualityOperatorCodeRefactoringProvider>(@"class FooBar
{
	public void Foo (object x , object y)
	{
		if (x.$Equals(y)) {
		}
	}
}", @"class FooBar
{
	public void Foo (object x , object y)
	{
		if (x == y) {
		}
	}
}");
        }

        [Test]
        public void TestNegatedMemberReferenceEquals()
        {
            Test<ConvertEqualsToEqualityOperatorCodeRefactoringProvider>(@"class FooBar
{
	public void Foo(object x, object y)
	{
		if (!x.$Equals(y)) {
		}
	}
}", @"class FooBar
{
	public void Foo(object x, object y)
	{
		if (x != y) {
		}
	}
}");
        }
    }
}

