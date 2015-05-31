using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class ReplaceWithOfTypeLongCountTests : CSharpDiagnosticTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

