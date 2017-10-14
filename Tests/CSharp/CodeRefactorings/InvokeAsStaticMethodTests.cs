using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class InvokeAsStaticMethodTests : CSharpCodeRefactoringTestBase
    {

        [Fact]
        public void HandlesBasicCase()
        {
            Test<InvokeAsStaticMethodCodeRefactoringProvider>(@"
class A { }
static class B
{
    public static void Ext (this A a, int i) { }
}
class C
{
    void F()
    {
        A a = new A();
        a.$Ext (1);
    }
}", @"
class A { }
static class B
{
    public static void Ext (this A a, int i) { }
}
class C
{
    void F()
    {
        A a = new A();
        B.Ext(a, 1);
    }
}");
        }

        [Fact]
        public void HandlesBasicCaseWithComment()
        {
            Test<InvokeAsStaticMethodCodeRefactoringProvider>(@"
class A { }
static class B
{
    public static void Ext (this A a, int i) { }
}
class C
{
    void F()
    {
        A a = new A();
        // Some comment
        a.$Ext (1);
    }
}", @"
class A { }
static class B
{
    public static void Ext (this A a, int i) { }
}
class C
{
    void F()
    {
        A a = new A();
        // Some comment
        B.Ext(a, 1);
    }
}");
        }

        [Fact]
        public void HandlesReturnValueUsage()
        {
            Test<InvokeAsStaticMethodCodeRefactoringProvider>(@"
class A { }
static class B
{
    public static bool Ext (this A a, int i)
    {
        return false;
    }
}
class C
{
    void F()
    {
        A a = new A();
        if (a.$Ext (1))
            return;
    }
}", @"
class A { }
static class B
{
    public static bool Ext (this A a, int i)
    {
        return false;
    }
}
class C
{
    void F()
    {
        A a = new A();
        if (B.Ext(a, 1))
            return;
    }
}");
        }

        [Fact]
        public void IgnoresStaticMethodCalls()
        {
            TestWrongContext<InvokeAsStaticMethodCodeRefactoringProvider>(@"
class A { }
static class B
{
    public static void Ext (this A a, int i) { }
}
class C
{
    void F()
    {
        A a = new A();
        B.$Ext(a, 1);
    }
}");
        }

        [Fact]
        public void IgnoresRegularMemberMethodCalls()
        {
            TestWrongContext<InvokeAsStaticMethodCodeRefactoringProvider>(@"
class A
{
    public void Ext (int i) { }
}
class C
{
    void F()
    {
        A a = new A();
        a.$Ext(1);
    }
}");
        }
    }
}

