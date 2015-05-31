using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class ReplaceWithOfTypeCountTests : CSharpDiagnosticTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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
