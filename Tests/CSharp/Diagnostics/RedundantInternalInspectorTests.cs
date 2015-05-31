using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class RedundantInternalInspectorTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestInspectorCase1()
        {
            Analyze<RedundantInternalAnalyzer>(@"
namespace Test
{
    $internal$ class Foo
    {
        internal void Bar(string str)
        {
        }
    }
}", @"
namespace Test
{
    class Foo
    {
        internal void Bar(string str)
        {
        }
    }
}");
        }

        [Test]
        public void TestInspectorCase1WithComment()
        {
            Analyze<RedundantInternalAnalyzer>(@"
namespace Test
{
    /// <summary>
    /// Class description.
    /// </summary>
    $internal$ class Foo
    {
        internal void Bar(string str)
        {
        }
    }
}", @"
namespace Test
{
    /// <summary>
    /// Class description.
    /// </summary>
    class Foo
    {
        internal void Bar(string str)
        {
        }
    }
}");
        }

        [Test]
        public void TestNestedClass()
        {
            Analyze<RedundantInternalAnalyzer>(@"
namespace Test
{
    class Foo
    {
        internal class Nested
        {
        }
    }
}");
        }

        [Test]
        public void TestNestedInPublicClass()
        {
            Analyze<RedundantInternalAnalyzer>(@"
namespace Test
{
    public class Foo
    {
        internal class Nested
        {
        }
    }
}");
        }
    }
}
