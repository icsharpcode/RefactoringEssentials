using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeFixes;

namespace RefactoringEssentials.Tests.CSharp.CodeFixes
{
    [TestFixture]
    public class CS1105ExtensionMethodMustBeDeclaredStaticTests : CSharpCodeFixTestBase
    {
        [Test]
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

