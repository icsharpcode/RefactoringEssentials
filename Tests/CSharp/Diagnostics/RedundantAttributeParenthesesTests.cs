using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class RedundantAttributeParenthesesTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void Test()
        {
            Analyze<RedundantAttributeParenthesesAnalyzer>(@"
[$Test()$]
class TestClass { }", @"
[Test]
class TestClass { }");
        }

        [Test]
        public void IgnoreNoParentheses()
        {
            Analyze<RedundantAttributeParenthesesAnalyzer>(@"
[Test]
class TestClass { }");

        }

        [Test]
        public void IgnoreParameters()
        {
            Analyze<RedundantAttributeParenthesesAnalyzer>(@"
[Test(1)]
class TestClass { }");

        }

        [Test]
        public void TestDisable()
        {
            Analyze<RedundantAttributeParenthesesAnalyzer>(@"
#pragma warning disable " + CSharpDiagnosticIDs.RedundantAttributeParenthesesAnalyzerID + @"
[Test ()]
class TestClass { }");
        }

    }
}
