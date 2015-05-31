using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class ConvertStringFormatToInterpolatedStringTests : CSharpCodeRefactoringTestBase
    {
        [Test]
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

        [Test]
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

    }
}

