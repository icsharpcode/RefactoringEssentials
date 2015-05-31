using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class ReplaceWithSingleCallToSingleOrDefaultTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestSimpleCase()
        {
            Analyze<ReplaceWithSingleCallToSingleOrDefaultAnalyzer>(@"using System.Linq;
public class CSharpDemo {
    public void Bla () {
        int[] arr;
        var bla = $arr.Where(x => x < 4).SingleOrDefault()$;
    }
}", @"using System.Linq;
public class CSharpDemo {
    public void Bla () {
        int[] arr;
        var bla = arr.SingleOrDefault(x => x < 4);
    }
}");
        }

        [Test]
        public void TestDisable()
        {
            Analyze<ReplaceWithSingleCallToSingleOrDefaultAnalyzer>(@"using System.Linq;
public class CSharpDemo {
	public void Bla () {
		int[] arr;
#pragma warning disable " + CSharpDiagnosticIDs.ReplaceWithSingleCallToSingleOrDefaultAnalyzerID + @"
		var bla = arr.Where (x => x < 4).SingleOrDefault ();
	}
}");
        }
    }
}
