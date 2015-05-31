using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [Ignore("TODO - AST pattern machting!")]
    [TestFixture]
    public class AdditionalOfTypeTests : CSharpDiagnosticTestBase
    {
        [Test]
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

        [Test]
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