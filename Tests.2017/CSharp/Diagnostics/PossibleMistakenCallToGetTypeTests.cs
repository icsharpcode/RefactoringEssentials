using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class PossibleMistakenCallToGetTypeTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestGetTypeCase()
        {
            Analyze<PossibleMistakenCallToGetTypeAnalyzer>(@"
using System;

public class Bar
{
	public void FooBar(Type a)
	{
		Console.WriteLine($a.GetType()$);
	}
}
", @"
using System;

public class Bar
{
	public void FooBar(Type a)
	{
		Console.WriteLine(a);
	}
}
");
        }

        [Fact]
        public void TestStaticCall()
        {
            Analyze<PossibleMistakenCallToGetTypeAnalyzer>(@"
using System;

public class Bar
{
	public void FooBar(Type a)
	{
		string abc = ""def"";
		Type.GetType(abc, true);
	}
}
");
        }

        [Fact]
        public void TestDisable()
        {
            Analyze<PossibleMistakenCallToGetTypeAnalyzer>(@"
public class Bar
{
	public void FooBar(Type a)
	{
#pragma warning disable " + CSharpDiagnosticIDs.PossibleMistakenCallToGetTypeAnalyzerID + @"
		Console.WriteLine(a.GetType());
	}
}
");
        }


    }
}

