using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class AddBracesTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestAddBracesToIf()
        {
            Test<AddBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        $if (true)
            Console.WriteLine(""Hello"");
    }
}", @"class TestClass
{
    void Test()
    {
        if (true)
        {
            Console.WriteLine(""Hello"");
        }
    }
}");
        }

        [Fact]
        public void TestAddBracesToIfWithComment()
        {
            Test<AddBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        // Some comment
        $if (true)
            Console.WriteLine(""Hello"");
    }
}", @"class TestClass
{
    void Test()
    {
        // Some comment
        if (true)
        {
            Console.WriteLine(""Hello"");
        }
    }
}");
        }

        [Fact(Skip="Broken.")]
        public void TestAddBracesToIfWithCommentInBlock()
        {
            Test<AddBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        $if (true)
            // Some comment
            Console.WriteLine(""Hello"");
    }
}", @"class TestClass
{
    void Test()
    {
        if (true)
        {
            // Some comment
            Console.WriteLine(""Hello"");
        }
    }
}");
        }

        [Fact]
        public void TestAddBracesToElse()
        {
            Test<AddBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        if (true)
        {
            Console.WriteLine(""Hello"");
        } $else
            Console.WriteLine(""World"");
    }
}", @"class TestClass
{
    void Test()
    {
        if (true)
        {
            Console.WriteLine(""Hello"");
        } else
        {
            Console.WriteLine(""World"");
        }
    }
}");
        }

        [Fact]
        public void TestAddBracesToDoWhile()
        {
            Test<AddBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        $do
            Console.WriteLine(""Hello""); 
        while (true);
    }
}", @"class TestClass
{
    void Test()
    {
        do
        {
            Console.WriteLine(""Hello"");
        }
        while (true);
    }
}");
        }

        [Fact]
        public void TestAddBracesToForeach()
        {
            Test<AddBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        $foreach (var a in b)
            Console.WriteLine(""Hello"");
    }
}", @"class TestClass
{
    void Test()
    {
        foreach (var a in b)
        {
            Console.WriteLine(""Hello"");
        }
    }
}");
        }

        [Fact]
        public void TestAddBracesToFor()
        {
            Test<AddBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        $for (;;)
            Console.WriteLine(""Hello"");
    }
}", @"class TestClass
{
    void Test()
    {
        for (;;)
        {
            Console.WriteLine(""Hello"");
        }
    }
}");
        }

        [Fact]
        public void TestAddBracesToLock()
        {
            Test<AddBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        $lock (this)
            Console.WriteLine(""Hello"");
    }
}", @"class TestClass
{
    void Test()
    {
        lock (this)
        {
            Console.WriteLine(""Hello"");
        }
    }
}");
        }

        [Fact]
        public void TestAddBracesToUsing()
        {
            Test<AddBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        $using (var a = new A ())
            Console.WriteLine(""Hello"");
    }
}", @"class TestClass
{
    void Test()
    {
        using (var a = new A ())
        {
            Console.WriteLine(""Hello"");
        }
    }
}");
        }

        [Fact]
        public void TestAddBracesToWhile()
        {
            Test<AddBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        $while (true)
            Console.WriteLine(""Hello"");
    }
}", @"class TestClass
{
    void Test()
    {
        while (true)
        {
            Console.WriteLine(""Hello"");
        }
    }
}");
        }

        [Fact]
        public void TestBlockAlreadyInserted()
        {
            TestWrongContext<AddBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        $if (true)
        {
            Console.WriteLine(""Hello"");
        }
    }
}");
        }

        [Fact]
        public void TestNullNode()
        {
            TestWrongContext<AddBracesCodeRefactoringProvider>(@"class TestClass
{
    void Test()
    {
        if (true)
        {
            Console.WriteLine(""Hello"");
        }
    }
}

        $          
");
        }
    }
}

