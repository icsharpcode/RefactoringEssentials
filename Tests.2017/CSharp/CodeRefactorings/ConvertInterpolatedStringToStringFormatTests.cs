using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ConvertInterpolatedStringToStringFormatTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestSimpleStringFormat()
        {
            Test<ConvertInterpolatedStringToStringFormatCodeRefactoringProvider>(@"
class TestClass
{
    void Foo ()
    {
        var world = ""World"";
        var str = $""Hel$lo {world}"";
    }
}", @"
class TestClass
{
    void Foo ()
    {
        var world = ""World"";
        var str = string.Format(""Hello {0}"", world);
    }
}");
        }

        [Fact]
        public void TestComplexStringFormat()
        {
            Test<ConvertInterpolatedStringToStringFormatCodeRefactoringProvider>(@"
class TestClass
{
    void Foo ()
    {
        var str = $""Hell$o {0.5d:0.0} {2134:0X}"";
    }
}", @"
class TestClass
{
    void Foo ()
    {
        var str = string.Format(""Hello {0:0.0} {1:0X}"", 0.5d, 2134);
    }
}");
        }


        [Fact]
        public void TestRepeats()
        {
            Test<ConvertInterpolatedStringToStringFormatCodeRefactoringProvider>(@"
class TestClass
{
    void Foo ()
    {
        var world = ""World"";
        var str = $""Hel$lo {world} {world}"";
    }
}", @"
class TestClass
{
    void Foo ()
    {
        var world = ""World"";
        var str = string.Format(""Hello {0} {0}"", world);
    }
}");
        }

        [Fact]
        public void TestEscapes()
        {
            Test<ConvertInterpolatedStringToStringFormatCodeRefactoringProvider>(@"
class TestClass
{
    void Foo ()
    {
        var world = ""World"";
        var str = $""Hel$lo\n {world}"";
    }
}", @"
class TestClass
{
    void Foo ()
    {
        var world = ""World"";
        var str = string.Format(""Hello\n {0}"", world);
    }
}");
        }
    }
}

