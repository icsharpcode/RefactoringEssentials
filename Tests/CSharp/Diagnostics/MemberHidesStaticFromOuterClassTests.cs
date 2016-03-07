using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class MemberHidesStaticFromOuterClassTests : CSharpDiagnosticTestBase
    {
        [Test]
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


        [Test]
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

