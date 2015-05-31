using System;
using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class UseSystemStringEmptyTests : CSharpCodeRefactoringTestBase
    {
        [Test()]
        public void TestSimpleString()
        {
            string result = RunContextAction(
                new UseSystemStringEmptyCodeRefactoringProvider(),
                "class TestClass" + Environment.NewLine +
                "{" + Environment.NewLine +
                "	void Test ()" + Environment.NewLine +
                "	{" + Environment.NewLine +
                "		string str = …\"\";" + Environment.NewLine +
                "	}" + Environment.NewLine +
                "}"
            );

            Assert.AreEqual(
                "class TestClass" + Environment.NewLine +
                "{" + Environment.NewLine +
                "	void Test ()" + Environment.NewLine +
                "	{" + Environment.NewLine +
                "		string str = string.Empty;" + Environment.NewLine +
                "	}" + Environment.NewLine +
                "}", result);
        }
    }
}

