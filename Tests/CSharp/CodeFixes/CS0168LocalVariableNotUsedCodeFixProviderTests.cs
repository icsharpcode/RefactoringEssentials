using RefactoringEssentials.CSharp.CodeFixes;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeFixes
{
    public class CS0168LocalVariableNotUsedCodeFixProviderTests : CSharpCodeFixTestBase
    {
        [Fact]
        public void TestUnusedVariable()
        {
            var input = @"
class TestClass {
    void TestMethod ()
    {
        int $i$;
    }
}";
            var output = @"
class TestClass {
    void TestMethod ()
    {
    }
}";
            Test<CS0168LocalVariableNotUsedCodeFixProvider>(input, output);
        }

        [Fact]
        public void TestUnusedVariable2()
        {
            var input2 = @"
class TestClass {
    void TestMethod ()
    {
        int $i$, j;
        j = 1;
    }
}";
            var output2 = @"
class TestClass {
    void TestMethod ()
    {
        int j;
        j = 1;
    }
}";
            Test<CS0168LocalVariableNotUsedCodeFixProvider>(input2, output2);
        }

        [Fact]
        public void TestUsedVariable()
        {
            var input1 = @"
class TestClass {
    void TestMethod ()
    {
        int i = 0;
    }
}";
            var input2 = @"
class TestClass {
    void TestMethod ()
    {
        int i;
        i = 0;
    }
}";
            TestWrongContext<CS0168LocalVariableNotUsedCodeFixProvider>(input1);
            TestWrongContext<CS0168LocalVariableNotUsedCodeFixProvider>(input2);
        }

        [Fact]
        public void TestUnusedForeachVariable()
        {
            var input = @"
class TestClass {
    void TestMethod ()
    {
        var array = new int[10];
        foreach (var i in array) {
        }
    }
}";
            TestWrongContext<CS0168LocalVariableNotUsedCodeFixProvider>(input);
        }

        [Fact]
        public void TestUsedForeachVariable()
        {
            var input = @"
class TestClass {
    void TestMethod ()
    {
        var array = new int[10];
        int j = 0;
        foreach (var i in array) {
            j += i;
        }
    }
}";
            TestWrongContext<CS0168LocalVariableNotUsedCodeFixProvider>(input);
        }
    }
}