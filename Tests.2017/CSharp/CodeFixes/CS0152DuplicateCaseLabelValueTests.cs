using RefactoringEssentials.CSharp.CodeFixes;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeFixes
{
    public class CS0152DuplicateCaseLabelValueTests : CSharpCodeFixTestBase
    {
        [Fact]
        public void TestDuplicateSections()
        {
            Test<CS0152DuplicateCaseLabelValueCodeFixProvider>(@"
class Test
{
	void TestMethod (int i = 0)
	{
		switch (i) {
		case 0:
			System.Console.WriteLine();
			break;
		case 0:
			System.Console.WriteLine();
			break;
		default:
		case 0:
			break;
		}
	}
}", @"
class Test
{
	void TestMethod (int i = 0)
	{
		switch (i) {
		case 0:
			System.Console.WriteLine();
			break;
		default:
		case 0:
			break;
		}
	}
}");
        }

        [Fact]
        public void TestNoDuplicate()
        {
            TestWrongContext<CS0152DuplicateCaseLabelValueCodeFixProvider>(@"
class Test
{
	void TestMethod (int i = 0)
	{
		switch (i) {
		case 0:
			System.Console.WriteLine();
			break;
		case 0:
			System.Console.WriteLine(213);
			break;
		default:
		case 0:
			break;
		}
	}
}");
        }


        [Fact]
        public void TestDuplicateLabels()
        {
            Test<CS0152DuplicateCaseLabelValueCodeFixProvider>(@"
class Test
{
	void TestMethod (int i = 0)
	{
		switch (i) {
		case 0:
		case 0:
			System.Console.WriteLine();
			break;
		default:
		case 0:
			break;
		}
	}
}", @"
class Test
{
	void TestMethod (int i = 0)
	{
		switch (i) {
		case 0:
			System.Console.WriteLine();
			break;
		default:
		case 0:
			break;
		}
	}
}");
        }

    }
}

