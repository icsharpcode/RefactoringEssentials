using RefactoringEssentials.Samples.CSharp;
using RefactoringEssentials.Tests.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.Samples.CSharp
{
	/// <summary>
	/// Tests for SampleCodeRefactoringProvider.
	/// </summary>
    public class SampleCodeRefactoringTests : CSharpCodeRefactoringTestBase
    {
        /*
            This class demonstrates usual implementation of unit tests for
            code refactorings by implementing tests for SampleCodeRefactoringProvider class.

            Since SampleCodeRefactoringProvider suggests to add an "I" prefix to interface
            names not already having it, we test how this refactoring reacts to different
            interface namings.
        */

        [Fact(Skip="Sample refactoring tests: Remove this line to activate them.")]
        public void TestInterfaceWithoutIPrefix()
        {
            /*
                By calling the Test() method we can describe the input C# code which
                the tested refactoring has to operate on and (in 2nd parameter) the output
                code expected after refactoring has been applied. Input and output are
                compared as normal strings, therefore please make sure to use same
                code formatting (tabs, whitespaces etc.) in both.

                Please note the "$" character before "Test" in input code:
                It marks the token where the refactoring is expected to become active.
                Means here: Our refactoring must be presented as a "bulb" in editor when
                cursor is at "Test".
            */
            Test<SampleCodeRefactoringProvider>(@"
interface $Test
{
    void TestMethod();
}", @"
interface ITest
{
    void TestMethod();
}");
        }

        [Fact(Skip="Sample refactoring tests: Remove this line to activate them.")]
        public void TestInterfaceWithIPrefix()
        {
            /*
                Using TestWrongContext() means the opposite to Test():
                We use it here to assure that our refactoring will NOT be suggested
                when cursor is at the syntax token marked with "$".

                In this example: Our refactoring must not be suggested
                for interfaces already having an "I" prefix!
            */
            TestWrongContext<SampleCodeRefactoringProvider>(@"
interface $ITest
{
    void TestMethod();
}");
        }

        [Fact(Skip="Sample refactoring tests: Remove this line to activate them.")]
        public void TestInterfaceWithINonPrefix()
        {
            /*
                One more test for the case that interface name starts with
                "I", but that doesn't seem to be used as prefix.
            */
            Test<SampleCodeRefactoringProvider>(@"
interface $Item
{
    void TestMethod();
}", @"
interface IItem
{
    void TestMethod();
}");
        }
    }
}

