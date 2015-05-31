using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class ReplaceWithOfTypeLastOrDefaultTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestCaseBasic()
        {
            Analyze<ReplaceWithOfTypeLastOrDefaultAnalyzer>(@"using System.Linq;
class Test
{
    public void Foo(object[] obj)
    {
        $obj.Select(q => q as Test).LastOrDefault(q => q != null)$;
    }
}", @"using System.Linq;
class Test
{
    public void Foo(object[] obj)
    {
        obj.OfType<Test>().LastOrDefault();
    }
}");
        }

        [Test]
        public void TestCaseBasicWithFollowUpExpresison()
        {
            Analyze<ReplaceWithOfTypeLastOrDefaultAnalyzer>(@"using System.Linq;
class Test
{
    public void Foo(object[] obj)
    {
        $obj.Select(q => q as Test).LastOrDefault(q => q != null && Foo(q))$;
    }
}", @"using System.Linq;
class Test
{
    public void Foo(object[] obj)
    {
        obj.OfType<Test>().LastOrDefault(q => Foo(q));
    }
}");
        }

        [Test]
        public void TestDisable()
        {
            Analyze<ReplaceWithOfTypeLastOrDefaultAnalyzer>(@"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
#pragma warning disable " + CSharpDiagnosticIDs.ReplaceWithOfTypeLastOrDefaultAnalyzerID + @"
		obj.Select (q => q as Test).LastOrDefault (q => q != null);
	}
}");
        }

        [Test]
        public void TestJunk()
        {
            Analyze<ReplaceWithOfTypeLastOrDefaultAnalyzer>(@"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
		obj.Select (x => q as Test).LastOrDefault (q => q != null);
	}
}");
            Analyze<ReplaceWithOfTypeLastOrDefaultAnalyzer>(@"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
		obj.Select (q => q as Test).LastOrDefault (q => 1 != null);
	}
}");

        }

    }
}

