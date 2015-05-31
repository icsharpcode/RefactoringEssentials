using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class GenerateSwitchLabelsTests : CSharpCodeRefactoringTestBase
    {
        [Test]
        public void Test()
        {
            Test<GenerateSwitchLabelsCodeRefactoringProvider>(@"
using System;

class TestClass
{
    void Test (ConsoleModifiers mods)
    {
        $switch (mods)
        {
        }
    }
}", @"
using System;

class TestClass
{
    void Test (ConsoleModifiers mods)
    {
        switch (mods)
        {
            case ConsoleModifiers.Alt:
                break;
            case ConsoleModifiers.Shift:
                break;
            case ConsoleModifiers.Control:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}");
        }

        [Test]
        public void TestAddMissing()
        {
            Test<GenerateSwitchLabelsCodeRefactoringProvider>(@"
using System;

class Foo
{
	void Bar (ConsoleModifiers mods)
	{
		$switch (mods) {
		case ConsoleModifiers.Alt:
			break;
		default:
			throw new ArgumentOutOfRangeException ();
		}
	}
}
", @"
using System;

class Foo
{
	void Bar (ConsoleModifiers mods)
	{
        switch (mods)
        {
            case ConsoleModifiers.Alt:
                break;
            case ConsoleModifiers.Shift:
                break;
            case ConsoleModifiers.Control:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
");

        }

    }
}
