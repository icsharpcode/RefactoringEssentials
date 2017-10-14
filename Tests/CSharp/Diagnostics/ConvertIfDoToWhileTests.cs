using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class ConvertIfDoToWhileTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestBasicCase()
        {
            Analyze<ConvertIfDoToWhileAnalyzer>(@"class FooBar
{
    public void FooFoo(int x)
    {
        $if$ (x < 10)
        {
            do
            {
                Console.WriteLine(x++);
            } while (x < 10);
        }
    }
}", @"class FooBar
{
    public void FooFoo(int x)
    {
        while (x < 10)
        {
            Console.WriteLine(x++);
        }
    }
}");
        }

        [Fact]
        public void TestWithoutBlocks()
        {
            Analyze<ConvertIfDoToWhileAnalyzer>(@"class FooBar
{
    public void FooFoo(int x)
    {
        $if$ (x < 10)
            do 
                Console.WriteLine(x++);
            while (x < 10);
    }
}", @"class FooBar
{
    public void FooFoo(int x)
    {
        while (x < 10)
            Console.WriteLine(x++);
    }
}");
        }

        [Fact]
        public void TestInvalidCase()
        {
            Analyze<ConvertIfDoToWhileAnalyzer>(@"class FooBar
{
	public void FooFoo (int x)
	{
		if (x < 10) {
			do {
				Console.WriteLine (x++);
			} while (x < 11);
		}
	}
}");
        }

        [Fact]
        public void TestDisable()
        {
            Analyze<ConvertIfDoToWhileAnalyzer>(@"class FooBar
{
	public void FooFoo (int x)
	{
#pragma warning disable " + CSharpDiagnosticIDs.ConvertIfDoToWhileAnalyzerID + @"
		if (x < 10) {
			do {
				Console.WriteLine (x++);
			} while (x < 10);
		}
	}
}");
        }
    }
}

