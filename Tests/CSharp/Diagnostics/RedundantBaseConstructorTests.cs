using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class RedundantBaseConstructorTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void Test()
        {
            var input = @"
class BaseClass
{
    public BaseClass()
    {
    }
}
class TestClass : BaseClass
{
    public TestClass(int data) $: base()$ { }
}
";
            var output = @"
class BaseClass
{
    public BaseClass()
    {
    }
}
class TestClass : BaseClass
{
    public TestClass(int data) { }
}
";
            Analyze<RedundantBaseConstructorCallAnalyzer>(input, output);
        }

        [Fact]
        public void TestDisable()
        {
            var input = @"
class BaseClass
{
    public BaseClass()
    {
    }
}
class TestClass : BaseClass
{
// ReSharper disable once RedundantBaseConstructorCall
#pragma warning disable " + CSharpDiagnosticIDs.RedundantBaseConstructorCallAnalyzerID + @"
    public TestClass(int data) : base() { }
}
";
            Analyze<RedundantBaseConstructorCallAnalyzer>(input);
        }
    }
}
