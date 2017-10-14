using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class NonPublicMethodWithTestAttributeTests : CSharpDiagnosticTestBase
    {
        const string NUnitClasses = @"using System;
using NUnit.Framework;

namespace NUnit.Framework {
	public class TestFixtureAttribute : System.Attribute {}
	public class TestAttribute : System.Attribute {}
}";

        [Fact]
        public void TestImplicitPrivate()
        {
            Analyze<NonPublicMethodWithTestAttributeAnalyzer>(NUnitClasses +
                @"
[TestFixture]
class Tests 
{
	[Test]
	void $NonPublicMethod$()
	{
	}
}", NUnitClasses +
                @"
[TestFixture]
class Tests 
{
	[Test]
	public void NonPublicMethod()
	{
	}
}");
        }

        [Fact]
        public void TestExplicitPrivate()
        {
            Analyze<NonPublicMethodWithTestAttributeAnalyzer>(NUnitClasses +
                @"
[TestFixture]
class Tests 
{
	[Test]
	private void $NonPublicMethod$()
	{
	}
}", NUnitClasses +
                @"
[TestFixture]
class Tests 
{
	[Test]
	public void NonPublicMethod()
	{
	}
}");
        }

        [Fact]
        public void TestExplicitProtected()
        {
            Analyze<NonPublicMethodWithTestAttributeAnalyzer>(NUnitClasses +
                @"
[TestFixture]
class Tests 
{
	[Test]
	protected void $NonPublicMethod$()
	{
	}
}", NUnitClasses +
                @"
[TestFixture]
class Tests 
{
	[Test]
	public void NonPublicMethod()
	{
	}
}");
        }

        [Fact]
        public void TestExplicitInternal()
        {
            Analyze<NonPublicMethodWithTestAttributeAnalyzer>(NUnitClasses +
                @"
[TestFixture]
class Tests 
{
	[Test]
	internal void $NonPublicMethod$()
	{
	}
}", NUnitClasses +
                @"
[TestFixture]
class Tests 
{
	[Test]
	public void NonPublicMethod()
	{
	}
}");
        }

        [Fact]
        public void TestDisable()
        {
            Analyze<NonPublicMethodWithTestAttributeAnalyzer>(NUnitClasses + @"
class Tests 
{
#pragma warning disable " + CSharpDiagnosticIDs.NonPublicMethodWithTestAttributeAnalyzerID + @"
	[Test]
	void NonPublicMethod()
	{
	}
}
");
        }
    }
}

