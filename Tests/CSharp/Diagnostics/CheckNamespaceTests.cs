using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class CheckNamespaceTests : CSharpDiagnosticTestBase
    {
        [Fact(Skip="TODO")]
        public void TestWrongNamespace()
        {
            TestIssue<CheckNamespaceAnalyzer>(@"namespace Foo {}");
        }

        [Fact(Skip="TODO")]
        public void TestSubstring()
        {
            TestIssue<CheckNamespaceAnalyzer>(@"namespace TestFoo {}");
        }


        [Fact(Skip="TODO")]
        public void TestGlobalClass()
        {
            TestIssue<CheckNamespaceAnalyzer>(@"class Foo {}");
        }


        [Fact(Skip="TODO")]
        public void CheckValidNamespace()
        {
            Analyze<CheckNamespaceAnalyzer>(@"namespace Test {}");
        }

        [Fact(Skip="TODO")]
        public void TestDisable()
        {
            Analyze<CheckNamespaceAnalyzer>(@"
// ReSharper disable once CheckNamespace
namespace Foo {}");
        }

    }
}

