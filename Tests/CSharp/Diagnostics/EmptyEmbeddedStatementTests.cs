using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class EmptyEmbeddedStatementTests : CSharpDiagnosticTestBase
    {
        [Fact]
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

        [Fact]
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

        [Fact]
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
