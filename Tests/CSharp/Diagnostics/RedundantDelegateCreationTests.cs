using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class RedundantDelegateCreationTests : InspectionActionTestBase
    {
        [Test]
        public void TestAdd()
        {
            Test<RedundantDelegateCreationAnalyzer>(@"
using System;

public class FooBase
{
	public event EventHandler<EventArgs> Changed;

	FooBase()
	{
		Changed += new EventHandler<EventArgs>(HandleChanged);
	}

	void HandleChanged(object sender, EventArgs e)
	{
	}
}", @"
using System;

public class FooBase
{
	public event EventHandler<EventArgs> Changed;

	FooBase()
	{
		Changed += HandleChanged;
	}

	void HandleChanged(object sender, EventArgs e)
	{
	}
}");
        }

        [Test]
        public void TestRemove()
        {
            Test<RedundantDelegateCreationAnalyzer>(@"
using System;

public class FooBase
{
	public event EventHandler<EventArgs> Changed;

	FooBase()
	{
		Changed -= new EventHandler<EventArgs>(HandleChanged);
	}

	void HandleChanged(object sender, EventArgs e)
	{
	}
}", @"
using System;

public class FooBase
{
	public event EventHandler<EventArgs> Changed;

	FooBase()
	{
		Changed -= HandleChanged;
	}

	void HandleChanged(object sender, EventArgs e)
	{
	}
}");
        }

        [Test]
        public void TestDisable()
        {
            Analyze<RedundantDelegateCreationAnalyzer>(@"
using System;

public class FooBase
{
	public event EventHandler<EventArgs> Changed;

	FooBase()
	{
		// ReSharper disable once RedundantDelegateCreation
		Changed += new EventHandler<EventArgs>(HandleChanged);
	}

	void HandleChanged(object sender, EventArgs e)
	{
	}
}");
        }
    }
}

