using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class ReplaceWithOfTypeTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestCaseSelectWhereCase1()
        {
            Analyze<ReplaceWithOfTypeAnalyzer>(@"using System.Linq;
class Test
{
    public void Foo(object[] obj)
    {
        $obj.Where(o => o is Test).Select(o => o as Test)$;
    }
}", @"using System.Linq;
class Test
{
    public void Foo(object[] obj)
    {
        obj.OfType<Test>();
    }
}");
        }

        [Fact]
        public void TestCaseSelectWhereGarbage()
        {
            Analyze<ReplaceWithOfTypeAnalyzer>(@"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
		obj.Where(o => o is Test).Select (x => o as Test);
	}
}");
            Analyze<ReplaceWithOfTypeAnalyzer>(@"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
		obj.SelectNotNull(o => null as Test);
	}
}");
        }


        [Fact]
        public void TestCaseSelectWhereCase2WithParens()
        {
            Analyze<ReplaceWithOfTypeAnalyzer>(@"using System.Linq;
class Test
{
    public void Foo(object[] obj)
    {
        $obj.Where(o => (o is Test)).Select (o => ((Test)(o)))$;
    }
}", @"using System.Linq;
class Test
{
    public void Foo(object[] obj)
    {
        obj.OfType<Test>();
    }
}");

            //Does not make sense!
            //			Analyze<ReplaceWithOfTypeAnalyzer>(@"using System.Linq;
            //class Test
            //{
            //	public void Foo(object[] obj)
            //	{
            //		obj.SelectNotNull(o => o as Test);
            //	}
            //}", @"using System.Linq;
            //class Test
            //{
            //	public void Foo(object[] obj)
            //	{
            //		obj.OfType<Test> ();
            //	}
            //}");
        }



        [Fact]
        public void TestDisable()
        {
            Analyze<ReplaceWithOfTypeAnalyzer>(@"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
#pragma warning disable " + CSharpDiagnosticIDs.ReplaceWithOfTypeAnalyzerID + @"
		obj.Where(o => o is Test).Select (o => o as Test);
	}
}");
        }



    }

}

