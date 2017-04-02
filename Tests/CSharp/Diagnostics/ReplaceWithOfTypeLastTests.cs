using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class ReplaceWithOfTypeLastTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestCaseBasic()
        {
            Analyze<ReplaceWithOfTypeLastAnalyzer>(@"using System.Linq;
class Test
{
    public void Foo(object[] obj)
    {
        $obj.Select(q => q as Test).Last(q => q != null)$;
    }
}", @"using System.Linq;
class Test
{
    public void Foo(object[] obj)
    {
        obj.OfType<Test>().Last();
    }
}");
        }

        [Fact]
        public void TestCaseBasicWithFollowUpExpresison()
        {
            Analyze<ReplaceWithOfTypeLastAnalyzer>(@"using System.Linq;
class Test
{
    public void Foo(object[] obj)
    {
        $obj.Select(q => q as Test).Last(q => q != null && Foo(q))$;
    }
}", @"using System.Linq;
class Test
{
    public void Foo(object[] obj)
    {
        obj.OfType<Test>().Last(q => Foo(q));
    }
}");
        }

        [Fact]
        public void TestDisable()
        {
            Analyze<ReplaceWithOfTypeLastAnalyzer>(@"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
#pragma warning disable " + CSharpDiagnosticIDs.ReplaceWithOfTypeLastAnalyzerID + @"
		obj.Select (q => q as Test).Last (q => q != null);
	}
}");
        }

        [Fact]
        public void TestJunk()
        {
            Analyze<ReplaceWithOfTypeLastAnalyzer>(@"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
		obj.Select (x => q as Test).Last (q => q != null);
	}
}");
            Analyze<ReplaceWithOfTypeLastAnalyzer>(@"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
		obj.Select (q => q as Test).Last (q => 1 != null);
	}
}");

        }

    }
}

