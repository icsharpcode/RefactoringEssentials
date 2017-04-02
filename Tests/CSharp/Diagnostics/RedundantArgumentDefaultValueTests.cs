using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class RedundantArgumentDefaultValueTests : CSharpDiagnosticTestBase
    {
        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestSimpleCase()
        {
            Test<RedundantArgumentDefaultValueAnalyzer>(@"
class Test
{ 
	public void Bar (int foo = 22)
	{
		const int s = 22;
		Bar (s);
	}
}
", @"
class Test
{ 
	public void Bar (int foo = 22)
	{
		const int s = 22;
		Bar ();
	}
}
");
        }


        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestNamedArgument()
        {
            Test<RedundantArgumentDefaultValueAnalyzer>(@"
class Test
{ 
	public void Bar (int foo = 22)
	{
		Bar(foo: 22);
	}
}
", @"
class Test
{ 
	public void Bar (int foo = 22)
	{
		Bar ();
	}
}
");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestMultipleRemoveFirst()
        {
            Test<RedundantArgumentDefaultValueAnalyzer>(@"
class Test
{ 
	public void Bar (int foo = 22, int test = 3)
	{
		const int s = 22;
		Bar (s, 3);
	}
}
", 2, @"
class Test
{ 
	public void Bar (int foo = 22, int test = 3)
	{
		const int s = 22;
		Bar ();
	}
}
", 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestMultipleRemoveSecond()
        {
            Test<RedundantArgumentDefaultValueAnalyzer>(@"
class Test
{ 
	public void Bar (int foo = 22, int test = 3)
	{
		const int s = 22;
		Bar (s, 3);
	}
}
", 2, @"
class Test
{ 
	public void Bar (int foo = 22, int test = 3)
	{
		const int s = 22;
		Bar (s);
	}
}
", 1);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestInvalid()
        {
            Analyze<RedundantArgumentDefaultValueAnalyzer>(@"
class Test
{ 
	public void Bar (int foo = 22)
	{
		Bar (21);
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestInvalidCase2()
        {
            Analyze<RedundantArgumentDefaultValueAnalyzer>(@"
class Test
{ 
	public void Bar (int foo = 22, int bar = 3)
	{
		Bar (22, 4);
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestDisable()
        {
            Analyze<RedundantArgumentDefaultValueAnalyzer>(@"
class Test
{ 
	public void Bar (int foo = 22)
	{
		const int s = 22;
		// ReSharper disable once RedundantArgumentDefaultValue
		Bar (s);
	}
}
");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestConstructor()
        {
            Test<RedundantArgumentDefaultValueAnalyzer>(@"
class Test
{ 
	public Test (int a, int b, int c = 4711)
	{
		new Test (1, 2, 4711);
	}
}
", @"
class Test
{ 
	public Test (int a, int b, int c = 4711)
	{
		new Test (1, 2);
	}
}
");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestIndexer()
        {
            Test<RedundantArgumentDefaultValueAnalyzer>(@"
class Test
{ 
	public int this [int a, int b = 2] {
		get {
			return this [a, 2];
		}
	}
}
", @"
class Test
{ 
	public int this [int a, int b = 2] {
		get {
			return this [a];
		}
	}
}
");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void SimpleCase()
        {
            Test<RedundantArgumentDefaultValueAnalyzer>(@"
class TestClass
{
	void Foo(string a1 = ""a1"")
	{
	}

	void Bar()
	{
		Foo (""a1"");
	}
}", @"
class TestClass
{
	void Foo(string a1 = ""a1"")
	{
	}

	void Bar()
	{
		Foo ();
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void ChecksConstructors()
        {
            Test<RedundantArgumentDefaultValueAnalyzer>(@"
class TestClass
{
	public TestClass(string a1 = ""a1"")
	{
	}

	void Bar()
	{
		var foo = new TestClass (""a1"");
	}
}", @"
class TestClass
{
	public TestClass(string a1 = ""a1"")
	{
	}

	void Bar()
	{
		var foo = new TestClass ();
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void IgnoresAllParametersPreceedingANeededOne()
        {
            Test<RedundantArgumentDefaultValueAnalyzer>(@"
class TestClass
{
	void Foo(string a1 = ""a1"", string a2 = ""a2"", string a3 = ""a3"")
	{
	}

	void Bar()
	{
		Foo (""a1"", ""Another string"", ""a3"");
	}
}", @"
class TestClass
{
	void Foo(string a1 = ""a1"", string a2 = ""a2"", string a3 = ""a3"")
	{
	}

	void Bar()
	{
		Foo (""a1"", ""Another string"");
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void ChecksParametersIfParamsAreUnused()
        {
            Test<RedundantArgumentDefaultValueAnalyzer>(@"
class TestClass
{
	void Foo(string a1 = ""a1"", string a2 = ""a2"", string a3 = ""a3"", params string[] extraStrings)
	{
	}

	void Bar()
	{
		Foo (""a1"", ""a2"", ""a3"");
	}
}", 3, @"
class TestClass
{
	void Foo(string a1 = ""a1"", string a2 = ""a2"", string a3 = ""a3"", params string[] extraStrings)
	{
	}

	void Bar()
	{
		Foo ();
	}
}", 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void IgnoresIfParamsAreUsed()
        {
            Analyze<RedundantArgumentDefaultValueAnalyzer>(@"
class TestClass
{
	void Foo(string a1 = ""a1"", string a2 = ""a2"", string a3 = ""a3"", params string[] extraStrings)
	{
	}

	void Bar()
	{
		Foo (""a1"", ""a2"", ""a3"", ""extraString"");
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void NamedArgument()
        {
            Test<RedundantArgumentDefaultValueAnalyzer>(@"
class TestClass
{
	void Foo(string a1 = ""a1"", string a2 = ""a2"")
	{
	}

	void Bar()
	{
		Foo (a2: ""a2"");
	}
}", @"
class TestClass
{
	void Foo(string a1 = ""a1"", string a2 = ""a2"")
	{
	}

	void Bar()
	{
		Foo ();
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void DoesNotStopAtDifferingNamedParameters()
        {
            Test<RedundantArgumentDefaultValueAnalyzer>(@"
class TestClass
{
	void Foo(string a1 = ""a1"", string a2 = ""a2"", string a3 = ""a3"")
	{
	}

	void Bar()
	{
		Foo (""a1"", ""a2"", a3: ""non-default"");
	}
}", @"
class TestClass
{
	void Foo(string a1 = ""a1"", string a2 = ""a2"", string a3 = ""a3"")
	{
	}

	void Bar()
	{
		Foo (a3: ""non-default"");
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void DoesNotStopAtNamedParamsArray()
        {
            Test<RedundantArgumentDefaultValueAnalyzer>(@"
class TestClass
{
	void Foo(string a1 = ""a1"", string a2 = ""a2"", params string[] extras)
	{
	}

	void Bar()
	{
		Foo (""a1"", ""a2"", extras: new [] { ""extra1"" });
	}
}", @"
class TestClass
{
	void Foo(string a1 = ""a1"", string a2 = ""a2"", params string[] extras)
	{
	}

	void Bar()
	{
		Foo (extras: new[] {
	""extra1""
});
	}
}");
        }

    }
}

