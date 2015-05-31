using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class ReplaceWithSingleCallToLastTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestSimpleCase()
        {
            Analyze<ReplaceWithSingleCallToLastAnalyzer>(@"using System.Linq;
public class CSharpDemo {
    public void Bla () {
        int[] arr;
        var bla = $arr.Where(x => x < 4).Last()$;
    }
}", @"using System.Linq;
public class CSharpDemo {
    public void Bla () {
        int[] arr;
        var bla = arr.Last(x => x < 4);
    }
}");
        }

        [Test]
        public void TestDisable()
        {
            Analyze<ReplaceWithSingleCallToLastAnalyzer>(@"using System.Linq;
public class CSharpDemo {
	public void Bla () {
		int[] arr;
#pragma warning disable " + CSharpDiagnosticIDs.ReplaceWithSingleCallToLastAnalyzerID + @"
		var bla = arr.Where (x => x < 4).Last ();
	}
}");
        }
    }
}
