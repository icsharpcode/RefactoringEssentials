using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;
using System;

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

        void olcol(object sender, EventArgs e)
        {
            throw new NotImplementedException();
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
#pragma warning disable " + CSharpDiagnosticIDs.RedundantDelegateCreationAnalyzerID + @"
		Changed += new EventHandler<EventArgs>(HandleChanged);
	}

	void HandleChanged(object sender, EventArgs e)
	{
	}
}");
        }

        [Test]
        public void TestBug33763()
        {
            var input = @"
public class Foo
    {
        Foo foo;

        public Foo FooBar {
            get {
                foo = new Foo(foo);
                return foo;
            }
        }
        public Foo(Foo f) { }
    }
";
            Analyze<RedundantDelegateCreationAnalyzer>(input);
        }

    }
}