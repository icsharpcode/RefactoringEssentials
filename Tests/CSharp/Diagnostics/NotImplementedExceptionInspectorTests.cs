using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class NotImplementedExceptionInspectorTests : CSharpDiagnosticTestBase
    {
        [Test]
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
