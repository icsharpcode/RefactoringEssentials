using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ConvertReferenceEqualsToEqualityOperatorTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

