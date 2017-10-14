using RefactoringEssentials.CSharp.CodeFixes;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeFixes
{
    public class ReturnMustNotBeFollowedByAnyExpressionCodeFixProviderTests : CSharpCodeFixTestBase
    {
        [Fact]
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


        [Fact]
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

        [Fact]
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

