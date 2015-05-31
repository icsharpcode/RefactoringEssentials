using System;
using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class IntroduceFormatItemTests : CSharpCodeRefactoringTestBase
    {
        [Test()]
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

            Assert.AreEqual(
                "class TestClass" + Environment.NewLine +
                "{" + Environment.NewLine +
                "	void Test ()" + Environment.NewLine +
                "	{" + Environment.NewLine +
                "		string str = string.Format(\"Hello {0}!\", \"World\");" + Environment.NewLine +
                "	}" + Environment.NewLine +
                "}", result);
        }

        [Test()]
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

            Assert.AreEqual(
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
