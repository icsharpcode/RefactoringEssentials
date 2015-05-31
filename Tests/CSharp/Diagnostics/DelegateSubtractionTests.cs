using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class DelegateSubtractionTests : CSharpDiagnosticTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
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
        [Test]
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

        [Test]
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

