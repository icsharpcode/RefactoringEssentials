using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class StaticEventSubscriptionTests : CSharpDiagnosticTestBase
    {
        [Test]

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

        [Test]
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


        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

