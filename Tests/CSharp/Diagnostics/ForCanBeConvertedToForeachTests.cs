using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class ForCanBeConvertedToForeachTests : CSharpDiagnosticTestBase
    {
        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestArrayCase()
        {
            Test<ForCanBeConvertedToForeachAnalyzer>(@"
class Test
{
	void Foo (object[] o)
	{
		for (int i = 0; i < o.Length; i++) {
			var p = o [i];
			System.Console.WriteLine (p);
		}
	}
}", @"
class Test
{
	void Foo (object[] o)
	{
		foreach (var p in o) {
			System.Console.WriteLine (p);
		}
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestIListCase()
        {
            Test<ForCanBeConvertedToForeachAnalyzer>(@"
using System.Collections.Generic;

class Test
{
	void Foo(IList<int> o)
	{
		for (int i = 0; i < o.Count; i++) {
			var p = o [i];
			System.Console.WriteLine (p);
		}
	}
}", @"
using System.Collections.Generic;

class Test
{
	void Foo(IList<int> o)
	{
		foreach (var p in o) {
			System.Console.WriteLine (p);
		}
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestInvalid()
        {
            Analyze<ForCanBeConvertedToForeachAnalyzer>(@"
class Test
{
	void Foo (object[] o)
	{
		for (int i = 0; i < o.Length; i++) {
			var p = o [i];
			System.Console.WriteLine (p);
			System.Console.WriteLine (i++);
		}
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestInvalidCase2()
        {
            Analyze<ForCanBeConvertedToForeachAnalyzer>(@"
class Test
{
	void Foo (object[] o)
	{
		for (int i = 0; i < o.Length; i++) {
			var p = o [i];
			System.Console.WriteLine (p);
			System.Console.WriteLine (i);
		}
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestInvalidCase3()
        {
            Analyze<ForCanBeConvertedToForeachAnalyzer>(@"
class Test
{
	void Foo (object[] o)
	{
		for (int i = 0; i < o.Length; i++) {
			var p = o [i];
			p = o[0];
			System.Console.WriteLine (p);
		}
	}
}");
        }


        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestComplexExpression()
        {
            Test<ForCanBeConvertedToForeachAnalyzer>(@"
using System.Collections.Generic;

class Test
{
	IList<int> Bar { get { return null; } }

	void Foo(object o)
	{
		for (int i = 0; i < ((Test)o).Bar.Count; i++) {
			var p = ((Test)o).Bar [i];
			System.Console.WriteLine (p);
		}
	}
}", @"
using System.Collections.Generic;

class Test
{
	IList<int> Bar { get { return null; } }

	void Foo(object o)
	{
		foreach (var p in ((Test)o).Bar) {
			System.Console.WriteLine (p);
		}
	}
}");
        }


        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestOptimizedFor()
        {
            Test<ForCanBeConvertedToForeachAnalyzer>(@"
using System.Collections.Generic;

class Test
{
	IList<int> Bar { get { return null; } }

	void Foo(object o)
	{
		for (var i = 0, maxBarCount = ((Test)o).Bar.Count; i < maxBarCount; ++i) {
			var p = ((Test)o).Bar[i];
			System.Console.WriteLine(p);
		}
	}
}", @"
using System.Collections.Generic;

class Test
{
	IList<int> Bar { get { return null; } }

	void Foo(object o)
	{
		foreach (var p in ((Test)o).Bar) {
			System.Console.WriteLine (p);
		}
	}
}");
        }



        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestDisable()
        {
            Analyze<ForCanBeConvertedToForeachAnalyzer>(@"
class Test
{
	void Foo (object[] o)
	{
		// ReSharper disable once ForCanBeConvertedToForeach
		for (int i = 0; i < o.Length; i++) {
			var p = o [i];
			System.Console.WriteLine (p);
		}
	}
}");
        }
    }
}

