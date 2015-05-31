using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class ReplaceWithOperatorAssignmentTests : CSharpCodeRefactoringTestBase
    {
        [Test]
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

