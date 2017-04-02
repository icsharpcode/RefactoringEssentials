using System;
using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class UseSystemStringEmptyTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
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

            Assert.Equal(
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

