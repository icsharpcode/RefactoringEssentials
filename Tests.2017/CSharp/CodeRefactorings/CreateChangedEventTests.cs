using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class CreateChangedEventTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestSimpleCase()
        {
            Test<CreateChangedEventCodeRefactoringProvider>(@"class TestClass
{
    string test;
    public string $Test {
        get {
            return test;
        }
        set {
            test = value;
        }
    }
}", @"class TestClass
{
    string test;
    public string Test {
        get {
            return test;
        }
        set {
            test = value;
            OnTestChanged(System.EventArgs.Empty);
        }
    }

    protected virtual void OnTestChanged(System.EventArgs e)
    {
        TestChanged?.Invoke(this, e);
    }

    public event System.EventHandler TestChanged;
}");
        }

        [Fact]
        public void TestSimplify()
        {
            Test<CreateChangedEventCodeRefactoringProvider>(@"using System;
class TestClass
{
    string test;
    public string $Test {
        get {
            return test;
        }
        set {
            test = value;
        }
    }
}", @"using System;
class TestClass
{
    string test;
    public string Test {
        get {
            return test;
        }
        set {
            test = value;
            OnTestChanged(EventArgs.Empty);
        }
    }

    protected virtual void OnTestChanged(EventArgs e)
    {
        TestChanged?.Invoke(this, e);
    }

    public event EventHandler TestChanged;
}");
        }

        [Fact]
        public void TestStaticClassCase()
        {
            Test<CreateChangedEventCodeRefactoringProvider>(@"static class TestClass
{
    static string test;
    public static string $Test {
        get {
            return test;
        }
        set {
            test = value;
        }
    }
}", @"static class TestClass
{
    static string test;
    public static string Test {
        get {
            return test;
        }
        set {
            test = value;
            OnTestChanged(System.EventArgs.Empty);
        }
    }

    static void OnTestChanged(System.EventArgs e)
    {
        TestChanged?.Invoke(null, e);
    }

    public static event System.EventHandler TestChanged;
}");
        }

        [Fact]
        public void TestSealedCase()
        {
            Test<CreateChangedEventCodeRefactoringProvider>(@"sealed class TestClass
{
    string test;
    public string $Test {
        get {
            return test;
        }
        set {
            test = value;
        }
    }
}", @"sealed class TestClass
{
    string test;
    public string Test {
        get {
            return test;
        }
        set {
            test = value;
            OnTestChanged(System.EventArgs.Empty);
        }
    }

    void OnTestChanged(System.EventArgs e)
    {
        TestChanged?.Invoke(this, e);
    }

    public event System.EventHandler TestChanged;
}");
        }

        [Fact]
        public void TestWrongLocation()
        {
            TestWrongContext<CreateChangedEventCodeRefactoringProvider>(@"class TestClass
{
    string test;
    public $string Test {
        get {
            return test;
        }
        set {
            test = value;
        }
    }
}");

            TestWrongContext<CreateChangedEventCodeRefactoringProvider>(@"class TestClass
{
    string test;
    public string $FooBar.Test {
        get {
            return test;
        }
        set {
            test = value;
        }
    }
}");

            TestWrongContext<CreateChangedEventCodeRefactoringProvider>(@"class TestClass
{
    string test;
    public string Test ${
        get {
            return test;
        }
        set {
            test = value;
        }
    }
}");
        }

    }
}

