using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{

    [TestFixture]
    public class RedundantBaseConstructorTests : CSharpDiagnosticTestBase
    {
        [Test]
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
    $public TestClass(int data) : base() { }$
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

        [Test]
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
