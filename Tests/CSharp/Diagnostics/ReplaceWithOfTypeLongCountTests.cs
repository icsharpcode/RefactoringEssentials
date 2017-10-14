using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class ReplaceWithOfTypeLongCountTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestCaseBasic()
        {
            Analyze<ReplaceWithOfTypeLongCountAnalyzer>(@"using System.Linq;
class Test
{
    public void Foo(object[] obj)
    {
        $obj.Select(q => q as Test).LongCount(q => q != null)$;
    }
}", @"using System.Linq;
class Test
{
    public void Foo(object[] obj)
    {
        obj.OfType<Test>().LongCount();
    }
}");
        }

        [Fact]
        public void TestCaseBasicWithFollowUpExpresison()
        {
            Analyze<ReplaceWithOfTypeLongCountAnalyzer>(@"using System.Linq;
class Test
{
    public void Foo(object[] obj)
    {
        $obj.Select(q => q as Test).LongCount(q => q != null && Foo(q))$;
    }
}", @"using System.Linq;
class Test
{
    public void Foo(object[] obj)
    {
        obj.OfType<Test>().LongCount(q => Foo(q));
    }
}");
        }

        [Fact]
        public void TestDisable()
        {
            Analyze<ReplaceWithOfTypeLongCountAnalyzer>(@"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
#pragma warning disable " + CSharpDiagnosticIDs.ReplaceWithOfTypeLongCountAnalyzerID + @"
		obj.Select (q => q as Test).LongCount (q => q != null);
	}
}");
        }

        [Fact]
        public void TestJunk()
        {
            Analyze<ReplaceWithOfTypeLongCountAnalyzer>(@"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
		obj.Select (x => q as Test).LongCount (q => q != null);
	}
}");
            Analyze<ReplaceWithOfTypeLongCountAnalyzer>(@"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
		obj.Select (q => q as Test).LongCount (q => 1 != null);
	}
}");

        }

    }
}

