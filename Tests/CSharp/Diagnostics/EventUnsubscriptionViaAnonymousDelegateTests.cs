using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class EventUnsubscriptionViaAnonymousDelegateTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestDelegateCase()
        {
            Analyze<EventUnsubscriptionViaAnonymousDelegateAnalyzer>(@"using System;

class Bar
{
	public event EventHandler Foo;

	void Test ()
	{
		Foo $-=$ delegate { };
	}
}");
        }

        [Fact]
        public void TestLambdaCase()
        {
            Analyze<EventUnsubscriptionViaAnonymousDelegateAnalyzer>(@"using System;

class Bar
{
	public event EventHandler Foo;

	void Test ()
	{
		Foo $-=$ (s ,e) => { };
	}
}");
        }

        [Fact]
        public void TestDisable()
        {
            Analyze<EventUnsubscriptionViaAnonymousDelegateAnalyzer>(@"using System;

class Bar
{
	public event EventHandler Foo;

	void Test ()
	{
#pragma warning disable " + CSharpDiagnosticIDs.EventUnsubscriptionViaAnonymousDelegateAnalyzerID + @"
		Foo -= delegate { };
	}
}");
        }


    }
}

