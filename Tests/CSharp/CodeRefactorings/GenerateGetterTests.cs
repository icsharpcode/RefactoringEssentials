using System;
using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [Ignore("Needs insertion cursor mode.")]
    [TestFixture]
    public class GenerateGetterTests : CSharpCodeRefactoringTestBase
    {
        [Test()]
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
            Assert.AreEqual(
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