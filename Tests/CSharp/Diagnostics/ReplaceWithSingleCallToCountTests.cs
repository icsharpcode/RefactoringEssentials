using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class ReplaceWithSingleCallToCountTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestSimpleCase()
        {
            Analyze<ReplaceWithSingleCallToCountAnalyzer>(@"using System.Linq;
public class CSharpDemo {
    public void Bla () {
        int[] arr;
        var bla = $arr.Where(x => x < 4).Count()$;
    }
}", @"using System.Linq;
public class CSharpDemo {
    public void Bla () {
        int[] arr;
        var bla = arr.Count(x => x < 4);
    }
}");
        }

        [Test]
        public void TestDisable()
        {
            Analyze<ReplaceWithSingleCallToCountAnalyzer>(@"using System.Linq;
public class CSharpDemo {
	public void Bla () {
		int[] arr;
#pragma warning disable " + CSharpDiagnosticIDs.ReplaceWithSingleCallToCountAnalyzerID + @"
		var bla = arr.Where (x => x < 4).Count ();
	}
}");
        }
    }
}
