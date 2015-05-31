using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class UnassignedReadonlyFieldTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestField()
        {
            Test<UnassignedReadonlyFieldAnalyzer>(@"class Test
{
	readonly object fooBar;
}", @"class Test
{
	public Test (object fooBar)
	{
		this.fooBar = fooBar;
	}
	readonly object fooBar;
}");
        }

        [Test]
        public void TestValueTypeField()
        {
            Test<UnassignedReadonlyFieldAnalyzer>(@"class Test
{
	readonly int fooBar;
}", @"class Test
{
	public Test (int fooBar)
	{
		this.fooBar = fooBar;
	}
	readonly int fooBar;
}");
        }


        [Test]
        public void TestDisable()
        {
            Analyze<UnassignedReadonlyFieldAnalyzer>(@"class Test
{
	// ReSharper disable once UnassignedReadonlyField.Compiler
	readonly object fooBar;
}");
        }


        [Test]
        public void TestPragmaDisable()
        {
            Analyze<UnassignedReadonlyFieldAnalyzer>(@"class Test
{
	#pragma warning disable 649
	readonly int test;
	#pragma warning restore 649
}");
        }

        [Test]
        public void TestAlreadyInitalized()
        {
            Analyze<UnassignedReadonlyFieldAnalyzer>(@"class Test
{
	public Test (object fooBar)
	{
		this.fooBar = fooBar;
	}
	readonly object fooBar;
}");
        }

        [Test]
        public void TestAlreadyInitalizedCase2()
        {
            Analyze<UnassignedReadonlyFieldAnalyzer>(@"
using System;
public class FooBar
{
	sealed class Bar
	{
		public int foo;
	}

	readonly string foo;
	
	public FooBar()
	{
		this.foo = """";
	}
}
");
        }
    }
}

