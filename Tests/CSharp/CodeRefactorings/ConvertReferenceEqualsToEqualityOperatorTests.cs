using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class ConvertReferenceEqualsToEqualityOperatorTests : CSharpCodeRefactoringTestBase
    {
        [Test]
        public void TestEquality()
        {
            Test<ConvertReferenceEqualsToEqualityOperatorCodeRefactoringProvider>(@"class FooBar
{
	public void Foo(object x, object y)
	{
		if ($ReferenceEquals(x, y)) {
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
            Test<ConvertReferenceEqualsToEqualityOperatorCodeRefactoringProvider>(@"class FooBar
{
	public void Foo(object x, object y)
	{
		if (!$ReferenceEquals(x, y)) {
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
            Test<ConvertReferenceEqualsToEqualityOperatorCodeRefactoringProvider>(@"class FooBar
{
	public void Foo(object x, object y)
	{
		if (object.$ReferenceEquals(x, y)) {
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
		if (x == y) {
		}
	}
	public new static bool ReferenceEquals(object o, object x)
	{
		return false;
	}
}");
        }

        [Test]
        public void TestMemberReferencEquals()
        {
            Test<ConvertReferenceEqualsToEqualityOperatorCodeRefactoringProvider>(@"class FooBar
{
	public void Foo (object x , object y)
	{
		if ($ReferenceEquals(x, y)) {
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
            Test<ConvertReferenceEqualsToEqualityOperatorCodeRefactoringProvider>(@"class FooBar
{
	public void Foo(object x, object y)
	{
		if (!$ReferenceEquals(x, y)) {
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

