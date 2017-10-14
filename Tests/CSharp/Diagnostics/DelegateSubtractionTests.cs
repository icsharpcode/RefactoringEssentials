using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class DelegateSubtractionTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestSubtraction()
        {
            Analyze<DelegateSubtractionAnalyzer>(@"
using System;
class Foo
{
	void Bar (Action a, Action b)
	{
		($a - b$) ();
	}
}
");
        }

        [Fact]
        public void TestAssignmentSubtraction()
        {
            Analyze<DelegateSubtractionAnalyzer>(@"
using System;
class Foo
{
	void Bar (Action a, Action b)
	{
		$a -= b$;
	}
}
");
        }

        [Fact]
        public void TestDisable()
        {
            Analyze<DelegateSubtractionAnalyzer>(@"
using System;
class Foo
{
	void Bar (Action a, Action b)
	{
#pragma warning disable " + CSharpDiagnosticIDs.DelegateSubtractionAnalyzerID + @"
		(a - b) ();
	}
}
");
        }

        /// <summary>
        /// Bug 18061 - Incorrect "delegate subtraction has unpredictable result" warning
        /// </summary>
        [Fact]
        public void TestBug18061()
        {
            Analyze<DelegateSubtractionAnalyzer>(@"
using System;
class Test
{
	public event EventHandler Foo;

	void Bar (EventHandler bar)
	{
		Foo -= bar;
	}
}
");
        }

        [Fact]
        public void TestDoNotShowOnResolveError()
        {
            Analyze<DelegateSubtractionAnalyzer>(@"
using System;
class Test
{
	void Bar (EventHandler bar)
	{
		NotDefined -= bar;
	}
}
");
        }

    }
}

