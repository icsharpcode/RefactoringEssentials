using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeFixes;

namespace RefactoringEssentials.Tests.CSharp.CodeFixes
{
    [TestFixture]
    public class CS0152DuplicateCaseLabelValueTests : CSharpCodeFixTestBase
    {
        [Test]
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

        [Test]
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


        [Test]
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

