using System;
using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class InvertIfTestsTests : CSharpCodeRefactoringTestBase
    {
        [Test]
        public void TestSimple()
        {
            string result = RunContextAction(
                                         new InvertIfCodeRefactoringProvider(),
                                         "class TestClass" + Environment.NewLine +
                                         "{" + Environment.NewLine +
                                         "    void Test ()" + Environment.NewLine +
                                         "    {" + Environment.NewLine +
                                         "        $if (true) {" + Environment.NewLine +
                                         "            Case1 ();" + Environment.NewLine +
                                         "        } else {" + Environment.NewLine +
                                         "            Case2 ();" + Environment.NewLine +
                                         "        }" + Environment.NewLine +
                                         "    }" + Environment.NewLine +
                                         "}"
                                     );

            Assert.AreEqual(
                "class TestClass" + Environment.NewLine +
                "{" + Environment.NewLine +
                "    void Test ()" + Environment.NewLine +
                "    {" + Environment.NewLine +
                "        if (false)" + Environment.NewLine +
                "        {" + Environment.NewLine +
                "            Case2();" + Environment.NewLine +
                "        }" + Environment.NewLine +
                "        else" + Environment.NewLine +
                "        {" + Environment.NewLine +
                "            Case1();" + Environment.NewLine +
                "        }" + Environment.NewLine +
                "    }" + Environment.NewLine +
                "}", result);
        }

        [Test]
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

        [Test]
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


        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

    }
}
