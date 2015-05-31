using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class UseIsOperatorTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestTypeOfIsAssignableFrom()
        {
            Test<UseIsOperatorAnalyzer>(@"using System;
class FooBar
{
	void Foo()
	{
		if (typeof(FooBar).IsAssignableFrom (this.GetType ())) {
		}
	}
}
", @"using System;
class FooBar
{
	void Foo()
	{
		if (this is FooBar) {
		}
	}
}
");
        }

        [Test]
        public void TestTypeOfIsInstanceOfType()
        {
            Test<UseIsOperatorAnalyzer>(@"using System;
class FooBar
{
	void Foo()
	{
		if (typeof(FooBar).IsInstanceOfType (1 + 2)) {
		}
	}
}
", @"using System;
class FooBar
{
	void Foo()
	{
		if ((1 + 2) is FooBar) {
		}
	}
}
");
        }

        [Test]
        public void TestDisable()
        {
            Analyze<UseIsOperatorAnalyzer>(@"using System;
class FooBar
{
	void Foo()
	{
		// ReSharper disable once UseIsOperator
		if (typeof(FooBar).IsAssignableFrom (this.GetType ())) {
		}
	}
}");
        }
    }
}

