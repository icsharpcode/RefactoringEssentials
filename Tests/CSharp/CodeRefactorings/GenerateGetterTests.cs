using System;
using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class GenerateGetterTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void Test()
        {
            Test<GenerateGetterAction>(@"using System;
class TestClass
{
    int $myField;
}", @"using System;
class TestClass
{
    int myField;

    public int MyField
    {
        get
        {
            return myField;
        }
    }
}");

            Test<GenerateGetterAction>(@"using System;
class TestClass
{
    static int $myField;
}", @"using System;
class TestClass
{
    static int myField;

    public static int MyField
    {
        get
        {
            return myField;
        }
    }
}");
        }

        [Fact]
        public void TestSimilarPropertyExists()
        {
            Test<GenerateGetterAction>(@"using System;
class TestClass
{
    int $myField;

    public int Test => 5;
}", @"using System;
class TestClass
{
    int myField;

    public int MyField
    {
        get
        {
            return myField;
        }
    }

    public int Test => 5;
}");
        }

        [Fact]
        public void TestAlreadyImplemented()
        {
            TestWrongContext<GenerateGetterAction>(@"using System;
class TestClass
{
    int $myField;

    public int MyField { get { return myField; } }
}");

            TestWrongContext<GenerateGetterAction>(@"using System;
class TestClass
{
    int $myField;

    public int MyField { get { return this.myField; } }
}");

            TestWrongContext<GenerateGetterAction>(@"using System;
class TestClass
{
    int $myField;

    public int MyField => myField;
}");
            TestWrongContext<GenerateGetterAction>(@"using System;
class TestClass
{
    int $myField;

    public int MyField => this.myField;
}");

            TestWrongContext<GenerateGetterAction>(@"using System;
class TestClass
{
    int $myField;

    public int MyField { get { return myField; } set { myField = value; } };
}");
        }
    }
}