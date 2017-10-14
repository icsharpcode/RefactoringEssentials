using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ReplaceOperatorAssignmentWithAssignmentTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
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

        [Fact]
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