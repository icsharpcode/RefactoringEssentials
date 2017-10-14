using RefactoringEssentials.CSharp;
using RefactoringEssentials.Tests.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp
{
	/// <summary>
	/// Tests for ConvertInstanceToStaticMethodCodeRefactoringProvider.
	/// </summary>
    public class ConvertInstanceToStaticMethodCodeRefactoringTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void MethodWithoutParameters1()
        {
            Test<ConvertInstanceToStaticMethodCodeRefactoringProvider>(@"
class Foo
{
    void $Test()
    {
        int a = 0;
    }
}", @"
class Foo
{
    static void Test(Foo instance)
    {
        int a = 0;
    }
}");
        }

        [Fact]
        public void MethodWithoutParameters2()
        {
            Test<ConvertInstanceToStaticMethodCodeRefactoringProvider>(@"
class Foo
{
    public void $Test()
    {
        int a = 0;
    }
}", @"
class Foo
{
    public static void Test(Foo instance)
    {
        int a = 0;
    }
}");
        }

        [Fact]
        public void MethodWithParameters()
        {
            Test<ConvertInstanceToStaticMethodCodeRefactoringProvider>(@"
class Foo
{
    public void $Test(int b)
    {
        int a = 0;
    }
}", @"
class Foo
{
    public static void Test(Foo instance, int b)
    {
        int a = 0;
    }
}");
        }

        [Fact]
        public void AlreadyStaticMethod()
        {
            TestWrongContext<ConvertInstanceToStaticMethodCodeRefactoringProvider>(@"
class Foo
{
    public static void $Test(int b)
    {
        int a = 0;
    }
}");
        }

        [Fact]
        public void MethodUsingInstanceMember()
        {
            Test<ConvertInstanceToStaticMethodCodeRefactoringProvider>(@"
class Foo
{
    int member;

    void AnotherMethod(int a)
    {
    }

    void $Test()
    {
        int a = 0;
        member = a;
        AnotherMethod(a);
    }
}", @"
class Foo
{
    int member;

    void AnotherMethod(int a)
    {
    }

    static void Test(Foo instance)
    {
        int a = 0;
        instance.member = a;
        instance.AnotherMethod(a);
    }
}");
        }

        [Fact]
        public void MethodUsingInstanceMemberWithThis()
        {
            Test<ConvertInstanceToStaticMethodCodeRefactoringProvider>(@"
class Foo
{
    int member;

    void AnotherMethod(int a)
    {
    }

    void $Test()
    {
        int a = 0;
        this.member = a;
        this.AnotherMethod(a);
    }
}", @"
class Foo
{
    int member;

    void AnotherMethod(int a)
    {
    }

    static void Test(Foo instance)
    {
        int a = 0;
        instance.member = a;
        instance.AnotherMethod(a);
    }
}");
        }

        [Fact]
        public void RecursiveMethodCall()
        {
            Test<ConvertInstanceToStaticMethodCodeRefactoringProvider>(@"
class Foo
{
    int member;

    void $Test()
    {
        int a = 0;
        Test();
    }
}", @"
class Foo
{
    int member;

    static void Test(Foo instance)
    {
        int a = 0;
        Foo.Test(instance);
    }
}");
        }

        [Fact]
        public void RecursiveMethodCallWithThis()
        {
            Test<ConvertInstanceToStaticMethodCodeRefactoringProvider>(@"
class Foo
{
    int member;

    void $Test()
    {
        int a = 0;
        this.Test();
    }
}", @"
class Foo
{
    int member;

    static void Test(Foo instance)
    {
        int a = 0;
        Foo.Test(instance);
    }
}");
        }

        [Fact]
        public void MethodWithInternalReference()
        {
            Test<ConvertInstanceToStaticMethodCodeRefactoringProvider>(@"
class Foo
{
    int member;

    void AnotherMethod(int a)
    {
        Test();
    }

    void $Test()
    {
    }
}", @"
class Foo
{
    int member;

    void AnotherMethod(int a)
    {
        Foo.Test(this);
    }

    static void Test(Foo instance)
    {
    }
}");
        }

        [Fact]
        public void MethodWithExternalReference()
        {
            Test<ConvertInstanceToStaticMethodCodeRefactoringProvider>(@"
class Foo
{
    public void $Test()
    {
        int a = 0;
    }
}

class Foo2
{
    void Test(Foo foo)
    {
        foo.Test();
    }
}", @"
class Foo
{
    public static void Test(Foo instance)
    {
        int a = 0;
    }
}

class Foo2
{
    void Test(Foo foo)
    {
        Foo.Test(foo);
    }
}");
        }

        [Fact]
        public void TestInterfaceContext()
        {
            TestWrongContext<ConvertInstanceToStaticMethodCodeRefactoringProvider>(
                "interface Test { void $TestMethod(); }"
            );
        }
    }
}

