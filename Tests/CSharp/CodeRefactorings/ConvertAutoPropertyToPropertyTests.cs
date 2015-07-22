using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class ConvertAutoPropertyToPropertyTests : CSharpCodeRefactoringTestBase
    {
        [Test]
        public void TestSimpleProperty()
        {
            Test<ConvertAutoPropertyToPropertyCodeRefactoringProvider>(@"class TestClass
{
	string $Test { get; set; }
}", @"class TestClass
{
    string Test
    {
        get
        {
            throw new System.NotImplementedException();
        }

        set
        {
            throw new System.NotImplementedException();
        }
    }
}");
        }

        [Ignore()]
        [Test]
        public void TestSimplify()
        {
            Test<ConvertAutoPropertyToPropertyCodeRefactoringProvider>(@"
using Sytem;
class TestClass
{
	string $Test { get; set; }
}", @"
using Sytem;
class TestClass
{
    string Test
    {
        get
        {
            throw new NotImplementedException();
        }

        set
        {
            throw new NotImplementedException();
        }
    }
}");
        }
    }
}

