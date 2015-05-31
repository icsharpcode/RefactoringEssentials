using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeFixes;

namespace RefactoringEssentials.Tests.CSharp.CodeFixes
{
    [TestFixture]
    public class ReturnMustNotBeFollowedByAnyExpressionCodeFixProviderTests : CSharpCodeFixTestBase
    {
        [Test]
        public void TestSimpleCase()
        {
            Test<ReturnMustNotBeFollowedByAnyExpressionCodeFixProvider>(@"class Foo
{
    void Bar (string str)
    {
        $return$ str;
    }
}", @"class Foo
{
    void Bar (string str)
    {
        return;
    }
}");
        }


        [Test]
        public void TestReturnTypeFix()
        {
            Test<ReturnMustNotBeFollowedByAnyExpressionCodeFixProvider>(@"class Foo
{
    void Bar (string str)
    {
        return str;
    }
}", @"class Foo
{
    string Bar (string str)
    {
        return str;
    }
}", 1);
        }

        [Test]
        public void TestAnonymousMethod()
        {
            Test<ReturnMustNotBeFollowedByAnyExpressionCodeFixProvider>(@"class Foo
{
    void Bar (string str)
    {
        System.Action func = delegate {
            $return$ str;
        };
    }
}", @"class Foo
{
    void Bar (string str)
    {
        System.Action func = delegate {
            return;
        };
    }
}");
        }
    }
}

