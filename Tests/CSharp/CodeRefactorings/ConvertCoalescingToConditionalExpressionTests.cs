using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ConvertCoalescingToConditionalExpressionTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestSimpleCase()
        {
            Test<ConvertCoalescingToConditionalExpressionCodeRefactoringProvider>(@"
class Test
{
	object Foo(object o, object p)
	{
		return o $?? p;
	}
}
", @"
class Test
{
	object Foo(object o, object p)
	{
		return o != null ? o : p;
	}
}
");
        }

        [Fact]
        public void TestSimpleCaseWithComment()
        {
            Test<ConvertCoalescingToConditionalExpressionCodeRefactoringProvider>(@"
class Test
{
	object Foo(object o, object p)
	{
		// Some comment
		return o $?? p;
	}
}
", @"
class Test
{
	object Foo(object o, object p)
	{
		// Some comment
		return o != null ? o : p;
	}
}
");
        }

        [Fact]
        public void TestNullable()
        {
            Test<ConvertCoalescingToConditionalExpressionCodeRefactoringProvider>(@"class Test
{
    void TestCase()
    {
		int? x = null;
		int y = x $?? 0;
    }
}", @"class Test
{
    void TestCase()
    {
		int? x = null;
		int y = x != null ? x.Value : 0;
    }
}");
        }
    }
}

