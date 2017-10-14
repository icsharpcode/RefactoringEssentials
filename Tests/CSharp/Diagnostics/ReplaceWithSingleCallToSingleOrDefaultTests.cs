using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class ReplaceWithSingleCallToSingleOrDefaultTests : CSharpDiagnosticTestBase
    {
        [Fact]
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

        [Fact]
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
