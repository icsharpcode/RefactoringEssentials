using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
	public class EmptyDestructorTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestBasicCase()
        {
            Analyze<EmptyDestructorAnalyzer>(@"
class Foo
{
	$~Foo()
	{
	}$
}", @"
class Foo
{
}");
        }

        [Fact]
        public void TestCaseWithNesting()
        {
            Analyze<EmptyDestructorAnalyzer>(@"
class Foo
{
	$~Foo()
	{
		{}
		;
		{;}
	}$
}", @"
class Foo
{
}");
        }

        [Fact]
        public void TestDisabledForNonEmpty()
        {
            Analyze<EmptyDestructorAnalyzer>(@"
class Foo
{
	~Foo()
	{
		System.Console.WriteLine();
	}
}");
        }

        [Fact]
        public void TestDisable()
        {
            Analyze<EmptyDestructorAnalyzer>(@"
class Foo
{
#pragma warning disable " + CSharpDiagnosticIDs.EmptyDestructorAnalyzerID + @"
	~Foo()
	{
	}
}");
        }
    }
}