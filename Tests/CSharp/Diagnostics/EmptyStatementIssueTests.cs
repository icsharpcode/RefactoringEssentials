using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class EmptyStatementTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestBasicCase()
        {
            Analyze<EmptyStatementAnalyzer>(@"
class Test
{
	public void Foo ()
	{
		$;$
	}
}
", @"
class Test
{
	public void Foo ()
	{
	}
}
");
        }

        [Test]
        public void TestDisable()
        {
            Analyze<EmptyStatementAnalyzer>(@"
class Test
{
	public void Foo ()
	{
#pragma warning disable " + CSharpDiagnosticIDs.EmptyStatementAnalyzerID + @"
		;
	}
}
");
        }

        [Test]
        public void TestEmbeddedStatements()
        {
            Analyze<EmptyStatementAnalyzer>(@"
class Test
{
	public void Foo ()
	{
		for (;;) ;
		if (true) ; else ;
		while (true) ;
		do ; while (true);
	}
}
");
        }

        [Test]
        public void TestInvalidCase()
        {
            Analyze<EmptyStatementAnalyzer>(@"
class Test
{
	public void Foo ()
	{
		label:
			;
	}
}
");
        }

    }
}

