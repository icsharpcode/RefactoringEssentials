using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class ReplaceWithOfTypeCountTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestCaseBasic()
        {
            Analyze<ReplaceWithOfTypeCountAnalyzer>(@"using System.Linq;
class Test
{
    public void Foo(object[] obj)
    {
        $obj.Select(q => q as Test).Count(q => q != null)$;
    }
}", @"using System.Linq;
class Test
{
    public void Foo(object[] obj)
    {
        obj.OfType<Test>().Count();
    }
}");
        }

        [Fact]
        public void TestCaseBasicWithFollowUpExpresison()
        {
            Analyze<ReplaceWithOfTypeCountAnalyzer>(@"using System.Linq;
class Test
{
    public void Foo(object[] obj)
    {
        $obj.Select(q => q as Test).Count(q => q != null && Foo(q))$;
    }
}", @"using System.Linq;
class Test
{
    public void Foo(object[] obj)
    {
        obj.OfType<Test>().Count(q => Foo(q));
    }
}");
        }

        [Fact]
        public void TestDisable()
        {
            Analyze<ReplaceWithOfTypeCountAnalyzer>(@"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
#pragma warning disable " + CSharpDiagnosticIDs.ReplaceWithOfTypeCountAnalyzerID + @"
		obj.Select (q => q as Test).Count (q => q != null);
	}
}");
        }

        [Fact]
        public void TestJunk()
        {
            Analyze<ReplaceWithOfTypeCountAnalyzer>(@"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
		obj.Select (x => q as Test).Count (q => q != null);
	}
}");
            Analyze<ReplaceWithOfTypeCountAnalyzer>(@"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
		obj.Select (q => q as Test).Count (q => 1 != null);
	}
}");

        }

    }
}
