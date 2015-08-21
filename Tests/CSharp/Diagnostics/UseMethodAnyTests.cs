using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
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
            Analyze<UseMethodAnyAnalyzer>(ConstructExpression("args.$Count() != 0$"), ConstructExpression("args.Any()"));
        }

        [Test]
        public void TestAnyGreater()
        {
            Analyze<UseMethodAnyAnalyzer>(ConstructExpression("args.$Count() > 0$"), ConstructExpression("args.Any ()"));
        }

        [Test]
        public void TestAnyLower()
        {
            Analyze<UseMethodAnyAnalyzer>(ConstructExpression("$0 < args.Count()$"), ConstructExpression("args.Any ()"));
        }


        [Test]
        public void TestAnyGreaterEqual()
        {
            Analyze<UseMethodAnyAnalyzer>(ConstructExpression("$args.Count() >= 1$"), ConstructExpression("args.Any ()"));
        }

        [Test]
        public void TestAnyLessEqual()
        {
            Analyze<UseMethodAnyAnalyzer>(ConstructExpression("$1 <= args.Count()$"), ConstructExpression("args.Any ()"));
        }


        [Test]
        public void TestNotAnyEqual()
        {
            Analyze<UseMethodAnyAnalyzer>(ConstructExpression("$args.Count() == 0$"), ConstructExpression("!args.Any ()"));
        }

        [Test]
        public void TestNotAnyLess()
        {
            Analyze<UseMethodAnyAnalyzer>(ConstructExpression("$args.Count() < 1$"), ConstructExpression("!args.Any ()"));
        }

        [Test]
        public void TestNotAnyGreater()
        {
            Analyze<UseMethodAnyAnalyzer>(ConstructExpression("$1 > args.Count()$"), ConstructExpression("!args.Any ()"));
        }

        [Test]
        public void TestNotAnyLessEqual()
        {
            Analyze<UseMethodAnyAnalyzer>(ConstructExpression("$args.Count() <= 0$"), ConstructExpression("!args.Any ()"));
        }

        [Test]
        public void TestNotAnyGreaterEqual()
        {
            Analyze<UseMethodAnyAnalyzer>(ConstructExpression("$0 >= args.Count()$"), ConstructExpression("!args.Any ()"));
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
#pragma warning disable " + CSharpDiagnosticIDs.UseMethodAnyAnalyzerID  + @"
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

