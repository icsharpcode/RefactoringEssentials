using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class ReplaceOperatorAssignmentWithAssignmentTests : CSharpCodeRefactoringTestBase
    {
        [Test]
        public void TestAdd()
        {
            Test<ReplaceOperatorAssignmentWithAssignmentCodeRefactoringProvider>(@"
class Test
{
    void Foo (int i)
    {
        i $+= 1 + 2;
    }
}", @"
class Test
{
    void Foo (int i)
    {
        i = i + 1 + 2;
    }
}");
        }

        [Test]
        public void TestAddWithComment()
        {
            Test<ReplaceOperatorAssignmentWithAssignmentCodeRefactoringProvider>(@"
class Test
{
    void Foo (int i)
    {
        // Some comment
        i $+= 1 + 2;
    }
}", @"
class Test
{
    void Foo (int i)
    {
        // Some comment
        i = i + 1 + 2;
    }
}");
        }
    }
}