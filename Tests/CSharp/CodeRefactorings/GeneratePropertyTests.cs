using System;
using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [Ignore("Needs insertion cursor mode.")]
    [TestFixture]
    public class GeneratePropertyTests : CSharpCodeRefactoringTestBase
    {
        [Test()]
        public void Test()
        {
            string result = RunContextAction(
                new GeneratePropertyAction(),
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
                "		set {" + Environment.NewLine +
                "			myField = value;" + Environment.NewLine +
                "		}" + Environment.NewLine +
                "	}" + Environment.NewLine +
                "	int myField;" + Environment.NewLine +
                "}", result);
        }

        [Test()]
        public void HandlesFieldsWhichMatchThePropertyNameTest()
        {
            Test<GeneratePropertyAction>(
@"
class TestClass
{
	public int $MyField;
}",
@"
class TestClass
{
	public int MyField {
		get {
			return myField;
		}
		set {
			myField = value;
		}
	}
	public int myField;
}");
        }

        [Test()]
        public void HandlesMultiDeclarationTest()
        {
            Test<GeneratePropertyAction>(
@"
class TestClass
{
	int $MyField, myOtherField;
}",
@"
class TestClass
{
	public int MyField {
		get {
			return myField;
		}
		set {
			myField = value;
		}
	}
	int myField, myOtherField;
}");
        }

        [Test]
        public void CannotGeneratePropertyForReadOnlyField()
        {
            TestWrongContext(
                new GeneratePropertyAction(),
                "using System;" + Environment.NewLine +
                    "class TestClass" + Environment.NewLine +
                    "{" + Environment.NewLine +
                    "	readonly int $myField;" + Environment.NewLine +
                    "}"
            );
        }

        [Test]
        public void CannotGeneratePropertyForConst()
        {
            TestWrongContext(
                new GeneratePropertyAction(),
                "using System;" + Environment.NewLine +
                    "class TestClass" + Environment.NewLine +
                    "{" + Environment.NewLine +
                    "	const int $myField = 0;" + Environment.NewLine +
                    "}"
            );
        }
    }
}