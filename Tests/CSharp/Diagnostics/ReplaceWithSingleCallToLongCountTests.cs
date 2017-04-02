using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class ReplaceWithSingleCallToLongCountTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestSimpleCase()
        {
            Analyze<ReplaceWithSingleCallToLongCountAnalyzer>(@"using System.Linq;
public class CSharpDemo {
    public void Bla () {
        int[] arr;
        var bla = $arr.Where(x => x < 4).LongCount()$;
    }
}", @"using System.Linq;
public class CSharpDemo {
    public void Bla () {
        int[] arr;
        var bla = arr.LongCount(x => x < 4);
    }
}");
        }

        [Fact]
        public void TestDisable()
        {
            Analyze<ReplaceWithSingleCallToLongCountAnalyzer>(@"using System.Linq;
public class CSharpDemo {
	public void Bla () {
		int[] arr;
#pragma warning disable " + CSharpDiagnosticIDs.ReplaceWithSingleCallToLongCountAnalyzerID + @"
		var bla = arr.Where (x => x < 4).LongCount ();
	}
}");
        }
    }
}
