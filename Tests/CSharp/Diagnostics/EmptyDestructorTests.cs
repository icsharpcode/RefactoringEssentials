using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class EmptyDestructorTests : InspectionActionTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
        public void TestDisable()
        {
            Analyze<EmptyDestructorAnalyzer>(@"
class Foo
{
#pragma warning disable " + DiagnosticIDs.EmptyDestructorAnalyzerID + @"
	~Foo()
	{
	}
}");
        }
    }
}