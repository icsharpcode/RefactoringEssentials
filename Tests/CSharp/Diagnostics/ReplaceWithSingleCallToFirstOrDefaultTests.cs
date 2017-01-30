using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class ReplaceWithSingleCallToFirstOrDefaultTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestSimpleCase()
        {
            Analyze<ReplaceWithSingleCallToFirstOrDefaultAnalyzer>(@"using System.Linq;
public class CSharpDemo {
    public void Bla () {
        int[] arr;
        var bla = $arr.Where(x => x < 4).FirstOrDefault()$;
    }
}", @"using System.Linq;
public class CSharpDemo {
    public void Bla () {
        int[] arr;
        var bla = arr.FirstOrDefault(x => x < 4);
    }
}");
        }

        [Fact]
        public void TestDisable()
        {
            Analyze<ReplaceWithSingleCallToFirstOrDefaultAnalyzer>(@"using System.Linq;
public class CSharpDemo {
	public void Bla () {
		int[] arr;
#pragma warning disable " + CSharpDiagnosticIDs.ReplaceWithSingleCallToFirstOrDefaultAnalyzerID + @"
		var bla = arr.Where (x => x < 4).FirstOrDefault ();
	}
}");
        }
    }
}
