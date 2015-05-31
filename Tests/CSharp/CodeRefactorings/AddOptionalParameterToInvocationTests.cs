using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class AddOptionalParameterToInvocationTests : CSharpCodeRefactoringTestBase
    {
        [Test]
        public void TestSimple()
        {
            Test<AddOptionalParameterToInvocationCodeRefactoringProvider>(@"
class TestClass
{
    public void Foo(string msg = ""Hello"") {}
    public void Bar() {
        $Foo();
    }
}", @"
class TestClass
{
    public void Foo(string msg = ""Hello"") {}
    public void Bar() {
        Foo(""Hello"");
    }
}");
        }

        [Test]
        public void TestSimpleWithComment()
        {
            Test<AddOptionalParameterToInvocationCodeRefactoringProvider>(@"
class TestClass
{
    public void Foo(string msg = ""Hello"") {}
    public void Bar() {
        // Some comment
        $Foo();
    }
}", @"
class TestClass
{
    public void Foo(string msg = ""Hello"") {}
    public void Bar() {
        // Some comment
        Foo(""Hello"");
    }
}");
        }

        [Test]
        public void TestMultiple1()
        {
            Test<AddOptionalParameterToInvocationCodeRefactoringProvider>(@"
class TestClass
{
    public void Foo(string msg = ""Hello"", string msg2 = ""Bar"") {}
    public void Bar() {
        $Foo();
    }
}", @"
class TestClass
{
    public void Foo(string msg = ""Hello"", string msg2 = ""Bar"") {}
    public void Bar() {
        Foo(""Hello"");
    }
}");
        }

        [Test]
        public void TestExtensionMethod()
        {
            Test<AddOptionalParameterToInvocationCodeRefactoringProvider>(@"
static class Extensions
{
    public static void Foo(this string self, string msg = ""Hello"") {}
}
class TestClass
{
    public void Bar() {
        ""test"".$Foo();
    }
}", @"
static class Extensions
{
    public static void Foo(this string self, string msg = ""Hello"") {}
}
class TestClass
{
    public void Bar() {
        ""test"".Foo(""Hello"");
    }
}");
        }

        [Test]
        public void TestMultiple2()
        {
            Test<AddOptionalParameterToInvocationCodeRefactoringProvider>(@"
class TestClass
{
    public void Foo(string msg = ""Hello"", string msg2 = ""Bar"") {}
    public void Bar() {
        $Foo();
    }
}", @"
class TestClass
{
    public void Foo(string msg = ""Hello"", string msg2 = ""Bar"") {}
    public void Bar() {
        Foo(msg2: ""Bar"");
    }
}", 1);
        }

        [Test]
        public void TestMultiple3()
        {
            Test<AddOptionalParameterToInvocationCodeRefactoringProvider>(@"
class TestClass
{
    public void Foo(string msg = ""Hello"", string msg2 = ""Bar"") { }
    public void Bar()
    {
        $Foo();
    }
}", @"
class TestClass
{
    public void Foo(string msg = ""Hello"", string msg2 = ""Bar"") { }
    public void Bar()
    {
        Foo(""Hello"", ""Bar"");
    }
}", 2);
        }

        [Test]
        public void TestNoMoreParameters()
        {
            TestWrongContext<AddOptionalParameterToInvocationCodeRefactoringProvider>(@"
class TestClass
{
    public void Foo(string msg = ""Hello"", string msg2 = ""Bar"") {}
    public void Bar() {
        $Foo(string.Empty, string.Empty);
    }
}
");
        }

        [Test]
        public void TestParams()
        {
            TestWrongContext<AddOptionalParameterToInvocationCodeRefactoringProvider>(@"
class TestClass
{
    public void Foo(params string[] p) {}
    public void Bar()
    {
        $Foo();
    }
}
");
        }
    }
}
