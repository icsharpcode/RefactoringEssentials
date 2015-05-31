using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class NameOfSuggestionTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestArgumentNullException()
        {
            Analyze<NameOfSuggestionAnalyzer>(@"
using System;
class A
{
	void F(int foo)
	{
		throw new ArgumentNullException($""foo""$, ""bar"");
	}
}", @"
using System;
class A
{
	void F(int foo)
	{
		throw new ArgumentNullException(nameof(foo), ""bar"");
	}
}");
        }

        [Test]
        public void TestArgumentException()
        {
            Analyze<NameOfSuggestionAnalyzer>(@"
using System;
class A
{
	void F(object foo)
	{
		if(foo != null)
			throw new ArgumentException(""bar"", $""foo""$);
	}
}", @"
using System;
class A
{
	void F(object foo)
	{
		if(foo != null)
			throw new ArgumentException(""bar"", nameof(foo));
	}
}");
        }

        [Test]
        public void TestArgumentOutOfRangeExceptionSwap()
        {
            Analyze<NameOfSuggestionAnalyzer>(@"
using System;
class A
{
	void F(int foo)
	{
		throw new ArgumentOutOfRangeException($""foo""$, ""foo"");
	}
}", @"
using System;
class A
{
	void F(int foo)
	{
		throw new ArgumentOutOfRangeException(nameof(foo), ""foo"");
	}
}", 0);
        }

    }
}

