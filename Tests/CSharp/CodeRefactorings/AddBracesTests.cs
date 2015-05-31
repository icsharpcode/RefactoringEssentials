using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class AddBracesTests : CSharpCodeRefactoringTestBase
    {
        [Test]
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

        [Test]
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

        [Ignore("broken")]
        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

