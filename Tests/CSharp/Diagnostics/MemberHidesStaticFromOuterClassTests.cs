using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class MemberHidesStaticFromOuterClassTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestSimpleCase()
        {
            TestIssue<MemberHidesStaticFromOuterClassAnalyzer>(@"
public class Foo
{
	public class Bar
	{
		public string Test { get; set; }
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
		// ReSharper disable once MemberHidesStaticFromOuterClass
		public string Test { get; set; }
	}

	public static string Test { get; set; }
}
");
        }


    }
}

