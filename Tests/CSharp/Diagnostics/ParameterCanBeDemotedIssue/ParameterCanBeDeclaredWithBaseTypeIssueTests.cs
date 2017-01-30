using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class ParameterCanBeDeclaredWithBaseTypeTests : CSharpDiagnosticTestBase
    {
        [Fact(Skip="TODO: Issue not ported yet")]
        public void BasicTest()
        {
            Test<ParameterCanBeDeclaredWithBaseTypeAnalyzer>(@"
class A
{
	public virtual void Foo() {}
}
class B : A
{
	public virtual void Bar() {}
}
class C
{
	void F(B b)
	{
		b.Foo();
	}
}", @"
class A
{
	public virtual void Foo() {}
}
class B : A
{
	public virtual void Bar() {}
}
class C
{
	void F(A b)
	{
		b.Foo();
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void IgnoresUnusedParameters()
        {
            Analyze<ParameterCanBeDeclaredWithBaseTypeAnalyzer>(@"
class A
{
	void F(A a1)
	{
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void IgnoresDirectionalParameters()
        {
            Analyze<ParameterCanBeDeclaredWithBaseTypeAnalyzer>(@"
interface IA
{
}
class A : IA
{
	void F(out A a1)
	{
		object.Equals(a1, null);
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void IgnoresOverrides()
        {
            Analyze<ParameterCanBeDeclaredWithBaseTypeAnalyzer>(@"
interface IA
{
	void Foo();
}
class B : IA
{
	public virtual void Foo() {}
	public virtual void Bar() {}
}
class TestBase
{
	public void F(B b) {
		b.Foo();
		b.Bar();
	}
}
class TestClass : TestBase
{
	public override void F(B b)
	{
		b.Foo();
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void IgnoresOverridables()
        {
            Analyze<ParameterCanBeDeclaredWithBaseTypeAnalyzer>(@"
interface IA
{
	void Foo();
}
class B : IA
{
	public virtual void Foo() {}
	public virtual void Bar() {}
}
class TestClass
{
	public virtual void F(B b)
	{
		b.Foo();
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void HandlesNeededProperties()
        {
            Analyze<ParameterCanBeDeclaredWithBaseTypeAnalyzer>(@"
interface IA
{
	void Foo(string s);
}
class B : IA
{
	public virtual void Foo(string s) {}
	public string Property { get; }
}
class TestClass
{
	public void F(B b)
	{
		b.Foo(b.Property);
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void InterfaceTest()
        {
            Test<ParameterCanBeDeclaredWithBaseTypeAnalyzer>(@"
interface IA
{
	void Foo();
}
class B : IA
{
	public virtual void Foo() {}
	public virtual void Bar() {}
}
class C
{
	void F(B b)
	{
		b.Foo();
	}
}", @"
interface IA
{
	void Foo();
}
class B : IA
{
	public virtual void Foo() {}
	public virtual void Bar() {}
}
class C
{
	void F(IA b)
	{
		b.Foo();
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void RespectsExpectedTypeInIfStatement()
        {
            Analyze<ParameterCanBeDeclaredWithBaseTypeAnalyzer>(@"
class C
{
	void F (bool b, bool c)
	{
		if (b && c)
			return;
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void MultipleInterfaceTest()
        {
            Test<ParameterCanBeDeclaredWithBaseTypeAnalyzer>(@"
interface IA1
{
	void Foo();
}
interface IA2
{
	void Bar();
}
class B : IA1, IA2
{
	public virtual void Foo() {}
	public virtual void Bar() {}
}
class C : B {}
class Test
{
	void F(C c)
	{
		c.Foo();
		c.Bar();
	}
}", @"
interface IA1
{
	void Foo();
}
interface IA2
{
	void Bar();
}
class B : IA1, IA2
{
	public virtual void Foo() {}
	public virtual void Bar() {}
}
class C : B {}
class Test
{
	void F(B c)
	{
		c.Foo();
		c.Bar();
	}
}");
        }

        string baseInput = @"
interface IA
{
	void Foo();
}
interface IB : IA
{
	void Bar();
}
interface IC : IA
{
	new void Foo();
	void Baz();
}
class D : IB
{
	public void Foo() {}
	public void Bar() {}
}
class E : D, IC
{
	public void Baz() {}
	void IC.Foo() {}
}";

        [Fact(Skip="TODO: Issue not ported yet")]
        public void FindsTopInterface()
        {
            Test<ParameterCanBeDeclaredWithBaseTypeAnalyzer>(baseInput + @"
class Test
{
	void F(E e)
	{
		e.Foo();
	}
}", baseInput + @"
class Test
{
	void F(IA e)
	{
		e.Foo();
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void DoesNotChangeOverload()
        {
            Analyze<ParameterCanBeDeclaredWithBaseTypeAnalyzer>(baseInput + @"
class Test
{
	void F(IB b)
	{
		Bar (b);
	}
	
	void Bar (IA a)
	{
	}

	void Bar (IB b)
	{
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void AssignmentToExplicitlyTypedVariable()
        {
            Analyze<ParameterCanBeDeclaredWithBaseTypeAnalyzer>(baseInput + @"
class Test
{
	void F(IB b)
	{
		IB b2;
		b2 = b;
		object.Equals(b, b2);
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void GenericMethod()
        {
            Analyze<ParameterCanBeDeclaredWithBaseTypeAnalyzer>(baseInput + @"
class Test
{
	void F(IB b)
	{
		Generic (b);
	}

	void Generic<T> (T arg) where T : IA
	{
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void VariableDeclarationWithTypeInference()
        {
            Analyze<ParameterCanBeDeclaredWithBaseTypeAnalyzer>(baseInput + @"
class Test
{
	void Foo (IB b)
	{
		var b2 = b;
		Foo (b2);
	}

	void Foo (IA a)
	{
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void RespectsOutgoingCallsTypeRestrictions()
        {
            Test<ParameterCanBeDeclaredWithBaseTypeAnalyzer>(baseInput + @"
class Test
{
	void F(E e)
	{
		e.Foo();
		DemandType(e);
	}

	void DemandType(D d)
	{
	}
}", baseInput + @"
class Test
{
	void F(D e)
	{
		e.Foo();
		DemandType(e);
	}

	void DemandType(D d)
	{
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void AccountsForNonInvocationMethodGroupUsageInMethodCall()
        {
            Analyze<ParameterCanBeDeclaredWithBaseTypeAnalyzer>(@"
delegate void FooDelegate (string s);
interface IBase
{
	void Bar();
}
interface IDerived : IBase
{
	void Foo(string s);
}
class TestClass
{
	public void Bar (IDerived derived)
	{
		derived.Bar();
		Baz (derived.Foo);
	}

	void Baz (FooDelegate fd)
	{
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void AccountsForNonInvocationMethodGroupUsageInVariableDeclaration()
        {
            Analyze<ParameterCanBeDeclaredWithBaseTypeAnalyzer>(@"
delegate void FooDelegate (string s);
interface IBase
{
	void Bar();
}
interface IDerived : IBase
{
	void Foo(string s);
}
class TestClass
{
	public void Bar (IDerived derived)
	{
		derived.Bar();
		FooDelegate d = derived.Foo;
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void AccountsForNonInvocationMethodGroupUsageInAssignmentExpression()
        {
            Analyze<ParameterCanBeDeclaredWithBaseTypeAnalyzer>(@"
delegate void FooDelegate (string s);
interface IBase
{
	void Bar();
}
interface IDerived : IBase
{
	void Foo(string s);
}
class TestClass
{
	public void Bar (IDerived derived)
	{
		derived.Bar();
		FooDelegate d;
		d = derived.Foo;
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void AccountsForIndexers()
        {
            Test<ParameterCanBeDeclaredWithBaseTypeAnalyzer>(@"
class TestClass
{
	void Write(string[] s)
	{
		object.Equals(s, s);
		var element = s[1];
	}
}", 1, @"
class TestClass
{
	void Write(System.Collections.Generic.IList<string> s)
	{
		object.Equals(s, s);
		var element = s[1];
	}
}", 1, 1);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void AccountsForArrays()
        {
            Analyze<ParameterCanBeDeclaredWithBaseTypeAnalyzer>(@"
class TestClass
{
	void Write(string[] s)
	{
		var i = s.Length;
		SetValue (out s[1]);
	}

	void SetValue (out string s)
	{
	} 
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void LimitsParamsParametersToArrays()
        {
            Analyze<ParameterCanBeDeclaredWithBaseTypeAnalyzer>(@"
class TestClass
{
	void Write(params string[] s)
	{
		System.Console.WriteLine (s);
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void DoesNotSuggestProgramEntryPointChanges()
        {
            Analyze<ParameterCanBeDeclaredWithBaseTypeAnalyzer>(@"
class TestClass
{
	public static void Main (string[] args)
	{
		if (args.Length > 2) {
		}
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void IgnoresImplicitInterfaceImplementations()
        {
            Analyze<ParameterCanBeDeclaredWithBaseTypeAnalyzer>(@"
interface IHasFoo
{
	void Foo (string s);
}
class TestClass : IHasFoo
{
	public void Foo(string s)
	{
		object o = s;
	} 
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void IgnoresEnumParameters()
        {
            Analyze<ParameterCanBeDeclaredWithBaseTypeAnalyzer>(@"
enum ApplicableValues
{
	None,
	Some
}
class TestClass
{
	public void Foo(ApplicableValues av)
	{
		object o = av;
	} 
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void CallToOverriddenMember()
        {
            Test<ParameterCanBeDeclaredWithBaseTypeAnalyzer>(@"
class TestBase
{
	public virtual void Foo()
	{
	}
}
class Test : TestBase
{
	void F (Test t)
	{
		t.Foo();
	}
	
	public override void Foo()
	{
	}
}", @"
class TestBase
{
	public virtual void Foo()
	{
	}
}
class Test : TestBase
{
	void F (TestBase t)
	{
		t.Foo();
	}
	
	public override void Foo()
	{
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void CallToShadowingMember()
        {
            Analyze<ParameterCanBeDeclaredWithBaseTypeAnalyzer>(@"
class TestBase
{
	public virtual void Foo()
	{
	}
}
class Test : TestBase
{
	void F (Test t)
	{
		t.Foo();
	}
	
	public new void Foo()
	{
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void CallToShadowingMember2()
        {
            Test<ParameterCanBeDeclaredWithBaseTypeAnalyzer>(@"
class TestBaseBase
{
	public virtual void Foo()
	{
	}
}
class TestBase : TestBaseBase
{
	protected virtual new void Foo()
	{
	}
}
class Test : TestBase
{
	void F (Test t)
	{
		t.Foo();
	}
	
	public override void Foo()
	{
	}
}", @"
class TestBaseBase
{
	public virtual void Foo()
	{
	}
}
class TestBase : TestBaseBase
{
	protected virtual new void Foo()
	{
	}
}
class Test : TestBase
{
	void F (TestBase t)
	{
		t.Foo();
	}
	
	public override void Foo()
	{
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void CallToShadowingMemberWithBaseInterface()
        {
            Test<ParameterCanBeDeclaredWithBaseTypeAnalyzer>(@"
interface IFoo
{
	void Foo();
}
class TestBaseBase : IFoo
{
	public virtual void Foo()
	{
	}
}
class TestBase : TestBaseBase
{
	protected virtual new void Foo()
	{
	}
}
class Test : TestBase
{
	void F (Test t)
	{
		t.Foo();
	}
	
	protected override void Foo()
	{
	}
}", @"
interface IFoo
{
	void Foo();
}
class TestBaseBase : IFoo
{
	public virtual void Foo()
	{
	}
}
class TestBase : TestBaseBase
{
	protected virtual new void Foo()
	{
	}
}
class Test : TestBase
{
	void F (TestBase t)
	{
		t.Foo();
	}
	
	protected override void Foo()
	{
	}
}");
        }

        /// <summary>
        /// Bug 9617 - Incorrect "parameter can be demoted to base class" warning for arrays
        /// </summary>
        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestBug9617()
        {
            Test<ParameterCanBeDeclaredWithBaseTypeAnalyzer>(@"class Test
{
	object Foo (object[] arr)
	{
	    return arr [0];
	}
}", 1, @"class Test
{
	object Foo (System.Collections.IList arr)
	{
	    return arr [0];
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestBug9617Case2()
        {
            Test<ParameterCanBeDeclaredWithBaseTypeAnalyzer>(@"class Test
{
	int Foo (int[] arr)
	{
	    return arr [0];
	}
}", 1, @"class Test
{
	int Foo (System.Collections.Generic.IList<int> arr)
	{
	    return arr [0];
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void DoNotDemoteStringComparisonToReferenceComparison_WithinLambda()
        {
            Test<ParameterCanBeDeclaredWithBaseTypeAnalyzer>(@"using System; using System.Linq; using System.Collections.Generic;
class Test
{
	IEnumerable<User> users;
	User GetUser (String id)
	{
		return users.Where(u => u.Name == id).SingleOrDefault();
	}
}
class User {
	public string Name;
}
", 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestMicrosoftSuppressMessage()
        {
            Analyze<ParameterCanBeDeclaredWithBaseTypeAnalyzer>(@"
class A
{
	public virtual void Foo() {}
}
class B : A
{
	public virtual void Bar() {}
}
class C
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage(""Microsoft.Design"", ""CA1011:ConsiderPassingBaseTypesAsParameters"")]
	void F(B b)
	{
		b.Foo();
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestDisableAll()
        {
            Analyze<ParameterCanBeDeclaredWithBaseTypeAnalyzer>(@"// ReSharper disable All

class A
{
	public virtual void Foo() {}
}
class B : A
{
	public virtual void Bar() {}
}
class C
{
	void F(B b)
	{
		b.Foo();
	}
}");
        }


        /// <summary>
        /// Bug 14099 - Do not suggest demoting Exception to _Exception
        /// </summary>
        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestBug14099()
        {
            Analyze<ParameterCanBeDeclaredWithBaseTypeAnalyzer>(@"
using System;

public class Test
{
	public void Foo (Exception ex)
	{
		System.Console.WriteLine (ex.HelpLink);
	}
}
");
        }


        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestPreferGenerics()
        {
            Analyze<ParameterCanBeDeclaredWithBaseTypeAnalyzer>(@"using System.Collections.Generic;

class Test
{
	int Foo (ICollection<object> arr)
	{
		return arr.Count;
	}
}");
        }


    }
}

