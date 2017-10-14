using System;
using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class SplitLocalVariableDeclarationAndAssignmentTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestSimpleExpression()
        {
            string result = RunContextAction(
                                         new SplitLocalVariableDeclarationAndAssignmentCodeRefactoringProvider(),
                                         "class TestClass" + Environment.NewLine +
                                         "{" + Environment.NewLine +
                                         "    void Test ()" + Environment.NewLine +
                                         "    {" + Environment.NewLine +
                                         "        int $myInt = 5 + 3 * (2 - 10);" + Environment.NewLine +
                                         "    }" + Environment.NewLine +
                                         "}"
                                     );

            Assert.Equal(
                "class TestClass" + Environment.NewLine +
                "{" + Environment.NewLine +
                "    void Test ()" + Environment.NewLine +
                "    {" + Environment.NewLine +
                "        int myInt;" + Environment.NewLine +
                "        myInt = 5 + 3 * (2 - 10);" + Environment.NewLine +
                "    }" + Environment.NewLine +
                "}", result);
        }

        [Fact]
        public void TestSimpleExpressionWithComment()
        {
            string result = RunContextAction(
                                         new SplitLocalVariableDeclarationAndAssignmentCodeRefactoringProvider(),
                                         "class TestClass" + Environment.NewLine +
                                         "{" + Environment.NewLine +
                                         "    void Test ()" + Environment.NewLine +
                                         "    {" + Environment.NewLine +
                                         "        // Some comment" + Environment.NewLine +
                                         "        int $myInt = 5 + 3 * (2 - 10);" + Environment.NewLine +
                                         "    }" + Environment.NewLine +
                                         "}"
                                     );

            Assert.Equal(
                "class TestClass" + Environment.NewLine +
                "{" + Environment.NewLine +
                "    void Test ()" + Environment.NewLine +
                "    {" + Environment.NewLine +
                "        // Some comment" + Environment.NewLine +
                "        int myInt;" + Environment.NewLine +
                "        myInt = 5 + 3 * (2 - 10);" + Environment.NewLine +
                "    }" + Environment.NewLine +
                "}", result);
        }

        [Fact]
        public void TestVarType()
        {
            string result = RunContextAction(
                                         new SplitLocalVariableDeclarationAndAssignmentCodeRefactoringProvider(),
                                         "class TestClass" + Environment.NewLine +
                                         "{" + Environment.NewLine +
                                         "    void Test ()" + Environment.NewLine +
                                         "    {" + Environment.NewLine +
                                         "        var $aVar = this;" + Environment.NewLine +
                                         "    }" + Environment.NewLine +
                                         "}"
                                     );
            Assert.Equal(
                "class TestClass" + Environment.NewLine +
                "{" + Environment.NewLine +
                "    void Test ()" + Environment.NewLine +
                "    {" + Environment.NewLine +
                "        TestClass aVar;" + Environment.NewLine +
                "        aVar = this;" + Environment.NewLine +
                "    }" + Environment.NewLine +
                "}", result);
        }

        [Fact]
        public void TestForStatement()
        {
            string result = RunContextAction(
                                         new SplitLocalVariableDeclarationAndAssignmentCodeRefactoringProvider(),
                                         "class TestClass" + Environment.NewLine +
                                         "{" + Environment.NewLine +
                                         "    void Test ()" + Environment.NewLine +
                                         "    {" + Environment.NewLine +
                                         "        for (int $i = 1; i < 10; i++) {}" + Environment.NewLine +
                                         "    }" + Environment.NewLine +
                                         "}"
                                     );
            string expected = @"class TestClass
{
    void Test ()
    {
        int i;
        for (i = 1; i < 10; i++) {}
    }
}";
            Assert.Equal(HomogenizeEol(expected), HomogenizeEol(result));
        }

        [Fact]
        public void TestPopupAtAssign()
        {
            Test<SplitLocalVariableDeclarationAndAssignmentCodeRefactoringProvider>(@"class Test
{
    public static void Main (string[] args)
    {
        var foo $= 5;
    }
}", @"class Test
{
    public static void Main (string[] args)
    {
        int foo;
        foo = 5;
    }
}");
        }

        [Fact]
        public void TestPopupAtBeginningOfExpression()
        {
            Test<SplitLocalVariableDeclarationAndAssignmentCodeRefactoringProvider>(@"class Test
{
    public static void Main (string[] args)
    {
        var foo = $5;
    }
}", @"class Test
{
    public static void Main (string[] args)
    {
        int foo;
        foo = 5;
    }
}");
        }

        [Fact]
        public void TestMultipleInitializers()
        {
            Test<SplitLocalVariableDeclarationAndAssignmentCodeRefactoringProvider>(@"class Test
{
    public static void Main (string[] args)
    {
        int a, b, $foo = 5 + 12, c;
        Console.WriteLine(foo);
    }
}", @"class Test
{
    public static void Main (string[] args)
    {
        int a, b, foo, c;
        foo = 5 + 12;
        Console.WriteLine(foo);
    }
}");
        }

        [Fact]
        public void TestVarDeclarationWithComment()
        {
            Test<SplitLocalVariableDeclarationAndAssignmentCodeRefactoringProvider>(@"class Test
{
    public void T()
    {
        // Some comment
        int $i = 5;
    }
}", @"class Test
{
    public void T()
    {
        // Some comment
        int i;
        i = 5;
    }
}");
        }

        [Fact]
        public void TestForStatementWithComment()
        {
            Test<SplitLocalVariableDeclarationAndAssignmentCodeRefactoringProvider>(@"class Test
{
    public void T()
    {
        // Some comment
        for (int $i = 1; i < 10; i++) {}
    }
}", @"class Test
{
    public void T()
    {
        // Some comment
        int i;
        for (i = 1; i < 10; i++) {}
    }
}");
        }

        [Fact]
        public void TestHideInExpression()
        {
            TestWrongContext<SplitLocalVariableDeclarationAndAssignmentCodeRefactoringProvider>(@"class Test
{
    public static void Main (string[] args)
    {
        var foo = 5 $+ 5;
    }
}");
        }

        [Fact]
        public void TestLocalConstants()
        {
            TestWrongContext<SplitLocalVariableDeclarationAndAssignmentCodeRefactoringProvider>(@"class Test
{
    public static void Main (string[] args)
    {
        const int foo $= 5;
    }
}");
        }
    }
}

