using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class NonPublicMethodWithTestAttributeTests : CSharpDiagnosticTestBase
    {
        const string NUnitClasses = @"using System;
using NUnit.Framework;

namespace NUnit.Framework {
	public class TestFixtureAttribute : System.Attribute {}
	public class TestAttribute : System.Attribute {}
}";

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
        public void TestDisable()
        {
            Analyze<NonPublicMethodWithTestAttributeAnalyzer>(NUnitClasses + @"
[TestFixture]
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

