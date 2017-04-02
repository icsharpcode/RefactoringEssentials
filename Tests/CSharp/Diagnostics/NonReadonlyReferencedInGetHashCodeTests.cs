using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class NonReadonlyReferencedInGetHashCodeTests : CSharpDiagnosticTestBase
    {

        [Fact]
        public void TestInspectorCase1()
        {
            Analyze<NonReadonlyReferencedInGetHashCodeAnalyzer>(@"using System;
public class TestClass1
{
	public int a = 1;
}

public class TestClass2
{
	public override int GetHashCode()
	{
		TestClass1 c = new TestClass1();
		int b = 1;
		b++;
		return c.$a$;
	}
}
");
        }

        [Fact]
        public void TestInspectorCase2()
        {
            Analyze<NonReadonlyReferencedInGetHashCodeAnalyzer>(@"using System;
public class TestClass1
{
	public int a = 1;
}

public class TestClass2
{
	private int b;
	public override int GetHashCode()
	{
		TestClass1 c = new TestClass1();
		$b$++;
		return c.$a$;
	}
}");
        }

        [Fact]
        public void TestInspectorCase3()
        {
            Analyze<NonReadonlyReferencedInGetHashCodeAnalyzer>(@"using System;
public class TestClass1
{
	public int a = 1;
}

public class TestClass2
{
	public override int GetHashCode()
	{
		TestClass1 c = new TestClass1();
		int b = 1;
		b++;
		return c.GetHashCode();
	}
}");
        }

        [Fact]
        public void TestInspectorCase4()
        {
            Analyze<NonReadonlyReferencedInGetHashCodeAnalyzer>(@"
public class Test1
{
	public int a = 1;
}

public class Test2
{
	private int q;
	public Test1 r;
	public override int GetHashCode()
	{
		Test1 c = new Test1();
		$q$ = 1 + $q$ + $r$.$a$;
		return c.$a$;
	}
}


public class Test3
{
	private int q;
	public Test2 r;
	public override int GetHashCode()
	{
		Test1 c = new Test1();
		c.GetHashCode();
		$q$ = 1 + $q$ + $r$.$r$.$a$;
		return c.$a$;
	}
}");
        }


        [Fact]
        public void TestDisable()
        {
            Analyze<NonReadonlyReferencedInGetHashCodeAnalyzer>(@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace resharper_test
{
	class Foo
	{
		private readonly int fooval;
		private int tmpval;
#pragma warning disable " + CSharpDiagnosticIDs.NonReadonlyReferencedInGetHashCodeAnalyzerID + @"
		public override int GetHashCode()
		{
			int a = 6;
			tmpval = a + 3;
			a = tmpval + 5;
			return fooval;
		}
	}
}
");
        }



        [Fact]
        public void TestConst()
        {
            Analyze<NonReadonlyReferencedInGetHashCodeAnalyzer>(@"using System;
public class TestClass1
{
	public const int a = 1;
	
	public override int GetHashCode()
	{
		return a;
	}
}
");
        }

        [Fact]
        public void TestReadOnly()
        {
            Analyze<NonReadonlyReferencedInGetHashCodeAnalyzer>(@"using System;
public class TestClass1
{
	public readonly int a = 1;
	
	public override int GetHashCode()
	{
		return a;
	}
}
");
        }
    }
}