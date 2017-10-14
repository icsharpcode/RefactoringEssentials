using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class ReplaceWithOfTypeFirstOrDefaultTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestCaseBasic()
        {
            Analyze<ReplaceWithOfTypeFirstOrDefaultAnalyzer>(@"using System.Linq;
class Test
{
    public void Foo(object[] obj)
    {
        $obj.Select(q => q as Test).FirstOrDefault(q => q != null)$;
    }
}", @"using System.Linq;
class Test
{
    public void Foo(object[] obj)
    {
        obj.OfType<Test>().FirstOrDefault();
    }
}");
        }

        [Fact]
        public void TestCaseBasicWithFollowUpExpression()
        {
            Analyze<ReplaceWithOfTypeFirstOrDefaultAnalyzer>(@"using System.Linq;
class Test
{
    public void Foo(object[] obj)
    {
        $obj.Select(q => q as Test).FirstOrDefault(q => q != null && Foo(q))$;
    }
}", @"using System.Linq;
class Test
{
    public void Foo(object[] obj)
    {
        obj.OfType<Test>().FirstOrDefault(q => Foo(q));
    }
}");
        }

        [Fact]
        public void TestDisable()
        {
            Analyze<ReplaceWithOfTypeFirstOrDefaultAnalyzer>(@"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
#pragma warning disable " + CSharpDiagnosticIDs.ReplaceWithOfTypeFirstOrDefaultAnalyzerID + @"
		obj.Select (q => q as Test).FirstOrDefault (q => q != null);
	}
}");
        }

        [Fact]
        public void TestJunk()
        {
            Analyze<ReplaceWithOfTypeFirstOrDefaultAnalyzer>(@"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
		obj.Select (x => q as Test).FirstOrDefault (q => q != null);
	}
}");
            Analyze<ReplaceWithOfTypeFirstOrDefaultAnalyzer>(@"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
		obj.Select (q => q as Test).FirstOrDefault (q => 1 != null);
	}
}");

        }

    }
}

