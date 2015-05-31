using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [Ignore("TODO")]
    [TestFixture]
    public class CheckNamespaceTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestWrongNamespace()
        {
            TestIssue<CheckNamespaceAnalyzer>(@"namespace Foo {}");
        }

        [Test]
        public void TestSubstring()
        {
            TestIssue<CheckNamespaceAnalyzer>(@"namespace TestFoo {}");
        }


        [Test]
        public void TestGlobalClass()
        {
            TestIssue<CheckNamespaceAnalyzer>(@"class Foo {}");
        }


        [Test]
        public void CheckValidNamespace()
        {
            Analyze<CheckNamespaceAnalyzer>(@"namespace Test {}");
        }

        [Test]
        public void TestDisable()
        {
            Analyze<CheckNamespaceAnalyzer>(@"
// ReSharper disable once CheckNamespace
namespace Foo {}");
        }

    }
}

