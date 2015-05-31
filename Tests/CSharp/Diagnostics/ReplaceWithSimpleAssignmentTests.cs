using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class ReplaceWithSimpleAssignmentTests : CSharpDiagnosticTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
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

