using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class RewriteIfReturnToReturnTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void When_Return_Value_Correctly()
        {
            var input = @"
class TestClass
{
	bool TestMethod (object obj)
	{
        return obj!= null;
	}
}";
            Analyze<RewriteIfReturnToReturnAnalyzer>(input);
        }

        [Test]
        public void When_Return_Statement_Corrected()
        {
            var input = @"
class TestClass
{
	bool TestMethod (object obj)
	{
        $if (obj != null)
            return true;$
        return false;
	}
}";

            var output = @"
class TestClass
{
	bool TestMethod (object obj)
	{
        return obj != null;
    }
}";
            Analyze<RewriteIfReturnToReturnAnalyzer>(input, output);
        }
    }
}