using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class AccessToStaticMemberViaDerivedTypeTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void MemberInvocation()
        {
            Analyze<AccessToStaticMemberViaDerivedTypeAnalyzer>(@"
class A
{
	public static void F() { }
}
class B : A { }
class C
{
	void Main()
	{
		$B$.F ();
	}
}", @"
class A
{
	public static void F() { }
}
class B : A { }
class C
{
	void Main()
	{
		A.F ();
	}
}"
            );
        }

        [Test]
        public void MemberInvocationWithComment()
        {
            Analyze<AccessToStaticMemberViaDerivedTypeAnalyzer>(@"
class A
{
	public static void F() { }
}
class B : A { }
class C
{
	void Main()
	{
		// Some comment
		$B$.F ();
	}
}", @"
class A
{
	public static void F() { }
}
class B : A { }
class C
{
	void Main()
	{
		// Some comment
		A.F ();
	}
}"
            );
        }

        [Test]
        public void TestDisable()
        {
            Analyze<AccessToStaticMemberViaDerivedTypeAnalyzer>(@"
class A
{
	public static void F() { }
}
class B : A { }
class C
{
	void Main()
	{
#pragma warning disable " + CSharpDiagnosticIDs.AccessToStaticMemberViaDerivedTypeAnalyzerID + @"
		B.F ();
	}
}");
        }

        [Test]
        public void PropertyAccess()
        {
            Analyze<AccessToStaticMemberViaDerivedTypeAnalyzer>(@"
class A
{
	public static string Property { get; set; }
}
class B : A { }
class C
{
	void Main()
	{
		System.Console.WriteLine($B$.Property);
	}
}", @"
class A
{
	public static string Property { get; set; }
}
class B : A { }
class C
{
	void Main()
	{
		System.Console.WriteLine(A.Property);
	}
}"
            );
        }

        [Test]
        public void FieldAccess()
        {
            Analyze<AccessToStaticMemberViaDerivedTypeAnalyzer>(@"class A
{
	public static string Property;
}
class B : A { }
class C
{
	void Main()
	{
		System.Console.WriteLine($B$.Property);
	}
}", @"class A
{
	public static string Property;
}
class B : A { }
class C
{
	void Main()
	{
		System.Console.WriteLine(A.Property);
	}
}"
            );
        }

        [Test]
        public void NestedClass()
        {
            Analyze<AccessToStaticMemberViaDerivedTypeAnalyzer>(@"
class A
{
	public class B
	{
		public static void F() { }
	}
	public class C : B { }
}
class D
{
	void Main()
	{
		$A.C$.F ();
	}
}", @"
class A
{
	public class B
	{
		public static void F() { }
	}
	public class C : B { }
}
class D
{
	void Main()
	{
		A.B.F ();
	}
}"
            );
        }

        [Test]
        public void ExpandsTypeWithNamespaceIfNeccessary()
        {
            Analyze<AccessToStaticMemberViaDerivedTypeAnalyzer>(@"namespace First
{
	class A
	{
		public static void F() { }
	}
}
namespace Second
{
	public class B : First.A { }
	class C
	{
		void Main()
		{
			$B$.F ();
		}
	}
}", @"namespace First
{
	class A
	{
		public static void F() { }
	}
}
namespace Second
{
	public class B : First.A { }
	class C
	{
		void Main()
		{
			First.A.F ();
		}
	}
}"
            );
        }

        [Test]
        public void IgnoresCorrectCalls()
        {
            Analyze<AccessToStaticMemberViaDerivedTypeAnalyzer>(@"
class A
{
	public static void F() { }
}
class B
{
	void Main()
	{
		A.F();
	}
}");
        }

        [Test]
        public void IgnoresNonStaticCalls()
        {
            Analyze<AccessToStaticMemberViaDerivedTypeAnalyzer>(@"
class A
{
	public void F() { }
}
class B : A { }
class C
{
	void Main()
	{
		B b = new B();
		b.F();
	}
}");
        }

        [Test]
        public void IgnoresOwnMemberFunctions()
        {
            Analyze<AccessToStaticMemberViaDerivedTypeAnalyzer>(@"
class A
{
	protected static void F() { }
}
class B : A
{
	void Main()
	{
		F();
		this.F();
		base.F();
	}
}");
        }

        [Test]
        public void IgnoresCuriouslyRecurringTemplatePattern()
        {
            Analyze<AccessToStaticMemberViaDerivedTypeAnalyzer>(@"
class Base<T>
{
	public static void F() { }
}
class Derived : Base<Derived> {}
class Test
{
	void Main()
	{
		Derived.F();
	}
}");
        }
    }
}

