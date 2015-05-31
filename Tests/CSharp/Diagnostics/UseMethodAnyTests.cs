using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class UseMethodAnyTests : CSharpDiagnosticTestBase
    {
        static string ConstructExpression(string expr)
        {
            return @"
using System;
using System.Linq;
class Bar
{
	public void Foo (string[] args)
	{
		if (" + expr + @")
			Console.WriteLine ();
	}
}
";
        }

        [Test]
        public void TestAnyNotEqual()
        {
            Test<UseMethodAnyAnalyzer>(ConstructExpression("args.Count () != 0"), ConstructExpression("args.Any ()"));
        }

        [Test]
        public void TestAnyGreater()
        {
            Test<UseMethodAnyAnalyzer>(ConstructExpression("args.Count () > 0"), ConstructExpression("args.Any ()"));
        }

        [Test]
        public void TestAnyLower()
        {
            Test<UseMethodAnyAnalyzer>(ConstructExpression("0 < args.Count ()"), ConstructExpression("args.Any ()"));
        }


        [Test]
        public void TestAnyGreaterEqual()
        {
            Test<UseMethodAnyAnalyzer>(ConstructExpression("args.Count () >= 1"), ConstructExpression("args.Any ()"));
        }

        [Test]
        public void TestAnyLessEqual()
        {
            Test<UseMethodAnyAnalyzer>(ConstructExpression("1 <= args.Count ()"), ConstructExpression("args.Any ()"));
        }


        [Test]
        public void TestNotAnyEqual()
        {
            Test<UseMethodAnyAnalyzer>(ConstructExpression("args.Count () == 0"), ConstructExpression("!args.Any ()"));
        }

        [Test]
        public void TestNotAnyLess()
        {
            Test<UseMethodAnyAnalyzer>(ConstructExpression("args.Count () < 1"), ConstructExpression("!args.Any ()"));
        }

        [Test]
        public void TestNotAnyGreater()
        {
            Test<UseMethodAnyAnalyzer>(ConstructExpression("1 > args.Count ()"), ConstructExpression("!args.Any ()"));
        }

        [Test]
        public void TestNotAnyLessEqual()
        {
            Test<UseMethodAnyAnalyzer>(ConstructExpression("args.Count () <= 0"), ConstructExpression("!args.Any ()"));
        }

        [Test]
        public void TestNotAnyGreaterEqual()
        {
            Test<UseMethodAnyAnalyzer>(ConstructExpression("0 >= args.Count ()"), ConstructExpression("!args.Any ()"));
        }

        [Test]
        public void TestDisable()
        {
            Analyze<UseMethodAnyAnalyzer>(@"
using System;
using System.Linq;
class Bar
{
	public void Foo (string[] args)
	{
		// ReSharper disable once UseMethodAny
		if (args.Count () > 0)
			Console.WriteLine();
	}
}
");
        }

        [Test]
        public void TestWrongMethod()
        {
            Analyze<UseMethodAnyAnalyzer>(@"
using System;
using System.Linq;
class Bar
{
	public int Count () { return 5; }

	public void Foo (Bar args)
	{
		if (args.Count () > 0)
			Console.WriteLine();
	}
}
");
        }



    }
}

