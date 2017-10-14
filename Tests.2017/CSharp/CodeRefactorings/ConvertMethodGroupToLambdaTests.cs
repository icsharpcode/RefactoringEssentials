using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ConvertMethodGroupToLambdaTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestVoidMethod()
        {
            Test<ConvertMethodGroupToLambdaCodeRefactoringProvider>(@"
using System;
public class Test
{
    void Foo ()
    {
        Action act = $Foo;
    }
}
", @"
using System;
public class Test
{
    void Foo ()
    {
        Action act = () => Foo();
    }
}
");
        }

        [Fact]
        public void TestVoidMethodWithComment1()
        {
            Test<ConvertMethodGroupToLambdaCodeRefactoringProvider>(@"
using System;
public class Test
{
    void Foo ()
    {
		// Some comment
        Action act = $Foo;
    }
}
", @"
using System;
public class Test
{
    void Foo ()
    {
		// Some comment
        Action act = () => Foo();
    }
}
");
        }

        [Fact]
        public void TestVoidMethodWithComment2()
        {
            Test<ConvertMethodGroupToLambdaCodeRefactoringProvider>(@"
using System;
public class Test
{
    void Foo ()
    {
        Action act = $Foo; // Some comment
    }
}
", @"
using System;
public class Test
{
    void Foo ()
    {
        Action act = () => Foo(); // Some comment
    }
}
");
        }

        [Fact]
        public void TestParameter()
        {
            Test<ConvertMethodGroupToLambdaCodeRefactoringProvider>(@"
using System;
public class Test
{
    void Foo (int x, int y)
    {
        Action<int,int> act = $Foo;
    }
}
", @"
using System;
public class Test
{
    void Foo (int x, int y)
    {
        Action<int,int> act = (arg1, arg2) => Foo(arg1, arg2);
    }
}
");
        }

        [Fact]
        public void TestFunction()
        {
            Test<ConvertMethodGroupToLambdaCodeRefactoringProvider>(@"
using System;
public class Test
{
    bool Foo (int x, int y)
    {
        Func<int,int,bool> act = $Foo;
    }
}
", @"
using System;
public class Test
{
    bool Foo (int x, int y)
    {
        Func<int,int,bool> act = (arg1, arg2) => Foo(arg1, arg2);
    }
}
");
        }

        [Fact]
        public void TestOverloads()
        {
            Test<ConvertMethodGroupToLambdaCodeRefactoringProvider>(@"
using System;
public class Test
{
    static void Foo (int x) { }
    static void Foo (int x, int y) { }
    static void Foo () { }

    void Bar ()
    {
        Action<int, int> act = Test.$Foo;
    }
}", @"
using System;
public class Test
{
    static void Foo (int x) { }
    static void Foo (int x, int y) { }
    static void Foo () { }

    void Bar ()
    {
        Action<int, int> act = (arg1, arg2) => Test.Foo(arg1, arg2);
    }
}");
        }


    }
}

