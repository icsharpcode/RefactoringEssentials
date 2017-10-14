using System;
using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class SplitStringTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestSimpleString()
        {
            string result = RunContextAction(
                new SplitStringCodeRefactoringProvider(),
                "class TestClass" + Environment.NewLine +
                "{" + Environment.NewLine +
                "	void Test ()" + Environment.NewLine +
                "	{" + Environment.NewLine +
                "		System.Console.WriteLine (\"Hello$World\");" + Environment.NewLine +
                "	}" + Environment.NewLine +
                "}"
            );

            Assert.Equal(
                "class TestClass" + Environment.NewLine +
                "{" + Environment.NewLine +
                "	void Test ()" + Environment.NewLine +
                "	{" + Environment.NewLine +
                "		System.Console.WriteLine (\"Hello\" + \"World\");" + Environment.NewLine +
                "	}" + Environment.NewLine +
                "}", result);
        }

        [Fact]
        public void TestVerbatimString()
        {
            string result = RunContextAction(
                new SplitStringCodeRefactoringProvider(),
                "class TestClass" + Environment.NewLine +
                "{" + Environment.NewLine +
                "	void Test ()" + Environment.NewLine +
                "	{" + Environment.NewLine +
                "		System.Console.WriteLine (@\"Hello$World\");" + Environment.NewLine +
                "	}" + Environment.NewLine +
                "}"
            );

            Assert.Equal(
                "class TestClass" + Environment.NewLine +
                "{" + Environment.NewLine +
                "	void Test ()" + Environment.NewLine +
                "	{" + Environment.NewLine +
                "		System.Console.WriteLine (@\"Hello\" + @\"World\");" + Environment.NewLine +
                "	}" + Environment.NewLine +
                "}", result);
        }
    }
}

