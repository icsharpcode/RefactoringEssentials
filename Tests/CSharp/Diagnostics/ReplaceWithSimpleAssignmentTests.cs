using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class ReplaceWithSimpleAssignmentTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestOrCase()
        {
            Analyze<ReplaceWithSimpleAssignmentAnalyzer>(@"
class Test
{
    void Foo (bool b)
    {
        $b |= true$;
    }
}
", @"
class Test
{
    void Foo (bool b)
    {
        b = true;
    }
}
");
        }

        [Fact]
        public void TestAndCase()
        {
            Analyze<ReplaceWithSimpleAssignmentAnalyzer>(@"
class Test
{
    void Foo (bool b)
    {
        $b &= false$;
    }
}
", @"
class Test
{
    void Foo (bool b)
    {
        b = false;
    }
}
");
        }

        [Fact]
        public void TestDisable()
        {
            Analyze<ReplaceWithSimpleAssignmentAnalyzer>(@"
class Test
{
	void Foo (bool b)
	{
#pragma warning disable " + CSharpDiagnosticIDs.ReplaceWithSimpleAssignmentAnalyzerID + @"
		b |= true;
	}
}
");
        }


    }
}

