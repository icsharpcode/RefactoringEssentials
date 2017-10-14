using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class RedundantArgumentNameTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void MethodInvocation1()
        {
            Analyze<RedundantArgumentNameAnalyzer>(@"
class TestClass
{
	public void Foo(int a, int b, double c = 0.1){}
	public void F()
	{
		Foo(1,$b:$ 2);
	}
}
", @"
class TestClass
{
	public void Foo(int a, int b, double c = 0.1){}
	public void F()
	{
		Foo(1, 2);
	}
}
");
        }

        [Fact]
        public void MethodInvocation2()
        {
            Analyze<RedundantArgumentNameAnalyzer>(@"
class TestClass
{
	public void Foo(int a, int b, double c = 0.1){}
	public void F()
	{
		Foo($a:$ 1, $b:$ 2, $c:$ 0.2);
	}
}
", @"
class TestClass
{
	public void Foo(int a, int b, double c = 0.1){}
	public void F()
	{
		Foo(1, b: 2, c: 0.2);
	}
}
", 0);
        }

        [Fact]
        public void MethodInvocation3()
        {
            Analyze<RedundantArgumentNameAnalyzer>(@"
class TestClass
{
	public void Foo(int a, int b, double c = 0.1){}
	public void F()
	{
		Foo($a:$ 1, $b:$ 2, $c:$ 0.2);
	}
}
", @"
class TestClass
{
	public void Foo(int a, int b, double c = 0.1){}
	public void F()
	{
		Foo(1, 2, c: 0.2);
	}
}
", 1);
        }


        [Fact]
        public void MethodInvocation4()
        {
            Analyze<RedundantArgumentNameAnalyzer>(@"
class TestClass
{
	public void Foo (int a = 2, int b = 3, int c = 4, int d = 5, int e = 5)
	{
	}

	public void F ()
	{
		Foo(1, $b:$ 2, d: 2, c: 3, e:19);
	}
}
", @"
class TestClass
{
	public void Foo (int a = 2, int b = 3, int c = 4, int d = 5, int e = 5)
	{
	}

	public void F ()
	{
		Foo(1, 2, d: 2, c: 3, e:19);
	}
}
");
        }

        [Fact]
        public void IndexerExpression()
        {
            Analyze<RedundantArgumentNameAnalyzer>(@"
public class TestClass
{
	public int this[int i, int j]
	{
		set { }
		get { return 0; }
	}
}
internal class Test
{
	private void Foo()
	{
		var TestBases = new TestClass();
		int a = TestBases[$i:$ 1, $j:$ 2];
	}
}
", @"
public class TestClass
{
	public int this[int i, int j]
	{
		set { }
		get { return 0; }
	}
}
internal class Test
{
	private void Foo()
	{
		var TestBases = new TestClass();
		int a = TestBases[1, j: 2];
	}
}
", 0);
        }

        [Fact]
        public void TestAttributes()
        {
            Analyze<RedundantArgumentNameAnalyzer>(@"using System;
class MyAttribute : Attribute
{
	public MyAttribute(int x, int y) {}
}


[MyAttribute($x:$ 1, $y:$ 2)]
class TestClass
{
}
", @"using System;
class MyAttribute : Attribute
{
	public MyAttribute(int x, int y) {}
}


[MyAttribute(1, 2)]
class TestClass
{
}
", 1);
        }

        [Fact]
        public void TestObjectCreation()
        {
            Analyze<RedundantArgumentNameAnalyzer>(@"
class TestClass
{
	public TestClass (int x, int y)
	{
	}

	public void Foo ()
	{
		new TestClass (0, $y:$1);
	}
}
", @"
class TestClass
{
	public TestClass (int x, int y)
	{
	}

	public void Foo ()
	{
		new TestClass (0, 1);
	}
}
");
        }


        [Fact]
        public void Invalid()
        {
            Analyze<RedundantArgumentNameAnalyzer>(@"
public class TestClass
{
	public int this[int i, int j , int k]
	{
		set { }
		get { return 0; }
	}
}
internal class Test
{
	private void Foo()
	{
		var TestBases = new TestClass();
		int a = TestBases[j: 1, i: 2, k: 3];
	}
}
");
        }

    }
}