using RefactoringEssentials.CSharp.CodeFixes;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeFixes
{
    public class CS1520MethodMustHaveAReturnTypeTests : CSharpCodeFixTestBase
    {
        [Fact]
        public void TestMethod()
        {
            Test<CS1520MethodMustHaveAReturnTypeCodeFixProvider>(
                @"class Foo
{
    static $Fa$(string str)
    {
    }
}",
                @"class Foo
{
    static void Fa(string str)
    {
    }
}", 1, true);
        }

        [Fact]
        public void TestConstructor()
        {
            Test<CS1520MethodMustHaveAReturnTypeCodeFixProvider>(
                @"class Foo
{
    static $Fa$(string str)
    {
    }
}",
                @"class Foo
{
    static Foo(string str)
    {
    }
}", 0, true);
        }

        [Fact]
        public void TestConstructorWithBase()
        {
            Test<CS1520MethodMustHaveAReturnTypeCodeFixProvider>(
                @"class Foo
{
    static Fa(string str) : base (str)
    {
    }
}",
                @"class Foo
{
    static Foo(string str) : base (str)
    {
    }
}", 0, true);
        }
    }
}

