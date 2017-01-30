using System;
using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class IntroduceFormatItemTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestFirst()
        {
            string result = RunContextAction(
                new AddNewFormatItemCodeRefactoringProvider(),
                "class TestClass" + Environment.NewLine +
                "{" + Environment.NewLine +
                "	void Test ()" + Environment.NewLine +
                "	{" + Environment.NewLine +
                "		string str = \"Hello <-World->!\";" + Environment.NewLine +
                "	}" + Environment.NewLine +
                "}"
            );

            Assert.Equal(
                "class TestClass" + Environment.NewLine +
                "{" + Environment.NewLine +
                "	void Test ()" + Environment.NewLine +
                "	{" + Environment.NewLine +
                "		string str = string.Format(\"Hello {0}!\", \"World\");" + Environment.NewLine +
                "	}" + Environment.NewLine +
                "}", result);
        }

        [Fact]
        public void TestSecond()
        {
            string result = RunContextAction(
                new AddNewFormatItemCodeRefactoringProvider(),
                "class TestClass" + Environment.NewLine +
                "{" + Environment.NewLine +
                "	void Test ()" + Environment.NewLine +
                "	{" + Environment.NewLine +
                "		string str = string.Format(\"<-Hello-> {0}!\", \"World\");" + Environment.NewLine +
                "	}" + Environment.NewLine +
                "}"
            );

            Assert.Equal(
                "class TestClass" + Environment.NewLine +
                "{" + Environment.NewLine +
                "	void Test ()" + Environment.NewLine +
                "	{" + Environment.NewLine +
                "		string str = string.Format(\"{1} {0}!\", \"World\", \"Hello\");" + Environment.NewLine +
                "	}" + Environment.NewLine +
                "}", result);
        }
    }
}
