using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{

    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class PolymorphicFieldLikeEventInvocationTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestSimpleCase()
        {
            TestIssue<PolymorphicFieldLikeEventInvocationAnalyzer>(@"
using System;

public class Bar
{
	public virtual event EventHandler FooBarEvent;
}

public class Foo : Bar
{
	public override event EventHandler FooBarEvent;

	public void FooBar()
	{
		FooBarEvent(this, EventArgs.Empty);
	}
}
");
        }

        [Test]
        public void TestCustomEvent()
        {
            // Should be marked as error
            TestIssue<PolymorphicFieldLikeEventInvocationAnalyzer>(@"
using System;

public class Bar
{
	public virtual event EventHandler FooBarEvent;
}

public class Foo : Bar
{
	public override event EventHandler FooBarEvent {
		add {}
		remove {}
	}

	public void FooBar()
	{
		FooBarEvent(this, EventArgs.Empty);
	}
}
");
        }

        [Test]
        public void TestDisable()
        {
            Analyze<PolymorphicFieldLikeEventInvocationAnalyzer>(@"
using System;

public class Bar
{
	public virtual event EventHandler FooBarEvent;
}

public class Foo : Bar
{
	public override event EventHandler FooBarEvent;

	public void FooBar()
	{
		// ReSharper disable once PolymorphicFieldLikeEventInvocation
		FooBarEvent(this, EventArgs.Empty);
	}
}
");
        }

    }
}

