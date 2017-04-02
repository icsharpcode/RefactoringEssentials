using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class UseMethodIsInstanceOfTypeTests : CSharpDiagnosticTestBase
    {
        [Fact(Skip="TODO: Issue not ported yet")]
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

        [Fact(Skip="TODO: Issue not ported yet")]
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

