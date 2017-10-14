using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class CallToObjectEqualsViaBaseTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void SimpleCase()
        {
            Analyze<CallToObjectEqualsViaBaseAnalyzer>(@"
class Foo
{
	Foo()
	{
		bool b = $base.Equals(""blah"")$;
	}
}", @"
class Foo
{
	Foo()
	{
		bool b = object.ReferenceEquals(this, ""blah"");
	}
}");
        }

        [Fact]
        public void NonObjectBase()
        {
            Analyze<CallToObjectEqualsViaBaseAnalyzer>(@"
class Foo
{
}
class Bar : Foo
{
	void Baz ()
	{
		bool b = $base.Equals(""blah"")$;
	}
}", @"
class Foo
{
}
class Bar : Foo
{
	void Baz ()
	{
		bool b = object.ReferenceEquals(this, ""blah"");
	}
}");
        }

        [Fact]
        public void IgnoresCallsToOtherObjects()
        {
            Analyze<CallToObjectEqualsViaBaseAnalyzer>(@"
class Foo
{
}
class Bar : Foo
{
	void Baz ()
	{
		var foo1 = new Foo();
		var foo2 = new Foo();
		bool b = foo1.Equals(foo2);
	}
}");
        }
    }
}