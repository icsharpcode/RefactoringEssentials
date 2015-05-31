using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class EventUnsubscriptionViaAnonymousDelegateTests : CSharpDiagnosticTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
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

