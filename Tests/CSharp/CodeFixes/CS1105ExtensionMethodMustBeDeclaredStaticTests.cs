using RefactoringEssentials.CSharp.CodeFixes;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeFixes
{
    public class CS1105ExtensionMethodMustBeDeclaredStaticTests : CSharpCodeFixTestBase
    {
        [Fact]
        public void TestMethod()
        {
            Test<CS1105ExtensionMethodMustBeDeclaredStaticCodeFixProvider>(@"
static class Foo
{
	public void $FooBar$(this string foo)
	{

	}
}", @"
static class Foo
{
	public static void FooBar(this string foo)
	{

	}
}");
        }
    }
}

