using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class UnusedAnonymousMethodSignatureTests : CSharpDiagnosticTestBase
    {
        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestSimpleUsage()
        {
            Test<UnusedAnonymousMethodSignatureAnalyzer>(@"
using System;

class TestClass
{
	void TestMethod()
	{
		Action<int> x = delegate (int p) {};
	}
}", @"
using System;

class TestClass
{
	void TestMethod()
	{
		Action<int> x = delegate {};
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestNecessaryParameter()
        {
            Analyze<UnusedAnonymousMethodSignatureAnalyzer>(@"
class TestClass
{
	void TestMethod()
	{
		//Even if just one is used, it's still necessary
		Action<int> x = delegate(int p, int q) { Console.WriteLine(p); };
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestVisitChild()
        {
            Test<UnusedAnonymousMethodSignatureAnalyzer>(@"
using System;

class TestClass
{
	void TestMethod()
	{
		Action<int> x = delegate(int p) {
			Console.WriteLine(p);
			Action<int> y = delegate(int q) {};
		};
	}
}", @"
using System;

class TestClass
{
	void TestMethod()
	{
		Action<int> x = delegate(int p) {
			Console.WriteLine(p);
			Action<int> y = delegate {};
		};
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestAmbiguousCase()
        {
            Analyze<UnusedAnonymousMethodSignatureAnalyzer>(@"
using System;
class TestClass
{
	void Foo(Action<int> action) {}
	void Foo(Action<string> action) {}
	void TestMethod()
	{
		Foo(delegate(int p) {});
	}
}
");
        }

        /// <summary>
        /// Bug 15058 - Wrong context for unused parameter list 
        /// </summary>
        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestBug15058()
        {
            Analyze<UnusedAnonymousMethodSignatureAnalyzer>(@"
using System;
using System.Threading;

class TestClass
{
	void TestMethod()
	{
		var myThing = new Thread (delegate () { doStuff (); });
	}
}
");
        }
    }
}
