using System;
using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ConvertHexToDecTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
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

            Assert.Equal(
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
