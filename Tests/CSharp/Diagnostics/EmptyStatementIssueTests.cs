using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class EmptyStatementTests : CSharpDiagnosticTestBase
    {
        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

