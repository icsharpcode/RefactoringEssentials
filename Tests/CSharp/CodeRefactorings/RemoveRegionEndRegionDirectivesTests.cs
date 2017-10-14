using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class RemoveRegionEndRegionDirectivesTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestSimpleRegion()
        {
            Test<RemoveRegionEndRegionDirectivesCodeRefactoringProvider>(@"class TestClass{
#region$ Foo
    void Test ()
    {
    }
#endregion
}", @"class TestClass{
    void Test ()
    {
    }
}");
        }

        [Fact]
        public void TestNestedRegion()
        {
            Test<RemoveRegionEndRegionDirectivesCodeRefactoringProvider>(@"class TestClass
{
    #region$ Foo
    void Test ()
    {
        #region Nested
        Foo ();
        #endregion
    }
    #endregion
}", @"class TestClass
{
    void Test ()
    {
        #region Nested
        Foo ();
        #endregion
    }
}");
        }


        [Fact]
        public void TestEndRegion()
        {
            Test<RemoveRegionEndRegionDirectivesCodeRefactoringProvider>(@"class TestClass{
#region Foo
    void Test ()
    {
    }
$#endregion
}", @"class TestClass{
    void Test ()
    {
    }
}");
        }
    }
}

