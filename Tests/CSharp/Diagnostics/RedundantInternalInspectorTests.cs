using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class RedundantInternalInspectorTests : CSharpDiagnosticTestBase
    {
        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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
