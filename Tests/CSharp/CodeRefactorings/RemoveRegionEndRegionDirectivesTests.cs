using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class RemoveRegionEndRegionDirectivesTests : ContextActionTestBase
    {
        [Test]
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

        [Test]
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


        [Test]
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

