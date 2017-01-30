using System;
using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
	public class GenerateGetterTests : CSharpCodeRefactoringTestBase
    {
        [Fact(Skip="Needs insertion cursor mode.")]
        public void Test()
        {
            string result = RunContextAction(
                new GenerateGetterAction(),
                "using System;" + Environment.NewLine +
                    "class TestClass" + Environment.NewLine +
                    "{" + Environment.NewLine +
                    "	int $myField;" + Environment.NewLine +
                    "}"
            );
            Assert.Equal(
                "using System;" + Environment.NewLine +
                "class TestClass" + Environment.NewLine +
                "{" + Environment.NewLine +
                "	public int MyField {" + Environment.NewLine +
                "		get {" + Environment.NewLine +
                "			return myField;" + Environment.NewLine +
                "		}" + Environment.NewLine +
                "	}" + Environment.NewLine +
                "	int myField;" + Environment.NewLine +
                "}", result);
        }
    }
}