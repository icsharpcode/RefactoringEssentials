using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class ReplaceWithSingleCallToLongCountTests : CSharpDiagnosticTestBase
    {
        [Test]
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

        [Test]
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
