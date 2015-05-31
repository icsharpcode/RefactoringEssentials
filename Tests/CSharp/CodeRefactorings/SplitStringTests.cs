using System;
using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class SplitStringTests : CSharpCodeRefactoringTestBase
    {
        [Test()]
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

            Assert.AreEqual(
                "class TestClass" + Environment.NewLine +
                "{" + Environment.NewLine +
                "	void Test ()" + Environment.NewLine +
                "	{" + Environment.NewLine +
                "		System.Console.WriteLine (\"Hello\" + \"World\");" + Environment.NewLine +
                "	}" + Environment.NewLine +
                "}", result);
        }

        [Test()]
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

            Assert.AreEqual(
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

