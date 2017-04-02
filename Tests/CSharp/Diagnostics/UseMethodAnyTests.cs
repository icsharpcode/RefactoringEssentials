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
			Console.WriteLine();
	}
}
";
        }

        [Fact]
        public void TestAnyNotEqual()
        {
            Analyze<UseMethodAnyAnalyzer>(ConstructExpression("$args.Count() != 0$"), ConstructExpression("args.Any()"));
        }

        [Fact]
        public void TestAnyGreater()
        {
            Analyze<UseMethodAnyAnalyzer>(ConstructExpression("$args.Count() > 0$"), ConstructExpression("args.Any()"));
        }

        [Fact]
        public void TestAnyLower()
        {
            Analyze<UseMethodAnyAnalyzer>(ConstructExpression("$0 < args.Count()$"), ConstructExpression("args.Any()"));
        }


        [Fact]
        public void TestAnyGreaterEqual()
        {
            Analyze<UseMethodAnyAnalyzer>(ConstructExpression("$args.Count() >= 1$"), ConstructExpression("args.Any()"));
        }

        [Fact]
        public void TestAnyLessEqual()
        {
            Analyze<UseMethodAnyAnalyzer>(ConstructExpression("$1 <= args.Count()$"), ConstructExpression("args.Any()"));
        }


        [Fact]
        public void TestNotAnyEqual()
        {
            Analyze<UseMethodAnyAnalyzer>(ConstructExpression("$args.Count() == 0$"), ConstructExpression("!args.Any()"));
        }

        [Fact]
        public void TestNotAnyLess()
        {
            Analyze<UseMethodAnyAnalyzer>(ConstructExpression("$args.Count() < 1$"), ConstructExpression("!args.Any()"));
        }

        [Fact]
        public void TestNotAnyGreater()
        {
            Analyze<UseMethodAnyAnalyzer>(ConstructExpression("$1 > args.Count()$"), ConstructExpression("!args.Any()"));
        }

        [Fact]
        public void TestNotAnyLessEqual()
        {
            Analyze<UseMethodAnyAnalyzer>(ConstructExpression("$args.Count() <= 0$"), ConstructExpression("!args.Any()"));
        }

        [Fact]
        public void TestNotAnyGreaterEqual()
        {
            Analyze<UseMethodAnyAnalyzer>(ConstructExpression("$0 >= args.Count()$"), ConstructExpression("!args.Any()"));
        }

        [Fact]
        public void TestDisable()
        {
            Analyze<UseMethodAnyAnalyzer>(@"
using System;
using System.Linq;
class Bar
{
	public void Foo (string[] args)
	{
		#pragma warning disable RECS0116
		if (args.Count() > 0)
			Console.WriteLine();
	}
}
");
        }

        [Fact]
        public void TestWrongMethod()
        {
            Analyze<UseMethodAnyAnalyzer>(@"
using System;
using System.Linq;
class Bar
{
	public int Count() { return 5; }

	public void Foo (Bar args)
	{
		if (args.Count() > 0)
			Console.WriteLine();
	}
}
");
        }



    }
}

