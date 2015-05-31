using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{

    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
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
    public TestClass(int data) : base() { }
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
    public TestClass(int data)
	{ }
}
";
            Test<RedundantBaseConstructorCallAnalyzer>(input, 1, output);
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
    public TestClass(int data) : base() { }
}
";
            Analyze<RedundantBaseConstructorCallAnalyzer>(input);
        }
    }
}
