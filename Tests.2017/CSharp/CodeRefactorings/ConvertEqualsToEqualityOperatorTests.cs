using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ConvertEqualsToEqualityOperatorTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

