using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
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

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestAnyNotEqual()
        {
            Test<UseMethodAnyAnalyzer>(ConstructExpression("args.Count () != 0"), ConstructExpression("args.Any ()"));
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestAnyGreater()
        {
            Test<UseMethodAnyAnalyzer>(ConstructExpression("args.Count () > 0"), ConstructExpression("args.Any ()"));
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestAnyLower()
        {
            Test<UseMethodAnyAnalyzer>(ConstructExpression("0 < args.Count ()"), ConstructExpression("args.Any ()"));
        }


        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestAnyGreaterEqual()
        {
            Test<UseMethodAnyAnalyzer>(ConstructExpression("args.Count () >= 1"), ConstructExpression("args.Any ()"));
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestAnyLessEqual()
        {
            Test<UseMethodAnyAnalyzer>(ConstructExpression("1 <= args.Count ()"), ConstructExpression("args.Any ()"));
        }


        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestNotAnyEqual()
        {
            Test<UseMethodAnyAnalyzer>(ConstructExpression("args.Count () == 0"), ConstructExpression("!args.Any ()"));
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestNotAnyLess()
        {
            Test<UseMethodAnyAnalyzer>(ConstructExpression("args.Count () < 1"), ConstructExpression("!args.Any ()"));
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestNotAnyGreater()
        {
            Test<UseMethodAnyAnalyzer>(ConstructExpression("1 > args.Count ()"), ConstructExpression("!args.Any ()"));
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestNotAnyLessEqual()
        {
            Test<UseMethodAnyAnalyzer>(ConstructExpression("args.Count () <= 0"), ConstructExpression("!args.Any ()"));
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestNotAnyGreaterEqual()
        {
            Test<UseMethodAnyAnalyzer>(ConstructExpression("0 >= args.Count ()"), ConstructExpression("!args.Any ()"));
        }

        [Fact(Skip="TODO: Issue not ported yet")]
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

        [Fact(Skip="TODO: Issue not ported yet")]
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

