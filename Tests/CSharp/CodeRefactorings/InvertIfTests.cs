using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class InvertIfTestsTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestSimple()
        {
            Test<InvertIfCodeRefactoringProvider>(@"
class TestClass
{
    void Test ()
    {
        $if (true)
        {
            Case1 ();
        }
        else
        {
            Case2 ();
        }
    }
}", @"
class TestClass
{
    void Test ()
    {
        if (false)
        {
            Case2();
        }
        else
        {
            Case1();
        }
    }
}");
        }

        [Fact]
        public void TestReturn()
        {
            Test<InvertIfCodeRefactoringProvider>(
                @"class TestClass
{
    void Test ()
    {
        $if (true) {
            Case1();
        }
    }
}",
                @"class TestClass
{
    void Test ()
    {
        if (false)
            return;
        Case1();
    }
}"
            );
        }

        [Fact]
        public void TestInLoop()
        {
            Test<InvertIfCodeRefactoringProvider>(
                @"class TestClass
{
    void Test ()
    {
        while(true)
        {
           $if (true) {
                Case1();
            }
        }
    }
}",
                @"class TestClass
{
    void Test ()
    {
        while(true)
        {
            if (false)
                continue;
            Case1();
        }
    }
}");
        }


        [Fact]
        public void Test2()
        {
            Test<InvertIfCodeRefactoringProvider>(
                @"class TestClass
{
    void Test ()
    {
        $if (true) {
            Case1();
            Case2();
        }
        else 
        {
            return;
        }
    }
}",
                @"class TestClass
{
    void Test ()
    {
        if (false)
            return;
        Case1();
        Case2();
    }
}"
            );
        }

        [Fact]
        public void TestNonVoidMoreComplexMethod()
        {
            Test<InvertIfCodeRefactoringProvider>(
                @"class TestClass
{
    int Test ()
    {
        $if (true) {
            Case1();
        }
        else 
        {
            return 0;
            testDummyCode();
        }
    }
}",
                @"class TestClass
{
    int Test ()
    {
        if (false)
        {
            return 0;
            testDummyCode();
        }
        Case1();
    }
}"
            );

        }

        [Fact]
        public void TestComplexMethod()
        {
            Test<InvertIfCodeRefactoringProvider>(
                @"class TestClass
{
    int Test ()
    {
        $if (true) {
            Case1();
        }
        else 
            continue;
        return 0;
    }
}",
                @"class TestClass
{
    int Test ()
    {
        if (false)
            continue;
        Case1();
        return 0;
    }
}"
            );
        }

        [Fact]
        public void TestComment()
        {
            Test<InvertIfCodeRefactoringProvider>(
                @"class TestClass
{
    int Test ()
    {
        $if (true) {
            Case1();
        }
        else 
        {
            //TestComment
            return 0;
        }
    }
}",
                @"class TestClass
{
    int Test ()
    {
        if (false)
        {
            //TestComment
            return 0;
        }
        Case1();
    }
}"
            );
        }

        /// <summary>
        /// InvertIfCodeRefactoringProvider: InvalidOperationException on nested "if ... if ... else" #62
        /// </summary>
        [Fact]
        public void TestIssue62()
        {
            Test<InvertIfCodeRefactoringProvider>(
                @"class TestClass
{
    int TestMethod(int a)
    {
        if (a > 0)
            $if (a < 5)
            {
                return 1;
            }
            else
            {
                return 2;
            }

        return 0;
    }
}",
            @"class TestClass
{
    int TestMethod(int a)
    {
        if (a > 0)
            if (a >= 5)
            {
                return 2;
            }
            else
            {
                return 1;
            }

        return 0;
    }
}"
            );
        }

    }
}
