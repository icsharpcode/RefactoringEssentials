using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class ProhibitedModifiersTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestNonStaticMembersInStaticClass()
        {
            Test<ProhibitedModifiersAnalyzer>(@"
static class Foo
{
	public void Bar () 
	{
	}
}
", @"
static class Foo
{
	public static void Bar () 
	{
	}
}
");
        }

        [Test]
        public void TestValidStaticClass()
        {
            Analyze<ProhibitedModifiersAnalyzer>(@"
static class Foo
{
	public const int f = 1;
	static Foo () {}

	public static void Bar () 
	{
	}
}
");
        }


        [Test]
        public void TestNonStaticFieldsInStaticClass()
        {
            Test<ProhibitedModifiersAnalyzer>(@"
static class Foo
{
	int a, b, c;
}
", 3, @"
static class Foo
{
	static int a, b, c;
}
", 1);
        }

        [Ignore("Code issues don't run with errors atm.")]
        [Test]
        public void TestStaticConstructorWithPublicModifier()
        {
            Test<ProhibitedModifiersAnalyzer>(@"
class Test
{
	static int a;

	public static Test ()
	{
		a = 100;
	}
}", @"
class Test
{
	static int a;

	static Test ()
	{
		a = 100;
	}
}");
        }

        [Test]
        public void TestVirtualMemberInSealedClass()
        {
            Test<ProhibitedModifiersAnalyzer>(@"
sealed class Test
{
	public virtual void FooBar ()
	{
	}
}", @"
sealed class Test
{
	public void FooBar ()
	{
	}
}");
        }


        [Test]
        public void TestPrivateVirtualMembers()
        {
            TestIssue<ProhibitedModifiersAnalyzer>(@"
class Foo
{
	virtual void Bar () 
	{
	}
}
");
        }


        [Test]
        public void TestSealed()
        {
            Test<ProhibitedModifiersAnalyzer>(@"
class Foo
{
	public sealed void Bar () 
	{
	}
}
", @"
class Foo
{
	public void Bar () 
	{
	}
}
");
        }

        [Test]
        public void TestValidSealed()
        {
            Analyze<ProhibitedModifiersAnalyzer>(@"
class Foo
{
	public override sealed void Bar () 
	{
	}
}
");
        }


        [Test]
        public void TestExtensionMethodInNonStaticClass()
        {
            Test<ProhibitedModifiersAnalyzer>(@"
class Foo
{
	public static void Bar (this int i) 
	{
	}
}
", @"
static class Foo
{
	public static void Bar (this int i) 
	{
	}
}
");
        }

        [Test]
        public void TestExtensionMethodInNonStaticClassFix2()
        {
            Test<ProhibitedModifiersAnalyzer>(@"
class Foo
{
	public static void Bar (this int i) 
	{
	}
}
", @"
class Foo
{
	public static void Bar (int i) 
	{
	}
}
", 1);
        }

    }
}

