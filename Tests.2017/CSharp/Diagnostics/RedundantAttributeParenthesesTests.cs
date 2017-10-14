using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class RedundantAttributeParenthesesTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void DefaultCase()
        {
            Analyze<RedundantAttributeParenthesesAnalyzer>(@"
[Test$()$]
class TestClass { }", @"
[Test]
class TestClass { }");
        }

        [Fact]
        public void IgnoreNoParentheses()
        {
            Analyze<RedundantAttributeParenthesesAnalyzer>(@"
[Test]
class TestClass { }");

        }

        [Fact]
        public void IgnoreParameters()
        {
            Analyze<RedundantAttributeParenthesesAnalyzer>(@"
[Test(1)]
class TestClass { }");

        }

        [Fact]
        public void TestDisable()
        {
            Analyze<RedundantAttributeParenthesesAnalyzer>(@"
#pragma warning disable " + CSharpDiagnosticIDs.RedundantAttributeParenthesesAnalyzerID + @"
[Test ()]
class TestClass { }");
        }

    }
}
