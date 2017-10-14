using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class MemberHidesStaticFromOuterClassTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestProperty()
        {
            Analyze<MemberHidesStaticFromOuterClassAnalyzer>(@"
public class Foo
{
	public class Bar
	{
		public string $Test$ { get; set; }
	}

	public static string Test { get; set; }
}
");
        }


        [Fact]
        public void TestDisable()
        {
            Analyze<MemberHidesStaticFromOuterClassAnalyzer>(@"
public class Foo
{
	public class Bar
	{
#pragma warning disable " + CSharpDiagnosticIDs.MemberHidesStaticFromOuterClassAnalyzerID + @"
		public string Test { get; set; }
	}

	public static string Test { get; set; }
}
");
        }


    }
}

