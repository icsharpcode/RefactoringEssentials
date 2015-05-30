using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class StaticMethodInvocationToExtensionMethodInvocationTests : InspectionActionTestBase
    {

        [Test]
        public void HandlesBasicCase()
        {
            Analyze<InvokeAsExtensionMethodAnalyzer>(@"
class A { }
static class B
{
	public static bool Ext (this A a, int i);
}
class C
{
	void F()
	{
		A a = new A();
		B.$Ext$(a, 1);
	}
}", @"
class A { }
static class B
{
	public static bool Ext (this A a, int i);
}
class C
{
	void F()
	{
		A a = new A();
		a.Ext(1);
	}
}");
        }

        [Test]
        public void HandlesBasicCaseWithComment()
        {
            Analyze<InvokeAsExtensionMethodAnalyzer>(@"
class A { }
static class B
{
	public static bool Ext (this A a, int i);
}
class C
{
	void F()
	{
		A a = new A();
		// Some comment
		B.$Ext$(a, 1);
	}
}", @"
class A { }
static class B
{
	public static bool Ext (this A a, int i);
}
class C
{
	void F()
	{
		A a = new A();
		// Some comment
		a.Ext(1);
	}
}");
        }

        [Test]
        public void HandlesReturnValueUsage()
        {
            Analyze<InvokeAsExtensionMethodAnalyzer>(@"
class A { }
static class B
{
	public static void Ext (this A a, int i);
}
class C
{
	void F()
	{
		A a = new A();
		if (B.$Ext$ (a, 1))
			return;
	}
}", @"
class A { }
static class B
{
	public static void Ext (this A a, int i);
}
class C
{
	void F()
	{
		A a = new A();
		if (a.Ext (1))
			return;
	}
}");
        }

        [Test]
        public void IgnoresIfNullArgument()
        {
            Analyze<InvokeAsExtensionMethodAnalyzer>(@"
class A { }
static class B
{
	public static void Ext (this A a);
}
class C
{
	void F()
	{
		B.Ext(null);
	}
}");
        }

        [Test]
        public void IgnoresIfNotExtensionMethod()
        {
            Analyze<InvokeAsExtensionMethodAnalyzer>(@"
class A { }
static class B
{
	public static void Ext (A a);
}
class C
{
	void F()
	{
		B.Ext (new A());
	}
}");
        }

        [Test]
        public void IgnoresIfAlreadyExtensionMethodCallSyntax()
        {
            Analyze<InvokeAsExtensionMethodAnalyzer>(@"
class A { }
static class B
{
	public static void Ext (this A a, int i);
}
class C
{
	void F()
	{
		A a = new A();
		a.Ext (1);
	}
}");
        }

        [Test]
        public void IgnoresPropertyInvocation()
        {
            Analyze<InvokeAsExtensionMethodAnalyzer>(@"
static class B
{
	public static int Ext { get; set; }
}
class C
{
	void F()
	{
		B.Ext();
	}
}");
        }


        [Test]
        public void TestDisable()
        {
            Analyze<InvokeAsExtensionMethodAnalyzer>(@"
class A { }
static class B
{
	public static bool Ext (this A a, int i);
}
class C
{
	void F()
	{
		A a = new A();
#pragma warning disable " + DiagnosticIDs.InvokeAsExtensionMethodAnalyzerID + @"
		B.Ext (a, 1);
	}
}");
        }
    }
}

