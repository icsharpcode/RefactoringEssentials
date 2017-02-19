using System;
using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class RemoveBracesTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestSimpleBraces()
        {
            string result = RunContextAction(
                                         new RemoveBracesCodeRefactoringProvider(),
                                         "class TestClass" + Environment.NewLine +
                                         "{" + Environment.NewLine +
                                         "    void Test()" + Environment.NewLine +
                                         "    {" + Environment.NewLine +
                                         "        if (true) ${" + Environment.NewLine +
                                         "            ;" + Environment.NewLine +
                                         "        }" + Environment.NewLine +
                                         "    }" + Environment.NewLine +
                                         "}"
                                     );

            Assert.Equal(
                "class TestClass" + Environment.NewLine +
                "{" + Environment.NewLine +
                "    void Test()" + Environment.NewLine +
                "    {" + Environment.NewLine +
                "        if (true) ;" + Environment.NewLine +
                "    }" + Environment.NewLine +
                "}", result);
        }

        [Fact]
        public void TestSimpleBracesWithComment()
        {
            string result = RunContextAction(
                                         new RemoveBracesCodeRefactoringProvider(),
                                         "class TestClass" + Environment.NewLine +
                                         "{" + Environment.NewLine +
                                         "    void Test()" + Environment.NewLine +
                                         "    {" + Environment.NewLine +
                                         "        // Some comment" + Environment.NewLine +
                                         "        if (true) ${" + Environment.NewLine +
                                         "            ;" + Environment.NewLine +
                                         "        }" + Environment.NewLine +
                                         "    }" + Environment.NewLine +
                                         "}"
                                     );

            Assert.Equal(
                "class TestClass" + Environment.NewLine +
                "{" + Environment.NewLine +
                "    void Test()" + Environment.NewLine +
                "    {" + Environment.NewLine +
                "        // Some comment" + Environment.NewLine +
                "        if (true) ;" + Environment.NewLine +
                "    }" + Environment.NewLine +
                "}", result);
        }

        [Fact]
        public void TestTryCatch()
        {
            TestWrongContext<RemoveBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        try ${ ; } catch (Exception) { ; }
    }
}");
        }

        [Fact]
        public void TestTryCatchCatch()
        {
            TestWrongContext<RemoveBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        try { ; } catch (Exception) ${ ; }
    }
}");
        }

        [Fact]
        public void TestTryCatchFinally()
        {
            TestWrongContext<RemoveBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        try { ; } catch (Exception) { ; } finally ${ ; }
    }
}");
        }


        [Fact]
        public void TestSwitchCatch()
        {
            TestWrongContext<RemoveBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        switch (foo    ) ${ default: break;}
    }
}");
        }

        [Fact]
        public void TestMethodDeclaration()
        {
            TestWrongContext<RemoveBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    ${
        ;
    }
}");
        }

        [Fact]
        public void TestRemoveBracesFromIf()
        {
            Test<RemoveBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        $if (true) {
            Console.WriteLine(""Hello"");
        }
    }
}", @"class TestClass
{
    void Test()
    {
        if (true) Console.WriteLine(""Hello"");
    }
}");
        }

        [Fact]
        public void TestRemoveBracesFromElse()
        {
            Test<RemoveBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        if (true) {
            Console.WriteLine(""Hello"");
        } $else {
            Console.WriteLine(""World"");
        }
    }
}", @"class TestClass
{
    void Test()
    {
        if (true) {
            Console.WriteLine(""Hello"");
        }
        else Console.WriteLine(""World"");
    }
}");
        }

        [Fact]
        public void TestRemoveBracesFromDoWhile()
        {
            Test<RemoveBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        $do {
            Console.WriteLine(""Hello"");
        } while (true);
    }
}", @"class TestClass
{
    void Test()
    {
        do Console.WriteLine(""Hello"");
        while (true);
    }
}");
        }

        [Fact]
        public void TestRemoveBracesFromForeach()
        {
            Test<RemoveBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        $foreach (var a in b) {
            Console.WriteLine(""Hello"");
        }
    }
}", @"class TestClass
{
    void Test()
    {
        foreach (var a in b) Console.WriteLine(""Hello"");
    }
}");
        }

        [Fact]
        public void TestRemoveBracesFromFor()
        {
            Test<RemoveBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        $for (;;) {
            Console.WriteLine(""Hello"");
        }
    }
}", @"class TestClass
{
    void Test()
    {
        for (;;) Console.WriteLine(""Hello"");
    }
}");
        }

        [Fact]
        public void TestRemoveBracesFromLock()
        {
            Test<RemoveBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        $lock (this) {
            Console.WriteLine(""Hello"");
        }
    }
}", @"class TestClass
{
    void Test()
    {
        lock (this) Console.WriteLine(""Hello"");
    }
}");
        }

        [Fact]
        public void TestRemoveBracesFromUsing()
        {
            Test<RemoveBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        $using (var a = new A()) {
            Console.WriteLine(""Hello"");
        }
    }
}", @"class TestClass
{
    void Test()
    {
        using (var a = new A()) Console.WriteLine(""Hello"");
    }
}");
        }

        [Fact]
        public void TestRemoveBracesFromWhile()
        {
            Test<RemoveBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        $while (true) {
            Console.WriteLine(""Hello"");
        }
    }
}", @"class TestClass
{
    void Test()
    {
        while (true) Console.WriteLine(""Hello"");
    }
}");
        }

        [Fact]
        public void TestDoNotRemoveBracesFromBlockWithVariable()
        {
            TestWrongContext<RemoveBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        $if (true) {
            double PI = Math.PI;
        }
    }
}");
        }

        [Fact]
        public void TestDoNotRemoveBracesFromBlockWithLabel()
        {
            TestWrongContext<RemoveBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        $if (true) {
            here: Console.WriteLine(""Hello"");
        }
    }
}");
        }

        [Fact]
        public void TestNullNode()
        {
            TestWrongContext<RemoveBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        if (true) {
            Console.WriteLine(""Hello"");
        }
    }
}

        $          
");
        }
    }
}

