using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ReplaceWithOperatorAssignmentTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestAdd()
        {
            Test<ReplaceWithOperatorAssignmentCodeRefactoringProvider>(@"
class Test
{
	void Foo (int i)
	{
		i $= i + 1 + 2;
	}
}", @"
class Test
{
	void Foo (int i)
	{
        i += 1 + 2;
	}
}");
        }


    }
}

