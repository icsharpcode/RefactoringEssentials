using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class ReplaceWithSingleCallToLastOrDefaultTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestSimpleCase()
        {
            Analyze<ReplaceWithSingleCallToLastOrDefaultAnalyzer>(@"using System.Linq;
public class CSharpDemo {
    public void Bla () {
        int[] arr;
        var bla = $arr.Where(x => x < 4).LastOrDefault()$;
    }
}", @"using System.Linq;
public class CSharpDemo {
    public void Bla () {
        int[] arr;
        var bla = arr.LastOrDefault(x => x < 4);
    }
}");
        }

        [Fact]
        public void TestDisable()
        {
            Analyze<ReplaceWithSingleCallToLastOrDefaultAnalyzer>(@"using System.Linq;
public class CSharpDemo {
	public void Bla () {
		int[] arr;
#pragma warning disable " + CSharpDiagnosticIDs.ReplaceWithSingleCallToLastOrDefaultAnalyzerID + @"
		var bla = arr.Where (x => x < 4).LastOrDefault ();
	}
}");
        }
    }
}
