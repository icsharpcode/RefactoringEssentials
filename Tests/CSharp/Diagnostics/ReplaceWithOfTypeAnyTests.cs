using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class ReplaceWithOfTypeAnyTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestCaseBasic()
        {
            Analyze<ReplaceWithOfTypeAnyAnalyzer>(@"using System.Linq;
class Test
{
    public void Foo(object[] obj)
    {
        $obj.Select(q => q as Test).Any(q => q != null)$;
    }
}", @"using System.Linq;
class Test
{
    public void Foo(object[] obj)
    {
        obj.OfType<Test>().Any();
    }
}");
        }

        [Fact]
        public void TestCaseBasicWithFollowUpExpresison()
        {
            Analyze<ReplaceWithOfTypeAnyAnalyzer>(@"using System.Linq;
class Test
{
    public void Foo(object[] obj)
    {
        $obj.Select(q => q as Test).Any(q => q != null && Foo(q))$;
    }
}", @"using System.Linq;
class Test
{
    public void Foo(object[] obj)
    {
        obj.OfType<Test>().Any(q => Foo(q));
    }
}");
        }

        [Fact]
        public void TestDisable()
        {
            Analyze<ReplaceWithOfTypeAnyAnalyzer>(@"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
#pragma warning disable " + CSharpDiagnosticIDs.ReplaceWithOfTypeAnyAnalyzerID + @"
		obj.Select (q => q as Test).Any (q => q != null);
	}
}");
        }

        [Fact]
        public void TestJunk()
        {
            Analyze<ReplaceWithOfTypeAnyAnalyzer>(@"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
		obj.Select (x => q as Test).Any (q => q != null);
	}
}");
            Analyze<ReplaceWithOfTypeAnyAnalyzer>(@"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
		obj.Select (q => q as Test).Any (q => 1 != null);
	}
}");

        }

    }
}

