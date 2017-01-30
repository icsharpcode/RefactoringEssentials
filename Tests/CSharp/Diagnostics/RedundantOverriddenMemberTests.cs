using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class RedundantOverriddenMemberTests : CSharpDiagnosticTestBase
    {
        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestInspectorCase1()
        {
            Test<RedundantOverriddenMemberAnalyzer>(@"namespace Demo
{
	public class BaseClass
	{
		virtual public void run()
		{
			int a = 1+1;
		}
	}
	public class CSharpDemo:BaseClass
	{
		override public void run()
		{
			base.run();
		}
	}
}", @"namespace Demo
{
	public class BaseClass
	{
		virtual public void run()
		{
			int a = 1+1;
		}
	}
	public class CSharpDemo:BaseClass
	{
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestResharperDisable()
        {
            Analyze<RedundantOverriddenMemberAnalyzer>(@"namespace Demo
{
	public class BaseClass
	{
		virtual public void run()
		{
			int a = 1+1;
		}
	}
	//Resharper disable RedundantOverridenMember
	public class CSharpDemo:BaseClass
	{
		override public void run()
		{
			base.run();
		}
	}
	//Resharper restore RedundantOverridenMember
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestInspectorCase2()
        {
            Analyze<RedundantOverriddenMemberAnalyzer>(@"namespace Demo
{
	public class BaseClass
	{
		virtual public void run()
		{
			int a = 1+1;
		}
	}
	public class CSharpDemo:BaseClass
	{
		override public void run()
		{
			int b = 1+1;
			base.run();
		}
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestTestInspectorCase3()
        {
            Analyze<RedundantOverriddenMemberAnalyzer>(@"namespace Demo
{
	public class BaseClass
	{
		virtual public void run()
		{
			int a = 1+1;
		}
	}
	public class CSharpDemo:BaseClass
	{
		public void run1()
		{
			base.run();
		}
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestTestInspectorCase4()
        {
            Test<RedundantOverriddenMemberAnalyzer>(
                @"namespace Demo
{
	public class BaseClass
	{
		private int a;
		virtual public int A
		{
			get{ return a; }
			set{ a = value; }
		}
	}
	public class CSharpDemo:BaseClass
	{
		public override int A
		{
			get{ return base.A; }
			set{ base.A = value; }
		}
	}
}", @"namespace Demo
{
	public class BaseClass
	{
		private int a;
		virtual public int A
		{
			get{ return a; }
			set{ a = value; }
		}
	}
	public class CSharpDemo:BaseClass
	{
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestTestInspectorCase5()
        {
            Test<RedundantOverriddenMemberAnalyzer>(
                @"namespace Application
{
	public class SampleCollection<T>
	{ 
		private T[] arr = new T[100];
		public virtual T this[int i]
		{
			get{ return arr[i];}
			set{ arr[i] = value;}
		}
	}

	class Class2<T> : SampleCollection<T>
	{
		public override T this[int i]
		{
			get { return base[i];}
			set { base[i] = value; }
		}
	}
}", @"namespace Application
{
	public class SampleCollection<T>
	{ 
		private T[] arr = new T[100];
		public virtual T this[int i]
		{
			get{ return arr[i];}
			set{ arr[i] = value;}
		}
	}

	class Class2<T> : SampleCollection<T>
	{
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestTestInspectorCase6()
        {
            Test<RedundantOverriddenMemberAnalyzer>(
                @"using System;
using System.IO;

partial class A
{
	public virtual int AProperty
	{
		get;
		set;
	}

	public virtual int this[int i]
	{
		get { return i; }
		set { Console.WriteLine(value); }
	}
}

class B : A
{
	public override int AProperty
	{
		set
		{
			base.AProperty = value;
		}
	}
	public override int this[int i]
	{
		get
		{
			return base[i];
		}
	}
	public override string ToString()
	{
		return base.ToString();
	}
}

class C : A
{
	public override int AProperty
	{
		get
		{
			return base.AProperty;
		}
	}
}", 4,
            @"using System;
using System.IO;

partial class A
{
	public virtual int AProperty
	{
		get;
		set;
	}

	public virtual int this[int i]
	{
		get { return i; }
		set { Console.WriteLine(value); }
	}
}

class B : A
{
}

class C : A
{
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestRedundantEvent()
        {
            Test<RedundantOverriddenMemberAnalyzer>(@"namespace Demo
{
	public class BaseClass
	{
		public virtual event EventHandler FooBar { add {} remove {} }
	}
	public class CSharpDemo:BaseClass
	{
		public override event EventHandler FooBar { add { base.FooBar += value; } remove { base.FooBar -= value; } }
	}
}", @"namespace Demo
{
	public class BaseClass
	{
		public virtual event EventHandler FooBar { add {} remove {} }
	}
	public class CSharpDemo:BaseClass
	{
	}
}");
        }


        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestNonRedundantEvent()
        {
            var input = @"namespace Demo
{
	public class BaseClass
	{
		public virtual event EventHandler FooBar { add {} remove {} }
		public virtual event EventHandler FooBar2 { add {} remove {} }
	}
	public class CSharpDemo:BaseClass
	{
		public override event EventHandler FooBar { add { base.FooBar += value; } remove { base.FooBar2 -= value; } }
	}
}";
            Analyze<RedundantOverriddenMemberAnalyzer>(input);
        }


        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestGetHashCode()
        {
            Analyze<RedundantOverriddenMemberAnalyzer>(@"
class Bar
{
	public override bool Equals (object obj)
	{
		return false;
	}

	public override int GetHashCode ()
	{
		return base.GetHashCode ();
	}
}");
        }


        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestRedundantGetHashCode()
        {
            TestIssue<RedundantOverriddenMemberAnalyzer>(@"
class Bar
{
	public override int GetHashCode ()
	{
		return base.GetHashCode ();
	}
}");
        }


        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestPropertyBug()
        {
            Analyze<RedundantOverriddenMemberAnalyzer>(@"
class BaseFoo
{
    public virtual int Foo { get; set; }
}

class Bar : BaseFoo
{
    int bar;
    public override int Foo {
        get {
            return base.Foo;
        }
        set {
            base.Foo = bar = value;
        }
    }
}");
        }

        /// <summary>
        /// Bug 21533 - Incorrect warning: "Redundant method override"
        /// </summary>
        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestBug21533()
        {
            Analyze<RedundantOverriddenMemberAnalyzer>(@"
public class MyClass
{
    public virtual void SomeMethod(bool someArgument = false)
    {
    }
}

public class DerivedClass : MyClass
{
    public override void SomeMethod(bool someArgument = false)
    {
        base.SomeMethod(true);
    }
}
");
        }




    }
}