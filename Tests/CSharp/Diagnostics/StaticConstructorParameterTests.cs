namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
	public class StaticConstructorParameterTests : CSharpDiagnosticTestBase
    {
		//		[Fact(Skip="Should be a code fix")]
		//		public void TestSimpleCase()
		//		{
		//			Analyze<StaticConstructorParameterAnalyzer>(@"
		//class Foo
		//{
		//	static $Foo$(int bar)
		//	{
		//	}
		//}
		//", @"
		//class Foo
		//{
		//	static Foo()
		//	{
		//	}
		//}
		//");
		//		}

		//		[Fact(Skip="Should be a code fix")]
		//		public void TestNoIssue()
		//		{
		//			Analyze<StaticConstructorParameterAnalyzer>(@"
		//class Foo
		//{
		//	static Foo ()
		//	{
		//	}
		//}
		//");
		//		}

	}
}

