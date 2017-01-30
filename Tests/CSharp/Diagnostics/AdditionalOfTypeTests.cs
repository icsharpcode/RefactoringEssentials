using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class AdditionalOfTypeTests : CSharpDiagnosticTestBase
    {
        [Fact(Skip="TODO - AST pattern machting!")]
        public void TestAdditionalCase()
        {
            Analyze<AdditionalOfTypeAnalyzer>(@"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
		$obj.Where(o => (o is Test))$;
	}
}", @"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
		obj.OfType<Test> ();
	}
}");
        }

		[Fact(Skip = "TODO - AST pattern machting!")]
		public void TestInvalid()
        {
            Analyze<AdditionalOfTypeAnalyzer>(@"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
		obj.OfType<IDisposable> ().Where(o => this is IDisposable);
	}
}");
        }
    }
}