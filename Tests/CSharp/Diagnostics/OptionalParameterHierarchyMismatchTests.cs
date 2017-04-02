using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class OptionalParameterHierarchyMismatchTests : CSharpDiagnosticTestBase
    {
        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestSimpleCase()
        {
            Test<OptionalParameterHierarchyMismatchAnalyzer>(@"
class Base
{
	public virtual void TestMethod(int value = 1) {}
}
class Derived : Base
{
	public override void TestMethod(int value = 2) {}
}", @"
class Base
{
	public virtual void TestMethod(int value = 1) {}
}
class Derived : Base
{
	public override void TestMethod(int value = 1) {}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestNullCase()
        {
            Test<OptionalParameterHierarchyMismatchAnalyzer>(@"
class Base
{
	public virtual void TestMethod(object value = null) {}
}
class Derived : Base
{
	public override void TestMethod(object value = null) {}
}", 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestInterface()
        {
            Test<OptionalParameterHierarchyMismatchAnalyzer>(@"
interface IBase
{
	void TestMethod(int value = 1);
}
class Derived : IBase
{
	public void TestMethod(int value = 2) {}
}", @"
interface IBase
{
	void TestMethod(int value = 1);
}
class Derived : IBase
{
	public void TestMethod(int value = 1) {}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestIndexer()
        {
            Test<OptionalParameterHierarchyMismatchAnalyzer>(@"
interface IBase
{
	int this[int x = 1]
	{
		get;
	}
}
class Derived : IBase
{
	public int this[int x = 2]
	{
		get;
	}
}", @"
interface IBase
{
	int this[int x = 1]
	{
		get;
	}
}
class Derived : IBase
{
	public int this[int x = 1]
	{
		get;
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestDisabledForCorrect()
        {
            Test<OptionalParameterHierarchyMismatchAnalyzer>(@"
interface IBase
{
	void TestMethod(int value = 1);
}
class Derived : IBase
{
	public void TestMethod(int value = 1) {}
}", 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestAddOptional()
        {
            Test<OptionalParameterHierarchyMismatchAnalyzer>(@"
class Base
{
	public virtual void TestMethod(int value) {}
}
class Derived : Base
{
	public override void TestMethod(int value = 2) {}
}", @"
class Base
{
	public virtual void TestMethod(int value) {}
}
class Derived : Base
{
	public override void TestMethod(int value) {}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestRemoveOptional()
        {
            Test<OptionalParameterHierarchyMismatchAnalyzer>(@"
class Base
{
	public virtual void TestMethod(int value = 1) {}
}
class Derived : Base
{
	public override void TestMethod(int value) {}
}", @"
class Base
{
	public virtual void TestMethod(int value = 1) {}
}
class Derived : Base
{
	public override void TestMethod(int value = 1) {}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestTakeDeclarationValue()
        {
            Test<OptionalParameterHierarchyMismatchAnalyzer>(@"
class A
{
	public virtual void Foo(int a = 1)
	{

	}
}

class B : A
{
	public override void Foo(int a = 2)
	{

	}
}

class C : B
{
	public override void Foo(int a = 3)
	{

	}
}", 2, @"
class A
{
	public virtual void Foo(int a = 1)
	{

	}
}

class B : A
{
	public override void Foo(int a = 2)
	{

	}
}

class C : B
{
	public override void Foo(int a = 1)
	{

	}
}", 1);

        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestDisableForInterfaceMismatch()
        {
            Analyze<OptionalParameterHierarchyMismatchAnalyzer>(@"class A
{
    public virtual void Foo(int a = 1)
    {

    }
}

interface IA
{
    void Foo(int a = 2);
}


class B : A, IA
{
    public override void Foo(int a = 1)
    {

    }
}");
        }


        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestDisable()
        {
            Analyze<OptionalParameterHierarchyMismatchAnalyzer>(@"
class Base
{
	public virtual void TestMethod(int value = 1) {}
}
class Derived : Base
{
	// ReSharper disable once OptionalParameterHierarchyMismatch
	public override void TestMethod(int value = 2) {}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestEnumValue()
        {
            Test<OptionalParameterHierarchyMismatchAnalyzer>(@"
enum FooBar { Foo, Bar }
class Base
{
	public virtual void TestMethod(FooBar value = FooBar.Foo) {}
}
class Derived : Base
{
	public override void TestMethod(FooBar value) {}
}", @"
enum FooBar { Foo, Bar }
class Base
{
	public virtual void TestMethod(FooBar value = FooBar.Foo) {}
}
class Derived : Base
{
	public override void TestMethod(FooBar value = FooBar.Foo) {}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestDisableForExplicitInterfaceImplementation()
        {
            Analyze<OptionalParameterHierarchyMismatchAnalyzer>(@"
interface IA
{
    void Foo(int a = 1);
}


class B : IA
{
    void IA.Foo(int a)
    {
    }
}");
        }
    }
}