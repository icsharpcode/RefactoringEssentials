using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class NotImplementedExceptionInspectorTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestInspectorCase1()
        {
            Analyze<NotImplementedExceptionAnalyzer>(@"
class Foo
{
	void Bar (string str)
	{
		throw $new System.NotImplementedException()$;
	}
}");
        }
    }
}
