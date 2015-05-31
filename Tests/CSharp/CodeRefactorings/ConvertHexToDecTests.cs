using System;
using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class ConvertHexToDecTests : CSharpCodeRefactoringTestBase
    {
        [Test()]
        public void Test()
        {
            string result = RunContextAction(
                new ConvertHexToDecCodeRefactoringProvider(),
                "using System;" + Environment.NewLine +
                "class TestClass" + Environment.NewLine +
                "{" + Environment.NewLine +
                "	void Test ()" + Environment.NewLine +
                "	{" + Environment.NewLine +
                "		int i = $0x10;" + Environment.NewLine +
                "	}" + Environment.NewLine +
                "}"
            );

            Assert.AreEqual(
                "using System;" + Environment.NewLine +
                "class TestClass" + Environment.NewLine +
                "{" + Environment.NewLine +
                "	void Test ()" + Environment.NewLine +
                "	{" + Environment.NewLine +
                "		int i = 16;" + Environment.NewLine +
                "	}" + Environment.NewLine +
                "}", result);
        }
    }
}
