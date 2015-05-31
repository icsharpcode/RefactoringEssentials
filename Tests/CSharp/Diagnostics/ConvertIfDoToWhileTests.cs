using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class ConvertIfDoToWhileTests : CSharpDiagnosticTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

