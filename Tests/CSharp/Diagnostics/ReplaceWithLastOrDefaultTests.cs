using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class ReplaceWithLastOrDefaultTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestBasicCase()
        {
            Analyze<ReplaceWithLastOrDefaultAnalyzer>(@"using System.Linq;
class Bar
{
	public void FooBar(string[] args)
	{
		var first = $args.Any() ? args.Last() : null$;
	}
}", @"using System.Linq;
class Bar
{
	public void FooBar(string[] args)
	{
		var first = args.LastOrDefault();
	}
}");
        }

        [Fact]
        public void TestBasicCaseWithExpression()
        {
            Analyze<ReplaceWithLastOrDefaultAnalyzer>(@"using System.Linq;
class Bar
{
    public void FooBar(string[] args)
    {
        $args.Any(a => a != null) ? args.Last(a => a != null) : null$;
    }
}", @"using System.Linq;
class Bar
{
    public void FooBar(string[] args)
    {
        args.LastOrDefault(a => a != null);
    }
}");
        }

        [Fact]
        public void TestBasicCaseWithDefault()
        {
            Analyze<ReplaceWithLastOrDefaultAnalyzer>(@"using System.Linq;
class Bar
{
	public void FooBar<T>(T[] args)
	{
		var first = $args.Any() ? args.Last() : default(T)$;
	}
}", @"using System.Linq;
class Bar
{
	public void FooBar<T>(T[] args)
	{
		var first = args.LastOrDefault();
	}
}");
        }

        [Fact]
        public void TestDisable()
        {
            Analyze<ReplaceWithLastOrDefaultAnalyzer>(@"using System.Linq;
class Bar
{
	public void FooBar(string[] args)
	{
#pragma warning disable " + CSharpDiagnosticIDs.ReplaceWithLastOrDefaultAnalyzerID + @"
		var first = args.Any() ? args.Last() : null;
	}
}");
        }
    }
}

