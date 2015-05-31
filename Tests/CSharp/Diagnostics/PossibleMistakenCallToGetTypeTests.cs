using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class PossibleMistakenCallToGetTypeTests : CSharpDiagnosticTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
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

