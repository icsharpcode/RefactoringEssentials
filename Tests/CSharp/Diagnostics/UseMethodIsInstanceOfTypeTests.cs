using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class UseMethodIsInstanceOfTypeTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestBasicUsage()
        {
            Test<UseMethodIsInstanceOfTypeAnalyzer>(@"using System;
class FooBar
{
	void Foo()
	{
		Type t;
		if (t.IsAssignableFrom (this.GetType ())) {
		}
	}
}
", @"using System;
class FooBar
{
	void Foo()
	{
		Type t;
		if (t.IsInstanceOfType (this)) {
		}
	}
}
");
        }

        [Test]
        public void TestDisable()
        {
            Analyze<UseMethodIsInstanceOfTypeAnalyzer>(@"using System;
class FooBar
{
	void Foo()
	{
		Type t;
		// ReSharper disable once UseMethodIsInstanceOfType
		if (t.IsAssignableFrom (this.GetType ())) {
		}
	}
}
");
        }

    }
}

