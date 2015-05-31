using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class PublicConstructorInAbstractClassTests : CSharpDiagnosticTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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