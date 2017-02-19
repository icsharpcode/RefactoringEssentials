using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class UseIsOperatorTests : CSharpDiagnosticTestBase
    {
        [Fact(Skip="TODO: Issue not ported yet")]
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

        [Fact(Skip="TODO: Issue not ported yet")]
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

        [Fact(Skip="TODO: Issue not ported yet")]
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

