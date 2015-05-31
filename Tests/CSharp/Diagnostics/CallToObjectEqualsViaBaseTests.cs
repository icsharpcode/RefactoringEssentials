using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class CallToObjectEqualsViaBaseTests : CSharpDiagnosticTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
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