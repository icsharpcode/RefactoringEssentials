using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class GenerateSwitchLabelsTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
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

        [Fact]
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
