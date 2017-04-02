using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class StaticEventSubscriptionTests : CSharpDiagnosticTestBase
    {
        [Fact(Skip="TODO: Issue not ported yet")]

        public void TestAnonymousMethodSubscription()
        {
            TestIssue<StaticEventSubscriptionAnalyzer>(@"
using System;

class Foo
{
	public static event EventHandler FooBar;
	
	public void Test ()
	{
		FooBar += delegate { 
			Console.WriteLine (""Hello World!"");
		};
	}
}
");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestAnonymousMethodSubscription_ValidCase()
        {
            Analyze<StaticEventSubscriptionAnalyzer>(@"
using System;

class Foo
{
	public static event EventHandler FooBar;
	
	public static void Test ()
	{
		FooBar += delegate { 
			Console.WriteLine (""Hello World!"");
		};
	}
}
");
        }


        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestIssue()
        {
            TestIssue<StaticEventSubscriptionAnalyzer>(@"
using System;

class Foo
{
	public static event EventHandler FooBar;
	
	public void Test ()
	{
		FooBar += MyMethod;
	}
	void MyMethod (object sender, EventArgs args)
	{
	}
}
");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestNoIssue()
        {
            Analyze<StaticEventSubscriptionAnalyzer>(@"
using System;

class Foo
{
	public static event EventHandler FooBar;
	
	public void Test ()
	{
		FooBar += MyMethod;
	}
	
	public void Unsubscribe ()
	{
		FooBar -= MyMethod;
	}
	void MyMethod (object sender, EventArgs args)
	{
	}
}
");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestNonStatic()
        {
            Analyze<StaticEventSubscriptionAnalyzer>(@"
using System;

class Foo
{
	public event EventHandler FooBar;

	public void Test ()
	{
		FooBar += MyMethod;
	}

	void MyMethod (object sender, EventArgs args)
	{
	}
}
");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestNullAssignment()
        {
            Analyze<StaticEventSubscriptionAnalyzer>(@"
using System;

class Foo
{
	public static event EventHandler FooBar;
	
	public void Test ()
	{
		FooBar += MyMethod;
	}
	
	public void Unsubscribe ()
	{
		FooBar = null;
	}
	void MyMethod (object sender, EventArgs args)
	{
	}
}
");
        }
    }
}

