using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeFixes;

namespace RefactoringEssentials.Tests.CSharp.CodeFixes
{
    [TestFixture]
    public class CS1520MethodMustHaveAReturnTypeTests : CSharpCodeFixTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
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

