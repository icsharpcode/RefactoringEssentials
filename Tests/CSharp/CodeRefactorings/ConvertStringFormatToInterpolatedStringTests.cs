using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ConvertStringFormatToInterpolatedStringTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestSimpleStringFormat()
        {
            Test<ConvertStringFormatToInterpolatedStringCodeRefactoringProvider>(@"
class TestClass
{
    void Foo ()
    {
        var world = ""World"";
        var str = $string.Format (""Hello {0}"", world);
    }
}", @"
class TestClass
{
    void Foo ()
    {
        var world = ""World"";
        var str = $""Hello {world}"";
    }
}");
        }

        [Fact]
        public void TestComplexStringFormat()
        {
            Test<ConvertStringFormatToInterpolatedStringCodeRefactoringProvider>(@"
class TestClass
{
    void Foo ()
    {
        var str = $string.Format (""Hello {0:0.0} {1:0X}"", 0.5d,2134);
    }
}", @"
class TestClass
{
    void Foo ()
    {
        var str = $""Hello {0.5d:0.0} {2134:0X}"";
    }
}");
        }

        /// <summary>
        /// Newline character handling for "To interpolated string" #182
        /// </summary>
        [Fact]
        public void TestIssue182()
        {
            Test<ConvertStringFormatToInterpolatedStringCodeRefactoringProvider>(@"
class TestClass
{
    void Foo ()
    {
        var world = ""World"";
        var str = $string.Format (""Hello\n {0}"", world);
    }
}", @"
class TestClass
{
    void Foo ()
    {
        var world = ""World"";
        var str = $""Hello\n {world}"";
    }
}");
        }

        [Fact(Skip="Broken on windows")]
        public void TestVerbatimStringFormat()
        {
            Test<ConvertStringFormatToInterpolatedStringCodeRefactoringProvider>(
                "class TestClass\n{\n    void Foo ()\n    {\n        var world = \"World\";\n        var str = $string.Format (@\"Hello \"\" {0}\n!\", world);\n    }\n}", 
                "class TestClass\n{\n    void Foo ()\n    {\n        var world = \"World\";\n        var str = $\"Hello \\\" {world}\\n!\";\n    }\n}");
        }
    }
}

