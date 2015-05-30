using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class ObjectCreationAsStatementTests : InspectionActionTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
        public void TestDisable()
        {
            Analyze<ObjectCreationAsStatementAnalyzer>(@"
class Foo
{
	void Bar ()
	{
#pragma warning disable " + DiagnosticIDs.ObjectCreationAsStatementAnalyzerID + @"
		new Foo();
	}
}
");
        }
    }
}

