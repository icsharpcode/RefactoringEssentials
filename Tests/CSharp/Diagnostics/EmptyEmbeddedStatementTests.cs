using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class EmptyEmbeddedStatementTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestSimple()
        {
            Analyze<EmptyEmbeddedStatementAnalyzer>(@"
class TestClass
{
    void TestMethod (int i)
    {
        if (i > 0)$;$
    }
}", @"
class TestClass
{
    void TestMethod (int i)
    {
        if (i > 0)
        {
        }
    }
}");
        }

        [Test]
        public void TestForeach()
        {
            Analyze<EmptyEmbeddedStatementAnalyzer>(@"
class TestClass
{
    void TestMethod (int[] list)
    {
        foreach (var i in list)$;$
    }
}", @"
class TestClass
{
    void TestMethod (int[] list)
    {
        foreach (var i in list)
        {
        }
    }
}");
        }

        [Test]
        public void TestDisable()
        {
            Analyze<EmptyEmbeddedStatementAnalyzer>(@"
class TestClass
{
    void TestMethod (int i)
    {
#pragma warning disable " + CSharpDiagnosticIDs.EmptyEmbeddedStatementAnalyzerID + @"
        if (i > 0);
    }
}");
        }
    }

}
