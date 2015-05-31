using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class CS0618UsageOfObsoleteMemberTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestObsoleteMethodUsage()
        {
            var input = @"
using System;

public class Foo
{
	[Obsolete (""Use NewBar"")]
	public static void OldBar ()
	{
	}

	public static void NewBar ()
	{
	}
}

class MyClass
{
	public static void Main ()
	{
		Foo.OldBar ();
	}
}
";
            TestIssue<CS0618UsageOfObsoleteMemberAnalyzer>(input);
        }

        [Test]
        public void TestObsoleteProperty()
        {
            var input = @"
using System;

public class Foo
{
	[Obsolete ()]
	public int Bar { get; set; }
}

class MyClass
{
	public static void Main ()
	{
		new Foo().Bar = 5;
	}
}
";
            TestIssue<CS0618UsageOfObsoleteMemberAnalyzer>(input);
        }

        [Test]
        public void TestObsoleteIndexer()
        {
            var input = @"
using System;

public class Foo
{
	[Obsolete ()]
	public int this[int i] {
		get {
			return 0;
		}
	}
}

class MyClass
{

	public static void Main ()
	{
		var f = new Foo ();
		Console.WriteLine (f[0]);
	}
}
";
            TestIssue<CS0618UsageOfObsoleteMemberAnalyzer>(input);
        }

        [Test]
        public void TestObsoleteEvent()
        {
            var input = @"
using System;

public class Foo
{
	[Obsolete ()]
	public event EventHandler A;
}

class MyClass
{

	public static void Main ()
	{
		var f = new Foo ();
		f.A += delegate {

		};
	}
}
";
            TestIssue<CS0618UsageOfObsoleteMemberAnalyzer>(input);
        }

        [Test]
        public void TestObsoleteField()
        {
            var input = @"
using System;

public class Foo
{
	[Obsolete]
	public int A;
}

class MyClass
{

	public static void Main ()
	{
		var f = new Foo ();
		Console.WriteLine (f.A);
	}
}
";
            TestIssue<CS0618UsageOfObsoleteMemberAnalyzer>(input);
        }

        [Test]
        public void TestObsoleteBinaryOperator()
        {
            var input = @"
using System;

public class Foo
{
	[Obsolete]
	public static Foo operator+(Foo l, Foo r)
	{
		return l;
	}
}

class MyClass
{
	public static void Main ()
	{
		var f = new Foo ();
		Console.WriteLine (f + f);
	}
}
";
            TestIssue<CS0618UsageOfObsoleteMemberAnalyzer>(input);
        }


        [Test]
        public void TestPragmaDisable()
        {
            var input = @"
using System;

public class Foo
{
	[Obsolete ()]
	public int Bar { get; set; }
}

class MyClass
{
	public static void Main ()
	{
#pragma warning disable 618
		new Foo().Bar = 5;
#pragma warning restore 618
	}
}
";
            Analyze<CS0618UsageOfObsoleteMemberAnalyzer>(input);
        }


        /// <summary>
        /// Bug 17859 - Source analysis should not warn when obsolete member is used by obsolete member
        /// </summary>
        [Test]
        public void TestBug17859()
        {
            var input = @"
using System;

public class Foo
{
	[Obsolete]
	public static void OldBar ()
	{
	}

	[Obsolete]
	public static void OldBar2 ()
	{
		OldBar ();
	}
}
";
            Analyze<CS0618UsageOfObsoleteMemberAnalyzer>(input);
        }
    }
}

