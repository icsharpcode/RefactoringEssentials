using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class InvokeAsStaticMethodTests : CSharpCodeRefactoringTestBase
    {

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

