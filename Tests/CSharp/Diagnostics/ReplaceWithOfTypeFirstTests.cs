using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class ReplaceWithOfTypeFirstTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestCaseBasic()
        {
            Analyze<ReplaceWithOfTypeFirstAnalyzer>(@"using System.Linq;
class Test
{
    public void Foo(object[] obj)
    {
        $obj.Select(q => q as Test).First(q => q != null)$;
    }
}", @"using System.Linq;
class Test
{
    public void Foo(object[] obj)
    {
        obj.OfType<Test>().First();
    }
}");
        }

        [Test]
        public void TestCaseBasicWithFollowUpExpresison()
        {
            Analyze<ReplaceWithOfTypeFirstAnalyzer>(@"using System.Linq;
class Test
{
    public void Foo(object[] obj)
    {
        $obj.Select(q => q as Test).First(q => q != null && Foo(q))$;
    }
}", @"using System.Linq;
class Test
{
    public void Foo(object[] obj)
    {
        obj.OfType<Test>().First(q => Foo(q));
    }
}");
        }

        [Test]
        public void TestDisable()
        {
            Analyze<ReplaceWithOfTypeFirstAnalyzer>(@"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
#pragma warning disable " + CSharpDiagnosticIDs.ReplaceWithOfTypeFirstAnalyzerID + @"
		obj.Select (q => q as Test).First (q => q != null);
	}
}");
        }

        [Test]
        public void TestJunk()
        {
            Analyze<ReplaceWithOfTypeFirstAnalyzer>(@"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
		obj.Select (x => q as Test).First (q => q != null);
	}
}");
            Analyze<ReplaceWithOfTypeFirstAnalyzer>(@"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
		obj.Select (q => q as Test).First (q => 1 != null);
	}
}");

        }

    }
}

