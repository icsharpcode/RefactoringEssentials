using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{

    public class PolymorphicFieldLikeEventInvocationTests : CSharpDiagnosticTestBase
    {
        [Fact(Skip="TODO: Issue not ported yet")]
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

        [Fact(Skip="TODO: Issue not ported yet")]
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

        [Fact(Skip="TODO: Issue not ported yet")]
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

