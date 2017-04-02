using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class ObjectCreationAsStatementTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestSimpleCase()
        {
            Analyze<ObjectCreationAsStatementAnalyzer>(@"
class Foo
{
	void Bar()
	{
		$new Foo()$;
	}
}
");
        }

        [Fact]
        public void TestNoIssue()
        {
            Analyze<ObjectCreationAsStatementAnalyzer>(@"
class Foo
{
	void Bar()
	{
		System.Console.WriteLine(new Foo());
	}
}
");
        }

        [Fact]
        public void TestDisable()
        {
            Analyze<ObjectCreationAsStatementAnalyzer>(@"
class Foo
{
	void Bar ()
	{
#pragma warning disable " + CSharpDiagnosticIDs.ObjectCreationAsStatementAnalyzerID + @"
		new Foo();
	}
}
");
        }
    }
}

