using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class RedundantDelegateCreationTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestAdd()
        {
            var input = @"
using System;

public class FooBase
{
	public event EventHandler<EventArgs> Changed;

	FooBase()
	{
		Changed += $new EventHandler<EventArgs>(HandleChanged)$;
	}

	void HandleChanged(object sender, EventArgs e)
	{
	}
}";
            var output = @"
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
}";

            Analyze<RedundantDelegateCreationAnalyzer>(input, output);
        }

        [Test]
        public void TestRemove()
        {
            Analyze<RedundantDelegateCreationAnalyzer>(@"
using System;

public class FooBase
{
	public event EventHandler<EventArgs> Changed;

	FooBase()
	{
		Changed -= $new EventHandler<EventArgs>(HandleChanged)$;
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
#pragma warning disable " + CSharpDiagnosticIDs.RedundantDelegateCreationAnalyzerID + @"
		Changed += new EventHandler<EventArgs>(HandleChanged);
	}

	void HandleChanged(object sender, EventArgs e)
	{
	}
}");
        }
    }
}