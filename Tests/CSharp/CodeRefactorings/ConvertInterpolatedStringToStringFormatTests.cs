using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class ConvertInterpolatedStringToStringFormatTests : CSharpCodeRefactoringTestBase
    {
        [Test]
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

        [Test]
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


        [Test]
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
    }
}

