using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class PublicConstructorInAbstractClassTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestInspectorCase1()
        {
            Analyze<PublicConstructorInAbstractClassAnalyzer>(@"
abstract class TestClass
{
	public $TestClass$ ()
	{
	}
}", @"
abstract class TestClass
{
	protected TestClass ()
	{
	}
}");
        }

        [Fact]
        public void TestInspectorCase2()
        {
            Analyze<PublicConstructorInAbstractClassAnalyzer>(@"
abstract class TestClass
{
	static TestClass ()
	{
	}
	void TestMethod ()
	{
		var i = 1;
	}
}");
        }

        [Fact]
        public void TestInspectorCase3()
        {
            Analyze<PublicConstructorInAbstractClassAnalyzer>(@"
abstract class TestClass
{
	public $TestClass$ ()
	{
	}

	private TestClass ()
	{
	}
	
	public $TestClass$ (string str)
	{
		Console.WriteLine(str);
	}
}", @"
abstract class TestClass
{
	protected TestClass ()
	{
	}

	private TestClass ()
	{
	}
	
	protected TestClass (string str)
	{
		Console.WriteLine(str);
	}
}");
        }

        [Fact]
        public void TestDisable()
        {
            Analyze<PublicConstructorInAbstractClassAnalyzer>(@"
#pragma warning disable " + CSharpDiagnosticIDs.PublicConstructorInAbstractClassAnalyzerID + @"
abstract class TestClass
{
	public TestClass ()
	{
	}
}
");
        }
    }
}