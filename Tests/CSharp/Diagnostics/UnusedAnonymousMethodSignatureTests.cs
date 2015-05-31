using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class UnusedAnonymousMethodSignatureTests : CSharpDiagnosticTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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
        [Test]
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
